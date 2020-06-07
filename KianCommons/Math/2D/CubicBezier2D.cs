using ColossalFramework.Math;
using UnityEngine;

namespace KianCommons.Math {
    using static MathUtil;
    public struct CubicBezier2D {
        public Bezier2 _bezier2;
        public ControlPoint2D Start;
        public ControlPoint2D End;

        /// <summary>
        /// create cubic _bezier2 for CS networks. Calculates bezier.
        /// </summary>
        public CubicBezier2D(ControlPoint2D start, ControlPoint2D end) {
            Start = start;
            End = end;
            _bezier2 = default;
            RecalculateBezier();
        }

        public void RecalculateBezier() {
            _bezier2.a = Start.Point;
            _bezier2.b = End.Point;
            const float offsetRatio = 0.3f;
            if (IsStraight(out float distance)) {
                _bezier2.b = Start.Point + Start.Dir * distance * offsetRatio;
                _bezier2.c = End.Point + End.Dir * distance * offsetRatio;
            } else {
                float num2 = Vector2D.Dot(Start.Dir, End.Dir);
                if (num2 >= -0.999f && Line2.Intersect(Start.Point, Start.Point + Start.Dir, End.Point, End.Point + End.Dir, out float u, out float v)) {
                    u = Mathf.Clamp(u, distance * 0.1f, distance);
                    v = Mathf.Clamp(v, distance * 0.1f, distance);
                    distance = u + v;
                    _bezier2.b = Start.Point + Start.Dir * Mathf.Min(u, distance * offsetRatio);
                    _bezier2.c = End.Point + End.Dir * Mathf.Min(v, distance * offsetRatio);
                } else {
                    _bezier2.b = Start.Point + Start.Dir * (distance * offsetRatio);
                    _bezier2.c = End.Point + End.Dir * (distance * offsetRatio);
                }
            }
        }

        public CubicBezier2D Invert() {
            return new CubicBezier2D {
                Start = this.End,
                End = this.Start,
                _bezier2 = this._bezier2.Invert(),
            };
        }

        public bool IsStraight(out float distance) {
            Vector2D vector = End.Point - Start.Point;
            distance = vector.magnitude;
            float dot = Vector2D.Dot(Start.Dir, End.Dir);
            float dot2 = Vector2D.Dot(Start.Dir, vector);
            return dot < -0.999f && dot2 > 0.999f * distance;
        }

        public bool IsStraight() => IsStraight(out _);

        public float ArcLength(float step = 0.1f) {
            if (IsStraight(out float lenght)) {
                return lenght;
            }
            float ret = 0;
            float t;
            for (t = step; t < 1f; t += step) {
                float len = (_bezier2.Position(t) - _bezier2.Position(t - step)).magnitude;
                ret += len;
            }
            {
                float len = (_bezier2.d - _bezier2.Position(t - step)).magnitude;
                ret += len;
            }
            if (EqualAprox(ret, 0))
                return 0f;
            return ret;
        }

        /// <summary>
        /// Travels some distance on beizer and calculates the point and tangent at that distance.
        /// </summary>
        /// <param name="distance">distance to travel on the arc in meteres</param>
        /// <returns>point on the curve at the given distance and its tangent.</returns>
        public ControlPoint2D Travel2(float distance) {
            if (IsStraight(out float length)) {
                return new ControlPoint2D {
                    Point = distance * Start.Dir,
                    Dir = Start.Dir,
                };
            }
            float t = _bezier2.Travel(0, distance);
            var tangent = Tangent(t);
            var position = _bezier2.Position(t);
            return new ControlPoint2D(position, tangent);
        }

        /// <summary>
        /// results are normalized.
        /// fast for t==0 or t==1
        /// </summary>
        /// <param name="lefSide">is normal to the left of tangent (going away from origin) </param>
        public void NormalTangent(float t, bool lefSide, out Vector2D normal, out Vector2D tangent) {
            if (MathUtil.EqualAprox(t, 0) || IsStraight()) {
                tangent = Start.Dir;
            } else if (MathUtil.EqualAprox(t, 1)) {
                tangent = -End.Dir;
            } else {
                tangent = Tangent(t);
            }
            normal = tangent.Rotate90CW();
            if (lefSide) normal = -normal;
        }

        public ControlPoint2D At(float t, bool lefSide, out Vector2D normal) {
            ControlPoint2D ret;
            if (MathUtil.EqualAprox(t, 0) || IsStraight()) {
                ret = Start;
            } else if (MathUtil.EqualAprox(t, 1)) {
                ret = End.Reverse;
            } else {
                ret.Dir = Tangent(t);
                ret.Point = _bezier2.Position(t);
            }
            normal = ret.Dir.Rotate90CW();
            if (lefSide) normal = -normal;
            return ret;
        }

        /// <summary>
        /// not normalized
        /// </summary>
        public Vector2D Derivative(float t) => _bezier2.Tangent(t);

        /// <summary>
        /// normalized
        /// </summary>
        public Vector2D Tangent(float t) => Derivative(t).normalized;

        public Vector2D Position(float t) => _bezier2.Position(t);

        public float GetClosestT(Vector2D pos) {
            float minDistance = 1E+11f;
            float t = 0f;
            Vector2D a = _bezier2.a;
            for (int i = 1; i <= 16f; i++) {
                Vector2D vector = Position(i * (1f / 16));
                float distance = Segment2.DistanceSqr(a, vector, pos, out float u);
                if (distance < minDistance) {
                    minDistance = distance;
                    t = (i - 1 + u) * (1f / 16);
                }
                a = vector;
            }

            float precision = 1f / 32;
            for (int j = 0; j < 4; j++) {
                Vector2D pos1 = _bezier2.Position(Mathf.Max(0f, t - precision));
                Vector2D vector2 = _bezier2.Position(t);
                Vector2D b = _bezier2.Position(Mathf.Min(1f, t + precision));
                float distance1 = Segment2.DistanceSqr(pos1, vector2, pos, out float u);
                float distance2 = Segment2.DistanceSqr(vector2, b, pos, out float u2);
                t = !(distance1 < distance2) ? Mathf.Min(1f, t + precision * u2) : Mathf.Max(0f, t - precision * (1f - u));
                precision *= 0.5f;
            }
            return t;
        }
    }
}

