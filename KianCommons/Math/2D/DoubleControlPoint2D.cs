using UnityEngine;
using UnityEngine.Assertions;

namespace KianCommons.Math {
    public struct DoubleControlPoint2D {
        public Vector2D Point;
        public Vector2D Dir1;
        public Vector2D Dir2;
        public ControlPoint2D ControlPoint1 => new ControlPoint2D { Point = Point, Dir = Dir1 };
        public ControlPoint2D ControlPoint2 => new ControlPoint2D { Point = Point, Dir = Dir2 };
        public DoubleControlPoint2D(Vector2D point, Vector2D dir1, Vector2D dir2) {
            Point = point;
            Dir1 = dir1.normalized;
            Dir2 = dir2.normalized;
        }

        /// <summary>
        /// returns the control point with -dir.
        /// </summary>
        public DoubleControlPoint2D Reverse => new DoubleControlPoint2D(Point, -Dir1, -Dir2);

        /// <summary>
        /// Assuming Control points represents infite line.
        /// </summary>
        public float DistanceSquare(Vector2D p) {
            var d1 = ControlPoint1.DistanceSquare(p);
            var d2 = ControlPoint1.DistanceSquare(p);
            return Mathf.Min(d1, d2);
        }

        public float Distance(Vector2D p) => Mathf.Sqrt(DistanceSquare(p));

        public DoubleControlPoint2D RotateCW() => new DoubleControlPoint2D(Point, Dir1.Rotate90CW(), Dir2.Rotate90CW());
        public DoubleControlPoint2D RotateCCW() => new DoubleControlPoint2D(Point, Dir1.Rotate90CCW(), Dir2.Rotate90CCW());
        public DoubleControlPoint2D Rotate(bool bLeft) => bLeft ? RotateCCW() : RotateCW();
    }
}
