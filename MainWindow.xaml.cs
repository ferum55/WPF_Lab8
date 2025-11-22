using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Lab8_Var9
{
    public partial class MainWindow : Window
    {
        private readonly Random rnd = new Random();
        private readonly List<Mover> items = new List<Mover>();
        private TimeSpan last;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var c in Scene.Children)
            {
                if (c is Shape shape)
                {
                    var t = new MatrixTransform();

                    var m = Matrix.Identity;

                    // Початкові координати
                    m.OffsetX = rnd.Next(50, 800);
                    m.OffsetY = rnd.Next(50, 500);

                    t.Matrix = m;
                    shape.RenderTransform = t;

                    // Початкова швидкість
                    double dx = rnd.NextDouble() * 4 + 1;
                    double dy = rnd.NextDouble() * 4 + 1;

                    if (rnd.Next(2) == 0) dx = -dx;
                    if (rnd.Next(2) == 0) dy = -dy;

                    items.Add(new Mover(shape, t, dx, dy));
                }
            }

            // запускаємо цикл анімації
            CompositionTarget.Rendering += OnFrame;

            // чекаємо поки фігури промалюються
            this.LayoutUpdated += InitShapesOnce;
        }

        private void InitShapesOnce(object? sender, EventArgs e)
        {
            foreach (var mv in items)
            {
                if (mv.Initialized)
                    continue;

                // реальні bounds фігури після рендера
                mv.Bounds = mv.Shape.RenderedGeometry.Bounds;
                mv.Initialized = true;
            }

            // якщо ВСІ фігури ініціалізовані → більше не слухаємо подію
            if (items.TrueForAll(x => x.Initialized))
                this.LayoutUpdated -= InitShapesOnce;
        }

        private void OnFrame(object? sender, EventArgs e)
        {
            if (e is RenderingEventArgs re)
            {
                if (last == TimeSpan.Zero)
                {
                    last = re.RenderingTime;
                    return;
                }

                double dt = (re.RenderingTime - last).TotalSeconds;
                last = re.RenderingTime;

                double W = Scene.ActualWidth;
                double H = Scene.ActualHeight;

                if (W == 0 || H == 0)
                    return;

                foreach (var mv in items)
                {
                    if (!mv.Initialized)
                        continue;

                    var m = mv.Transform.Matrix;

                    if (double.IsNaN(m.OffsetX)) m.OffsetX = 100;
                    if (double.IsNaN(m.OffsetY)) m.OffsetY = 100;

                    // рух
                    double speed = 30;
                    m.OffsetX += mv.Dx * speed * dt;
                    m.OffsetY += mv.Dy * speed * dt;

                    double w = mv.Bounds.Width;
                    double h = mv.Bounds.Height;

                    // відбивання
                    if (m.OffsetX <= 0)
                    {
                        m.OffsetX = 0;
                        mv.Dx = -mv.Dx;
                    }
                    else if (m.OffsetX + w >= W)
                    {
                        m.OffsetX = W - w;
                        mv.Dx = -mv.Dx;
                    }

                    if (m.OffsetY <= 0)
                    {
                        m.OffsetY = 0;
                        mv.Dy = -mv.Dy;
                    }
                    else if (m.OffsetY + h >= H)
                    {
                        m.OffsetY = H - h;
                        mv.Dy = -mv.Dy;
                    }

                    mv.Transform.Matrix = m;
                }
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }

    public sealed class Mover
    {
        public Shape Shape;
        public MatrixTransform Transform;
        public double Dx;
        public double Dy;

        public Rect Bounds;
        public bool Initialized;

        public Mover(Shape shape, MatrixTransform t, double dx, double dy)
        {
            Shape = shape;
            Transform = t;
            Dx = dx;
            Dy = dy;
            Initialized = false;
        }
    }
}
