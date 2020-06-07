using ColossalFramework.Math;
using UnityEngine;

namespace KianCommons.Math {
    public struct QBezier2D {
        public ControlPoint2D Start;
        public ControlPoint2D End;
        public Vector2D MiddlePoint { get; private set; }
        public QBezier2D(ControlPoint2D start, ControlPoint2D end) {
            Start = start;
            End = end;
            MiddlePoint = Vector2D.zero;
            CalculateMiddlePoint();
        }
        public Vector2D CalculateMiddlePoint() {
            if (ControlPoint2D.Intersect(Start, End, out var m))
                return MiddlePoint = m;
            return MiddlePoint = (Start.Point + End.Point) * 0.5f;
        }

        public QBezier2D(Vector2D start, Vector2D middle, Vector2D end) {
            Start.Point = start;
            MiddlePoint = middle;
            End.Point = end;
            Start.Dir = (middle - start).normalized;
            End.Dir = (end - middle).normalized;
        }

        public Vector2D P0 {
            get => Start.Point;
            set => Start.Point = value;
        }
        public Vector2D P2 {
            get => End.Point;
            set => End.Point = value;
        }
        public Vector2D P1 {
            get => MiddlePoint;
            set => MiddlePoint = value;
        }

        public bool IsLinear() => Vector2D.EqualApprox(P2 - P1, P1 - P0);

        public override string ToString() => $"Bezier2D({P0} {P1} {P2})";

        #region Position Calculation
        public Vector2D Position(float t) {
            // https://en.wikipedia.org/wiki/B%C3%A9zier_curve#Quadratic_B%C3%A9zier_curves
            float t1 = 1 - t;
            return t1 * (t1 * P0 + t * P1) + t * (t1 * P1 + t * P2);
        }

        public Vector2D PointLinear(float t) => P0 + t * P2;


        private Vector2D _Tangent(float t) => (2 * (1 - t) * (P1 - P0) + 2 * t * (P2 - P1));

        /// <returns>normalized tangent going from start to end.</returns>
        public Vector2D Tangent(float t) => _Tangent(t).normalized;

        public ControlPoint2D At(float t) => new ControlPoint2D(Position(t), _Tangent(t));

        public Vector2D Normal(float t, bool leftSide) => leftSide ? Tangent(t).Rotate90CCW() : Tangent(t).Rotate90CW();

        public ControlPoint2D LeftNormalAt(float t) => new ControlPoint2D(Position(t), _Tangent(t).Rotate90CCW());

        public ControlPoint2D RightNormalAt(float t) => new ControlPoint2D(Position(t), _Tangent(t).Rotate90CCW());

        /// <param name="leftSide">moving from start to end</param>
        public ControlPoint2D NormalAt(float t, bool leftSide) => leftSide ? LeftNormalAt(t) : RightNormalAt(t);

        /// <summary>
        /// results are normalized.
        /// fast for t==0 or t==1
        /// </summary>
        /// <param name="lefSide">is normal to the left of tangent (going away from origin) </param>
        public void NormalTangent(float t, bool lefSide, out Vector2D normal, out Vector2D tangent) {
            tangent = Tangent(t);
            normal = tangent.Rotate90CW();
            if (lefSide) normal = -normal;
        }
        #endregion

        public float ArcLength(float step = 0.1f) {
            if (IsLinear()) {
                return (P2 - P0).magnitude;
            }
            float ret = 0;
            float t;
            Vector2D prev_position = P0;
            for (t = step; t < 1f; t += step) {
                var position = Position(t);
                float len = (position - prev_position).magnitude;
                ret += len;
                prev_position = position;
            }
            {
                float len = (P2 - prev_position).magnitude;
                ret += len;
            }
            if (MathUtil.EqualAprox(ret, 0))
                return 0f;
            return ret;
        }

        /// <summary>
        /// Travels some distance on beizer and calculates the point and tangent at that distance.
        /// </summary>
        /// <param name="distance">distance to travel on the arc in meteres</param>
        public float Travel(float distance, float startT = 0, float step = 1/16f) {
            if (IsLinear()) {
                return distance / (P2 - P0).magnitude;
            }

            float t;
            Vector2D prev_position = P0;
            for (t = step; t < 1f; t += step) {
                var position = Position(t);
                float len = (position - prev_position).magnitude;
                if (distance > len - MathUtil.Epsilon) {
                    distance -= len;
                    prev_position = position;
                } else {
                    t += (distance / len) * step;
                    break;
                }
            }

            if (MathUtil.EqualAprox(t, 0))
                t = 0f;
            return t;
        }

        public ControlPoint2D Travel2(float distance) {
            if (IsLinear()) {
                var point = Start.Point + distance * Start.Dir;
                return new ControlPoint2D(point, Start.Dir);
            }
            float t = Travel(distance);
            return At(t);
        }

        public float GetClosestT(Vector2D pos) {
            float minDistance = 1E+11f;
            float t = 0f;
            Vector2D point_prev = P0;
            for (int i = 1; i <= 16; i++) {
                Vector2D point = Position(i * (1 / 16f));
                float distance = Segment2.DistanceSqr(point_prev, point, pos, out float t2);
                if (distance < minDistance) {
                    minDistance = distance;
                    t = (i - 1 + t2) * (1 / 16f);
                }
                point_prev = point;
            }

            float num = 1f/32;
            for (int j = 0; j < 4; j++) {
                Vector2D point_m = Position(Mathf.Max(0f, t - num));
                Vector2D point0 = Position(t);
                Vector2D point_p = Position(Mathf.Min(1f, t + num));
                float distance_m = Segment2.DistanceSqr(point_m, point0, pos, out float t_m);
                float distance_p = Segment2.DistanceSqr(point0, point_p, pos, out float t_p);
                t = (distance_m >= distance_p) ? Mathf.Min(1f, t + num * t_p) : Mathf.Max(0f, t - num * (1f - t_m));
                num *= 0.5f;
            }
            return t;
        }

        public Bezier2 ToBezier2() {
            return new Bezier2 {
                a = P0, b = P1, c = P1, d = P2
            };
        }

    }
}
