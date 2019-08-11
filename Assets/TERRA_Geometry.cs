using UnityEngine;
using System.Collections;

namespace SCARLET.TERRA
{
    internal struct Sphere
    {
        internal Vector3 Pos;
        internal float Radius;

        internal Sphere(Vector3 pos, float radius)
        {
            Pos = pos;
            Radius = radius;
        }
    }

    internal struct Line
    {
        internal Vector3 PosA;
        internal Vector3 PosB;

        internal float Length => Vector3.Distance(PosA, PosB);

        internal Line(Vector3 posA, Vector3 posB)
        {
            PosA = posA;
            PosB = posB;
        }
    }

    internal static class Geometry
    {
        internal static bool LineIntersectsSphere(Line line, Sphere sphere)
        {
            var ray = new Ray(line.PosA, (line.PosB - line.PosA).normalized);
            Vector3 displacement = ray.origin - sphere.Pos;

            float distToP1 = Vector3.Dot(ray.direction, displacement);
            float distToP2sqr = distToP1 * distToP1 - Vector3.Dot(displacement, displacement) + sphere.Radius * sphere.Radius;
            return (distToP2sqr < 0) ? false : true;
        }
    }
}