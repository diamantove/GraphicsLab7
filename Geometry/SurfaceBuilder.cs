using PolyhedraLab.Models;
using PolyhedraLab.Transforms;

namespace PolyhedraLab.Geometry;

public static class SurfaceBuilder
{
    // Профиль задаётся парами (радиус, координата вдоль оси), от нижнего торца к верхнему.
    public static Polyhedron Revolution(IReadOnlyList<(double Radius, double Height)> profile, int axis, int steps, bool caps)
    {
        if (steps < 3 || steps > 200 || profile.Count < 2 || profile.Count > 300)
            throw new ArgumentException("Нужно 2–300 точек и 3–200 разбиений.");
        if (axis < 0 || axis > 2 || profile.Any(p => !double.IsFinite(p.Radius) || !double.IsFinite(p.Height) || p.Radius < 0 || Math.Abs(p.Height) > 10000 || p.Radius > 10000))
            throw new ArgumentException("Радиус должен быть неотрицательным, координаты — конечными и не больше 10000.");
        if (profile.Zip(profile.Skip(1)).Any(p => p.First.Height >= p.Second.Height))
            throw new ArgumentException("Координаты вдоль оси должны строго возрастать: профиль идёт снизу вверх.");
        var vertices = new List<Point3D>();
        var rings = new List<int[]>();
        foreach (var (r, h) in profile)
        {
            int count = r < 1e-10 ? 1 : steps;
            var ring = new int[count];
            for (int j = 0; j < count; j++)
            {
                double angle = 2 * Math.PI * j / steps;
                // Базовая ось Z; циклическая перестановка сохраняет ориентацию.
                var p = new Point3D(r * Math.Cos(angle), r * Math.Sin(angle), h);
                p = axis switch
                {
                    0 => new(p.Z, p.X, p.Y),
                    1 => new(p.Y, p.Z, p.X),
                    _ => p
                };
                ring[j] = vertices.Count;
                vertices.Add(p);
            }
            rings.Add(ring);
        }
        var faces = new List<Face>();
        for (int i = 0; i + 1 < rings.Count; i++)
            for (int j = 0; j < steps; j++)
            {
                int k = (j + 1) % steps;
                int[] a = rings[i], b = rings[i + 1];
                if (a.Length == 1 && b.Length == 1)
                    continue;
                if (a.Length == 1)
                    faces.Add(new Face([a[0], b[k], b[j]]));
                else if (b.Length == 1)
                    faces.Add(new Face([a[j], a[k], b[0]]));
                else
                    faces.Add(new Face([a[j], a[k], b[k], b[j]]));
            }
        if (caps)
        {
            if (rings[0].Length > 1)
                faces.Add(new Face(rings[0].Reverse()));
            if (rings[^1].Length > 1)
                faces.Add(new Face(rings[^1]));
        }
        if (faces.Count == 0)
            throw new ArgumentException("Профиль целиком лежит на оси.");
        return new Polyhedron("Фигура вращения", vertices, faces);
    }

    public static double Function(int index, double x, double y) => index switch
    {
        0 => Math.Sin(x) * Math.Cos(y),
        1 => (x * x - y * y) / 4,
        2 => Math.Sin(Math.Sqrt(x * x + y * y)),
        3 => (x * x + y * y) / 6,
        _ => throw new ArgumentException("Выберите функцию.")
    };

    public static Polyhedron Graph(int function, double x0, double x1, double y0, double y1, int nx, int ny)
    {
        if (!new[] { x0, x1, y0, y1 }.All(double.IsFinite) || x0 >= x1 || y0 >= y1 ||
            new[] { x0, x1, y0, y1 }.Any(v => Math.Abs(v) > 1000) || nx < 1 || ny < 1 || nx > 200 || ny > 200)
            throw new ArgumentException("Нужны возрастающие диапазоны в пределах ±1000 и 1–200 разбиений.");
        var vertices = new List<Point3D>();
        var faces = new List<Face>();
        for (int i = 0; i <= nx; i++)
            for (int j = 0; j <= ny; j++)
            {
                double x = x0 + (x1 - x0) * i / nx, y = y0 + (y1 - y0) * j / ny;
                vertices.Add(new(x, y, Function(function, x, y)));
            }
        // Треугольники планарны даже тогда, когда четыре угла ячейки не в одной плоскости.
        for (int i = 0; i < nx; i++)
            for (int j = 0; j < ny; j++)
            {
                int a = i * (ny + 1) + j, b = a + ny + 1;
                faces.Add(new Face([a, b, b + 1]));
                faces.Add(new Face([a, b + 1, a + 1]));
            }
        return new Polyhedron("График функции", vertices, faces);
    }

    public static Polyhedron Sphere()
    {
        var profile = Enumerable.Range(0, 25).Select(i =>
        {
            double a = -Math.PI / 2 + Math.PI * i / 24;
            return (1.5 * Math.Cos(a), 1.5 * Math.Sin(a));
        }).ToArray();
        return Revolution(profile, 1, 40, false);
    }
}
