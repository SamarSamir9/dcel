using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCEL
{
    public struct VecRat2 : IEquatable<VecRat2>
    {
        public Rational X, Y;

        public VecRat2(Rational x, Rational y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator VecRat2(VecInt2 v)
        {
            return new VecRat2(v.X, v.Y);
        }

        public static VecRat2 Zero
        {
            get { return new VecRat2(0, 0); }
        }

        public static VecRat2 UnitX
        {
            get { return new VecRat2(1, 0); }
        }

        public static VecRat2 UnitY
        {
            get { return new VecRat2(0, 1); }
        }

        public static VecRat2 One
        {
            get { return new VecRat2(1, 1); }
        }

        public static VecRat2 operator +(VecRat2 u, VecRat2 v)
        {
            return new VecRat2(u.X + v.X, u.Y + v.Y);
        }

        public static VecRat2 operator -(VecRat2 u, VecRat2 v)
        {
            return new VecRat2(u.X - v.X, u.Y - v.Y);
        }

        public static VecRat2 operator *(int c, VecRat2 u)
        {
            return new VecRat2(c * u.X, c * u.Y);
        }

        public static VecRat2 operator *(VecRat2 u, int c)
        {
            return new VecRat2(c * u.X, c * u.Y);
        }

        public static VecRat2 operator *(Rational c, VecRat2 u)
        {
            return new VecRat2(c * u.X, c * u.Y);
        }

        public static VecRat2 operator *(VecRat2 u, Rational c)
        {
            return new VecRat2(c * u.X, c * u.Y);
        }

        public static Rational operator *(VecRat2 u, VecRat2 v)
        {
            return u.X * v.X + u.Y * v.Y;
        }

        public static VecRat2 operator /(VecRat2 u, int d)
        {
            return new VecRat2(u.X / d, u.Y / d);
        }

        public static VecRat2 operator /(VecRat2 u, Rational d)
        {
            return new VecRat2(u.X / d, u.Y / d);
        }

        public static VecRat2 operator -(VecRat2 u)
        {
            return -1 * u;
        }

        public static bool operator ==(VecRat2 u, VecRat2 v)
        {
            return u.X == v.X && u.Y == v.Y;
        }

        public static bool operator >=(VecRat2 u, VecRat2 v)
        {
            return u.X >= v.X && u.Y >= v.Y;
        }

        public static bool operator >(VecRat2 u, VecRat2 v)
        {
            return u.X > v.X && u.Y > v.Y;
        }

        public static bool operator !=(VecRat2 u, VecRat2 v)
        {
            return !(u == v);
        }

        public static bool operator <(VecRat2 u, VecRat2 v)
        {
            return v > u;
        }

        public static bool operator <=(VecRat2 u, VecRat2 v)
        {
            return v >= u;
        }

        public static int CompareReadingOrder(VecRat2 u, VecRat2 v)
        {
            if (u.Y == v.Y)
                return u.X.CompareTo(v.X);
            else
                return v.Y.CompareTo(u.Y);
        }

        public static Rational ManhattanDist(VecRat2 u, VecRat2 v)
        {
            return Rational.Abs(u.X - v.X) + Rational.Abs(u.Y - v.Y);
        }

        public static VecRat2 AbsoluteDifference(VecRat2 u, VecRat2 v)
        {
            return new VecRat2(Rational.Abs(u.X - v.X), Rational.Abs(u.Y - v.Y));
        }

        public static VecRat2 Clamp(VecRat2 value, VecRat2 lo, VecRat2 hi)
        {
            return new VecRat2(Rational.Clamp(value.X, lo.X, hi.X), Rational.Clamp(value.Y, lo.Y, hi.Y));
        }

        public static VecRat2 Lerp(VecRat2 value1, VecRat2 value2, Rational amount)
        {
            return new VecRat2(Rational.Lerp(value1.X, value2.X, amount), Rational.Lerp(value1.Y, value2.Y, amount));
        }

        public static Rational Dot(VecRat2 u, VecRat2 v)
        {
            return u.X * v.X + u.Y * v.Y;
        }

        public static Rational DistanceSquared(VecRat2 u, VecRat2 v)
        {
            VecRat2 w = v - u;
            return Dot(w, w);
        }

        public Rational LengthSquared()
        {
            return X * X + Y * Y;
        }

        public override string ToString()
        {
            return String.Format("({0},{1})", X, Y);
        }

        public static VecRat2 Right(VecRat2 v)
        {
            return new VecRat2(v.Y, -v.X);
        }

        public static VecRat2 Left(VecRat2 v)
        {
            return new VecRat2(-v.Y, v.X);
        }

        public static VecRat2 Rotate(VecRat2 v, Rotation rotation)
        {
            if (rotation == Rotation.None)
                return v;
            else if (rotation == Rotation.CCW90)
                return Left(v);
            else if (rotation == Rotation.CCW180)
                return -v;
            else
                return Right(v);
        }

        #region IEquatable<VecRat2> Members

        public bool Equals(VecRat2 other)
        {
            return other.X == this.X && other.Y == this.Y;
        }

        #endregion

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}