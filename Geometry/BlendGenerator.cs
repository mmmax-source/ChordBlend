// BlendGenerator.cs
using System;
using System.Collections.Generic;
using Rhino.Geometry;

namespace ChordBlend.Geometry
{
    public static class BlendGenerator
    {
        public static Curve Generate(Curve input, double chordLength, double strength, int mode)
        {
            if (input == null || !input.IsValid)
                return null;

            List<Point3d> pts = ExtractPolylinePoints(input);
            if (pts == null || pts.Count < 3)
                return null;

            bool isClosed = input.IsClosed;
            if (isClosed && pts[0].DistanceTo(pts[pts.Count - 1]) < 1e-9)
                pts.RemoveAt(pts.Count - 1);

            // ✅ Updated count after potential removal
            int count = pts.Count;

            var trimPts = new List<Point3d[]>();
            var blends = new List<Curve>();

            for (int i = (isClosed ? 0 : 1); i < (isClosed ? count : count - 1); i++)
            {
                Point3d p0 = pts[(i - 1 + count) % count];
                Point3d p1 = pts[i];
                Point3d p2 = pts[(i + 1) % count];

                Point3d a, b;
                Curve blend = (mode == 0)
                    ? GenerateArc(p0, p1, p2, chordLength, out a, out b)
                    : GenerateG2Blend(p0, p1, p2, chordLength, strength, out a, out b);

                if (blend != null)
                {
                    blends.Add(blend);
                    trimPts.Add(new Point3d[] { a, b });
                }
            }

            var segments = BuildSegments(pts, trimPts, isClosed);
            var final = new List<Curve>();

            if (isClosed)
            {
                for (int i = 0; i < blends.Count; i++)
                {
                    final.Add(blends[i]);
                    final.Add(segments[i]);
                }
            }
            else
            {
                final.Add(segments[0]);
                for (int i = 0; i < blends.Count; i++)
                {
                    final.Add(blends[i]);
                    final.Add(segments[i + 1]);
                }
            }

            Curve[] joined = Curve.JoinCurves(final);
            return (joined != null && joined.Length == 1) ? joined[0] : null;
        }

        private static Curve GenerateArc(Point3d p0, Point3d p1, Point3d p2, double chord, out Point3d pt1, out Point3d pt2)
        {
            pt1 = pt2 = Point3d.Unset;
            Vector3d v1 = p0 - p1;
            Vector3d v2 = p2 - p1;
            double len1 = v1.Length;
            double len2 = v2.Length;
            if (!v1.Unitize() || !v2.Unitize()) return null;

            double angle = Vector3d.VectorAngle(v1, v2);
            if (angle < 1e-6 || angle > Math.PI - 1e-6) return null;

            double d = chord / (2.0 * Math.Sin(angle / 2.0));
            if (d > len1 || d > len2) return null;

            pt1 = p1 + v1 * d;
            pt2 = p1 + v2 * d;

            var arc = new Arc(pt1, -v1, pt2);
            return arc.IsValid ? arc.ToNurbsCurve() : null;
        }

        private static Curve GenerateG2Blend(Point3d p0, Point3d p1, Point3d p2, double chord, double strength, out Point3d pt1, out Point3d pt2)
        {
            pt1 = pt2 = Point3d.Unset;
            Vector3d vin = p1 - p0;
            Vector3d vout = p2 - p1;
            if (!vin.Unitize() || !vout.Unitize()) return null;

            double angle = Vector3d.VectorAngle(-vin, vout);
            double norm = angle / Math.PI;
            double adaptive = (1 - norm) * (1 - norm);
            double localStrength = (strength + 1) * (0.4 + 0.6 * adaptive);

            double d = chord / (2.0 * Math.Sin(angle / 2.0));
            if (d > (p1 - p0).Length || d > (p2 - p1).Length) return null;

            pt1 = p1 + (-vin) * d;
            pt2 = p1 + vout * d;

            double a = localStrength * chord;

            Point3d c0 = pt1;
            Point3d c5 = pt2;
            Point3d c1 = new Point3d(c0.X + vin.X * (a / 5.0), c0.Y + vin.Y * (a / 5.0), c0.Z + vin.Z * (a / 5.0));
            Vector3d v_c1c0 = c1 - c0;
            Point3d c2 = c1 + v_c1c0;
            Point3d c4 = new Point3d(c5.X - vout.X * (a / 5.0), c5.Y - vout.Y * (a / 5.0), c5.Z - vout.Z * (a / 5.0));
            Vector3d v_c5c4 = c4 - c5;
            Point3d c3 = c4 + v_c5c4;

            return NurbsCurve.Create(false, 5, new[] { c0, c1, c2, c3, c4, c5 });
        }

        private static List<Point3d> ExtractPolylinePoints(Curve curve)
        {
            Polyline pl;
            if (curve.TryGetPolyline(out pl))
                return new List<Point3d>(pl);
            return null;
        }

        private static List<Curve> BuildSegments(List<Point3d> pts, List<Point3d[]> trim, bool closed)
        {
            var segs = new List<Curve>();
            int n = trim.Count;

            if (closed)
            {
                for (int i = 0; i < n; i++)
                {
                    segs.Add(new Line(trim[i][1], trim[(i + 1) % n][0]).ToNurbsCurve());
                }
            }
            else
            {
                segs.Add(new Line(pts[0], trim[0][0]).ToNurbsCurve());
                for (int i = 0; i < n - 1; i++)
                {
                    segs.Add(new Line(trim[i][1], trim[i + 1][0]).ToNurbsCurve());
                }
                segs.Add(new Line(trim[n - 1][1], pts[pts.Count - 1]).ToNurbsCurve());
            }

            return segs;
        }
    }
}