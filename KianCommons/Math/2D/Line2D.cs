using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KianCommons.Math {

    /// <summary>
    /// Infinite line passing through points A and B. For finite line use Segment2.
    /// </summary>
    public struct Line2D {
        public Vector2D A, B;
        public Line2D(Vector2D a, Vector2D b) {
            A = a; B = b;
        }
        public Line2D(ControlPoint2D cp) {
            A = cp.Point; B = A + cp.Dir;
        }

        public static bool IntersectLine(Line2D line1, Line2D line2, out Vector2D center) =>
            ControlPoint2D.Intersect(line1.ToControlPoint(), line2.ToControlPoint(), out center);

        public ControlPoint2D ToControlPoint() => new ControlPoint2D(A,B-A);

        public float DistanceSqr(Vector2D point) {
            return ToControlPoint().DistanceSquare(point);
        }
    }
}