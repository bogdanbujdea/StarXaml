using System;
using Windows.UI;
using Windows.UI.Xaml;
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
        public MainPage()
        {
            InitializeComponent();
            Loaded += ViewLoaded;
        }

        Random random = new Random((int)DateTime.Now.Ticks);
        private void ViewLoaded(object sender, RoutedEventArgs e)
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
    }
}
