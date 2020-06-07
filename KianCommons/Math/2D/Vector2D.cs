using UnityEngine;

namespace KianCommons.Math {
    public struct Vector2D {
        public const float kEpsilon = 1E-05F;
        public float x;
        public float z;
        public float this[int index] {
            get {
                switch (index) {
                    case 0: return x;
                    case 1: return z;
                }
                throw new System.Exception($"Index:{index} out of range");
            }
            set {
                switch (index) {
                    case 0: x = value; return;
                    case 1: z = value; return;
                }
                throw new System.Exception($"Index:{index} out of range");
            }
        }

        public static Vector2D down => Vector2.down;
        public static Vector2D up => Vector2.up;
        public static Vector2D one => Vector2.one;
        public static Vector2D zero => Vector2.zero;
        public static Vector2D left => Vector2.left;
        public static Vector2D right => Vector2.right;

        public Vector2D(float x, float z) { this.x = x; this.z = z; } 
        public Vector2D(Vector2 v) { this.x = v.x; this.z = v.y; }
        public Vector3D ToVector3D(float h = 0) => new Vector3D(this, h);
        public static implicit operator Vector2D(Vector2 v) => new Vector2D(v);
        public static implicit operator Vector2(Vector2D v) => new Vector2(v.x,v.z);

        public override bool Equals(object other) {
            if (!(other is Vector2D)) return false;
                Vector2D vector = (Vector2D)other;
            return x == vector.x && z == vector.z;
        }
        public override int GetHashCode() => x.GetHashCode() ^ z.GetHashCode() << 2;
        public override string ToString() => $"(x:{x}, z:{z})";
        public string ToString(string format) => $"(x:{x.ToString(format)},z:{z.ToString(format)})";

        public float magnitude => Mathf.Sqrt(sqrMagnitude);
        public float sqrMagnitude => x * x + z * z;
        public Vector2D normalized => this / magnitude;

        public static Vector2D operator +(Vector2D a, Vector2D b) => new Vector2D(a.x + b.x, a.z + b.z);
        public static Vector2D operator -(Vector2D a, Vector2D b) => new Vector2D(a.x - b.x, a.z - b.z);
        public static Vector2D operator -(Vector2D a) => new Vector2D(- a.x , -a.z);
        public static Vector2D operator *(float d, Vector2D a) => new Vector2D(d * a.x, d * a.z);
        public static Vector2D operator *(Vector2D a, float d) => d * a;
        public static Vector2D operator /(Vector2D a, float d) => new Vector2D(a.x/d, a.z/d);
        public static bool operator ==(Vector2D lhs, Vector2D rhs) => (lhs - rhs).sqrMagnitude< 9.99999944E-11f;
        public static bool operator !=(Vector2D lhs, Vector2D rhs) => lhs != rhs;

        public static bool EqualApprox(Vector2D lhs, Vector2D rhs, float epsilon = MathUtil.Epsilon) =>
            (lhs - rhs).sqrMagnitude < epsilon * epsilon;

        public Vector2D Scale(Vector2D scale) => Vector2.Scale(this, scale);
        public Vector2D Extend(float magnitude) => NewMagnitude(magnitude + this.magnitude);
        public Vector2D NewMagnitude(float magnitude) => magnitude * normalized;
        public bool IsNormalized => MathUtil.EqualAprox(magnitude, 1);

        /// <summary>
        /// return value is between 0 to pi. v1 and v2 are interchangable.
        /// </summary>
        public static float UnsignedAngleRad(Vector2D v1, Vector2D v2) {
            //cos(a) = v1.v2 /(|v1||v2|)         
            float dot = Vector2.Dot(v1, v2);
            float magnitude = Mathf.Sqrt(v1.sqrMagnitude * v2.sqrMagnitude);
            float angle = Mathf.Acos(dot/magnitude);
            return angle;
        }

        public static float Dot(Vector2D lhs, Vector2D rhs) => Vector2.Dot(lhs, rhs);

        public static float Determinent(Vector2D v1, Vector2D v2) =>
            v1.x * v2.z - v1.z * v2.x; // x1*z2 - z1*x2  

        public static Vector2D Vector2ByAgnleRad(float magnitude, float angle) {
            return new Vector2D(
                x: magnitude * Mathf.Cos(angle),
                z: magnitude * Mathf.Sin(angle)
                );
        }

        /// result is between -pi to +pi. angle is CCW with respect to Vector2D.right
        public float SignedAngleRadCCW() => Mathf.Atan2(x, z);

        /// <summary>
        /// return value is between -pi to pi. v1 and v2 are not interchangable.
        /// the angle goes CCW from v1 to v2.
        /// eg v1=0,1 v2=1,0 => angle=pi/2
        /// Note: to convert CCW to CW EITHER swap v1 and v2 OR take the negative of the result.
        /// </summary>
        public static float SignedAngleRadCCW(Vector2D v1, Vector2D v2) {
            float dot = Vector2.Dot(v1, v2);
            float det = Determinent(v1, v2);
            return Mathf.Atan2(det, dot);
        }

        public Vector2D Rotate90CCW() => new Vector2D(-z, +x);
        public Vector2D PerpendicularCCW() => normalized.Rotate90CCW();
        public Vector2D Rotate90CW() => new Vector2D(+z, -x);
        public Vector2D PerpendicularCW() => normalized.Rotate90CW();
    }
}
