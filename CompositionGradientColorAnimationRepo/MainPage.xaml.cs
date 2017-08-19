using System;
using System.Numerics;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace CompositionGradientColorAnimationRepo
{
    public sealed partial class MainPage : Page
    {
        private readonly CompositionLinearGradientBrush _gradientBrush;
        private readonly SpriteVisual _backgroundVisual;
        private static readonly Color GradientStop1StartColor = Color.FromArgb(255, 251, 218, 97);
        private static readonly Color GradientStop2StartColor = Color.FromArgb(255, 255, 90, 205);

        public MainPage()
        {
            InitializeComponent();
            Root.SizeChanged += OnRootSizeChanged;

            var compositor = Window.Current.Compositor;

            // Initially, we set the end point to be (0,0) 'cause we want to animate it at start.
            // If you don't want this behavior, simply set it to a different value within (1,1).
            _gradientBrush = compositor.CreateLinearGradientBrush();
            _gradientBrush.EndPoint = Vector2.Zero;

            // Create gradient initial colors.
            var gradientStop1 = compositor.CreateColorGradientStop();
            gradientStop1.Offset = 0.0f;
            gradientStop1.Color = GradientStop1StartColor;
            var gradientStop2 = compositor.CreateColorGradientStop();
            gradientStop2.Offset = 1.0f;
            gradientStop2.Color = GradientStop2StartColor;
            _gradientBrush.ColorStops.Add(gradientStop1);
            _gradientBrush.ColorStops.Add(gradientStop2);

            // Assign the gradient brush to the Root element's Visual.
            _backgroundVisual = compositor.CreateSpriteVisual();
            _backgroundVisual.Brush = _gradientBrush;
            ElementCompositionPreview.SetElementChildVisual(Root, _backgroundVisual);

            // There are 3 animations going on here.
            // First, we kick off an EndPoint offset animation to create an special entrance scene.
            // Once it's finished, we then kick off TWO other animations simultaneously. 
            // These TWO animations include a set of gradient stop color animations and
            // a rotation animation that rotates the gradient brush.

            var linearEase = compositor.CreateLinearEasingFunction();
            var batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            batch.Completed += (s, e) =>
            {
                StartGradientColorAnimations();
                StartGradientRotationAnimation();
            };
            var endPointOffsetAnimation = compositor.CreateVector2KeyFrameAnimation();
            endPointOffsetAnimation.Duration = TimeSpan.FromSeconds(3);
            endPointOffsetAnimation.InsertKeyFrame(1.0f, Vector2.One);
            _gradientBrush.StartAnimation(nameof(_gradientBrush.EndPoint), endPointOffsetAnimation);
            batch.End();

            void StartGradientColorAnimations()
            {
                var color1Animation = compositor.CreateColorKeyFrameAnimation();
                color1Animation.Duration = TimeSpan.FromSeconds(10);
                color1Animation.IterationBehavior = AnimationIterationBehavior.Forever;
                color1Animation.Direction = AnimationDirection.Alternate;
                color1Animation.InsertKeyFrame(0.0f, GradientStop1StartColor, linearEase);
                color1Animation.InsertKeyFrame(0.5f, Color.FromArgb(255, 65, 88, 208), linearEase);
                color1Animation.InsertKeyFrame(1.0f, Color.FromArgb(255, 43, 210, 255), linearEase);
                gradientStop1.StartAnimation(nameof(gradientStop1.Color), color1Animation);

                var color2Animation = compositor.CreateColorKeyFrameAnimation();
                color2Animation.Duration = TimeSpan.FromSeconds(10);
                color2Animation.IterationBehavior = AnimationIterationBehavior.Forever;
                color2Animation.Direction = AnimationDirection.Alternate;
                color2Animation.InsertKeyFrame(0.0f, GradientStop2StartColor, linearEase);
                color1Animation.InsertKeyFrame(0.5f, Color.FromArgb(255, 200, 80, 192), linearEase);
                color2Animation.InsertKeyFrame(1.0f, Color.FromArgb(255, 43, 255, 136), linearEase);
                gradientStop2.StartAnimation(nameof(gradientStop2.Color), color2Animation);
            }

            void StartGradientRotationAnimation()
            {
                var rotationAnimation = compositor.CreateScalarKeyFrameAnimation();
                rotationAnimation.Duration = TimeSpan.FromSeconds(15);
                rotationAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
                rotationAnimation.InsertKeyFrame(1.0f, 360.0f, linearEase);
                _gradientBrush.StartAnimation(nameof(_gradientBrush.RotationAngleInDegrees), rotationAnimation);
            }
        }

        private void OnRootSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize == e.PreviousSize) return;

            _backgroundVisual.Size = e.NewSize.ToVector2();
            _gradientBrush.CenterPoint = _backgroundVisual.Size / 2;
        }
    }
}
