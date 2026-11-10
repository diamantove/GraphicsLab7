using System.Windows.Controls;
using System.Windows.Threading;
using PolyhedraLab.Geometry;
using PolyhedraLab.Models;
using PolyhedraLab.Transforms;

namespace PolyhedraLab.UI;

public sealed class ConstructionPanel : UserControl
{
    private readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(40) };

    public ConstructionPanel(Scene scene, Action fit)
    {
        var panel = new StackPanel();
        Content = Ui.Scroll(panel);

        // Ui.Label(panel,
        //     "Фигура вращения: профиль в меридиональной плоскости. " +
        //     "Каждая строка: радиус и координата вдоль выбранной оси, через пробел. " +
        //     "Координата вдоль оси возрастает.");
        var profile = Ui.Input(panel, "Образующая (r h)", "0 -2\n0.8 -1.7\n1.2 -0.7\n0.6 0.5\n1 1.7\n0 2");
        profile.AcceptsReturn = true;
        profile.Height = 130;
        var axis = Ui.Choice(panel, "Ось вращения", "X", "Y", "Z");
        axis.SelectedIndex = 1;
        var steps = Ui.Input(panel, "Количество разбиений (3–200)", "40");
        var caps = Ui.Check(panel, "Закрывать торцы", true);
        Ui.Button(panel, "Построить фигуру вращения", () =>
        {
            var points = new List<(double, double)>();
            foreach (var line in profile.Text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                    throw new ArgumentException("В каждой строке профиля нужны два числа: r h.");
                points.Add((Ui.Number(parts[0]), Ui.Number(parts[1])));
            }
            scene.SetModel(SurfaceBuilder.Revolution(
                points, axis.SelectedIndex, Ui.Integer(steps), caps.IsChecked == true));
            fit();
        });

        var function = Ui.Choice(panel, "График z = f(x,y)",
            "sin(x) · cos(y)", "(x² − y²) / 4", "sin(√(x²+y²))", "(x² + y²) / 6");
        var range = Ui.Input(panel, "Диапазоны: x0 x1 y0 y1", "-3 3 -3 3");
        var divisions = Ui.Input(panel, "Разбиения по X и Y: nx ny (1–200)", "35 35");
        Ui.Button(panel, "Построить график", () =>
        {
            var r = Ui.Numbers(range, 4);
            var n = Ui.Numbers(divisions, 2);
            if (n.Any(v => v != Math.Truncate(v) || v < 1 || v > 200))
                throw new ArgumentException("Разбиения — целые числа от 1 до 200.");
            scene.SetModel(SurfaceBuilder.Graph(
                function.SelectedIndex, r[0], r[1], r[2], r[3], (int)n[0], (int)n[1]));
            fit();
        });

        Ui.Label(panel, "Вращение выбранного объекта");
        var rotationAxis = Ui.Choice(panel, "Ось", "X", "Y", "Z");
        var angle = Ui.Input(panel, "Угол в градусах", "15");
        var center = Ui.Check(panel, "Вращать относительно центра объекта", true);

        Matrix4x4 Rotation(double degrees) => rotationAxis.SelectedIndex switch
        {
            0 => Matrix4x4.RotationX(degrees),
            1 => Matrix4x4.RotationY(degrees),
            _ => Matrix4x4.RotationZ(degrees)
        };

        void Rotate(double degrees)
        {
            var matrix = Rotation(degrees);
            if (center.IsChecked == true)
                matrix = Matrix4x4.AroundPoint(scene.Selected.Center, matrix);
            scene.Selected.Transform(matrix);
            scene.Notify();
        }

        Ui.Button(panel, "Повернуть", () => Rotate(Ui.Number(angle.Text)));
        var animate = Ui.Check(panel, "Автовращение объекта (выбранная ось)", false);
        timer.Tick += (_, _) =>
        {
            try
            {
                Rotate(1.2);
            }
            catch (ArgumentException)
            {
                animate.IsChecked = false;
            }
        };
        animate.Checked += (_, _) => timer.Start();
        animate.Unchecked += (_, _) => timer.Stop();
        Unloaded += (_, _) =>
        {
            timer.Stop();
            animate.IsChecked = false;
        };
    }
}
