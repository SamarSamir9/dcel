using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCEL
{
    struct LineProjectionTransform
    {
        VecRat2 anchor;
        VecRat2 dir;
        Rational dot_anchor;
        Rational inv_dir_len_sq;

        public LineProjectionTransform(SegRat2 s)
        {
            this.anchor = s.A;
            this.dir = s.AB();
            this.dot_anchor = dir * anchor;
            this.inv_dir_len_sq = 1 / dir.LengthSquared();
        }

        public Rational Project(VecRat2 p)
        {
            return dir * p;
        }

        public SegRat1 Project(SegRat2 s)
        {
            return new SegRat1(Project(s.A), Project(s.B), s.AClosed, s.BClosed);
        }

        public VecRat2 Unproject(Rational dot_p)
        {
            return anchor + ((dot_p - dot_anchor) * dir) * inv_dir_len_sq;
        }

        public SegRat2 Unproject(SegRat1 dot_s)
        {
            return new SegRat2(Unproject(dot_s.A), Unproject(dot_s.B), dot_s.AClosed, dot_s.BClosed);
        }
    }
}
