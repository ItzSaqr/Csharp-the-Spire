using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace CardGame.WPF
{
    public class Animator
    {
        private readonly Image sprite;
        private readonly TranslateTransform translate;
        private readonly ScaleTransform scale;
        private readonly DropShadowEffect flash;

        // -1 = left, +1 = right
        private readonly int direction;

        public Animator(Image sprite, int direction)
        {
            this.sprite = sprite;
            this.direction = direction;

            translate = new TranslateTransform();
            scale = new ScaleTransform(1, 1);

            var group = new TransformGroup();
            group.Children.Add(scale);
            group.Children.Add(translate);
            sprite.RenderTransform = group;
            sprite.RenderTransformOrigin = new Point(0.5, 0.5);

            flash = new DropShadowEffect
            {
                Color = Colors.Red,
                BlurRadius = 0,
                ShadowDepth = 0,
                Opacity = 1
            };
        }

        public Task AttackAsync(double distance = 100, int durationMs = 300)
        {
            var tcs = new TaskCompletionSource<object>();

            var anim = new DoubleAnimationUsingKeyFrames();
            anim.KeyFrames.Add(new EasingDoubleKeyFrame(0,
                KeyTime.FromTimeSpan(TimeSpan.Zero)));
            anim.KeyFrames.Add(new EasingDoubleKeyFrame(direction * distance,
                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMs * 0.4)),
                new QuadraticEase { EasingMode = EasingMode.EaseOut }));
            anim.KeyFrames.Add(new EasingDoubleKeyFrame(0,
                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMs)),
                new QuadraticEase { EasingMode = EasingMode.EaseIn }));

            anim.Completed += (s, e) => tcs.SetResult(null);
            translate.BeginAnimation(TranslateTransform.XProperty, anim);

            return tcs.Task;
        }

        public Task HurtAsync(double knockback = 40, int durationMs = 350)
        {
            var tcs = new TaskCompletionSource<object>();

            var move = new DoubleAnimationUsingKeyFrames();
            move.KeyFrames.Add(new EasingDoubleKeyFrame(0,
                KeyTime.FromTimeSpan(TimeSpan.Zero)));
            move.KeyFrames.Add(new EasingDoubleKeyFrame(-direction * knockback,
                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMs * 0.25)),
                new QuadraticEase { EasingMode = EasingMode.EaseOut }));
            move.KeyFrames.Add(new EasingDoubleKeyFrame(0,
                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMs)),
                new QuadraticEase { EasingMode = EasingMode.EaseInOut }));

            var flashAnim = new DoubleAnimationUsingKeyFrames();
            flashAnim.KeyFrames.Add(new EasingDoubleKeyFrame(0,
                KeyTime.FromTimeSpan(TimeSpan.Zero)));
            flashAnim.KeyFrames.Add(new EasingDoubleKeyFrame(20,
                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(80))));
            flashAnim.KeyFrames.Add(new EasingDoubleKeyFrame(0,
                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(durationMs))));

            move.Completed += (s, e) =>
            {
                sprite.Effect = null;
                flash.BeginAnimation(DropShadowEffect.BlurRadiusProperty, null);
                tcs.SetResult(null);
            };

            sprite.Effect = flash;
            translate.BeginAnimation(TranslateTransform.XProperty, move);
            flash.BeginAnimation(DropShadowEffect.BlurRadiusProperty, flashAnim);

            return tcs.Task;
        }
    }
}
