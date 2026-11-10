using PolyhedraLab.Models;

namespace PolyhedraLab.Geometry;

public static class PolyhedronFactory
{
    public static Polyhedron Create(int type)
    {
        double f = (1 + Math.Sqrt(5)) / 2;
        var points = new List<Point3D>();
        string name;
        switch (type)
        {
            case 0:
                name = "Тетраэдр";
                points.AddRange([new(1, 1, 1), new(1, -1, -1), new(-1, 1, -1), new(-1, -1, 1)]);
                break;
            case 1:
                name = "Гексаэдр (куб)";
                foreach (int x in new[] { -1, 1 })
                    foreach (int y in new[] { -1, 1 })
                        foreach (int z in new[] { -1, 1 })
                            points.Add(new(x, y, z));
                break;
            case 2:
                name = "Октаэдр";
                points.AddRange([new(1, 0, 0), new(-1, 0, 0), new(0, 1, 0), new(0, -1, 0), new(0, 0, 1), new(0, 0, -1)]);
                break;
            case 3:
                name = "Икосаэдр";
                foreach (int a in new[] { -1, 1 })
                    foreach (int b in new[] { -1, 1 })
                        points.AddRange([new(0, a, b * f), new(a, b * f, 0), new(b * f, 0, a)]);
                break;
            case 4:
                name = "Додекаэдр";
                foreach (int x in new[] { -1, 1 })
                    foreach (int y in new[] { -1, 1 })
                        foreach (int z in new[] { -1, 1 })
                            points.Add(new(x, y, z));
                foreach (int a in new[] { -1, 1 })
                    foreach (int b in new[] { -1, 1 })
                        points.AddRange([new(0, a / f, b * f), new(a / f, b * f, 0), new(b * f, 0, a / f)]);
                break;
            default:
                throw new ArgumentException("Неизвестный многогранник.");
        }
        // Одинаковый радиус описанной сферы для удобства сравнения фигур.
        double scale = 1.6 / points[0].Length;
        points = points.Select(p => p * scale).ToList();
        return new Polyhedron(name, points, CreateFaces(type));
    }

    // Каждая строка — индексы вершин одной грани в порядке обхода.
    // Нумерация соответствует порядку добавления точек в Create.
    // Для пяти известных фигур проще хранить грани явно, чем искать их плоскости.
    private static IEnumerable<Face> CreateFaces(int type)
    {
        int[][] indices = type switch
        {
            0 => [
                [2,0,1], [1,0,3], [3,0,2], [2,1,3]
            ],
            1 => [
                [2,0,1,3], [1,0,4,5], [4,0,2,6],
                [3,1,5,7], [6,2,3,7], [5,4,6,7]
            ],
            2 => [
                [4,0,2], [2,0,5], [3,0,4], [5,0,3],
                [2,1,4], [5,1,2], [4,1,3], [3,1,5]
            ],
            3 => [
                [2,0,1], [1,0,7], [6,0,2], [5,0,6], [7,0,5],
                [2,1,8], [3,1,7], [8,1,3], [6,2,4], [4,2,8],
                [11,3,7], [8,3,9], [9,3,11], [6,4,10], [9,4,8],
                [10,4,9], [10,5,6], [7,5,11], [11,5,10], [10,9,11]
            ],
            4 => [
                [16,10,0,9,1], [14,8,0,10,2], [15,9,0,8,4],
                [3,16,1,11,17], [5,11,1,9,15], [3,12,2,10,16],
                [6,14,2,12,18], [18,12,3,17,7], [5,15,4,13,19],
                [6,13,4,8,14], [17,11,5,19,7], [19,13,6,18,7]
            ],
            _ => throw new ArgumentException("Неизвестный многогранник.")
        };
        return indices.Select(face => new Face(face));
    }
}
