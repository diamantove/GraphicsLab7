using PolyhedraLab.Transforms;

namespace PolyhedraLab.Models;

public sealed class Polyhedron
{
    public string Name
    {
        get;
    }
    public List<Point3D> Vertices
    {
        get;
    }
    public List<Face> Faces
    {
        get;
    }
    // Открытые сетки отображаются с обеих сторон, в том числе после загрузки OBJ.
    public bool TwoSided
    {
        get;
    }

    public Polyhedron(string name, IEnumerable<Point3D> vertices, IEnumerable<Face> faces)
    {
        Name = name;
        Vertices = vertices.ToList();
        Faces = faces.ToList();
        var edgeCounts = new Dictionary<(int, int), int>();
        foreach (var face in Faces)
            for (int i = 0; i < face.VertexIndices.Length; i++)
            {
                int a = face.VertexIndices[i], b = face.VertexIndices[(i + 1) % face.VertexIndices.Length];
                var edge = (Math.Min(a, b), Math.Max(a, b));
                edgeCounts.TryGetValue(edge, out int count);
                edgeCounts[edge] = count + 1;
            }
        TwoSided = edgeCounts.Values.Any(count => count != 2);
        UpdateNormals();
    }

    // Среднее вершин совпадает с центром правильного многогранника.
    // Аффинное преобразование сохраняет это свойство, даже после отражения/масштаба.
    public Point3D Center => Point3D.Average(Vertices);

    public IEnumerable<(int A, int B)> GetEdges()
    {
        var edges = new HashSet<(int, int)>();
        foreach (var face in Faces)
        {
            var indices = face.VertexIndices;
            for (int i = 0; i < indices.Length; i++)
            {
                int a = indices[i], b = indices[(i + 1) % indices.Length];
                if (edges.Add((Math.Min(a, b), Math.Max(a, b))))
                    yield return (a, b);
            }
        }
    }

    public void Transform(Matrix4x4 matrix)
    {
        // Сначала проверяем весь результат: ошибка не должна менять модель наполовину.
        var transformed = Vertices.Select(matrix.TransformPoint).ToArray();
        if (transformed.Any(p => !p.IsFinite || Math.Max(Math.Abs(p.X), Math.Max(Math.Abs(p.Y), Math.Abs(p.Z))) > 1e9))
            throw new ArgumentException("Слишком большие координаты. Уменьшите параметры преобразования.");
        for (int i = 0; i < transformed.Length; i++)
            Vertices[i] = transformed[i];
        // Отражение меняет ориентацию обхода. Возвращаем нормали наружу.
        double determinant =
            matrix[0, 0] * (matrix[1, 1] * matrix[2, 2] - matrix[1, 2] * matrix[2, 1]) -
            matrix[0, 1] * (matrix[1, 0] * matrix[2, 2] - matrix[1, 2] * matrix[2, 0]) +
            matrix[0, 2] * (matrix[1, 0] * matrix[2, 1] - matrix[1, 1] * matrix[2, 0]);
        if (determinant < 0)
            foreach (var face in Faces)
                Array.Reverse(face.VertexIndices);
        UpdateNormals();
    }

    public void UpdateNormals()
    {
        foreach (var face in Faces)
            face.UpdateNormal(Vertices);
    }

}
