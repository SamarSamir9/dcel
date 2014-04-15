using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCEL
{
    public struct SegRat1
    {
        public Rational A, B;
        public bool AClosed, BClosed;

        //The vector from a to b
        public Rational AB() { return B - A; }

        //The vector from b to a
        public Rational BA() { return A - B; }

        //Is this segment actually a point?
        public bool IsPoint() { return (AClosed && BClosed) && (A == B); }

        //Is this segment empty?
        public bool IsEmpty() { return !(AClosed && BClosed) && (A == B); }

        public SegRat1(Rational a, Rational b)
            : this(a, b, true)
        {
        }

        public SegRat1(Rational a, Rational b, bool closed)
            : this(a, b, closed, closed)
        {
        }

        public SegRat1(Rational a, Rational b, bool aClosed, bool bClosed)
        {
            this.A = a;
            this.B = b;
            this.AClosed = aClosed;
            this.BClosed = bClosed;
        }

        //Flip the orientation of this segment
        public void Flip()
        {
            MathAid.Swap(ref A, ref B);
            MathAid.Swap(ref AClosed, ref BClosed);
        }

        //Normalize the orientation of this segment (for making segment AB behave the same as segment BA)
        public void NormalizeOrientation()
        {
            if (A > B)
                Flip();
        }

        public SegRat1 Interior()
        {
            return new SegRat1(A, B, false);
        }

        public SegRat1 Closure()
        {
            return new SegRat1(A, B, true);
        }
    }
}
