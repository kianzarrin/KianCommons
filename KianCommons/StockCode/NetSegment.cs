namespace KianCommons.StockCode {
    using ColossalFramework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using static NetSegment;

    public partial struct NetSegment2 {
        // NetNode
        // Token: 0x060034C1 RID: 13505 RVA: 0x0023A834 File Offset: 0x00238C34
        const uint SEGMENT_HOLDER = BuildingManager.MAX_BUILDING_COUNT;

        public Notification.Problem m_problems;

        public Bounds m_bounds;

        public Vector3 m_middlePosition;

        public Vector3 m_startDirection;

        public Vector3 m_endDirection;

        public NetSegment.Flags m_flags;

        public float m_averageLength;

        public uint m_buildIndex;

        public uint m_modifiedIndex;

        public uint m_lanes;

        public uint m_path;

        public ushort m_startNode;

        public ushort m_endNode;

        public ushort m_blockStartLeft;

        public ushort m_blockStartRight;

        public ushort m_blockEndLeft;

        public ushort m_blockEndRight;

        public ushort m_trafficBuffer;

        public ushort m_noiseBuffer;

        public ushort m_startLeftSegment;

        public ushort m_startRightSegment;

        public ushort m_endLeftSegment;

        public ushort m_endRightSegment;

        public ushort m_infoIndex;

        public ushort m_nextGridSegment;

        public ushort m_nameSeed;

        public byte m_trafficDensity;

        public byte m_trafficLightState0;

        public byte m_trafficLightState1;

        public byte m_cornerAngleStart;

        public byte m_cornerAngleEnd;

        public byte m_fireCoverage;

        public byte m_wetness;

        public byte m_condition;

        public byte m_noiseDensity;

        public bool m_overridePathFindDirectionLimit;

        private static HashSet<ushort> m_tempCheckedSet = new HashSet<ushort>();

        private static HashSet<ushort> m_tempCheckedSet2 = new HashSet<ushort>();

        public static void RenderLod(RenderManager.CameraInfo cameraInfo, NetInfo.LodValue lod) {
            NetManager instance = Singleton<NetManager>.instance;
            MaterialPropertyBlock materialBlock = instance.m_materialBlock;
            materialBlock.Clear();
            Mesh mesh;
            int upperLoadCount;
            if(lod.m_lodCount <= 1) {
                mesh = lod.m_key.m_mesh.m_mesh1;
                upperLoadCount = 1;
            } else if(lod.m_lodCount <= 4) {
                mesh = lod.m_key.m_mesh.m_mesh4;
                upperLoadCount = 4;
            } else {
                mesh = lod.m_key.m_mesh.m_mesh8;
                upperLoadCount = 8;
            }
            for(int i = lod.m_lodCount; i < upperLoadCount; i++) {
                lod.m_leftMatrices[i] = default(Matrix4x4);
                lod.m_rightMatrices[i] = default(Matrix4x4);
                lod.m_meshScales[i] = default;
                lod.m_objectIndices[i] = default;
                lod.m_meshLocations[i] = cameraInfo.m_forward * -100000f;
            }
            materialBlock.SetMatrixArray(instance.ID_LeftMatrices, lod.m_leftMatrices);
            materialBlock.SetMatrixArray(instance.ID_RightMatrices, lod.m_rightMatrices);
            materialBlock.SetVectorArray(instance.ID_MeshScales, lod.m_meshScales);
            materialBlock.SetVectorArray(instance.ID_ObjectIndices, lod.m_objectIndices);
            materialBlock.SetVectorArray(instance.ID_MeshLocations, lod.m_meshLocations);
            if(lod.m_surfaceTexA != null) {
                materialBlock.SetTexture(instance.ID_SurfaceTexA, lod.m_surfaceTexA);
                materialBlock.SetTexture(instance.ID_SurfaceTexB, lod.m_surfaceTexB);
                materialBlock.SetVector(instance.ID_SurfaceMapping, lod.m_surfaceMapping);
                lod.m_surfaceTexA = null;
                lod.m_surfaceTexB = null;
            }
            if(lod.m_heightMap != null) {
                materialBlock.SetTexture(instance.ID_HeightMap, lod.m_heightMap);
                materialBlock.SetVector(instance.ID_HeightMapping, lod.m_heightMapping);
                materialBlock.SetVector(instance.ID_SurfaceMapping, lod.m_surfaceMapping);
                lod.m_heightMap = null;
            }
            if(mesh != null) {
                Bounds bounds = default(Bounds);
                bounds.SetMinMax(lod.m_lodMin - new Vector3(100f, 100f, 100f), lod.m_lodMax + new Vector3(100f, 100f, 100f));
                mesh.bounds = bounds;
                lod.m_lodMin = new Vector3(100000f, 100000f, 100000f);
                lod.m_lodMax = new Vector3(-100000f, -100000f, -100000f);
                instance.m_drawCallData.m_lodCalls++;
                instance.m_drawCallData.m_batchedCalls += lod.m_lodCount - 1;
                Graphics.DrawMesh(mesh, Matrix4x4.identity, lod.m_material, lod.m_key.m_layer, null, 0, materialBlock);
            }
            lod.m_lodCount = 0;
        }

        private void RenderInstance(ref NetSegment This, RenderManager.CameraInfo cameraInfo, ushort segmentID, int layerMask, NetInfo info, ref RenderManager.Instance data) {
            NetManager instance = Singleton<NetManager>.instance;
            if(data.m_dirty) {
                data.m_dirty = false;
                Vector3 position = instance.m_nodes.m_buffer[m_startNode].m_position;
                Vector3 position2 = instance.m_nodes.m_buffer[m_endNode].m_position;
                data.m_position = (position + position2) * 0.5f;
                data.m_rotation = Quaternion.identity;
                data.m_dataColor0 = info.m_color;
                data.m_dataColor0.a = 0f;
                data.m_dataFloat0 = Singleton<WeatherManager>.instance.GetWindSpeed(data.m_position);
                data.m_dataVector0 = new Vector4(0.5f / info.m_halfWidth, 1f / info.m_segmentLength, 1f, 1f);
                Vector4 colorLocation = RenderManager.GetColorLocation((uint)(49152 + segmentID));
                Vector4 vector = colorLocation;
                if(NetNode.BlendJunction(m_startNode)) {
                    colorLocation = RenderManager.GetColorLocation((uint)(86016 + m_startNode));
                }
                if(NetNode.BlendJunction(m_endNode)) {
                    vector = RenderManager.GetColorLocation((uint)(86016 + m_endNode));
                }
                data.m_dataVector3 = new Vector4(colorLocation.x, colorLocation.y, vector.x, vector.y);
                if(info.m_segments == null || info.m_segments.Length == 0) {
                    if(info.m_lanes != null) {
                        bool invert;
                        if((m_flags & Flags.Invert) != 0) {
                            invert = true;
                            instance.m_nodes.m_buffer[m_endNode].Info.m_netAI.GetNodeState(m_endNode, ref instance.m_nodes.m_buffer[m_endNode], segmentID, ref This, out _, out _); // unused code
                            instance.m_nodes.m_buffer[m_startNode].Info.m_netAI.GetNodeState(m_startNode, ref instance.m_nodes.m_buffer[m_startNode], segmentID, ref This, out _, out _); // unused code
                        } else {
                            invert = false;
                            instance.m_nodes.m_buffer[m_startNode].Info.m_netAI.GetNodeState(m_startNode, ref instance.m_nodes.m_buffer[m_startNode], segmentID, ref This, out _, out _); // unused code
                            instance.m_nodes.m_buffer[m_endNode].Info.m_netAI.GetNodeState(m_endNode, ref instance.m_nodes.m_buffer[m_endNode], segmentID, ref This, out _, out _); // unused code
                        }
                        float startAngle = (float)(int)m_cornerAngleStart * ((float)Math.PI / 128f);
                        float endAngle = (float)(int)m_cornerAngleEnd * ((float)Math.PI / 128f);
                        int propIndex = 0;
                        uint num = m_lanes;
                        for(int i = 0; i < info.m_lanes.Length; i++) {
                            if(num == 0) {
                                break;
                            }
                            instance.m_lanes.m_buffer[num].RefreshInstance(num, info.m_lanes[i], startAngle, endAngle, invert, ref data, ref propIndex);
                            num = instance.m_lanes.m_buffer[num].m_nextLane;
                        }
                    }
                } else {
                    float vScale = info.m_netAI.GetVScale();
                    CalculateCorner(segmentID, heightOffset: true, start: true, leftSide: true, out var posSL, out var dirSL, out var smoothStart);
                    CalculateCorner(segmentID, heightOffset: true, start: false, leftSide: true, out var posEL, out var dirEL, out var smoothEnd);
                    CalculateCorner(segmentID, heightOffset: true, start: true, leftSide: false, out var posSR, out var dirSR, out smoothStart);
                    CalculateCorner(segmentID, heightOffset: true, start: false, leftSide: false, out var posER, out var dirER, out smoothEnd);
                    CalculateMiddlePoints(posSL, dirSL, posER, dirER, smoothStart, smoothEnd, out var b1, out var c1);
                    CalculateMiddlePoints(posSR, dirSR, posEL, dirEL, smoothStart, smoothEnd, out var b2, out var c2);
                    data.m_dataMatrix0 = CalculateControlMatrix(posSL, b1, c1, posER, posSR, b2, c2, posEL, data.m_position, vScale);
                    data.m_dataMatrix1 = CalculateControlMatrix(posSR, b2, c2, posEL, posSL, b1, c1, posER, data.m_position, vScale);
                }
                if(info.m_requireSurfaceMaps) {
                    Singleton<TerrainManager>.instance.GetSurfaceMapping(data.m_position, out data.m_dataTexture0, out data.m_dataTexture1, out data.m_dataVector1);
                } else if(info.m_requireHeightMap) {
                    Singleton<TerrainManager>.instance.GetHeightMapping(data.m_position, out data.m_dataTexture0, out data.m_dataVector1, out data.m_dataVector2);
                }
            }
            if(info.m_segments != null && (layerMask & info.m_netLayers) != 0) {
                for(int j = 0; j < info.m_segments.Length; j++) {
                    NetInfo.Segment segment = info.m_segments[j];
                    if(!segment.CheckFlags(m_flags, out var turnAround)) {
                        continue;
                    }
                    Vector4 dataVector3 = data.m_dataVector3;
                    Vector4 dataVector0 = data.m_dataVector0;
                    if(segment.m_requireWindSpeed) {
                        dataVector3.w = data.m_dataFloat0;
                    }
                    if(turnAround) {
                        dataVector0.x = 0f - dataVector0.x;
                        dataVector0.y = 0f - dataVector0.y;
                    }
                    if(cameraInfo.CheckRenderDistance(data.m_position, segment.m_lodRenderDistance)) {
                        instance.m_materialBlock.Clear();
                        instance.m_materialBlock.SetMatrix(instance.ID_LeftMatrix, data.m_dataMatrix0);
                        instance.m_materialBlock.SetMatrix(instance.ID_RightMatrix, data.m_dataMatrix1);
                        instance.m_materialBlock.SetVector(instance.ID_MeshScale, dataVector0);
                        instance.m_materialBlock.SetVector(instance.ID_ObjectIndex, dataVector3);
                        instance.m_materialBlock.SetColor(instance.ID_Color, data.m_dataColor0);
                        if(segment.m_requireSurfaceMaps && data.m_dataTexture0 != null) {
                            instance.m_materialBlock.SetTexture(instance.ID_SurfaceTexA, data.m_dataTexture0);
                            instance.m_materialBlock.SetTexture(instance.ID_SurfaceTexB, data.m_dataTexture1);
                            instance.m_materialBlock.SetVector(instance.ID_SurfaceMapping, data.m_dataVector1);
                        } else if(segment.m_requireHeightMap && data.m_dataTexture0 != null) {
                            instance.m_materialBlock.SetTexture(instance.ID_HeightMap, data.m_dataTexture0);
                            instance.m_materialBlock.SetVector(instance.ID_HeightMapping, data.m_dataVector1);
                            instance.m_materialBlock.SetVector(instance.ID_SurfaceMapping, data.m_dataVector2);
                        }
                        instance.m_drawCallData.m_defaultCalls++;
                        Graphics.DrawMesh(segment.m_segmentMesh, data.m_position, data.m_rotation, segment.m_segmentMaterial, segment.m_layer, null, 0, instance.m_materialBlock);
                        continue;
                    }
                    NetInfo.LodValue combinedLod = segment.m_combinedLod;
                    if(combinedLod == null) {
                        continue;
                    }
                    if(segment.m_requireSurfaceMaps) {
                        if(data.m_dataTexture0 != combinedLod.m_surfaceTexA) {
                            if(combinedLod.m_lodCount != 0) {
                                RenderLod(cameraInfo, combinedLod);
                            }
                            combinedLod.m_surfaceTexA = data.m_dataTexture0;
                            combinedLod.m_surfaceTexB = data.m_dataTexture1;
                            combinedLod.m_surfaceMapping = data.m_dataVector1;
                        }
                    } else if(segment.m_requireHeightMap && data.m_dataTexture0 != combinedLod.m_heightMap) {
                        if(combinedLod.m_lodCount != 0) {
                            RenderLod(cameraInfo, combinedLod);
                        }
                        combinedLod.m_heightMap = data.m_dataTexture0;
                        combinedLod.m_heightMapping = data.m_dataVector1;
                        combinedLod.m_surfaceMapping = data.m_dataVector2;
                    }
                    ref Matrix4x4 reference = ref combinedLod.m_leftMatrices[combinedLod.m_lodCount];
                    reference = data.m_dataMatrix0;
                    ref Matrix4x4 reference2 = ref combinedLod.m_rightMatrices[combinedLod.m_lodCount];
                    reference2 = data.m_dataMatrix1;
                    combinedLod.m_meshScales[combinedLod.m_lodCount] = dataVector0;
                    combinedLod.m_objectIndices[combinedLod.m_lodCount] = dataVector3;
                    ref Vector4 reference3 = ref combinedLod.m_meshLocations[combinedLod.m_lodCount];
                    reference3 = data.m_position;
                    combinedLod.m_lodMin = Vector3.Min(combinedLod.m_lodMin, data.m_position);
                    combinedLod.m_lodMax = Vector3.Max(combinedLod.m_lodMax, data.m_position);
                    if(++combinedLod.m_lodCount == combinedLod.m_leftMatrices.Length) {
                        RenderLod(cameraInfo, combinedLod);
                    }
                }
            }
            if(info.m_lanes == null || ((layerMask & info.m_propLayers) == 0 && !cameraInfo.CheckRenderDistance(data.m_position, info.m_maxPropDistance + 128f))) {
                return;
            }
            bool invert2;
            NetNode.Flags flags3;
            Color color3;
            NetNode.Flags flags4;
            Color color4;
            if((m_flags & Flags.Invert) != 0) {
                invert2 = true;
                instance.m_nodes.m_buffer[m_endNode].Info.m_netAI.GetNodeState(m_endNode, ref instance.m_nodes.m_buffer[m_endNode], segmentID, ref This, out flags3, out color3); 
                instance.m_nodes.m_buffer[m_startNode].Info.m_netAI.GetNodeState(m_startNode, ref instance.m_nodes.m_buffer[m_startNode], segmentID, ref This, out flags4, out color4); 
            } else {
                invert2 = false;
                instance.m_nodes.m_buffer[m_startNode].Info.m_netAI.GetNodeState(m_startNode, ref instance.m_nodes.m_buffer[m_startNode], segmentID, ref This, out flags3, out color3); 
                instance.m_nodes.m_buffer[m_endNode].Info.m_netAI.GetNodeState(m_endNode, ref instance.m_nodes.m_buffer[m_endNode], segmentID, ref This, out flags4, out color4); 
            }
            float startAngle2 = (float)(int)m_cornerAngleStart * ((float)Math.PI / 128f);
            float endAngle2 = (float)(int)m_cornerAngleEnd * ((float)Math.PI / 128f);
            Vector4 objectIndex = new Vector4(data.m_dataVector3.x, data.m_dataVector3.y, 1f, data.m_dataFloat0);
            Vector4 objectIndex2 = new Vector4(data.m_dataVector3.z, data.m_dataVector3.w, 1f, data.m_dataFloat0);
            InfoManager.InfoMode currentMode = Singleton<InfoManager>.instance.CurrentMode;
            if(currentMode != 0 && !info.m_netAI.ColorizeProps(currentMode)) {
                objectIndex.z = 0f;
                objectIndex2.z = 0f;
            }
            int propIndex2 = ((info.m_segments != null && info.m_segments.Length != 0) ? (-1) : 0);
            uint num2 = m_lanes;
            if((m_flags & Flags.Collapsed) != 0) {
                for(int k = 0; k < info.m_lanes.Length; k++) {
                    if(num2 == 0) {
                        break;
                    }
                    instance.m_lanes.m_buffer[num2].RenderDestroyedInstance(cameraInfo, segmentID, num2, info, info.m_lanes[k], flags3, flags4, color3, color4, startAngle2, endAngle2, invert2, layerMask, objectIndex, objectIndex2, ref data, ref propIndex2);
                    num2 = instance.m_lanes.m_buffer[num2].m_nextLane;
                }
                return;
            }
            for(int l = 0; l < info.m_lanes.Length; l++) {
                if(num2 == 0) {
                    break;
                }
                instance.m_lanes.m_buffer[num2].RenderInstance(cameraInfo, segmentID, num2, info.m_lanes[l], flags3, flags4, color3, color4, startAngle2, endAngle2, invert2, layerMask, objectIndex, objectIndex2, ref data, ref propIndex2);
                num2 = instance.m_lanes.m_buffer[num2].m_nextLane;
            }
        }
        public void CalculateCorner(ushort segmentID, bool heightOffset, bool start, bool leftSide, out Vector3 cornerPos, out Vector3 cornerDirection, out bool smooth) {
            throw new NotImplementedException();
        }


    }

}