namespace KianCommons.StockCode._VehicleAI;
using ColossalFramework;
using ColossalFramework.Math;
using System;
using UnityEngine;
using KianCommons;
using System.Reflection;

public static class Extensions {
    public static void Set(this ref Vector4 v4, Vector3 xyz, float w) => v4.Set(xyz.x, xyz.y, xyz.z, w);

    public static ref PathUnit ToPathUnit(this uint id) => ref PathManager.instance.m_pathUnits.m_buffer[id];
    public static ref Vehicle ToVehicle(this ushort id) => ref VehicleManager.instance.m_vehicles.m_buffer[id];
    public static ref Building ToBuilding(this ushort id) => ref BuildingManager.instance.m_buildings.m_buffer[id];

}
internal class VehicleAI2 : VehicleAI {

    protected void UpdatePathTargetPositions(
        ushort vehicleID, ref Vehicle vehicleData, Vector3 refPos,
        ref int index, int max, float minSqrDistanceA, float minSqrDistanceB) {
        Vector4 targetPos = vehicleData.m_targetPos0;
        targetPos.w = 1000f;
        float minSqrDist = minSqrDistanceA;
        uint pathID = vehicleData.m_path;
        byte finePathPosIndex = vehicleData.m_pathPositionIndex;
        byte pathOffset = vehicleData.m_lastPathOffset;
        if (finePathPosIndex == byte.MaxValue) {
            // initial position
            finePathPosIndex = 0;
            if (index <= 0) {
                vehicleData.m_pathPositionIndex = 0;
            }
            if (!pathID.ToPathUnit().CalculatePathPositionOffset(finePathPosIndex >> 1, targetPos, out pathOffset)) {
                InvalidPath(vehicleID, ref vehicleData, vehicleID, ref vehicleData);
                return;
            }
        }
        if (!pathID.ToPathUnit().GetPosition(finePathPosIndex >> 1, out var curPathPos)) {
            InvalidPath(vehicleID, ref vehicleData, vehicleID, ref vehicleData);
            return;
        }
        NetInfo info = curPathPos.m_segment.ToSegment().Info;
        if (info.m_lanes.Length <= curPathPos.m_lane) {
            InvalidPath(vehicleID, ref vehicleData, vehicleID, ref vehicleData);
            return;
        }
        uint curLaneId = PathManager.GetLaneID(curPathPos);
        NetInfo.Lane laneInfo = info.m_lanes[curPathPos.m_lane];
        while (true) {
            if ((finePathPosIndex & 1) == 0) { // fine = course * 2
                if (laneInfo.m_laneType != NetInfo.LaneType.CargoVehicle) {
                    bool firstIter = true;
                    while (pathOffset != curPathPos.m_offset) {
                        if (firstIter) {
                            firstIter = false;
                        } else {
                            float distDiff = Mathf.Sqrt(minSqrDist) - Vector3.Distance(targetPos, refPos);
                            int pathOffsetDelta;
                            if (distDiff < 0f) {
                                pathOffsetDelta = 4;
                            } else {
                                // path offset byte format is 0..255
                                // path offset float format is 0..1 (offset that is fed to bezier)
                                pathOffsetDelta = Mathf.CeilToInt(distDiff * 256f / (curLaneId.ToLane().m_length + 1f));
                                pathOffsetDelta = Mathf.Max(0, pathOffsetDelta);
                                pathOffsetDelta = 4 + pathOffsetDelta;
                            }
                            if (pathOffset > curPathPos.m_offset) {
                                pathOffset = (byte)Mathf.Max(pathOffset - pathOffsetDelta, curPathPos.m_offset);
                            } else if (pathOffset < curPathPos.m_offset) {
                                pathOffset = (byte)Mathf.Min(pathOffset + pathOffsetDelta, curPathPos.m_offset);
                            }
                        }
                        CalculateSegmentPosition(vehicleID, ref vehicleData,
                            curPathPos, curLaneId, pathOffset,
                            out Vector3 pos, out var _, out var maxSpeed);
                        targetPos.Set(xyz: pos, w: Mathf.Min(targetPos.w, maxSpeed));
                        if ((pos - refPos).sqrMagnitude >= minSqrDist) {
                            if (index <= 0) {
                                vehicleData.m_lastPathOffset = pathOffset;
                            }
                            vehicleData.SetTargetPos(index++, targetPos);
                            minSqrDist = minSqrDistanceB;
                            refPos = targetPos;
                            targetPos.w = 1000f;
                            if (index == max) {
                                return;
                            }
                        }
                    }
                }
                finePathPosIndex = (byte)(finePathPosIndex + 1);
                pathOffset = 0;
                if (index <= 0) {
                    vehicleData.m_pathPositionIndex = finePathPosIndex;
                    vehicleData.m_lastPathOffset = pathOffset;
                }
            }

            // stopping
            if ((vehicleData.m_flags2 & Vehicle.Flags2.EndStop) != 0) {
                if (index <= 0) {
                    targetPos.w = 0f;
                    if (VectorUtils.LengthSqrXZ(vehicleData.GetLastFrameVelocity()) < 0.01f) {
                        vehicleData.m_flags2 &= ~Vehicle.Flags2.EndStop;
                    }
                } else {
                    targetPos.w = 1f;
                }
                while (index < max) vehicleData.SetTargetPos(index++, targetPos);
                break;
            }

            // vehicle is in transition now
            int nextCoarsePathIndex = finePathPosIndex >> 1 + 1; // course[0..11] = fine[0..23]/2
            uint nextPathID = pathID;
            if (nextCoarsePathIndex >= pathID.ToPathUnit().m_positionCount) {
                nextCoarsePathIndex = 0;
                nextPathID = pathID.ToPathUnit().m_nextPathUnit;
                if (nextPathID == 0) {
                    if (index <= 0) {
                        Singleton<PathManager>.instance.ReleasePath(vehicleData.m_path);
                        vehicleData.m_path = 0u;
                    }
                    targetPos.w = 1f;
                    vehicleData.SetTargetPos(index++, targetPos);
                    break;
                }
            }
            if (!nextPathID.ToPathUnit().GetPosition(nextCoarsePathIndex, out var nextPosition)) {
                InvalidPath(vehicleID, ref vehicleData, vehicleID, ref vehicleData);
                break;
            }

            // emergency can change path.
            if ((vehicleData.m_flags & Vehicle.Flags.Emergency2) != 0 && m_info.m_vehicleType == VehicleInfo.VehicleType.Car) {
                int bestLaneID = FindBestLane(vehicleID, ref vehicleData, nextPosition);
                if (bestLaneID != nextPosition.m_lane) {
                    nextPosition.m_lane = (byte)bestLaneID;
                    nextPathID.ToPathUnit().SetPosition(nextCoarsePathIndex, nextPosition);
                }
            }
            NetInfo nextSegmentInfo = nextPosition.m_segment.ToSegment().Info;
            if (nextSegmentInfo.m_lanes.Length <= nextPosition.m_lane) {
                InvalidPath(vehicleID, ref vehicleData, vehicleID, ref vehicleData);
                break;
            }
            uint nextlaneID = PathManager.GetLaneID(nextPosition);
            NetInfo.Lane nextLaneInfo = nextSegmentInfo.m_lanes[nextPosition.m_lane];

            ref NetSegment nextPositionSegment = ref nextPosition.m_segment.ToSegment();
            ref NetSegment curPositionSegment = ref curPathPos.m_segment.ToSegment();
            ushort curStartNode = curPositionSegment.m_startNode;
            ushort curEndNode = curPositionSegment.m_endNode;
            ushort nextStartNode = nextPositionSegment.m_startNode;
            ushort nextEndNode = nextPositionSegment.m_endNode;
            NetNode.Flags curNodeFlags = curStartNode.ToNode().m_flags | curEndNode.ToNode().m_flags;
            NetNode.Flags nextNodeFlags = nextStartNode.ToNode().m_flags | nextEndNode.ToNode().m_flags;

            if (nextStartNode != curStartNode && nextStartNode != curEndNode &&
                nextEndNode != curStartNode && nextEndNode != curEndNode &&
                (curNodeFlags & NetNode.Flags.Disabled) == 0 && (nextNodeFlags & NetNode.Flags.Disabled) != 0) {
                InvalidPath(vehicleID, ref vehicleData, vehicleID, ref vehicleData);
                break;
            }

            // park vehicle
            if (nextLaneInfo.m_laneType == NetInfo.LaneType.Pedestrian) {
                if (vehicleID == 0 || (vehicleData.m_flags & Vehicle.Flags.Parking) != 0) {
                    break;
                }
                byte offset2 = curPathPos.m_offset;
                if (ParkVehicle(vehicleID, ref vehicleData, curPathPos, nextPathID, nextCoarsePathIndex << 1, out byte segmentOffset)) {
                    if (segmentOffset != offset2) {
                        if (index <= 0) {
                            vehicleData.m_pathPositionIndex = (byte)(vehicleData.m_pathPositionIndex & 0xFFFFFFFEu);
                            vehicleData.m_lastPathOffset = offset2;
                        }
                        curPathPos.m_offset = segmentOffset;
                        pathID.ToPathUnit().SetPosition(finePathPosIndex >> 1, curPathPos);
                    }
                    vehicleData.m_flags |= Vehicle.Flags.Parking;
                } else {
                    InvalidPath(vehicleID, ref vehicleData, vehicleID, ref vehicleData);
                }
                break;
            }

            if ((nextLaneInfo.m_laneType & (NetInfo.LaneType.Vehicle | NetInfo.LaneType.CargoVehicle | NetInfo.LaneType.TransportVehicle)) == 0) {
                InvalidPath(vehicleID, ref vehicleData, vehicleID, ref vehicleData);
                break;
            }
            if (nextLaneInfo.m_vehicleType != m_info.m_vehicleType && NeedChangeVehicleType(vehicleID, ref vehicleData, nextPosition, nextlaneID, nextLaneInfo.m_vehicleType, ref targetPos)) {
                if (((Vector3)targetPos - refPos).sqrMagnitude >= minSqrDist) {
                    vehicleData.SetTargetPos(index++, targetPos);
                }
                if (index <= 0) {
                    while (index < max) {
                        vehicleData.SetTargetPos(index++, targetPos);
                    }
                    if (nextPathID != vehicleData.m_path) {
                        Singleton<PathManager>.instance.ReleaseFirstUnit(ref vehicleData.m_path);
                    }
                    vehicleData.m_pathPositionIndex = (byte)(nextCoarsePathIndex << 1);
                    PathUnit.CalculatePathPositionOffset(nextlaneID, targetPos, out vehicleData.m_lastPathOffset);
                    if (vehicleID != 0 && !ChangeVehicleType(vehicleID, ref vehicleData, nextPosition, nextlaneID)) {
                        InvalidPath(vehicleID, ref vehicleData, vehicleID, ref vehicleData);
                    }
                } else {
                    while (index < max) {
                        vehicleData.SetTargetPos(index++, targetPos);
                    }
                }
                break;
            }
            if (nextPosition.m_segment != curPathPos.m_segment && vehicleID != 0) {
                vehicleData.m_flags &= ~Vehicle.Flags.Leaving;
            }
            byte nextSegOffset;
            if ((vehicleData.m_flags & Vehicle.Flags.Flying) != 0) {
                nextSegOffset = (byte)(nextPosition.m_offset < 128 ? 255u : 0u);
            } else if (curLaneId != nextlaneID && laneInfo.m_laneType != NetInfo.LaneType.CargoVehicle) {
                PathUnit.CalculatePathPositionOffset(nextlaneID, targetPos, out nextSegOffset);
                Bezier3 transitionBezier = default;
                CalculateSegmentPosition(vehicleID, ref vehicleData,
                    curPathPos, curLaneId, curPathPos.m_offset,
                    out transitionBezier.a, out var curSegDir, out var _);
                bool calculateNextNextPos = pathOffset == 0;
                if (calculateNextNextPos) {
                    calculateNextNextPos = (vehicleData.m_flags & Vehicle.Flags.Reversed) == 0 ? vehicleData.m_leadingVehicle == 0 : vehicleData.m_trailingVehicle == 0;
                }
                Vector3 nextSegDir;
                float curMaxSpeed;
                if (calculateNextNextPos) {
                    if (!nextPathID.ToPathUnit().GetNextPosition(nextCoarsePathIndex, out var nextNextPosition)) {
                        nextNextPosition = default;
                    }
                    CalculateSegmentPosition(vehicleID, ref vehicleData,
                        nextNextPosition, nextPosition, nextlaneID, nextSegOffset,
                        curPathPos, curLaneId, curPathPos.m_offset, index,
                        out transitionBezier.d, out nextSegDir, out curMaxSpeed);
                } else {
                    CalculateSegmentPosition(vehicleID, ref vehicleData,
                        nextPosition, nextlaneID, nextSegOffset,
                        out transitionBezier.d, out nextSegDir, out curMaxSpeed);
                }
                if (curMaxSpeed < 0.01f || (nextPosition.m_segment.ToSegment().m_flags & (NetSegment.Flags.Collapsed | NetSegment.Flags.Flooded)) != 0) {
                    if (index <= 0) {
                        vehicleData.m_lastPathOffset = pathOffset;
                    }
                    targetPos = transitionBezier.a;
                    targetPos.w = 0f;
                    while (index < max) {
                        vehicleData.SetTargetPos(index++, targetPos);
                    }
                    break;
                }
                if (curPathPos.m_offset == 0) {
                    curSegDir = -curSegDir;
                }
                if (nextSegOffset < nextPosition.m_offset) {
                    nextSegDir = -nextSegDir;
                }
                curSegDir.Normalize();
                nextSegDir.Normalize();
                float distance;
                if (curPathPos.m_segment == nextPosition.m_segment) {
                    distance = (transitionBezier.d - transitionBezier.a).magnitude;
                    NetInfo info3 = curPathPos.m_segment.ToSegment().Info;
                    float endRadius = info3.m_netAI.GetEndRadius();
                    float a = Mathf.Abs(info3.m_lanes[curPathPos.m_lane].m_position - info3.m_lanes[nextPosition.m_lane].m_position) * 0.75f;
                    float num9 = Mathf.Max(info3.m_lanes[curPathPos.m_lane].m_width, info3.m_lanes[nextPosition.m_lane].m_width);
                    a = Mathf.Min(a, endRadius * (1f - info3.m_pavementWidth / info3.m_halfWidth) - num9 * 0.5f);
                    transitionBezier.b = transitionBezier.a + curSegDir * a * 1.333f;
                    transitionBezier.c = transitionBezier.d + nextSegDir * a * 1.333f;
                } else {
                    NetSegment.CalculateMiddlePoints(transitionBezier.a, curSegDir, transitionBezier.d, nextSegDir, smoothStart: true, smoothEnd: true, out transitionBezier.b, out transitionBezier.c, out distance);
                }
                if (distance > 1f) {
                    ushort nextNodeID = nextSegOffset switch {
                        0 => nextPosition.m_segment.ToSegment().m_startNode,
                        byte.MaxValue => nextPosition.m_segment.ToSegment().m_endNode,
                        _ => 0,
                    };
                    float num11 = (float)Math.PI / 2f * (1f + Vector3.Dot(curSegDir, nextSegDir));
                    if (distance > 1f) {
                        num11 /= distance;
                    }
                    curMaxSpeed = Mathf.Min(curMaxSpeed, CalculateTargetSpeed(vehicleID, ref vehicleData, 1000f, num11));
                    while (pathOffset < byte.MaxValue) {
                        float num12 = Mathf.Sqrt(minSqrDist) - Vector3.Distance(targetPos, refPos);
                        int num13 = !(num12 < 0f) ? 8 + Mathf.Max(0, Mathf.CeilToInt(num12 * 256f / (distance + 1f))) : 8;
                        byte lastPathOffset = pathOffset;
                        byte stopOffset;
                        bool flag3 = NeedStopAtNode(vehicleID, ref vehicleData, nextNodeID, ref nextNodeID.ToNode(), curPathPos, curLaneId, nextPosition, nextlaneID, transitionBezier, out stopOffset) && stopOffset >= lastPathOffset;
                        pathOffset = (byte)Mathf.Min(pathOffset + num13, 255);
                        if (flag3) {
                            pathOffset = (byte)Mathf.Min(pathOffset, stopOffset);
                        }
                        Vector3 bezierPos = transitionBezier.Position(pathOffset * (1f / 255));
                        targetPos.Set(bezierPos.x, bezierPos.y, bezierPos.z, Mathf.Min(targetPos.w, curMaxSpeed));
                        if (!((bezierPos - refPos).sqrMagnitude >= minSqrDist) && !flag3) {
                            continue;
                        }
                        if (index <= 0) {
                            vehicleData.m_lastPathOffset = pathOffset;
                        }
                        if (nextNodeID != 0) {
                            UpdateNodeTargetPos(vehicleID, ref vehicleData, nextNodeID, ref nextNodeID.ToNode(), ref targetPos, index);
                            if (flag3 && pathOffset == stopOffset) {
                                if (index <= 0) {
                                    vehicleData.m_lastPathOffset = lastPathOffset;
                                }
                                targetPos.w = 0f;
                                while (index < max) {
                                    vehicleData.SetTargetPos(index++, targetPos);
                                }
                                return;
                            }
                        }
                        vehicleData.SetTargetPos(index++, targetPos);
                        minSqrDist = minSqrDistanceB;
                        refPos = targetPos;
                        targetPos.w = 1000f;
                        if (index == max) {
                            return;
                        }
                    }
                }
            } else {
                PathUnit.CalculatePathPositionOffset(nextlaneID, targetPos, out nextSegOffset);
            }
            if (index <= 0) {
                if ((nextPosition.m_segment.ToSegment().m_flags & NetSegment.Flags.Untouchable) != 0 && (curPathPos.m_segment.ToSegment().m_flags & NetSegment.Flags.Untouchable) == 0) {
                    ushort buildingID = NetSegment.FindOwnerBuilding(nextPosition.m_segment, 363f);
                    if (buildingID != 0) {
                        BuildingInfo buildingInfo = buildingID.ToBuilding().Info;
                        InstanceID itemID = default;
                        itemID.Vehicle = vehicleID;
                        buildingInfo.m_buildingAI.EnterBuildingSegment(buildingID, ref buildingID.ToBuilding(), nextPosition.m_segment, nextPosition.m_offset, itemID);
                    }
                }
                if (nextCoarsePathIndex == 0) {
                    PathManager.instance.ReleaseFirstUnit(ref vehicleData.m_path);
                }
                if (nextCoarsePathIndex >= nextPathID.ToPathUnit().m_positionCount - 1 && nextPathID.ToPathUnit().m_nextPathUnit == 0 && vehicleID != 0) {
                    ArrivingToDestination(vehicleID, ref vehicleData);
                }
            }
            pathID = nextPathID;
            finePathPosIndex = (byte)(nextCoarsePathIndex << 1);
            pathOffset = nextSegOffset;
            if (index <= 0) {
                vehicleData.m_pathPositionIndex = finePathPosIndex;
                vehicleData.m_lastPathOffset = pathOffset;
                vehicleData.m_flags = vehicleData.m_flags & ~(Vehicle.Flags.OnGravel | Vehicle.Flags.Underground | Vehicle.Flags.Transition) | nextSegmentInfo.m_setVehicleFlags;
                if (LeftHandDrive(nextLaneInfo)) {
                    vehicleData.m_flags |= Vehicle.Flags.LeftHandDrive;
                } else {
                    vehicleData.m_flags &= Vehicle.Flags.Created | Vehicle.Flags.Deleted | Vehicle.Flags.Spawned | Vehicle.Flags.Inverted | Vehicle.Flags.TransferToTarget | Vehicle.Flags.TransferToSource | Vehicle.Flags.Emergency1 | Vehicle.Flags.Emergency2 | Vehicle.Flags.WaitingPath | Vehicle.Flags.Stopped | Vehicle.Flags.Leaving | Vehicle.Flags.Arriving | Vehicle.Flags.Reversed | Vehicle.Flags.TakingOff | Vehicle.Flags.Flying | Vehicle.Flags.Landing | Vehicle.Flags.WaitingSpace | Vehicle.Flags.WaitingCargo | Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget | Vehicle.Flags.Importing | Vehicle.Flags.Exporting | Vehicle.Flags.Parking | Vehicle.Flags.CustomName | Vehicle.Flags.OnGravel | Vehicle.Flags.WaitingLoading | Vehicle.Flags.Congestion | Vehicle.Flags.DummyTraffic | Vehicle.Flags.Underground | Vehicle.Flags.Transition | Vehicle.Flags.InsideBuilding;
                }
            }
            curPathPos = nextPosition;
            curLaneId = nextlaneID;
            info = nextSegmentInfo;
            laneInfo = nextLaneInfo;
        }
    }

    public override void SimulationStep(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics) {
        if ((leaderData.m_flags2 & Vehicle.Flags2.Blown) != 0) {
            SimulationStepBlown(vehicleID, ref vehicleData, ref frameData, leaderID, ref leaderData, lodPhysics);
            return;
        }
        if ((leaderData.m_flags2 & Vehicle.Flags2.Floating) != 0) {
            SimulationStepFloating(vehicleID, ref vehicleData, ref frameData, leaderID, ref leaderData, lodPhysics);
            return;
        }
        uint currentFrameIndex = SimulationManager.instance.m_currentFrameIndex;
        frameData.m_position += frameData.m_velocity * 0.5f;
        frameData.m_swayPosition += frameData.m_swayVelocity * 0.5f;
        float accelerationPower = m_info.m_acceleration;
        float brakingPower = m_info.m_braking;
        if ((vehicleData.m_flags & Vehicle.Flags.Emergency2) != 0) {
            accelerationPower *= 2f;
            brakingPower *= 2f;
        }
        float speed = frameData.m_velocity.magnitude;
        Vector3 deltaPos = (Vector3)vehicleData.m_targetPos0 - frameData.m_position;
        float deltaPosSquar = deltaPos.sqrMagnitude;
        float delta1 = (speed + accelerationPower) * (0.5f + 0.5f * (speed + accelerationPower) / brakingPower) + m_info.m_generatedInfo.m_size.z * 0.5f;
        float delta2 = Mathf.Max(speed + accelerationPower, 5f);
        if (lodPhysics >= 2 && (currentFrameIndex >> 4 & 3) == (vehicleID & 3)) {
            delta2 *= 2f;
        }
        float detla3 = Mathf.Max((delta1 - delta2) / 3f, 1f);
        float minSqrDistanceA = delta2 * delta2;
        float minSqrDistanceB = detla3 * detla3;
        int index = 0;
        bool flag = false;
        if ((deltaPosSquar < minSqrDistanceA || vehicleData.m_targetPos3.w < 0.01f) && (leaderData.m_flags & (Vehicle.Flags.WaitingPath | Vehicle.Flags.Stopped)) == 0) {
            if (leaderData.m_path != 0) {
                UpdatePathTargetPositions(vehicleID, ref vehicleData, frameData.m_position, ref index, 4,
                    minSqrDistanceA: minSqrDistanceA, minSqrDistanceB: minSqrDistanceB);
                if ((leaderData.m_flags & Vehicle.Flags.Spawned) == 0) {
                    frameData = vehicleData.m_frame0;
                    return;
                }
            }
            if ((leaderData.m_flags & Vehicle.Flags.WaitingPath) == 0) {
                while (index < 4) {
                    float minSqrDistance;
                    Vector3 refPos;
                    if (index == 0) {
                        minSqrDistance = minSqrDistanceA;
                        refPos = frameData.m_position;
                        flag = true;
                    } else {
                        minSqrDistance = minSqrDistanceB;
                        refPos = vehicleData.GetTargetPos(index - 1);
                    }
                    int num8 = index;
                    UpdateBuildingTargetPositions(vehicleID, ref vehicleData, refPos, leaderID, ref leaderData, ref index, minSqrDistance);
                    if (index == num8) {
                        break;
                    }
                }
                if (index != 0) {
                    Vector4 targetPos = vehicleData.GetTargetPos(index - 1);
                    while (index < 4) {
                        vehicleData.SetTargetPos(index++, targetPos);
                    }
                }
            }
            deltaPos = (Vector3)vehicleData.m_targetPos0 - frameData.m_position;
            deltaPosSquar = deltaPos.sqrMagnitude;
        }
        if (leaderData.m_path != 0 && (leaderData.m_flags & Vehicle.Flags.WaitingPath) == 0) {
            byte b = leaderData.m_pathPositionIndex;
            byte lastPathOffset = leaderData.m_lastPathOffset;
            if (b == byte.MaxValue) {
                b = 0;
            }
            int totalNoise;
            float num9 = 1f + leaderData.CalculateTotalLength(leaderID, out totalNoise);
            if (leaderData.m_path.ToPathUnit().GetPosition(b >> 1, out var position)) {
                if ((position.m_segment.ToSegment().m_flags & NetSegment.Flags.Flooded) != 0 && Singleton<TerrainManager>.instance.HasWater(VectorUtils.XZ(frameData.m_position))) {
                    leaderData.m_flags2 |= Vehicle.Flags2.Floating;
                }
                position.m_segment.ToSegment().AddTraffic(Mathf.RoundToInt(num9 * 2.5f), totalNoise);
                bool flag2 = false;
                if ((b & 1) == 0 || lastPathOffset == 0) {
                    uint laneID = PathManager.GetLaneID(position);
                    if (laneID != 0) {
                        Vector3 b2 = laneID.ToLane().CalculatePosition(position.m_offset * 0.003921569f);
                        float num10 = 0.5f * speed * speed / brakingPower;
                        float z = m_info.m_generatedInfo.m_size.z;
                        if (Vector3.Distance(frameData.m_position, b2) >= num10 + z * 0.5f - 1f) {
                            laneID.ToLane().ReserveSpace(num9);
                            flag2 = true;
                        }
                    }
                }
                if (!flag2 && leaderData.m_path.ToPathUnit().GetNextPosition(b >> 1, out position)) {
                    uint laneID2 = PathManager.GetLaneID(position);
                    if (laneID2 != 0) {
                        laneID2.ToLane().ReserveSpace(num9);
                    }
                }
            }
            if ((currentFrameIndex >> 4 & 0xF) == (leaderID & 0xF)) {
                bool flag3 = false;
                uint unitID = leaderData.m_path;
                int index2 = b >> 1;
                int num11 = 0;
                while (num11 < 5) {
                    if (PathUnit.GetNextPosition(ref unitID, ref index2, out position, out var invalid)) {
                        uint laneID3 = PathManager.GetLaneID(position);
                        if (laneID3 != 0 && !laneID3.ToLane().CheckSpace(num9)) {
                            num11++;
                            continue;
                        }
                    }
                    if (invalid) {
                        InvalidPath(vehicleID, ref vehicleData, leaderID, ref leaderData);
                    }
                    flag3 = true;
                    break;
                }
                if (!flag3) {
                    leaderData.m_flags |= Vehicle.Flags.Congestion;
                }
            }
        }
        float maxSpeed;
        if ((leaderData.m_flags & Vehicle.Flags.Stopped) != 0) {
            maxSpeed = 0f;
        } else {
            maxSpeed = vehicleData.m_targetPos0.w;
            if ((leaderData.m_flags & Vehicle.Flags.DummyTraffic) == 0) {
                VehicleManager.instance.m_totalTrafficFlow += (uint)Mathf.RoundToInt(speed * 100f / Mathf.Max(1f, vehicleData.m_targetPos0.w));
                VehicleManager.instance.m_maxTrafficFlow += 100u;
            }
        }
        Quaternion quaternion = Quaternion.Inverse(frameData.m_rotation);
        deltaPos = quaternion * deltaPos;
        Vector3 vector2 = quaternion * frameData.m_velocity;
        Vector3 vector3 = Vector3.forward;
        Vector3 zero = Vector3.zero;
        Vector3 collisionPush = Vector3.zero;
        float num12 = 0f;
        float num13 = 0f;
        bool blocked = false;
        float len = 0f;
        if (deltaPosSquar > 1f) {
            vector3 = VectorUtils.NormalizeXZ(deltaPos, out len);
            if (len > 1f) {
                Vector3 v = deltaPos;
                delta2 = Mathf.Max(speed, 2f);
                if (deltaPosSquar > delta2 * delta2) {
                    v *= delta2 / Mathf.Sqrt(deltaPosSquar);
                }
                bool flag4 = false;
                if (v.z < Mathf.Abs(v.x)) {
                    if (v.z < 0f) {
                        flag4 = true;
                    }
                    float num14 = Mathf.Abs(v.x);
                    if (num14 < 1f) {
                        v.x = Mathf.Sign(v.x);
                        if (v.x == 0f) {
                            v.x = 1f;
                        }
                        num14 = 1f;
                    }
                    v.z = num14;
                }
                vector3 = VectorUtils.NormalizeXZ(v, out var len2);
                len = Mathf.Min(len, len2);
                float num15 = (float)Math.PI / 2f * (1f - vector3.z);
                if (len > 1f) {
                    num15 /= len;
                }
                float num16 = len;
                maxSpeed = !(vehicleData.m_targetPos0.w < 0.1f) ? Mathf.Min(Mathf.Min(maxSpeed, CalculateTargetSpeed(vehicleID, ref vehicleData, 1000f, num15)), CalculateMaxSpeed(num16, vehicleData.m_targetPos1.w, brakingPower * 0.9f)) : Mathf.Min(CalculateTargetSpeed(vehicleID, ref vehicleData, 1000f, num15), CalculateMaxSpeed(num16, Mathf.Min(vehicleData.m_targetPos0.w, vehicleData.m_targetPos1.w), brakingPower * 0.9f));
                num16 += VectorUtils.LengthXZ(vehicleData.m_targetPos1 - vehicleData.m_targetPos0);
                maxSpeed = Mathf.Min(maxSpeed, CalculateMaxSpeed(num16, vehicleData.m_targetPos2.w, brakingPower * 0.9f));
                num16 += VectorUtils.LengthXZ(vehicleData.m_targetPos2 - vehicleData.m_targetPos1);
                maxSpeed = Mathf.Min(maxSpeed, CalculateMaxSpeed(num16, vehicleData.m_targetPos3.w, brakingPower * 0.9f));
                num16 += VectorUtils.LengthXZ(vehicleData.m_targetPos3 - vehicleData.m_targetPos2);
                if (vehicleData.m_targetPos3.w < 0.01f) {
                    num16 = Mathf.Max(0f, num16 - m_info.m_generatedInfo.m_size.z * 0.5f);
                }
                maxSpeed = Mathf.Min(maxSpeed, CalculateMaxSpeed(num16, 0f, brakingPower * 0.9f));
                if (!DisableCollisionCheck(leaderID, ref leaderData)) {
                    CheckOtherVehicles(vehicleID, ref vehicleData, ref frameData, ref maxSpeed, ref blocked, ref collisionPush, delta1, brakingPower * 0.9f, lodPhysics);
                }
                if (flag4) {
                    maxSpeed = 0f - maxSpeed;
                }
                num12 = !(maxSpeed < speed) ? Mathf.Min(b: speed + Mathf.Max(accelerationPower, Mathf.Min(brakingPower, 0f - speed)), a: maxSpeed) : Mathf.Max(b: speed - Mathf.Max(accelerationPower, Mathf.Min(brakingPower, speed)), a: maxSpeed);
            }
        } else if (speed < 0.1f && flag && ArriveAtDestination(leaderID, ref leaderData)) {
            leaderData.Unspawn(leaderID);
            if (leaderID == vehicleID) {
                frameData = leaderData.m_frame0;
            }
            return;
        }
        if ((leaderData.m_flags & Vehicle.Flags.Stopped) == 0 && maxSpeed < 0.1f) {
            blocked = true;
        }
        if (blocked) {
            vehicleData.m_blockCounter = (byte)Mathf.Min(vehicleData.m_blockCounter + 1, 255);
        } else {
            vehicleData.m_blockCounter = 0;
        }
        if (len > 1f) {
            num13 = Mathf.Asin(vector3.x) * Mathf.Sign(num12);
            zero = vector3 * num12;
        } else {
            num12 = 0f;
            zero = vector2 + Vector3.ClampMagnitude(deltaPos * 0.5f - vector2, brakingPower);
        }
        bool flag5 = (currentFrameIndex + leaderID & 0x10) != 0;
        Vector3 vector4 = zero - vector2;
        Vector3 vector5 = frameData.m_rotation * zero;
        frameData.m_velocity = vector5 + collisionPush;
        frameData.m_position += frameData.m_velocity * 0.5f;
        frameData.m_swayVelocity = frameData.m_swayVelocity * (1f - m_info.m_dampers) - vector4 * (1f - m_info.m_springs) - frameData.m_swayPosition * m_info.m_springs;
        frameData.m_swayPosition += frameData.m_swayVelocity * 0.5f;
        frameData.m_steerAngle = num13;
        frameData.m_travelDistance += zero.z;
        frameData.m_lightIntensity.x = 5f;
        frameData.m_lightIntensity.y = !(vector4.z < -0.1f) ? 0.5f : 5f;
        frameData.m_lightIntensity.z = !(num13 < -0.1f) || !flag5 ? 0f : 5f;
        frameData.m_lightIntensity.w = !(num13 > 0.1f) || !flag5 ? 0f : 5f;
        frameData.m_underground = (vehicleData.m_flags & Vehicle.Flags.Underground) != 0;
        frameData.m_transition = (vehicleData.m_flags & Vehicle.Flags.Transition) != 0;
        if ((vehicleData.m_flags & Vehicle.Flags.Parking) != 0 && len <= 1f && flag) {
            Vector3 forward = vehicleData.m_targetPos1 - vehicleData.m_targetPos0;
            if (forward.sqrMagnitude > 0.01f) {
                frameData.m_rotation = Quaternion.LookRotation(forward);
            }
        } else if (num12 > 0.1f) {
            if (vector5.sqrMagnitude > 0.01f) {
                frameData.m_rotation = Quaternion.LookRotation(vector5);
            }
        } else if (num12 < -0.1f && vector5.sqrMagnitude > 0.01f) {
            frameData.m_rotation = Quaternion.LookRotation(-vector5);
        }
        base.SimulationStep(vehicleID, ref vehicleData, ref frameData, leaderID, ref leaderData, lodPhysics);
    }

}
