using Microsoft.Win32;
using PolyhedraLab.Geometry;
using PolyhedraLab.IO;
using PolyhedraLab.Models;
using PolyhedraLab.Transforms;
using System.Windows.Controls;

namespace PolyhedraLab.UI;

public sealed class ModelPanel : UserControl
{
    public ModelPanel(Scene scene, Action fit)
    {
        var panel = new StackPanel();
        Content = Ui.Scroll(panel);

        var selection = Ui.Choice(panel, "Объект сцены", "");
        bool updating = false;
        void Refresh()
        {
            updating = true;
            selection.ItemsSource = scene.Objects.Select((m, i) => $"{i + 1}. {m.Name}").ToArray();
            selection.SelectedIndex = scene.SelectedIndex;
            updating = false;
        }
        scene.Changed += Refresh;
        selection.SelectionChanged += (_, _) =>
        {
            if (!updating && selection.SelectedIndex >= 0)
            {
                scene.SelectedIndex = selection.SelectedIndex;
                scene.Notify();
            }
        };
        var solid = Ui.Choice(panel, "Многогранник для проверки", "Тетраэдр", "Куб", "Октаэдр", "Икосаэдр", "Додекаэдр");
        Ui.Button(panel, "Показать многогранник", () =>
        {
            scene.SetModel(PolyhedronFactory.Create(solid.SelectedIndex));
            fit();
        });

        Ui.Button(panel, "Показать шар", () =>
        {
            scene.SetModel(SurfaceBuilder.Sphere());
            fit();
        });

        Ui.Button(panel, "Открыть OBJ", () =>
        {
            var dialog = new OpenFileDialog { Filter = "Wavefront OBJ|*.obj|Все файлы|*.*" };
            if (dialog.ShowDialog() != true)
                return;
            var model = ObjFile.Load(dialog.FileName);
            // Проверяем триангуляцию до изменения сцены.
            foreach (var face in model.Faces)
                Rendering.Triangulator.Triangulate(face, model.Vertices);
            scene.SetModel(model);
            fit();
        });
        Ui.Button(panel, "Сохранить выбранный объект в OBJ", () =>
        {
            var dialog = new SaveFileDialog { Filter = "Wavefront OBJ|*.obj", DefaultExt = ".obj", FileName = "model.obj" };
            if (dialog.ShowDialog() == true)
                ObjFile.Save(dialog.FileName, scene.Selected);
        });
        Ui.Button(panel, "Демо перекрытия (заменить сцену)", () =>
        {
            scene.OverlapDemo();
            fit();
        });

        var translation = Ui.Input(panel, "Смещение dx dy dz", "0.5 0 0");
        Ui.Button(panel, "Сместить", () =>
        {
            var v = Ui.Numbers(translation, 3);
            scene.Selected.Transform(AffineTransforms.Translation(v[0], v[1], v[2]));
            scene.Notify();
        });
        var scale = Ui.Input(panel, "Масштаб kx ky kz (не ноль)", "1.2 1.2 1.2");
        var centered = Ui.Check(panel, "Масштаб относительно центра объекта", true);
        Ui.Button(panel, "Масштабировать", () =>
        {
            var v = Ui.Numbers(scale, 3);
            if (v.Any(x => Math.Abs(x) < 1e-6 || Math.Abs(x) > 1000))
                throw new ArgumentException("Модуль масштаба должен быть от 0.000001 до 1000.");
            var matrix = AffineTransforms.Scaling(v[0], v[1], v[2]);
            if (centered.IsChecked == true)
                matrix = Matrix4x4.AroundPoint(scene.Selected.Center, matrix);
            scene.Selected.Transform(matrix);
            scene.Notify();
        });
        var plane = Ui.Choice(panel, "Плоскость отражения", "XY", "XZ", "YZ");
        Ui.Button(panel, "Отразить", () =>
        {
            scene.Selected.Transform(AffineTransforms.Reflection(plane.SelectedIndex));
            scene.Notify();
        });

        Refresh();
    }
}
