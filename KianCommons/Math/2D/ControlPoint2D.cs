using UnityEngine.Assertions;

namespace KianCommons.Math {
    public struct ControlPoint2D {
        public Vector2D Point;
        public Vector2D Dir;
        public ControlPoint2D(Vector2D point, Vector2D dir) {
            Point = point;
            Dir = dir.normalized;
        }

        /// <summary>
        /// returns the control point with -dir.
        /// </summary>
        public ControlPoint2D Reverse => new ControlPoint2D(Point, -Dir);


        #region Infinite line
        /// <summary>
        /// Assuming Control point represents infite line.
        /// </summary>
        public static bool Intersect(ControlPoint2D cp1, ControlPoint2D cp2, out Vector2D center) {
            float det = Vector2D.Determinent(cp1.Dir, cp2.Dir);
            if (MathUtil.EqualAprox(det, 0)) {
                // The lines are parallel.
                center = Vector2D.zero;
                return false;
            } else {
                float det1 = Vector2D.Determinent(cp1.Point, cp1.Dir);
                float det2 = Vector2D.Determinent(cp2.Point, cp2.Dir);
                center = (cp1.Dir * det2 - cp2.Dir * det1) / det;
                return true;
            }
        }

        /// <summary>
        /// Assuming Control point represents infite line.
        /// </summary>
        public Vector2D GetClosestPoint(Vector2D P) {
            HelpersExtensions.Assert(Dir.IsNormalized, "assumption failed: dir.IsNormalized "); 
            var AP = P - Point;
            var dot = Vector2D.Dot(AP, Dir);
            return Point + Dir * dot;
        }

        /// <summary>
        /// Assuming Control point represents infite line.
        /// </summary>
        public float DistanceSquare(Vector2D P) {
            var AP = P - Point;
            var dot = Vector2D.Dot(AP, Dir);
            return AP.sqrMagnitude - dot * dot;
        }


        public Line2D ToLine() => new Line2D(Point, Point + Dir);
        #endregion

        public ControlPoint2D RotateCW() => new ControlPoint2D(Point, Dir.Rotate90CW());
        public ControlPoint2D RotateCCW() => new ControlPoint2D(Point, Dir.Rotate90CCW());
        public ControlPoint2D Rotate(bool bLeft) => bLeft ? RotateCCW() : RotateCW();
    }
}
