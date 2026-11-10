using System.Windows.Controls;
using System.Windows.Threading;
using PolyhedraLab.Models;
using PolyhedraLab.Rendering;

namespace PolyhedraLab.UI;

public sealed class ViewPanel : UserControl
{
    private readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(40) };

    public ViewPanel(Camera camera, SceneRenderer renderer, Action draw)
    {
        var panel = new StackPanel();
        Content = Ui.Scroll(panel);
        var projection = Ui.Choice(panel, "Проекция", "Перспективная", "Параллельная");
        projection.SelectionChanged += (_, _) =>
        {
            camera.Perspective = projection.SelectedIndex == 0;
            draw();
        };
        var cull = Ui.Check(panel, "Отсекать нелицевые грани", true);
        var zbuffer = Ui.Check(panel, "Z-буфер", true);
        void Options()
        {
            renderer.CullBackFaces = cull.IsChecked == true;
            renderer.UseZBuffer = zbuffer.IsChecked == true;
            draw();
        }
        foreach (var box in new[] { cull, zbuffer })
        {
            box.Checked += (_, _) => Options();
            box.Unchecked += (_, _) => Options();
        }
        Ui.Label(panel, "Камера смотрит из положения P в цель T. Вектор обзора = T − P.");
        var position = Ui.Input(panel, "Положение P: x y z", "5 3.5 7");
        var target = Ui.Input(panel, "Цель T: x y z", "0 0 0");
        var fov = Ui.Input(panel, "Угол обзора (10–120°)", "55");
        var viewSize = Ui.Input(panel, "Высота параллельного вида", "6");
        void Sync()
        {
            position.Text = FormattableString.Invariant($"{camera.Position.X:0.###} {camera.Position.Y:0.###} {camera.Position.Z:0.###}");
            target.Text = FormattableString.Invariant($"{camera.Target.X:0.###} {camera.Target.Y:0.###} {camera.Target.Z:0.###}");
            viewSize.Text = FormattableString.Invariant($"{camera.ViewSize:0.###}");
        }
        Ui.Button(panel, "Применить камеру", () =>
        {
            var p = Ui.Numbers(position, 3);
            var t = Ui.Numbers(target, 3);
            double f = Ui.Number(fov.Text), size = Ui.Number(viewSize.Text);
            var pos = new Point3D(p[0], p[1], p[2]);
            var aim = new Point3D(t[0], t[1], t[2]);
            if ((aim - pos).Length < 0.001 || pos.Length > 1e6 || aim.Length > 1e6 || f < 10 || f > 120 || size < 0.01 || size > 1e6)
                throw new ArgumentException("Камера и цель должны различаться; угол 10–120°, высота вида 0.01–1000000, координаты в пределах ±1000000.");
            camera.Position = pos;
            camera.Target = aim;
            camera.FieldOfView = f;
            camera.ViewSize = size;
            draw();
        });
        Ui.Button(panel, "Камера вокруг цели: −10°", () =>
        {
            camera.Orbit(-10);
            Sync();
            draw();
        });

        Ui.Button(panel, "Камера вокруг цели: +10°", () =>
        {
            camera.Orbit(10);
            Sync();
            draw();
        });
        var animate = Ui.Check(panel, "Облёт камеры", false);
        timer.Tick += (_, _) =>
        {
            camera.Orbit(0.8);
            Sync();
            draw();
        };
        animate.Checked += (_, _) =>
        {
            camera.Perspective = true;
            projection.SelectedIndex = 0;
            timer.Start();
        };
        animate.Unchecked += (_, _) => timer.Stop();
        Unloaded += (_, _) =>
        {
            timer.Stop();
            animate.IsChecked = false;
        };
        Loaded += (_, _) => Sync();
    }
}
