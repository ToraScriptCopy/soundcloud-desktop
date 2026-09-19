using System;
using System.Windows;
using System.Windows.Media.Animation;
namespace WpfApp1
{
    // Smooth shell animations. No layout thrash, only opacity and
    // render transforms, so WebView2 never lags.
    public static class Fx
    {
        public static void Fade(UIElement el, int ms)
        {
            Fade(el, ms, 0, 1);
        }
        public static void Fade(UIElement el, int ms, double from, double to)
        {
            try
            {
                if (el == null) return;
                var anim = new DoubleAnimation(from, to, new Duration(TimeSpan.FromMilliseconds(ms)));
                anim.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
                el.BeginAnimation(UIElement.OpacityProperty, anim);
            }
            catch { }
        }
        public static void SlideUp(UIElement el, int ms, double fromY)
        {
            try
            {
                if (el == null) return;
                var tt = el.RenderTransform as System.Windows.Media.TranslateTransform;
                if (tt == null)
                {
                    tt = new System.Windows.Media.TranslateTransform(0, fromY);
                    el.RenderTransform = tt;
                }
                else tt.Y = fromY;
                var anim = new DoubleAnimation(fromY, 0, new Duration(TimeSpan.FromMilliseconds(ms)));
                anim.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
                tt.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, anim);
            }
            catch { }
        }
    }
}
