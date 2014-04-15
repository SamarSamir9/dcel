using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCEL
{
    public enum SegmentIntersectionType
    {
        None = 0,
        Point = 1,
        Segment = 2
    };

    public static class GeomAid
    {
        public static int TurnTest(VecRat2 a, VecRat2 b, VecRat2 c)
        {
            return Rational.Sign(TriangleTwiceSignedArea(a, b, c));
        }

        public static Rational TriangleTwiceArea(VecRat2 a, VecRat2 b, VecRat2 c)
        {
            return Rational.Abs(TriangleTwiceSignedArea(a, b, c));
        }

        public static Rational TriangleTwiceSignedArea(VecRat2 a, VecRat2 b, VecRat2 c)
        {
            return MathAid.Det2(b - a, c - a);
        }

        public static int TurnTest(SegRat2 s, VecRat2 p)
        {
            return TurnTest(s.A, s.B, p);
        }

        public static bool PointInSegment(Rational p, SegRat1 s)
        {
            s.NormalizeOrientation();
            return (s.AClosed && p == s.A)
                || (s.BClosed && p == s.B)
                || (s.A < p && p < s.B);
        }

        public static bool ColinearPointInSegment(VecRat2 p, SegRat2 s)
        {
            LineProjectionTransform transform = new LineProjectionTransform(s);
            return PointInSegment(transform.Project(p), transform.Project(s));
        }

        public static bool PointInSegment(VecRat2 p, SegRat2 s)
        {
            return TurnTest(s, p) == 0 && ColinearPointInSegment(p, s);
        }

        public static SegmentIntersectionType SegmentIntersection(SegRat1 s, SegRat1 t, ref Rational pointIntersection, ref SegRat1 segmentIntersection)
        {
            //This check is important for handling degenerate cases like s = [x, x).
            if (s.IsEmpty() || t.IsEmpty())
                return SegmentIntersectionType.None;

            s.NormalizeOrientation();
            t.NormalizeOrientation();

            if (s.A > t.A)
                MathAid.Swap(ref s, ref t);

            if (s.B < t.A)
                return SegmentIntersectionType.None;

            if (s.B == t.A)
            {
                if (s.BClosed && t.AClosed)
                {
                    pointIntersection = s.B;
                    return SegmentIntersectionType.Point;
                }
                else
                {
                    return SegmentIntersectionType.None;
                }
            }

            Rational b = Rational.Min(s.B, t.B);
            segmentIntersection.A = t.A;
            segmentIntersection.B = b;
            segmentIntersection.AClosed = (t.AClosed && (s.A < t.A || (s.AClosed && s.A == t.A)));
            segmentIntersection.BClosed = ((s.BClosed && t.BClosed) || (s.BClosed && b < t.B) || (t.BClosed && b < s.B));
            return SegmentIntersectionType.Segment;
        }

        public static SegmentIntersectionType ColinearSegmentIntersection(SegRat2 s, SegRat2 t, ref VecRat2 pointIntersection, ref SegRat2 segmentIntersection)
        {
            //This check is important for handling degenerate cases like s = [(x,y), (x,y)).
            if (s.IsEmpty() || t.IsEmpty())
                return SegmentIntersectionType.None;

            //This check is important because the LineProjectionTransform can only be formed with a non-point segment.
            if (s.IsPoint())
            {
                if (ColinearPointInSegment(s.A, t))
                {
                    pointIntersection = s.A;
                    return SegmentIntersectionType.Point;
                }
                else
                {
                    return SegmentIntersectionType.None;
                }
            }

            LineProjectionTransform transform = new LineProjectionTransform(s);
            SegRat1 proj_s = transform.Project(s);
            SegRat1 proj_t = transform.Project(t);

            Rational proj_pointIntersection = new Rational();
            SegRat1 proj_segmentIntersection = new SegRat1();

            SegmentIntersectionType result = SegmentIntersection(
                proj_s, proj_t, ref proj_pointIntersection, ref proj_segmentIntersection);

            if (result == SegmentIntersectionType.Point)
                pointIntersection = transform.Unproject(proj_pointIntersection);
            else if (result == SegmentIntersectionType.Segment)
                segmentIntersection = transform.Unproject(proj_segmentIntersection);

            return result;
        }

        /// <summary>
        /// Assumes the two lines u1-v1 and u2-v2 are not parallel.
        /// </summary>
        public static VecRat2 LineIntersection(VecRat2 a1, VecRat2 b1, VecRat2 a2, VecRat2 b2)
        {
            Rational s1det = MathAid.Det2(a1, b1);
            Rational s2det = MathAid.Det2(a2, b2);
            VecRat2 diff1 = a1 - b1;
            VecRat2 diff2 = a2 - b2;
            return (s1det * diff2 - s2det * diff1) / MathAid.Det2(diff1, diff2);
        }

        /// <summary>
        /// Assumes the two lines u1-v1 and u2-v2 are not parallel.
        /// </summary>
        public static VecRat2 LineIntersection(SegRat2 s1, SegRat2 s2)
        {
            return LineIntersection(s1.A, s1.B, s2.A, s2.B);
        }

        //If the two segments are disjoint, then None is returned.
        //Else, if the two segments are colinear, then either Segment is returned.
        //Else, if the two segments are not colinear, Point is returned.
        public static SegmentIntersectionType SegmentIntersection(SegRat2 s, SegRat2 t, ref VecRat2 pointIntersection, ref SegRat2 segmentIntersection)
        {
            //This check is important for handling degenerate cases like s = [(x,y), (x,y)).
            if (s.IsEmpty() || t.IsEmpty())
                return SegmentIntersectionType.None;

            //This check is important because the LineProjectionTransform can only be formed with a non-point segment.
            if (s.IsPoint())
            {
                if (PointInSegment(s.A, t))
                {
                    segmentIntersection = s;
                    return SegmentIntersectionType.Segment;
                }
                else
                {
                    return SegmentIntersectionType.None;
                }
            }

            int turn_s_ta = TurnTest(s, t.A);
            int turn_s_tb = TurnTest(s, t.B);

            if (turn_s_ta == 0 && turn_s_tb == 0)
                return ColinearSegmentIntersection(s, t, ref pointIntersection, ref segmentIntersection);

            if (s.AClosed)
            {
                if (t.AClosed && s.A == t.A)
                {
                    pointIntersection = s.A;
                    return SegmentIntersectionType.Point;
                }
                else if (t.BClosed && s.B == t.B)
                {
                    pointIntersection = s.B;
                    return SegmentIntersectionType.Point;
                }
            }

            if (s.BClosed)
            {
                if (t.AClosed && s.B == t.A)
                {
                    pointIntersection = s.B;
                    return SegmentIntersectionType.Point;
                }
                else if (t.BClosed && s.B == t.B)
                {
                    pointIntersection = s.B;
                    return SegmentIntersectionType.Point;
                }
            }

            int turn_t_sa = TurnTest(t, s.A);
            int turn_t_sb = TurnTest(t, s.B);

            int val_s = turn_s_ta * turn_s_tb;
            int val_t = turn_t_sa * turn_t_sb;

            if (val_s < 0 && val_t < 0)
            {
                pointIntersection = LineIntersection(s, t);
                return SegmentIntersectionType.Point;
            }
            else
            {
                return SegmentIntersectionType.None;
            }
        }
    }
}
