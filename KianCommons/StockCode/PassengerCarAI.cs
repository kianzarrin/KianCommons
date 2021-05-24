namespace KianCommons.StockCode {
    using ColossalFramework.Math;
    using UnityEngine;

    class PassengerCarAI {
        private static ushort CheckOverlap(ushort ignoreParked, ref Bezier3 bezier,
            Vector3 pos, Vector3 dir, float offset, float length,
            ushort otherID, ref VehicleParked otherData,
            ref bool overlap, ref float minPos, ref float maxPos)
        {
            if (otherID != ignoreParked) {
                VehicleInfo info = otherData.Info;
                Vector3 otherPos = otherData.m_position;
                Vector3 diff = otherPos - pos;
                float carWidth = info.m_generatedInfo.m_size.z;
                float num = (length + carWidth) * 0.5f + 1f;
                float diffLength = diff.magnitude;
                if (diffLength < num - 0.5f) {
                    overlap = true;
                    float distance;
                    float num2;
                    if (Vector3.Dot(diff, dir) >= 0f) {
                        distance = num + diffLength;
                        num2 = num - diffLength;
                    } else {
                        distance = num - diffLength;
                        num2 = num + diffLength;
                    }
                    maxPos = Mathf.Max(maxPos, bezier.Travel(offset, distance));
                    minPos = Mathf.Min(minPos, bezier.Travel(offset, -num2));
                }
            }
            return otherData.m_nextGridParked;
        }
    }
}
