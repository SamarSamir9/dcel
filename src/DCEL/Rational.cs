using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

namespace DCEL
{
    /// <summary>
    /// Stores a rational number that is always as reduced as possible (in mixed fraction form).
    /// The number zero is always stored as 0 + 0/1.
    /// Therefore every rational number has a unique normalized representation.
    /// The number is (integer + numerator / denominator), where denominator is always positive.
    /// </summary>
    public struct Rational : IComparable<Rational>, IEquatable<Rational>
    {
        public static double GreatestMagnitude = 0;

        private BigInteger integer, numerator, denominator;
        public BigInteger Integer { get { return integer; } }
        public BigInteger Numerator { get { return numerator; } }
        public BigInteger Denominator { get { return denominator; } }

        public Rational(BigInteger integer, BigInteger numerator, BigInteger denominator)
            : this(integer, numerator, denominator, true)
        {
        }

        private Rational(BigInteger integer, BigInteger numerator, BigInteger denominator, bool normalize)
        {
            GreatestMagnitude = Math.Max(GreatestMagnitude, (double)
                BigInteger.Max(
                    BigInteger.Abs(integer),
                    BigInteger.Max(
                        BigInteger.Abs(numerator),
                        BigInteger.Abs(denominator))));

            if (denominator.IsZero)
                throw new ArgumentException("Zero denominator.");

            this.integer = integer;
            this.numerator = numerator;
            this.denominator = denominator;

            if (normalize)
                Normalize();
        }

        private void Normalize()
        {
            if (numerator == 0)
            {
                denominator = 1;
            }
            else
            {
                //Make denominator positive
                if (denominator < 0)
                {
                    numerator *= -1;
                    denominator *= -1;
                }

                //Reduce fraction as much as possible
                BigInteger gcd = BigInteger.GreatestCommonDivisor(BigInteger.Abs(numerator), denominator);
                numerator /= gcd;
                denominator /= gcd;

                //If |numerator| > denominator, transfer integral part of numerator/denominator to integer
                integer += numerator / denominator;
                numerator %= denominator;

                //If sign of numerator != sign of integer, fix it
                if (integer != 0)
                {
                    if (integer < 0 && numerator > 0)
                    {
                        integer++;
                        numerator -= denominator;
                    }
                    else if (integer > 0 && numerator < 0)
                    {
                        integer--;
                        numerator += denominator;
                    }
                }
            }
        }

        public static Rational operator -(Rational x)
        {
            return new Rational(-x.integer, -x.numerator, x.denominator, false);
        }

        public static Rational operator +(Rational x, Rational y)
        {
            return new Rational(
                x.integer + y.integer,
                (x.numerator * y.denominator) + (y.numerator * x.denominator),
                x.denominator * y.denominator);
        }

        public static Rational operator -(Rational x, Rational y)
        {
            return new Rational(
                x.integer - y.integer,
                (x.numerator * y.denominator) - (y.numerator * x.denominator),
                x.denominator * y.denominator);
        }

        public static Rational operator *(Rational x, Rational y)
        {
            return x.integer * y.integer
                + new Rational(0, x.integer * y.numerator, y.denominator)
                + new Rational(0, y.integer * x.numerator, x.denominator)
                + new Rational(0, x.numerator * y.numerator, x.denominator * y.denominator);
        }

        public static Rational operator /(Rational x, Rational y)
        {
            BigInteger d = y.integer * y.denominator + y.numerator;
            return new Rational(0, x.integer * y.denominator, d)
                + new Rational(0, x.numerator * y.denominator, x.denominator * d);
        }

        public static implicit operator Rational(int x)
        {
            return new Rational(x, 0, 1, false);
        }

        public static implicit operator Rational(BigInteger x)
        {
            return new Rational(x, 0, 1, false);
        }

        public static explicit operator double(Rational x)
        {
            return (double)x.integer + (double)x.numerator / (double)x.denominator;
        }

        public static explicit operator float(Rational x)
        {
            return (float)x.integer + (float)x.numerator / (float)x.denominator;
        }

        public static bool operator ==(Rational x, Rational y)
        {
            return x.integer == y.integer
                && x.numerator == y.numerator
                && x.denominator == y.denominator;
        }

        public static bool operator !=(Rational x, Rational y)
        {
            return x.integer != y.integer
                || x.numerator != y.numerator
                || x.denominator != y.denominator;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator <=(Rational x, Rational y)
        {
            if (x.integer < y.integer)
                return true;
            else if (x.integer > y.integer)
                return false;
            else
                return x.numerator * y.denominator <= y.numerator * x.denominator;
        }
        public static bool operator >=(Rational x, Rational y)
        {
            if (x.integer > y.integer)
                return true;
            else if (x.integer < y.integer)
                return false;
            else
                return x.numerator * y.denominator >= y.numerator * x.denominator;
        }

        public static bool operator <(Rational x, Rational y)
        {
            if (x.integer < y.integer)
                return true;
            else if (x.integer > y.integer)
                return false;
            else
                return x.numerator * y.denominator < y.numerator * x.denominator;
        }
        public static bool operator >(Rational x, Rational y)
        {
            if (x.integer > y.integer)
                return true;
            else if (x.integer < y.integer)
                return false;
            else
                return x.numerator * y.denominator > y.numerator * x.denominator;
        }

        public override string ToString()
        {
            if (denominator == 1)
            {
                return integer.ToString();
            }
            if (denominator == 0)
            {
                return "Unassigned";
            }
            else
            {
                String s;
                if (numerator < 0)
                    s = "-";
                else if (integer != 0)
                    s = "+";
                else
                    s = "";
                return String.Format("{0}{1}{2}/{3}", integer != 0 ? integer.ToString() : "", s, BigInteger.Abs(numerator), denominator);
            }
        }

        public int CompareTo(Rational other)
        {
            if (this == other)
                return 0;
            else if (this < other)
                return -1;
            else
                return 1;
        }

        public bool Equals(Rational other)
        {
            return this == other;
        }

        public static Rational Abs(Rational x)
        {
            return new Rational(BigInteger.Abs(x.integer), BigInteger.Abs(x.numerator), x.denominator, false);
        }

        public static Rational Min(Rational x, Rational y)
        {
            if (x < y)
                return x;
            else
                return y;
        }

        public static int Sign(Rational x)
        {
            int integerSign = x.integer.Sign;
            if (integerSign != 0)
                return integerSign;
            return x.numerator.Sign;
        }

        /// <summary>
        /// Clamps the given value into the range [lo, hi].
        /// </summary>
        public static Rational Clamp(Rational value, Rational lo, Rational hi)
        {
            if (value <= lo)
                return lo;
            else if (value >= hi)
                return hi;
            else
                return value;
        }

        public static Rational Lerp(Rational value1, Rational value2, Rational amount)
        {
            if (value1 == value2)
                return value1;
            else
                return value1 + amount * (value2 - value1);
        }
    }
}