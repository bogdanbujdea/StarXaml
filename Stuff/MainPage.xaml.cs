using System;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Media3D;
using Windows.UI.Xaml.Shapes;

namespace Stuff
{
    public sealed partial class MainPage
    {
        private int _hitCount;
        private Point _lastPosition = new Point();

        public MainPage()
        {
            InitializeComponent();
            Loaded += ViewLoaded;
        }

        Random random = new Random((int)DateTime.Now.Ticks);
        private void ViewLoaded(object sender, RoutedEventArgs e)
        {
            StartStarsTimer();
            StartFireTimer();
            Window.Current.CoreWindow.PointerMoved += PointerCursorMoved;
        }

        private void StartFireTimer()
        {
            var fireTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            fireTimer.Tick += (o, args) => Fire();
            fireTimer.Start();
        }

        private void PointerCursorMoved(CoreWindow sender, PointerEventArgs args)
        {
            var point = new Point();
            point.X = args.CurrentPoint.Position.X - (StarShipImage.ActualWidth / 2);
            point.Y = args.CurrentPoint.Position.Y - (StarShipImage.ActualHeight / 2);

            int rotation = 0;
             if (_lastPosition.X < point.X)
                rotation = -25;
            else if (_lastPosition.X > point.X)
                rotation = 25;
            StarShipPanel.Transform3D = new CompositeTransform3D
            {
                CenterX = 0.5,
                CenterY = 0.5,
                RotationZ = rotation
            };
            _lastPosition = point;
            MoveStarShip(point);
        }

        private void MoveStarShip(Point point)
        {
            Canvas.SetLeft(StarShipPanel, point.X);
            Canvas.SetTop(StarShipPanel, point.Y);
        }

        private void StartStarsTimer()
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(10) };
            timer.Tick += (o, args) =>
            {
                CreateStar();
                CreateStar();
                CreateStar();

            };
            timer.Start();
        }

        private Point GetShipPosition()
        {
            var position = GetMousePosition();
            var point = new Point();
            if (position.X < 0)
                return point;
            if (position.X > StarGrid.ActualWidth)
            {
                point.X = Window.Current.CoreWindow.Bounds.Width;
                return point;
            }
            point.X = position.X - (StarShipImage.ActualWidth / 2);
            point.Y = position.Y - (StarShipImage.ActualHeight / 2);
            return point;
        }

        private Point GetMousePosition()
        {
            Rect bounds = Window.Current.CoreWindow.Bounds;
            var point = Window.Current.CoreWindow.PointerPosition;
            return new Point(point.X - bounds.X, point.Y - bounds.Y);
        }

        bool Inside(double x, double y, double radius, double centerX = 0, double centerY = 0)
        {
            return Math.Pow(x - centerX, 2) + Math.Pow(y - centerY, 2) < Math.Pow(radius, 2);
        }

        private void CreateStar()
        {
            var range = 2000;
            var x = random.Next(-range, range);
            var y = random.Next(-range, range);
            while (Inside(x, y, 500))
            {
                x = random.Next(-range, range);
                y = random.Next(-range, range);
            }
            var star = new Ellipse
            {
                Height = 20,
                Width = 20,
                Fill = new SolidColorBrush(Colors.White),
                Transform3D = new CompositeTransform3D
                {
                    TranslateX = x,
                    TranslateY = y,
                    TranslateZ = -15000
                }
            };
            StarGrid.Children.Add(star);
            var duration = TimeSpan.FromSeconds(4);
            if (Inside(x, y, 1000))
            {
                star.Opacity = 0.5;
                (star.Transform3D as CompositeTransform3D).RotationX = 90;
                (star.Transform3D as CompositeTransform3D).ScaleY = 50;
                duration = TimeSpan.FromSeconds(2);
            }

            var animation = new DoubleAnimation
            {
                To = 250,
                Duration = duration
            };
            Storyboard.SetTarget(animation, star.Transform3D);
            Storyboard.SetTargetProperty(animation, "TranslateZ");
            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.Completed += (sender, o) =>
            {
                StarGrid.Children.Remove(star);
                GC.Collect();
            };
            storyboard.Begin();
        }

        private void Fire()
        {
            var point = GetShipPosition();
            var bomb = new Ellipse
            {
                Width = 10,
                Height = 10,
                Margin = new Thickness(point.X + 70, 0, 0, 0),
                Fill = new SolidColorBrush(Colors.Crimson),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Transform3D = new CompositeTransform3D
                {
                    TranslateX = 0,
                    TranslateY = point.Y - Window.Current.CoreWindow.Bounds.Height,
                    TranslateZ = -5000
                }
            };
            (bomb.Transform3D as CompositeTransform3D).RotationX = 90;
            (bomb.Transform3D as CompositeTransform3D).ScaleY = 30;
            StarGrid.Children.Add(bomb);

            var animation = new DoubleAnimation
            {
                To = 200,
                Duration = TimeSpan.FromMilliseconds(1000)
            };
            Storyboard.SetTarget(animation, bomb.Transform3D);
            Storyboard.SetTargetProperty(animation, "TranslateZ");
            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.Completed += (sender, o) =>
            {
                StarGrid.Children.Remove(bomb);
                DeterminIfShipWasHit(bomb);
                HitCountTextBlock.Text = _hitCount.ToString();
                GC.Collect();
            };
            storyboard.Begin();
        }

        private void DeterminIfShipWasHit(Ellipse bomb)
        {
            var left = Canvas.GetLeft(StarShipPanel);
            if (left > bomb.Margin.Left)
            {
                Debug.WriteLine($"MISSED: {left}, {bomb.Margin.Left}");
            }
            else if (left + StarShipPanel.ActualWidth - bomb.Margin.Left > 0)
            {
                _hitCount++;
                Debug.WriteLine($"HIT: {left}, {bomb.Margin.Left}");
            }
            else
            {
                Debug.WriteLine($"MISSED: {left}, {bomb.Margin.Left}");
            }
        }

        private void Released(object sender, PointerRoutedEventArgs e)
        {
            Fire();
        }
    }
}
