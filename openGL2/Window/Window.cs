using ImGuiNET;
using openGL2.Objects;
using openGL2.Shaders;
using openGL2.Textures;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D11;
using Xunit;



namespace openGL2.Window
{
    public class Window : GameWindow
    {
        private Figure square;
        private Figure square2;

        private Shader shader;
        private Camera camera;


        // der er en dedikeret VAO til de forskellige VAO man får brug for

        int VAO;
        int VBO;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
            shader = new Shader();
            square = new Figure(shader, Figure.FigureType.QUAD, false);
            square2 = new Figure(shader, Figure.FigureType.CUBE, true);
            square2.MoveFigure();
            camera = new Camera(this);
        }

        int fbo;
        public static int texture;
        public static IntPtr imguiTexture;

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(new Color4(0.3f, 0.4f, 0.2f, 1));

            VBO = square.GetVBOHandle;
            VAO = square.GetVAOHandle;

            GL.Enable(EnableCap.DepthTest);
            // texture new:
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);



           
            Texture tex1 = new(@"..\..\..\Textures\TextureImages\rock.tga", "rock");
            Texture tex2 = new(@"..\..\..\Textures\TextureImages\Rainbow.tga", "rain");
            Texture White = Texture.GetWhite();

            Texture Chekered = Texture.GetCheckered();


            Texture albedo = new(@"..\..\..\Textures\TextureImages\brickAlbedo.tga", "albedo");
            Texture lightMap = new(@"..\..\..\Textures\TextureImages\LightmapTest.tga", "lightMap");
            Texture specular = new(@"..\..\..\Textures\TextureImages\brickLight.tga", "specular");
            Texture normalMap = new(@"..\..\..\Textures\TextureImages\brickNormal.tga", "normal");


            square.Albedo = albedo;
            square2.Albedo = albedo;
            square.LightMap = lightMap;
            square2.LightMap = lightMap;
            square.SpecularMap = specular;
            square2.SpecularMap = specular;
            square.NormalTexture = normalMap;
            square2.NormalTexture = normalMap;

            camera.UpdateView();
        }


        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
   
            //TODO spørg søren om de behøver deklerers hver gang
            KeyboardState input;
            MouseState mouse;
            if (IsFocused) 
            {
                // disse er kun nødvendige hvis winduet faktisk er i fokus
                input = KeyboardState;
                mouse = MouseState;
                camera.UpdateCamera(input, args, mouse);
            }   

            camera.UpdateView();

            ObjectHandler.DrawAllFiguresInScene();


            this.Context.SwapBuffers();

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);


            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
            }
        }




        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
        }

        protected override void OnUnload()
        {
            // Unbind all the resources by binding the targets to 0/null.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            // Delete all the resources.
            GL.DeleteBuffer(VBO);
            GL.DeleteVertexArray(VAO);

            GL.DeleteProgram(shader.ShaderProgramHandle);

            base.OnUnload();
        }

    }
}
