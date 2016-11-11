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

namespace StarXaml
{
    public sealed partial class MainPage
    {
        private int _hitCount;

        public MainPage()
        {
            InitializeComponent();
            Loaded += ViewLoaded;
        }

        readonly Random random = new Random((int)DateTime.Now.Ticks);

        private Point LastPosition { get; set; }

        private void ViewLoaded(object sender, RoutedEventArgs e)
        {
            StartStarsTimer();
            StartFireTimer();
            Window.Current.CoreWindow.PointerMoved += PointerCursorMoved;
        }

        private void StartFireTimer()
        {
            var fireTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            fireTimer.Tick += (o, args) => Fire();
            fireTimer.Start();
        }

        private void PointerCursorMoved(CoreWindow sender, PointerEventArgs args)
        {
            var point = new Point
            {
                X = args.CurrentPoint.Position.X - (StarShipImage.ActualWidth/2),
                Y = args.CurrentPoint.Position.Y - (StarShipImage.ActualHeight/2)
            };

            int rotation = 0;
            if (LastPosition.X < point.X)
                rotation = -25;
            else if (LastPosition.X > point.X)
                rotation = 25;
            StarShipPanel.Transform3D = new CompositeTransform3D
            {
                CenterX = 0.5,
                CenterY = 0.5,
                RotationZ = rotation
            };
            LastPosition = point;
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
                var compositeTransform3D = (star.Transform3D as CompositeTransform3D);
                if (compositeTransform3D != null)
                {
                    compositeTransform3D.RotationX = 90;
                    compositeTransform3D.ScaleY = 50;
                }
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
                Margin = new Thickness(point.X + 70, point.Y, 0, 0),
                Fill = new LinearGradientBrush(new GradientStopCollection {
                new GradientStop
                {
                    Color = Colors.Red,
                    Offset = 0
                },
                new GradientStop
                {
                    Color = Colors.DarkOrange,
                    Offset = 0.8
                },
                new GradientStop
                {
                    Color = Colors.Yellow,
                    Offset = 1
                }
                }, 90),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Transform3D = new CompositeTransform3D
                {
                    TranslateX = 0,
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
                var wasHit = ShipWasHit(bomb);
                if (wasHit)
                    _hitCount++;
                Debug.WriteLine("Was hit: " + wasHit);
                HitCountTextBlock.Text = _hitCount.ToString();
                GC.Collect();
            };
            storyboard.Begin();
        }

        private bool ShipWasHit(Ellipse bomb)
        {
            var shipRect = new Rect(GetShipPosition(), new Size(StarShipPanel.ActualWidth, StarShipPanel.ActualHeight));
            var bombRect = new Rect(new Point(bomb.Margin.Left, bomb.Margin.Top), new Size(10, 10));
            Debug.WriteLine("ship rect: " + shipRect);
            Debug.WriteLine("bomb rect: " + bombRect);
            shipRect.Intersect(bombRect);
            return shipRect.IsEmpty == false;
        }

        private void Released(object sender, PointerRoutedEventArgs e)
        {
            Fire();
        }
    }
}
