namespace KianCommons.StockCode {
    using ColossalFramework;

    internal class RoadBaseAI : NetAI {
        public override void UpdateNodeFlags(ushort nodeID, ref NetNode data) {
            base.UpdateNodeFlags(nodeID, ref data);
            NetNode.FlagsLong nodeFlags = data.flags;
            uint roadLevels = 0u;
            int roadLevelCount = 0;
            NetManager instance = Singleton<NetManager>.instance;
            int incommingSegmentCount = 0;
            int incommingLaneCount = 0;
            int vehicleSegmentCount = 0;
            bool wantTrafficLights = WantTrafficLights();
            bool hasCar = false;
            int roadServiceCount = 0;
            int trainCount = 0;
            int pedestrianZoneCount = 0;
            int publicTransportCount = 0;
            for (int i = 0; i < 8; i++) {
                ushort segmentId = data.GetSegment(i);
                if (segmentId == 0) {
                    continue;
                }
                NetInfo segmentinfo = segmentId.ToSegment().Info;
                if ((object)segmentinfo == null) {
                    continue;
                }
                uint roadLevel = 1U << (int)segmentinfo.m_class.m_level;
                if ((roadLevels & roadLevel) == 0) {
                    roadLevels |= roadLevel;
                    roadLevelCount++;
                }
                if (segmentinfo.m_netAI.WantTrafficLights()) {
                    wantTrafficLights = true;
                }
                if ((segmentinfo.m_vehicleTypes & VehicleInfo.VehicleType.Car) != 0 != ((m_info.m_vehicleTypes & VehicleInfo.VehicleType.Car) != 0)) {
                    hasCar = true;
                }
                int forward = 0;
                int backward = 0;
                segmentId.ToSegment().CountLanes(segmentId, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, VehicleInfo.VehicleType.Car | VehicleInfo.VehicleType.Tram | VehicleInfo.VehicleType.Trolleybus, VehicleInfo.VehicleCategory.All, ref forward, ref backward);
                if (segmentId.ToSegment().m_endNode == nodeID) {
                    if (forward != 0) {
                        incommingSegmentCount++; // incomming toward node
                        incommingLaneCount += forward;
                    }
                } else if (backward != 0) {
                    incommingSegmentCount++;
                    incommingLaneCount += backward;
                }
                if (forward != 0 || backward != 0) {
                    vehicleSegmentCount++;
                }
                if (segmentinfo.m_class.m_service == ItemClass.Service.Road) {
                    roadServiceCount++;
                } else if ((segmentinfo.m_vehicleTypes & VehicleInfo.VehicleType.Train) != 0) {
                    trainCount++;
                }
                if (segmentinfo.IsPedestrianZoneRoad()) {
                    pedestrianZoneCount++;
                } else if (segmentinfo.IsPublicTransportRoad()) {
                    publicTransportCount++;
                }
            }
            int carOnlyCount = roadServiceCount - pedestrianZoneCount - publicTransportCount;

            if (roadServiceCount >= 1 && trainCount >= 1) {
                nodeFlags &= ~NetNode.FlagsLong.CustomTrafficLights;
                nodeFlags = nodeFlags.SetFlags(
                    NetNode.FlagsLong.LevelCrossing | NetNode.FlagsLong.TrafficLights,
                    roadServiceCount < 1 || trainCount < 2);
                nodeFlags = nodeFlags.SetFlags( NetNode.FlagsLong.PedestrianBollards, pedestrianZoneCount < 1);
            } else {
                nodeFlags &= ~NetNode.FlagsLong.LevelCrossing;
                if (wantTrafficLights) {
                    wantTrafficLights = (incommingSegmentCount > 2 || (incommingSegmentCount >= 2 && vehicleSegmentCount >= 3 && incommingLaneCount > 6)) && (nodeFlags & NetNode.FlagsLong.Junction) != 0 && carOnlyCount >= 2;
                }
                if ((nodeFlags & NetNode.FlagsLong.CustomTrafficLights) == 0) {
                    nodeFlags = ((!wantTrafficLights) ? (nodeFlags & ~NetNode.FlagsLong.TrafficLights) : (nodeFlags | NetNode.FlagsLong.TrafficLights));
                } else if (!CanEnableTrafficLights(nodeID, ref data)) {
                    nodeFlags &= ~(NetNode.FlagsLong.TrafficLights | NetNode.FlagsLong.CustomTrafficLights);
                } else if (wantTrafficLights == ((data.flags & NetNode.FlagsLong.TrafficLights) != 0)) {
                    nodeFlags &= ~NetNode.FlagsLong.CustomTrafficLights;
                }
                nodeFlags = nodeFlags.SetFlags(
                    NetNode.FlagsLong.PedestrianBollards,
                    carOnlyCount < 1 || pedestrianZoneCount + publicTransportCount < 1);
            }

            nodeFlags = nodeFlags.SetFlags(NetNode.FlagsLong.Transition, roadLevelCount < 2 && !hasCar);
            nodeFlags = nodeFlags.SetFlags(NetNode.FlagsLong.PedestrianStreetTransition,
                (pedestrianZoneCount < 1 && publicTransportCount < 1) || (roadServiceCount - pedestrianZoneCount < 1 && trainCount < 1));
            nodeFlags = nodeFlags.SetFlags(NetNode.FlagsLong.RegularRoadEnd,
                (nodeFlags & NetNode.FlagsLong.PedestrianBollards) == 0 || pedestrianZoneCount < 2 || roadServiceCount - pedestrianZoneCount != 1);
            data.flags = nodeFlags;
        }
    }
}
