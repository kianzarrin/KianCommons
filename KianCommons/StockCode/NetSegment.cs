namespace KianCommons.StockCode {
    using ColossalFramework;
    using ColossalFramework.Math;
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

        // NetSegment
        public static bool CalculateArrowGroupData(ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays) {
            vertexCount += 4;
            triangleCount += 6;
            objectCount++;
            vertexArrays |= RenderGroup.VertexArrays.Vertices | RenderGroup.VertexArrays.Normals | RenderGroup.VertexArrays.Uvs;
            return true;
        }
        public static void PopulateArrowGroupData(Vector3 pos, Vector3 dir, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance) {
            float len142 = VectorUtils.LengthXZ(dir) * 1.42f;
            Vector3 dir2 = new Vector3(len142, Mathf.Abs(dir.y), len142);
            min = Vector3.Min(min, pos - dir2);
            max = Vector3.Max(max, pos + dir2);
            maxRenderDistance = Mathf.Max(maxRenderDistance, 20000f);
            Vector3 diff = pos - groupPosition;
            Vector3 normalRight = new Vector3(dir.z, 0f, - dir.x);
            data.m_vertices[vertexIndex] = diff - dir - normalRight; ;
            data.m_normals[vertexIndex] = pos;
            data.m_uvs[vertexIndex] = new Vector2(0, 0);

            vertexIndex++;
            data.m_vertices[vertexIndex] = diff - dir + normalRight;
            data.m_normals[vertexIndex] = pos;
            data.m_uvs[vertexIndex] = new Vector2(1f, 0f);

            vertexIndex++;
            data.m_vertices[vertexIndex] = diff + dir + normalRight;
            data.m_normals[vertexIndex] = pos;
            data.m_uvs[vertexIndex] = new Vector2(1f, 1f);

            vertexIndex++;
            data.m_vertices[vertexIndex] = diff + dir - normalRight;
            data.m_normals[vertexIndex] = pos;
            data.m_uvs[vertexIndex] = new Vector2(0f, 1f);

            vertexIndex++;
            data.m_triangles[triangleIndex++] = vertexIndex - 4;
            data.m_triangles[triangleIndex++] = vertexIndex - 1;
            data.m_triangles[triangleIndex++] = vertexIndex - 3;
            data.m_triangles[triangleIndex++] = vertexIndex - 3;
            data.m_triangles[triangleIndex++] = vertexIndex - 1;
            data.m_triangles[triangleIndex++] = vertexIndex - 2;
        }

        public bool CalculateGroupData(ushort segmentID, int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays) {
            bool result = false;
            bool hasProps = false;
            NetInfo info = Info;
            if(m_problems != Notification.Problem.None && layer == Singleton<NotificationManager>.instance.m_notificationLayer && Notification.CalculateGroupData(ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays)) {
                result = true;
            }
            if(info.m_hasForwardVehicleLanes != info.m_hasBackwardVehicleLanes && layer == Singleton<NetManager>.instance.m_arrowLayer && CalculateArrowGroupData(ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays)) {
                result = true;
            }
            if(info.m_lanes != null) {
                bool invert;
                NetNode.Flags flags;
                NetNode.Flags flags2;
                if((m_flags & Flags.Invert) != 0) {
                    invert = true;
                    m_endNode.ToNode().Info.m_netAI.GetNodeFlags(m_endNode, ref m_endNode.ToNode(), segmentID, ref this, out flags);
                    m_startNode.ToNode().Info.m_netAI.GetNodeFlags(m_startNode, ref m_startNode.ToNode(), segmentID, ref this, out flags2);
                } else {
                    invert = false;
                    m_startNode.ToNode().Info.m_netAI.GetNodeFlags(m_startNode, ref m_startNode.ToNode(), segmentID, ref this, out flags);
                    m_endNode.ToNode().Info.m_netAI.GetNodeFlags(m_endNode, ref m_endNode.ToNode(), segmentID, ref this, out flags2);
                }
                bool destroyed = (m_flags & Flags.Collapsed) != 0;
                uint laneID = m_lanes;
                for(int i = 0; i < info.m_lanes.Length; i++) {
                    if(laneID == 0) {
                        break;
                    }
                    if(laneID.ToLane().CalculateGroupData(laneID, info.m_lanes[i], destroyed, flags, flags2, invert, layer, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays, ref hasProps)) {
                        result = true;
                    }
                    laneID = laneID.ToLane().m_nextLane;
                }
            }
            if((info.m_netLayers & (1 << layer)) != 0) {
                bool hasSegments = !info.m_segments.IsNullorEmpty();
                if(hasSegments || hasProps) {
                    result = true;
                    if(hasSegments) {
                        for(int i = 0; i < info.m_segments.Length; i++) {
                            NetInfo.Segment segmentInfo = info.m_segments[i];
                            if(segmentInfo.m_layer == layer && segmentInfo.CheckFlags(m_flags, out _) && segmentInfo.m_combinedLod != null) {
                                CalculateGroupData(segmentInfo, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays);
                            }
                        }
                    }
                }
            }
            return result;
        }
        public static void CalculateGroupData(NetInfo.Segment segmentInfo, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays) {
            RenderGroup.MeshData meshData = segmentInfo.m_combinedLod.m_key.m_mesh.m_data;
            vertexCount += meshData.m_vertices.Length;
            triangleCount += meshData.m_triangles.Length;
            objectCount++;
            vertexArrays |= meshData.VertexArrayMask() | RenderGroup.VertexArrays.Colors | RenderGroup.VertexArrays.Uvs2 | RenderGroup.VertexArrays.Uvs4;
        }

        public void PopulateGroupData(ushort segmentID, int groupX, int groupZ, int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance, ref bool requireSurfaceMaps) {
            bool hasProps = false;
            NetInfo info = Info;
            NetManager instance = Singleton<NetManager>.instance;
            if(m_problems != Notification.Problem.None && layer == Singleton<NotificationManager>.instance.m_notificationLayer) {
                Vector3 middlePosition = m_middlePosition;
                middlePosition.y += info.m_maxHeight;
                Notification.PopulateGroupData(m_problems, middlePosition, 1f, groupX, groupZ, ref vertexIndex, ref triangleIndex, groupPosition, data, ref min, ref max, ref maxRenderDistance, ref maxInstanceDistance);
            }
            if(info.m_hasForwardVehicleLanes != info.m_hasBackwardVehicleLanes && layer == Singleton<NetManager>.instance.m_arrowLayer) {
                Bezier3 bezier = default(Bezier3);
                bezier.a = Singleton<NetManager>.instance.m_nodes.m_buffer[m_startNode].m_position;
                bezier.d = Singleton<NetManager>.instance.m_nodes.m_buffer[m_endNode].m_position;
                CalculateMiddlePoints(bezier.a, m_startDirection, bezier.d, m_endDirection, smoothStart: true, smoothEnd: true, out bezier.b, out bezier.c);
                Vector3 pos = bezier.Position(0.5f);
                pos.y += info.m_netAI.GetSnapElevation();
                Vector3 vector = VectorUtils.NormalizeXZ(bezier.Tangent(0.5f)) * (4f + info.m_halfWidth * 0.5f);
                if((m_flags & Flags.Invert) != 0 == info.m_hasForwardVehicleLanes) {
                    vector = -vector;
                }
                PopulateArrowGroupData(pos, vector, ref vertexIndex, ref triangleIndex, groupPosition, data, ref min, ref max, ref maxRenderDistance, ref maxInstanceDistance);
            }
            if(info.m_lanes != null) {
                bool invert;
                NetNode.Flags flags;
                NetNode.Flags flags2;
                if((m_flags & Flags.Invert) != 0) {
                    invert = true;
                    instance.m_nodes.m_buffer[m_endNode].Info.m_netAI.GetNodeFlags(m_endNode, ref instance.m_nodes.m_buffer[m_endNode], segmentID, ref this, out flags);
                    instance.m_nodes.m_buffer[m_startNode].Info.m_netAI.GetNodeFlags(m_startNode, ref instance.m_nodes.m_buffer[m_startNode], segmentID, ref this, out flags2);
                } else {
                    invert = false;
                    instance.m_nodes.m_buffer[m_startNode].Info.m_netAI.GetNodeFlags(m_startNode, ref instance.m_nodes.m_buffer[m_startNode], segmentID, ref this, out flags);
                    instance.m_nodes.m_buffer[m_endNode].Info.m_netAI.GetNodeFlags(m_endNode, ref instance.m_nodes.m_buffer[m_endNode], segmentID, ref this, out flags2);
                }
                bool terrainHeight = info.m_segments == null || info.m_segments.Length == 0;
                float startAngle = (float)(int)m_cornerAngleStart * ((float)Math.PI / 128f);
                float endAngle = (float)(int)m_cornerAngleEnd * ((float)Math.PI / 128f);
                bool destroyed = (m_flags & Flags.Collapsed) != 0;
                uint num = m_lanes;
                for(int i = 0; i < info.m_lanes.Length; i++) {
                    if(num == 0) {
                        break;
                    }
                    instance.m_lanes.m_buffer[num].PopulateGroupData(segmentID, num, info.m_lanes[i], destroyed, flags, flags2, startAngle, endAngle, invert, terrainHeight, layer, ref vertexIndex, ref triangleIndex, groupPosition, data, ref min, ref max, ref maxRenderDistance, ref maxInstanceDistance, ref hasProps);
                    num = instance.m_lanes.m_buffer[num].m_nextLane;
                }
            }
            if((info.m_netLayers & (1 << layer)) == 0) {
                return;
            }
            bool flag = info.m_segments != null && info.m_segments.Length != 0;
            if(!flag && !hasProps) {
                return;
            }
            min = Vector3.Min(min, m_bounds.min);
            max = Vector3.Max(max, m_bounds.max);
            maxRenderDistance = Mathf.Max(maxRenderDistance, 30000f);
            maxInstanceDistance = Mathf.Max(maxInstanceDistance, 1000f);
            if(!flag) {
                return;
            }
            float vScale = info.m_netAI.GetVScale();
            CalculateCorner(segmentID, heightOffset: true, start: true, leftSide: true, out var cornerPosSL, out var cornerDirectionSL, out var smoothStart);
            CalculateCorner(segmentID, heightOffset: true, start: false, leftSide: true, out var cornerPosEL, out var cornerDirectionEL, out var smoothEnd);
            CalculateCorner(segmentID, heightOffset: true, start: true, leftSide: false, out var cornerPosSR, out var cornerDirectionSR, out smoothStart);
            CalculateCorner(segmentID, heightOffset: true, start: false, leftSide: false, out var cornerPosER, out var cornerDirectionER, out smoothEnd);
            CalculateMiddlePoints(cornerPosSL, cornerDirectionSL, cornerPosER, cornerDirectionER, smoothStart, smoothEnd, out var b1, out var c1);
            CalculateMiddlePoints(cornerPosSR, cornerDirectionSR, cornerPosEL, cornerDirectionEL, smoothStart, smoothEnd, out var b2, out var c2);
            Vector3 position = instance.m_nodes.m_buffer[m_startNode].m_position;
            Vector3 position2 = instance.m_nodes.m_buffer[m_endNode].m_position;
            Vector4 meshScale = new Vector4(0.5f / info.m_halfWidth, 1f / info.m_segmentLength, 1f, 1f);
            Vector4 colorLocationStart = RenderManager.GetColorLocation((uint)(49152 + segmentID));
            Vector4 colorlocationEnd = colorLocationStart;
            if(NetNode.BlendJunction(m_startNode)) {
                colorLocationStart = RenderManager.GetColorLocation((uint)(86016 + m_startNode));
            }
            if(NetNode.BlendJunction(m_endNode)) {
                colorlocationEnd = RenderManager.GetColorLocation((uint)(86016 + m_endNode));
            }
            Vector4 objectIndex0 = new Vector4(colorLocationStart.x, colorLocationStart.y, colorlocationEnd.x, colorlocationEnd.y);
            for(int j = 0; j < info.m_segments.Length; j++) {
                NetInfo.Segment segment = info.m_segments[j];
                bool turnAround = false;
                if(segment.m_layer == layer && segment.CheckFlags(m_flags, out turnAround) && segment.m_combinedLod != null) {
                    Vector4 objectIndex = objectIndex0;
                    if(segment.m_requireWindSpeed) {
                        objectIndex.w = Singleton<WeatherManager>.instance.GetWindSpeed((position + position2) * 0.5f);
                    } else if(turnAround) {
                        objectIndex = new Vector4(objectIndex.z, objectIndex.w, objectIndex.x, objectIndex.y);
                    }
                    Matrix4x4 leftMatrix;
                    Matrix4x4 rightMatrix;
                    if(turnAround) {
                        leftMatrix = CalculateControlMatrix(cornerPosEL, c2, b2, cornerPosSR, cornerPosER, c1, b1, cornerPosSL, groupPosition, vScale);
                        rightMatrix = CalculateControlMatrix(cornerPosER, c1, b1, cornerPosSL, cornerPosEL, c2, b2, cornerPosSR, groupPosition, vScale);
                    } else {
                        leftMatrix = CalculateControlMatrix(cornerPosSL, b1, c1, cornerPosER, cornerPosSR, b2, c2, cornerPosEL, groupPosition, vScale);
                        rightMatrix = CalculateControlMatrix(cornerPosSR, b2, c2, cornerPosEL, cornerPosSL, b1, c1, cornerPosER, groupPosition, vScale);
                    }
                    PopulateGroupData(info, segment, leftMatrix, rightMatrix, meshScale, objectIndex, ref vertexIndex, ref triangleIndex, groupPosition, data, ref requireSurfaceMaps);
                }
            }
        }

        public static void PopulateGroupData(NetInfo info, NetInfo.Segment segmentInfo, Matrix4x4 leftMatrix, Matrix4x4 rightMatrix, Vector4 meshScale, Vector4 objectIndex,
            ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData meshData, ref bool requireSurfaceMaps) {
            if(segmentInfo.m_requireSurfaceMaps) {
                requireSurfaceMaps = true;
            }
            RenderGroup.MeshData meshData2 = segmentInfo.m_combinedLod.m_key.m_mesh.m_data;
            int[] triangles = meshData2.m_triangles;
            int num = triangles.Length;
            for(int i = 0; i < num; i++) {
                meshData.m_triangles[triangleIndex++] = triangles[i] + vertexIndex;
            }
            RenderGroup.VertexArrays vertexArrays = meshData2.VertexArrayMask();
            Vector3[] vertices = meshData2.m_vertices;
            Vector3[] normals = meshData2.m_normals;
            Vector4[] tangents = meshData2.m_tangents;
            Vector2[] uvs = meshData2.m_uvs;
            Vector2[] uvs2 = meshData2.m_uvs3;
            Color32[] colors = meshData2.m_colors;
            int num2 = vertices.Length;
            Vector2 vector = new Vector2(objectIndex.x, objectIndex.y);
            Vector2 vector2 = new Vector2(objectIndex.z, objectIndex.w);
            for(int i = 0; i < num2; i++) {
                Vector3 vertex = vertices[i];
                vertex.x = vertex.x * meshScale.x + 0.5f;
                vertex.z = vertex.z * meshScale.y + 0.5f;
                Vector4 vector4 = new Vector4(vertex.z, 1f - vertex.z, 3f * vertex.z, 0f - vertex.z);
                Vector4 vector5 = new Vector4(vector4.y * vector4.y * vector4.y, vector4.z * vector4.y * vector4.y, vector4.z * vector4.x * vector4.y, vector4.x * vector4.x * vector4.x);
                Vector4 vector6 = new Vector4(vector4.y * (-1f - vector4.w) * 3f, vector4.y * (1f - vector4.z) * 3f, vector4.x * (2f - vector4.z) * 3f, vector4.x * (0f - vector4.w) * 3f);
                Vector4 vector7 = leftMatrix * vector5;
                Vector4 vector8 = vector7 + (rightMatrix * vector5 - vector7) * vertex.x;
                Vector4 vector9 = leftMatrix * vector6;
                Vector3 vector10 = Vector3.Normalize(vector9 + (rightMatrix * vector6 - vector9) * vertex.x);
                Vector3 vector11 = Vector3.Normalize(new Vector3(vector10.z, 0f, 0f - vector10.x));
                Matrix4x4 matrix4x = default(Matrix4x4);
                matrix4x.SetColumn(0, vector11);
                matrix4x.SetColumn(1, Vector3.Cross(vector10, vector11));
                matrix4x.SetColumn(2, vector10);
                if(segmentInfo.m_requireHeightMap) {
                    vector8.y = Singleton<TerrainManager>.instance.SampleDetailHeight((Vector3)vector8 + groupPosition);
                }
                meshData.m_vertices[vertexIndex] = new Vector3(vector8.x, vector8.y + vertex.y, vector8.z);
                if((vertexArrays & RenderGroup.VertexArrays.Normals) != 0) {
                    meshData.m_normals[vertexIndex] = matrix4x.MultiplyVector(normals[i]);
                } else {
                    meshData.m_normals[vertexIndex] = new Vector3(0f, 1f, 0f);
                }
                if((vertexArrays & RenderGroup.VertexArrays.Tangents) != 0) {
                    Vector4 vector12 = tangents[i];
                    Vector3 vector13 = matrix4x.MultiplyVector(vector12);
                    vector12.x = vector13.x;
                    vector12.y = vector13.y;
                    vector12.z = vector13.z;
                    meshData.m_tangents[vertexIndex] = vector12;
                } else {
                    meshData.m_tangents[vertexIndex] = new Vector4(1f, 0f, 0f, 1f);
                }
                Color32 color = (((vertexArrays & RenderGroup.VertexArrays.Colors) == 0) ? new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue) : colors[i]);
                if((vertexArrays & RenderGroup.VertexArrays.Uvs) != 0) {
                    Vector2 vector14 = uvs[i];
                    vector14.y = Mathf.Lerp(vector14.y, vector8.w, (float)(int)color.g * 0.003921569f);
                    meshData.m_uvs[vertexIndex] = vector14;
                } else {
                    Vector2 vector15 = default(Vector2);
                    vector15.y = Mathf.Lerp(vector15.y, vector8.w, (float)(int)color.g * 0.003921569f);
                    meshData.m_uvs[vertexIndex] = vector15;
                }
                meshData.m_colors[vertexIndex] = info.m_netAI.GetGroupVertexColor(segmentInfo, i);
                meshData.m_uvs2[vertexIndex] = vector;
                meshData.m_uvs4[vertexIndex] = vector2;
                if((vertexArrays & RenderGroup.VertexArrays.Uvs3) != 0) {
                    meshData.m_uvs3[vertexIndex] = uvs2[i];
                }
                vertexIndex++;
            }
        }
        public static void RenderLod(RenderManager.CameraInfo cameraInfo, NetInfo.LodValue lod) {
            NetManager netMan = Singleton<NetManager>.instance;
            MaterialPropertyBlock materialBlock = netMan.m_materialBlock;
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
            materialBlock.SetMatrixArray(netMan.ID_LeftMatrices, lod.m_leftMatrices);
            materialBlock.SetMatrixArray(netMan.ID_RightMatrices, lod.m_rightMatrices);
            materialBlock.SetVectorArray(netMan.ID_MeshScales, lod.m_meshScales);
            materialBlock.SetVectorArray(netMan.ID_ObjectIndices, lod.m_objectIndices);
            materialBlock.SetVectorArray(netMan.ID_MeshLocations, lod.m_meshLocations);
            if(lod.m_surfaceTexA != null) {
                materialBlock.SetTexture(netMan.ID_SurfaceTexA, lod.m_surfaceTexA);
                materialBlock.SetTexture(netMan.ID_SurfaceTexB, lod.m_surfaceTexB);
                materialBlock.SetVector(netMan.ID_SurfaceMapping, lod.m_surfaceMapping);
                lod.m_surfaceTexA = null;
                lod.m_surfaceTexB = null;
            }
            if(lod.m_heightMap != null) {
                materialBlock.SetTexture(netMan.ID_HeightMap, lod.m_heightMap);
                materialBlock.SetVector(netMan.ID_HeightMapping, lod.m_heightMapping);
                materialBlock.SetVector(netMan.ID_SurfaceMapping, lod.m_surfaceMapping);
                lod.m_heightMap = null;
            }
            if(mesh != null) {
                Bounds bounds = default(Bounds);
                bounds.SetMinMax(lod.m_lodMin - new Vector3(100f, 100f, 100f), lod.m_lodMax + new Vector3(100f, 100f, 100f));
                mesh.bounds = bounds;
                lod.m_lodMin = new Vector3(100000f, 100000f, 100000f);
                lod.m_lodMax = new Vector3(-100000f, -100000f, -100000f);
                netMan.m_drawCallData.m_lodCalls++;
                netMan.m_drawCallData.m_batchedCalls += lod.m_lodCount - 1;
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
                    Vector4 objectIndex = data.m_dataVector3;
                    Vector4 meshScale = data.m_dataVector0;
                    if(segment.m_requireWindSpeed) {
                        objectIndex.w = data.m_dataFloat0;
                    }
                    if(turnAround) {
                        meshScale.x = 0f - meshScale.x;
                        meshScale.y = 0f - meshScale.y;
                    }
                    if(cameraInfo.CheckRenderDistance(data.m_position, segment.m_lodRenderDistance)) {
                        instance.m_materialBlock.Clear();
                        instance.m_materialBlock.SetMatrix(instance.ID_LeftMatrix, data.m_dataMatrix0);
                        instance.m_materialBlock.SetMatrix(instance.ID_RightMatrix, data.m_dataMatrix1);
                        instance.m_materialBlock.SetVector(instance.ID_MeshScale, meshScale);
                        instance.m_materialBlock.SetVector(instance.ID_ObjectIndex, objectIndex);
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
                    combinedLod.m_meshScales[combinedLod.m_lodCount] = meshScale;
                    combinedLod.m_objectIndices[combinedLod.m_lodCount] = objectIndex;
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