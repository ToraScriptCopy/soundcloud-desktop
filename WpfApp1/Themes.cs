using System.Windows.Media;
using Wpf.Ui.Appearance;
namespace WpfApp1
{
    public static class Themes
    {
        public const int Count = 10;
        public static void Apply(int index)
        {
            if (index <= 0)
            {
                ApplicationThemeManager.ApplySystemTheme();
                try { ApplicationAccentColorManager.ApplySystemAccent(); }
                catch { }
                return;
            }
            ApplicationTheme baseTheme = ApplicationTheme.Dark;
            Color accent = Color.FromRgb(0xFF, 0x55, 0x00);
            switch (index)
            {
                case 1:
                    baseTheme = ApplicationTheme.Dark;
                    accent = Color.FromRgb(0x00, 0x78, 0xD4);
                    break;
                case 2:
                    baseTheme = ApplicationTheme.Light;
                    accent = Color.FromRgb(0x00, 0x78, 0xD4);
                    break;
                case 3:
                    accent = Color.FromRgb(0xFF, 0x55, 0x00);
                    break;
                case 4:
                    accent = Color.FromRgb(0x16, 0xC6, 0x0C);
                    break;
                case 5:
                    accent = Color.FromRgb(0xE8, 0x11, 0x23);
                    break;
                case 6:
                    accent = Color.FromRgb(0x00, 0x78, 0xD4);
                    break;
                case 7:
                    accent = Color.FromRgb(0x87, 0x64, 0xB8);
                    break;
                case 8:
                    accent = Color.FromRgb(0xE3, 0x00, 0x8C);
                    break;
                case 9:
                    accent = Color.FromRgb(0x00, 0xB7, 0xC3);
                    break;
                default:
                    break;
            }
            ApplicationThemeManager.Apply(baseTheme);
            try { ApplicationAccentColorManager.Apply(accent, baseTheme, false, false); }
            catch { }
        }
        public static bool IsDark(int index)
        {
            return index != 2;
        }
    }
}
