using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azimuth.al
{
    public struct Vector3D
    {
        public static readonly Vector3D O = new Vector3D(0, 0, 0);
        public static readonly Vector3D I = new Vector3D(1, 0, 0);
        public static readonly Vector3D J = new Vector3D(0, 1, 0);
        public static readonly Vector3D K = new Vector3D(0, 0, 1);

        private double x;
        private double y;
        private double z;

        public double X
        {
            get
            {
                return x;
            }
        }

        public double Y
        {
            get
            {
                return y;
            }
        }

        public double Z
        {
            get
            {
                return z;
            }
        }

        public Vector3D(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3D(Vector2D other)
        {
            x = other.X;
            y = other.Y;
            z = 0;
        }

        public Vector3D ToSpherical()
        {
            double length = Length();
            return new Vector3D(length, (double)Math.Atan2(y, x), length == 0 ? 0F : (double) Math.Acos(z / length));
        }

        public Vector3D ToRectangular()
        {
            double sinPhi = (double) Math.Sin(z);
            return new Vector3D((double)(x * Math.Cos(y) * sinPhi), (double)(x * Math.Sin(y) * sinPhi), (double)(x * Math.Cos(z)));
        }

        public double Length2()
        {
            return x * x + y * y + z * z;
        }

        public double Length()
        {
            return (double)Math.Sqrt(Length2());
        }

        public double DistanceTo(Vector3D other)
        {
            return (other - this).Length();
        }

        public double AngleTo(Vector3D other)
        {
            return (double) Math.Acos(Dot(other) / Length() / other.Length());
        }

        public bool Equals(Vector3D other, double epslon = 0)
        {
            if (epslon == 0)
                return x == other.x && y == other.y && z == other.z;

            return DistanceTo(other) <= epslon;
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ", " + z + ")";
        }

        public double Dot(Vector3D other)
        {
            return x * other.x + y * other.y + z * other.z;
        }

        public Vector3D Cross(Vector3D other)
        {
            return new Vector3D(y * other.z - z * other.y, z * other.x - x * other.z, x * other.y - y * other.x);
        }

        public Vector3D Versor()
        {
            double length = Length();
            if (length == 0)
                return O;

            return this / length;
        }

        public Vector3D Rotate(double alpha, double beta, double gamma)
        {
            double ca = (double)Math.Cos(alpha);
            double sa = (double)Math.Sin(alpha);
            double cb = (double)Math.Cos(beta);
            double sb = (double)Math.Sin(beta);
            double cg = (double)Math.Cos(gamma);
            double sg = (double)Math.Sin(gamma);
            return new Vector3D
                (
                    (ca * cg - sa * cb * sg) * x + (sa * cg + ca * cb * sg) * y + sb * sg * z,
                    -(ca * sg + sa * cb * cg) * x + (ca * cb * cg - sa * sg) * y + sb * cg * z,
                    sb * sg * z - ca * sb * y + cb * z
                );
        }

        public Vector3D Rotate(Vector3D center, double alpha, double beta, double gamma)
        {
            return center + (this - center).Rotate(alpha, beta, gamma);
        }

        /*public Vector3D Rotate(Vector3D axis, double angle)
        {

        }*/

        public Vector3D OrtogonalProject(Vector3D direction)
        {
            return Dot(direction) * direction.Versor();
        }

        public static bool operator ==(Vector3D left, Vector3D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector3D left, Vector3D right)
        {
            return !left.Equals(right);
        }

        public static Vector3D operator +(Vector3D left, Vector3D right)
        {
            return new Vector3D(left.x + right.x, left.y + right.y, left.z + right.z);
        }

        public static Vector3D operator -(Vector3D left, Vector3D right)
        {
            return new Vector3D(left.x - right.x, left.y - right.y, left.z - right.z);
        }

        public static Vector3D operator -(Vector3D vec)
        {
            return new Vector3D(-vec.x, -vec.y, -vec.z);
        }

        public static Vector3D operator *(Vector3D vec, double factor)
        {
            return new Vector3D(vec.x * factor, vec.y * factor, vec.z * factor);
        }

        public static Vector3D operator *(double factor, Vector3D vec)
        {
            return vec * factor;
        }

        public static Vector3D operator /(Vector3D vec, double divisor)
        {
            return new Vector3D(vec.x / divisor, vec.y / divisor, vec.z / divisor);
        }

        public static double operator *(Vector3D left, Vector3D right)
        {
            return left.Dot(right);
        }

        public static explicit operator Vector2D(Vector3D vec)
        {
            return new Vector2D(vec);
        }
    }
}
