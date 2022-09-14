namespace KianCommons.StockCode.NetInfo_ {
    using System;
    using ColossalFramework;
    using UnityEngine;
    using static global::NetInfo;

    [Serializable]
    public class Node {
        public Mesh m_mesh;

        public Mesh m_lodMesh;

        public Material m_material;

        public Material m_lodMaterial;

        [BitMask]
        [CustomizableProperty("Flags Required")]
        public NetNode.Flags m_flagsRequired;

        [BitMask]
        [CustomizableProperty("Flags Required 2")]
        public NetNode.Flags2 m_flagsRequired2;

        [BitMask]
        [CustomizableProperty("Flags Forbidden")]
        public NetNode.Flags m_flagsForbidden;

        [BitMask]
        [CustomizableProperty("Flags Forbidden 2")]
        public NetNode.Flags2 m_flagsForbidden2;

        [BitMask]
        [CustomizableProperty("Connect Group")]
        public ConnectGroup m_connectGroup;

        [CustomizableProperty("Direct Connect")]
        public bool m_directConnect;

        public bool m_emptyTransparent;

        public string[] m_tagsRequired;

        public DynamicFlags m_nodeTagsRequired;

        public string[] m_tagsForbidden;

        public DynamicFlags m_nodeTagsForbidden;

        public bool m_forbidAnyTags;

        public byte m_minSameTags;

        public byte m_maxSameTags = 7;

        public byte m_minOtherTags;

        public byte m_maxOtherTags = 7;

        [NonSerialized]
        public Mesh m_nodeMesh;

        [NonSerialized]
        public Material m_nodeMaterial;

        [NonSerialized]
        public LodValue m_combinedLod;

        [NonSerialized]
        public float m_lodRenderDistance;

        [NonSerialized]
        public bool m_requireSurfaceMaps;

        [NonSerialized]
        public bool m_requireWindSpeed;

        [NonSerialized]
        public bool m_preserveUVs;

        [NonSerialized]
        public bool m_generateTangents;

        [NonSerialized]
        public int m_layer;

        private bool NeedCheckLimits => m_maxOtherTags < 7 || m_maxSameTags < 7 || m_minOtherTags > 0 || m_minSameTags > 0;

        public NetNode.FlagsLong flagsRequired {
            get {
                return NetNode.GetFlags(m_flagsRequired, m_flagsRequired2);
            }
            set {
                NetNode.GetFlagParts(value, out m_flagsRequired, out m_flagsRequired2);
            }
        }

        public NetNode.FlagsLong flagsForbidden {
            get {
                return NetNode.GetFlags(m_flagsForbidden, m_flagsForbidden2);
            }
            set {
                NetNode.GetFlagParts(value, out m_flagsForbidden, out m_flagsForbidden2);
            }
        }

        public bool CheckFlags(NetNode.FlagsLong flags) {
            if (((flagsRequired | flagsForbidden) & flags) == flagsRequired) {
                return true;
            }
            return false;
        }

        public bool CheckTags(ushort nodeID, ref NetNode node) {
            if (NeedCheckLimits) {
                return CheckTagsLimit(nodeID, ref node);
            }
            return CheckTags(node.m_tags);
        }

        public bool CheckTags(ushort nodeID, ref NetNode node, NetInfo segmentInfo) {
            if (!NeedCheckLimits || CheckTagsLimit(nodeID, ref node)) {
                return CheckTags(segmentInfo.m_netTags);
            }
            return false;
        }

        private bool CheckTagsLimit(ushort nodeID, ref NetNode node) {
            int otherTagsCount = 0;
            int sameTagsCount = 0;
            for (int i = 0; i < 8; i++) {
                ushort segmentId = node.GetSegment(i);
                if (segmentId == 0) {
                    continue;
                }
                if (CheckTags(segmentId.ToSegment().Info.m_netTags)) {
                    sameTagsCount++;
                    if (sameTagsCount > m_maxSameTags) {
                        return false;
                    }
                } else {
                    otherTagsCount++;
                    if (otherTagsCount > m_maxOtherTags) {
                        return false;
                    }
                }
            }
            if (otherTagsCount < m_minOtherTags || sameTagsCount < m_minSameTags) {
                return false;
            }
            return true;
        }

        public bool CheckTags(DynamicFlags tags) {
            if (((m_nodeTagsRequired | m_nodeTagsForbidden) & tags) == m_nodeTagsRequired) {
                return true;
            }
            return false;
        }
    }

}
