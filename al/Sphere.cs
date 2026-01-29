using System;

namespace Azimuth.al
{
    public class Sphere
    {
        private Vector3D center;

        public Vector3D Center => center;

        public double Radius
        {
            get;
        }

        public Sphere(Vector3D center, double radius)
        {
            this.center = center;
            Radius = radius;
        }

        public override string ToString()
        {
            return "{c=" + center + " r=" + Radius + "}";
        }

        public bool IntersectWithRay(Vector3D orig, Vector3D dir, out double t)
        {
            double t0, t1; // solutions for t if the ray intersects 

            // geometric solution
            Vector3D L = center - orig;
            double tca = L.Dot(dir);
            if (tca < 0)
            {
                t = 0;
                return false;
            }

            double radius2 = Radius * Radius;
            double d2 = L.Dot(L) - tca * tca;
            if (d2 > radius2)
            {
                t = 0;
                return false;
            }

            double thc = (double) Math.Sqrt(radius2 - d2);
            t0 = tca - thc;
            t1 = tca + thc;

            if (t0 > t1)
            {
                (t1, t0) = (t0, t1);
            }

            if (t0 < 0)
            {
                t0 = t1; // if t0 is negative, let's use t1 instead 
                if (t0 < 0)
                {
                    t = 0;
                    return false; // both t0 and t1 are negative
                }
            }

            t = t0;
            return true;
        }
    }
}
