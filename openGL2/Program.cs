
using ClickableTransparentOverlay;
using ImGuiNET;
using openGL2;
using openGL2.Window;
using OpenTK.Windowing.Desktop;
using System;


namespace OpenGL
{
    public class Program
    {

        static void Main(string[] args)
        {
            NativeWindowSettings nativeWindowSettings = new NativeWindowSettings()
            {
                ClientSize = new OpenTK.Mathematics.Vector2i(800, 600),
            };


            using (Window window = new(GameWindowSettings.Default, nativeWindowSettings))
            {
                UI ui = new();
                ui.Start();
                window.Run();

            }

        }

    }
    
}
