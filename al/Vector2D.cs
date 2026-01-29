using System;

namespace Azimuth.al
{
    public struct Vector2D
    {
        public static readonly Vector2D O = new Vector2D(0, 0);
        public static readonly Vector2D I = new Vector2D(1, 0);
        public static readonly Vector2D J = new Vector2D(0, 1);

        private double x;
        private double y;

        public double X => x;

        public double Y => y;

        public Vector2D(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2D(Vector2D other)
        {
            x = other.x;
            y = other.y;
        }

        public Vector2D(Vector3D other)
        {
            x = other.X;
            y = other.Y;
        }

        public Vector2D ToPolar()
        {
            return new Vector2D(Length(), Angle());
        }

        public Vector2D ToRectangular()
        {
            return new Vector2D((double) (x * Math.Cos(y)), (double) (x * Math.Sin(y)));
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vector2D))
                return false;

            var other = (Vector2D) obj;
            return Equals(other);
        }

        public double Length2()
        {
            return x * x + y * y;
        }

        public double Length()
        {
            return (double) Math.Sqrt(Length2());
        }

        public double Angle()
        {
            return (double) Math.Atan2(y, x);
        }

        public double DistanceTo(Vector2D other)
        {
            return (other - this).Length();
        }

        public double AngleTo(Vector2D other)
        {
            return (other - this).Angle();
        }

        public bool Equals(Vector2D other, double epslon = 0)
        {
            return epslon == 0 ? x == other.x && y == other.y : DistanceTo(other) <= epslon;
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }

        public double Dot(Vector2D other)
        {
            return x * other.x + y * other.y;
        }

        public Vector3D Cross(Vector2D other)
        {
            return new Vector3D(0, 0, x * other.y - y * other.x);
        }

        public Vector2D Rotate(double angle)
        {
            double c = (double) Math.Cos(angle);
            double s = (double) Math.Sin(angle);
            return new Vector2D(x * c - y * s, x * s + y * c);
        }

        public Vector2D Rotate(Vector2D center, double angle)
        {
            return center + (this - center).Rotate(angle);
        }

        public Vector2D OrtogonalProject(Vector2D direction)
        {
            return Dot(direction) * direction.Versor();
        }

        public Vector2D Versor()
        {
            double length = Length2();
            return length == 0 ? O : this / length;
        }

        public static bool operator ==(Vector2D left, Vector2D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2D left, Vector2D right)
        {
            return !left.Equals(right);
        }

        public static Vector2D operator +(Vector2D left, Vector2D right)
        {
            return new Vector2D(left.x + right.x, left.y + right.y);
        }

        public static Vector2D operator -(Vector2D left, Vector2D right)
        {
            return new Vector2D(left.x - right.x, left.y - right.y);
        }

        public static Vector2D operator -(Vector2D vec)
        {
            return new Vector2D(-vec.x, -vec.y);
        }

        public static Vector2D operator *(Vector2D vec, double factor)
        {
            return new Vector2D(vec.x * factor, vec.y * factor);
        }

        public static Vector2D operator *(double factor, Vector2D vec)
        {
            return vec * factor;
        }

        public static Vector2D operator /(Vector2D vec, double divisor)
        {
            return new Vector2D(vec.x / divisor, vec.y / divisor);
        }

        public static double operator *(Vector2D left, Vector2D right)
        {
            return left.Dot(right);
        }

        public static implicit operator Vector3D(Vector2D vec)
        {
            return new Vector3D(vec);
        }
    }
}
