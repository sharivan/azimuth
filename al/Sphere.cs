using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azimuth.al
{
    public class Sphere
    {
        private Vector3D center;
        private double radius;

        public Vector3D Center
        {
            get
            {
                return center;
            }
        }

        public double Radius
        {
            get
            {
                return radius;
            }
        }

        public Sphere(Vector3D center, double radius)
        {
            this.center = center;
            this.radius = radius;
        }

        public override string ToString()
        {
            return "{c=" + center + " r=" + radius + "}";
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

            double radius2 = radius * radius;
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
                double temp = t0;
                t0 = t1;
                t1 = temp;
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
