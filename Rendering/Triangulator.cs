using PolyhedraLab.Models;

namespace PolyhedraLab.Rendering;

public static class Triangulator
{
    // Отсечение ушей: в отличие от веера работает и для вогнутых OBJ-граней.
    public static List<(int A, int B, int C)> Triangulate(Face face, IReadOnlyList<Point3D> vertices)
    {
        var result = new List<(int, int, int)>();
        var indices = face.VertexIndices.Distinct().ToList();
        if (indices.Count < 3 || face.Normal.Length < 1e-12)
            return result;
        Point3D n = face.Normal;
        int drop = Math.Abs(n.X) > Math.Abs(n.Y) ? 0 : 1;
        if (Math.Abs(n.Z) > (drop == 0 ? Math.Abs(n.X) : Math.Abs(n.Y)))
            drop = 2;
        (double X, double Y) P(int i)
        {
            var p = vertices[i];
            return drop switch
            {
                0 => (p.Y, p.Z),
                1 => (p.X, p.Z),
                _ => (p.X, p.Y)
            };
        }
        double Cross(int a, int b, int c)
        {
            var p = P(a);
            var q = P(b);
            var r = P(c);
            return (q.X - p.X) * (r.Y - p.Y) - (q.Y - p.Y) * (r.X - p.X);
        }
        double area = 0;
        for (int i = 0; i < indices.Count; i++)
        {
            var a = P(indices[i]);
            var b = P(indices[(i + 1) % indices.Count]);
            area += a.X * b.Y - b.X * a.Y;
        }
        double sign = Math.Sign(area);
        if (sign == 0)
            return result;
        while (indices.Count > 3)
        {
            bool found = false;
            for (int i = 0; i < indices.Count; i++)
            {
                int a = indices[(i + indices.Count - 1) % indices.Count], b = indices[i], c = indices[(i + 1) % indices.Count];
                double turn = sign * Cross(a, b, c);
                if (Math.Abs(turn) < 1e-14)
                {
                    indices.RemoveAt(i);
                    found = true;
                    break;
                }
                if (turn < 0)
                    continue;
                bool occupied = indices.Any(p => p != a && p != b && p != c &&
                    sign * Cross(a, b, p) >= -1e-14 && sign * Cross(b, c, p) >= -1e-14 && sign * Cross(c, a, p) >= -1e-14);
                if (occupied)
                    continue;
                result.Add((a, b, c));
                indices.RemoveAt(i);
                found = true;
                break;
            }
            if (!found)
                throw new ArgumentException("Грань имеет самопересечение или некорректный обход вершин.");
        }
        if (indices.Count == 3)
            result.Add((indices[0], indices[1], indices[2]));
        return result;
    }
}
