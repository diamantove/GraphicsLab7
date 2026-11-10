using System.Globalization;
using System.IO;
using PolyhedraLab.Models;

namespace PolyhedraLab.IO;

public static class ObjFile
{
    public static Polyhedron Load(string path)
    {
        var vertices = new List<Point3D>();
        var faces = new List<Face>();
        int lineNumber = 0;
        foreach (string line in File.ReadLines(path))
        {
            lineNumber++;
            string[] parts = line.Split('#')[0].Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                continue;
            try
            {
                if (parts[0] == "v")
                {
                    if (parts.Length < 4)
                        throw new FormatException("Ожидаются три координаты.");
                    double Read(int i) => double.Parse(parts[i], CultureInfo.InvariantCulture);
                    double w = parts.Length > 4 ? Read(4) : 1;
                    var p = new Point3D(Read(1) / w, Read(2) / w, Read(3) / w);
                    if (!p.IsFinite || p.Length > 1e9)
                        throw new FormatException("Недопустимые координаты.");
                    vertices.Add(p);
                }
                else if (parts[0] == "f")
                {
                    int Index(string token)
                    {
                        int index = int.Parse(token.Split('/')[0], CultureInfo.InvariantCulture);
                        int result = index > 0 ? index - 1 : vertices.Count + index;
                        if (index == 0 || result < 0 || result >= vertices.Count)
                            throw new FormatException("Индекс вершины вне диапазона.");
                        return result;
                    }
                    var indices = parts.Skip(1).Select(Index).ToList();
                    if (indices.Count > 3 && indices[0] == indices[^1])
                        indices.RemoveAt(indices.Count - 1);
                    faces.Add(new Face(indices));
                }
                // vn/vt, материалы и группы не нужны для геометрии и пропускаются.
            }
            catch (Exception ex) when (ex is FormatException or OverflowException or ArgumentException)
            {
                throw new FormatException($"OBJ, строка {lineNumber}: {ex.Message}", ex);
            }
            if (vertices.Count > 200_000 || faces.Count > 400_000)
                throw new FormatException("Модель слишком велика для учебного программного рендера.");
        }
        if (vertices.Count == 0 || faces.Count == 0)
            throw new FormatException("В OBJ нужны вершины v и грани f.");
        return new Polyhedron(Path.GetFileNameWithoutExtension(path), vertices, faces);
    }

    public static void Save(string path, Polyhedron model)
    {
        using var writer = new StreamWriter(path);
        writer.WriteLine("# Graphics laboratory: vertices and polygon faces");
        foreach (var p in model.Vertices)
            writer.WriteLine(FormattableString.Invariant($"v {p.X:R} {p.Y:R} {p.Z:R}"));
        foreach (var face in model.Faces)
            writer.WriteLine("f " + string.Join(" ", face.VertexIndices.Select(v => v + 1)));
    }
}
