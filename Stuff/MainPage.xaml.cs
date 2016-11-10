using System;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Media3D;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Stuff
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        private int _hitCount = 0;
        public MainPage()
        {
            InitializeComponent();
            Loaded += ViewLoaded;
        }

        Random random = new Random((int)DateTime.Now.Ticks);
        private void ViewLoaded(object sender, RoutedEventArgs e)
        {
            StartStarsTimer();
            StartGameTimer();
            StartFireTimer();
        }

        private void StartFireTimer()
        {
            var fireTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            fireTimer.Tick += (o, args) => Fire();
            fireTimer.Start();
        }

        private void StartGameTimer()
        {
            var gameTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(2) };
            gameTimer.Tick += (o, args) => MoveStarShip();
            gameTimer.Start();
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

        private void MoveStarShip()
        {
            var x = GetLeftBorder();
            StarShipPanel.Margin = new Thickness(x, 0, 0, 0);
        }

        private double GetLeftBorder()
        {
            var position = GetMousePosition();
            var space = Window.Current.CoreWindow.Bounds.Width - StarGrid.ActualWidth;
            if (position.X < space / 2)
                return 0;
            if (position.X > StarGrid.ActualWidth + space / 2)
                return 750;
            return position.X - (StarShipImage.ActualWidth / 2);
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
            var x = GetLeftBorder();
            var bomb = new Ellipse
            {
                Width = 30,
                Height = 30,
                Margin = new Thickness(x + 70, 0, 0, 130),
                Fill = new SolidColorBrush(Colors.Red),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Transform3D = new CompositeTransform3D
                {
                    TranslateX = 0,
                    TranslateY = 0,
                    TranslateZ = -5000
                }
            };
            (bomb.Transform3D as CompositeTransform3D).RotationX = 90;
            (bomb.Transform3D as CompositeTransform3D).ScaleY = 5;
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
            var left = StarShipPanel.Margin.Left;
            if (left > bomb.Margin.Left)
            {
                Debug.WriteLine($"MISSED: {left}, {bomb.Margin.Left}");
            }
            else if (left + 180 - bomb.Margin.Left > 0)
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
