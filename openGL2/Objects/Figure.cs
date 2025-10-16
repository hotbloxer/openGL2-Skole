﻿using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using openGL2.Shaders;
using openGL2.Textures;
using openGL2.Window;

namespace openGL2.Objects
{
    public class Figure: IDisposable
    {
        private float[] _vertices;
        private int _VBOHandle;
        private int _VAOHandle;
        private string _name;
        public string Name { get => _name;  }

        private Shader _shader;

        private VertexInformation _vertexInformation;

        public int GetVBOHandle { get => _VBOHandle; }
        public int GetVAOHandle { get => _VAOHandle; }

        Matrix4 _modelView;

        public enum FigureType {QUAD, CUBE }
        public enum VertexInfo {POSITION, UV}

        private Texture _albedoTexture;
        public Texture Albedo { get => _albedoTexture; set => _albedoTexture = value; }

        private Texture _lightMap;
        public Texture LightMap { get => _lightMap; set => _lightMap = value; }

        private Texture _specularTexture;
        public Texture SpecularMap { get => _specularTexture; set => _specularTexture = value; }

        private Texture _normalTexture;
        public Texture NormalTexture { get => _normalTexture; set => _normalTexture = value; }


        public Figure (Shader shader, FigureType type = FigureType.QUAD,  bool withUV = true)
        {
            _shader = shader;

            _vertexInformation = GetVertexInformation(type);
            _vertices = VertexInformation.GetCombinedInfoForVertecis(_vertexInformation);

            _VBOHandle = GenerateVBOHandle();
            _VAOHandle = VertexArrayObjectHandler.VAO;
        
            _modelView = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(-0));

            _name = ObjectNamer();

            ObjectHandler.AddFigureToScene(this);
        }

        private string ObjectNamer ()
        {
            return $"objekt "+ ObjectHandler.GetFigures.Count;
        }


        private VertexInformation GetVertexInformation(FigureType type) 
        {
            switch (type)
            {
                case FigureType.QUAD:
                    return PrimitivesVertexFigures.GetSquare();

                case FigureType.CUBE:
                    return PrimitivesVertexFigures.GetCube();

                default:
                    return PrimitivesVertexFigures.GetSquare();
            }
        }


        public void UpdateModelsSpace ()
        {
            Shader.UpdateModelSpace(_modelView);
        }

     
        public void MoveFigure ()
        {
            
            _modelView *= Matrix4.CreateTranslation(1.1f, 0, 0);
        }

        public float[] GetVertices ()
        {
            return _vertices;
        }

        private int GenerateVBOHandle()
        {
            int VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            return VBO;
        }
        
        


        public void Draw ()
        {
            _shader.Use();

            GL.BindVertexArray(_VAOHandle);

            GL.BindTextureUnit((int) Shader.textureUnits.ALBEDO, _albedoTexture.ID);
            _shader.SetTextureUniform(Shader.textureUnits.ALBEDO);

            GL.BindTextureUnit((int)Shader.textureUnits.LIGHTMAP, _lightMap.ID);
            _shader.SetTextureUniform(Shader.textureUnits.LIGHTMAP);

            GL.BindTextureUnit((int)Shader.textureUnits.SPECULARMAP, _specularTexture.ID);
            _shader.SetTextureUniform(Shader.textureUnits.SPECULARMAP);

            GL.BindTextureUnit((int)Shader.textureUnits.NORMALMAP, _normalTexture.ID);
            _shader.SetTextureUniform(Shader.textureUnits.NORMALMAP);




            // TODO spørg søren hvorfor den her skal sættes hvert eneste render loop for at virke??
            _shader.SetUVTest(UI.displaUVTesting);
            _shader.SetUsingTexture(UI.useTexture);
            _shader.SetUsingBlinn(UI.UsingBlinnLight);
            _shader.UsingCellShader(UI.UsingCellShading);
            _shader.UsingRimLight(UI.UsingRimLight);
            _shader.SetLightColor(UI.LightColorTK);
            _shader.SetObjectColor(UI.ObjectColorTK);

            _shader.UpdateUniformValuesForRender();

            GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Length);
        }



        public void Dispose()
        {
            ObjectHandler.RemoveFigureFromScene(this);
        }
    }
}
