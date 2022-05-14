using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Heart
{
    public static class Program
    {
        private static void Main()
        {
            // Установка настроек окна программы
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(1024, 768),
                Title = "Heart",
            };
            // Запуск окна программы
            using var window = new Window(GameWindowSettings.Default, nativeWindowSettings);
            window.VSync = VSyncMode.On;
            window.Run();
        }
    }
}