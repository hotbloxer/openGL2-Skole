using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Vortice.Direct3D11on12;
using Vortice.DXGI;

namespace openGL2.Shaders
{
    public class Shader : IDisposable
    {

        string _vertexShaderSource;
        string _fragmentShaderSource;

        protected int _shaderProgramHandle;

        public int ShaderProgramHandle { get => _shaderProgramHandle; }


        protected List<ShaderPart> parts = new List<ShaderPart>();

        // den her samler alle shaders
        public static List<Shader> shaders = new List<Shader>();


        private Matrix4Uniform _view;
        private Matrix4Uniform _model;
        private Matrix4Uniform _modelView;
        private Matrix4Uniform _projection;
        private Matrix4Uniform _projectionViewModel;

        public Matrix4 View { get => _view.Matrix; set => _view.Matrix = value;}
        public Matrix4 Model{ get => _model.Matrix; set =>_model.Matrix = value; }
        public Matrix4 ModelView { get => _modelView.Matrix; set => _modelView.Matrix = value; }
        public Matrix4 Projection { get => _projection.Matrix; private set => _projection.Matrix = value; }


        public Shader() 
        {
            _fragmentShaderSource = SetDefaultFragmentShader();
            _vertexShaderSource = SetDefaultVertexShader();

            InitShader();

            shaders.Add(this);
        }


        private void InitShader()
        {
            _shaderProgramHandle = GL.CreateProgram();

            _view = new Matrix4Uniform(_shaderProgramHandle, "view");
            _model = new Matrix4Uniform(_shaderProgramHandle, "model");
            _modelView = new Matrix4Uniform(_shaderProgramHandle, "modelView");
            _projection = new Matrix4Uniform(_shaderProgramHandle, "projection");
            _projectionViewModel = new Matrix4Uniform(_shaderProgramHandle, "projectionViewModel");

            // adds vertex and fragments to list, da der skal udføres de samme operationer
            // dette reducere dublikationer og der kan tilføjes flere slags shaders senere
            parts.Add(new ShaderPart(ShaderType.VertexShader, _vertexShaderSource));
            parts.Add(new ShaderPart(ShaderType.FragmentShader, _fragmentShaderSource));
            
            SetUpShaderParts();

            GL.LinkProgram(_shaderProgramHandle);

            // test for link fejl
            GL.GetProgram(_shaderProgramHandle, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(_shaderProgramHandle);
                Console.WriteLine(infoLog);
            }

            DetachAndDeleteShaderParts();
        }

        private string SetDefaultVertexShader ()
        {
            return
            @$"#version 330 core 
            layout(location = 0) in vec3 aPosition;
            layout(location = 1) in vec2 aUV;
            layout(location = 2) in vec3 aNormal;
            layout(location = 3) in vec3 aTangent;  
            layout(location = 4) in vec3 aBiNormal;

            uniform mat4 modelView;
            uniform mat4 model;
            uniform mat4 view;
            uniform mat4 projection;
            uniform mat4 projectionViewModel;
            out vec2 uv;
            out vec3 vertexNormal;
            out vec3 fragPosition;
            out mat3 tbn;


            void main() 
            {{ 
                fragPosition = vec3(model * vec4(aPosition, 1.0));
                uv = aUV;


                mat4 modelview2 = model * view;
                mat3 normalMatrix = transpose(inverse(mat3( modelview2)));
                mat4 modelViewInstance = modelView;

                
    
                tbn = mat3(
                  normalize(normalMatrix * vec3(aBiNormal)),
                  normalize(normalMatrix * vec3(aTangent )),
                  normalize(normalMatrix * vec3(aNormal  )));

                //normalize(vec3(model * vec4(aNormal,   0.0))),
                //normalize(vec3(model * vec4(aBiNormal, 0.0))),
                //normalize(vec3(model * vec4(aTangent,  0.0))));

                vertexNormal =  aNormal;
                
                


                mat4 test = view;
                mat4 test2 = model;
                mat4 test3 = projection;

                //gl_Position = projection * view  * model * vec4(aPosition, 1.0) ;
                gl_Position = projectionViewModel * vec4(aPosition, 1.0) ;


            }}";

            //TODO fix normal matrixen
            //mat3 normalMatrix = mat3(transpose(inverse(modelView)));  
            //normal =  aNormal * normalMatrix;
        }
        private string SetDefaultFragmentShader()
        {
            return 
            @$"#version 330 core 
            out vec4 FragColor; 
            in vec2 uv;
            in vec3 vertexNormal;
            in vec3 fragPosition;
            in mat3 tbn;

            uniform sampler2D albedoTexture;
            uniform sampler2D lightmapTexture;
            uniform sampler2D normalTexture;
            uniform sampler2D specularTexture;

            uniform vec3 {cameraPosition};
            

            // ui ting
            uniform bool {uvTesting};
            uniform bool {useTexture};
            uniform bool {useBlinn};
            uniform bool {usingCellShading};  
            uniform bool {usingRimLight};
            
            uniform vec3 {lightColor};
            uniform vec3 {objectColor};

      
            vec3 lightPosition = vec3 (0, 2, 1);
            vec3 rimColor = vec3 (1, 1, 1);
          
            float ambientStrength = 0.1f;

            void main() 
            {{
      
            vec3 normal;


            if ({useTexture}) 

            {{
            //hent normal i range 0 - 1
            normal = texture(normalTexture, uv).rgb;
            // lav den til -1 til 1
            normal = normalize(normal * 2.0 - 1.0); 

            // Virker ikke, fucker alt op hvis den bliver sat til
             //normal = normal * tbn;
            }} 
            else 
            {{
                normal = vertexNormal;
            }}  



            //base color
            vec4 pixel = vec4({objectColor},1);

            if ({useTexture}) 
            {{pixel = vec4 (texture(albedoTexture, uv).rgb, 1);}}
            

            // ambient light
            vec4 ambient;
            vec4 LightTexPixel = texture(lightmapTexture, uv);
            if ({useTexture})
            {{
                ambient  = vec4((ambientStrength * lightColor), 1) * LightTexPixel ;    
            }}
           
            else 
            {{
               ambient = vec4(ambientStrength * lightColor, 1);
            }}
  
            

            // diffuse light
             
            vec3 normalizedNormal = normalize(normal);
            vec3 lightDir = normalize(lightPosition - fragPosition); 
            float diff = max(dot(normalizedNormal, lightDir), 0.0);
            vec4 diffuse = vec4(diff * lightColor, 1);
            


            // specular
            float specularStrength = 0.3;
            vec3 viewDir = normalize({cameraPosition} - fragPosition);

            float specValue = 0.0;
            // blinn shader
            if ({useBlinn})
            {{
                vec3 halfwayVector = (lightDir + viewDir) / length(lightDir + viewDir);
                specValue = pow(max(dot(normalizedNormal, halfwayVector), 0.0), 4);

            }}
            else 
            {{
                vec3 reflectDir = reflect(-lightDir, normalizedNormal); 
                specValue = pow(max(dot(viewDir, reflectDir), 0.0), 32);
            }}


            vec4 specular;
            vec3 specularTexPixel = texture(specularTexture, uv).rgb;
            if ({useTexture})
            {{
                
                specular = vec4 ((specularStrength * specValue * lightColor) * specularTexPixel, 1)   ;  
            }}
           
            else 
            {{
                specular = vec4 (specularStrength * specValue * lightColor , 1); 
            }}
            

            float rimLight = 0;
            if (usingRimLight) 
            {{
                rimLight = 1.0f - max(dot(viewDir, normalizedNormal), 0.0f);
                rimLight = smoothstep(0.85, 1, rimLight); 
            }}


            // afsluttende udregning
            FragColor = (ambient + diffuse + specular) * pixel + (rimLight * 0.8);
     
            

            





            if ({usingCellShading})
            {{
                vec4 color;
                float intensity = dot(lightDir, normal);
                if (intensity > 0.95)
                color = vec4(1.0,0.5,0.5,1.0);
                else if (intensity > 0.5)
                color = vec4(0.6,0.3,0.3,1.0);
                else
                if (intensity > 0.25)
                color = vec4(0.4,0.2,0.2,1.0);
                else
                color = vec4(0.2,0.1,0.1,1.0);
                FragColor = color;
                
            }}

            // from below here is testing area
                
            // uv test
            if ({uvTesting})
            {{ FragColor = vec4(uv, 0.0, 1.0); }}

            // afslutning
            }}";
        }


        // UI inputs

        string uvTesting = "uvTesting";
        public void SetUVTest(bool state)
        {
            SetUniformBool("uvTesting", state);
        }

        string useTexture = "useTexture";
        public void SetUsingTexture(bool state)
        {
            SetUniformBool(useTexture, state);
        }

        string useBlinn = "useBlinn";
        public void SetUsingBlinn (bool state)
        {
            SetUniformBool(useBlinn, state);
        }

        string cameraPosition = "cameraPosition";
        public void SetCameraPosition(Vector3 position)
        {
            SetUniformVector3(cameraPosition, position);
        }

        string usingCellShading = "toonShaderOn";
        public void UsingCellShader(bool state)
        {
            SetUniformBool(usingCellShading, state);
        }

        string usingRimLight = "usingRimLight";
        public void UsingRimLight(bool state)
        {
            SetUniformBool(usingRimLight, state);
        }

        string lightColor = "lightColor";
        public void SetLightColor (Vector3 color)
        {
            SetUniformVector3(lightColor, color);
        }

        string objectColor = "objectColor";
        public void SetObjectColor (Vector3 color)
        {
            SetUniformVector3(objectColor, color);
        }


        public void Dispose()
        {
            shaders.Remove(this);
        }



        public void UpdateUniformValuesForRender()
        {
            // det her regnes bagfra?? TODO Spørg Søren
            _projectionViewModel.Matrix = _model.Matrix * _view.Matrix * _projection.Matrix;
        }




        /// <summary>
        /// opdater alle views i alle de forskellige shaders på en gang
        /// </summary>
        /// <param name="view"></param
        /// 
        public static void UpdateView(Matrix4 view)
        {
            foreach (Shader shader in shaders)
            {
                shader.View = view;
            }
        }

        public static void UpdateCameraPosition(Vector3 cameraPosition)
        {
            foreach (Shader shader in shaders)
            {
                shader.SetCameraPosition(cameraPosition);
            }
        }

        protected void SetUniformBool(string uniformName, bool boolStatus)
        {
            // ikke nødvendig da den bliver kaldt i figurens opdatering der allerede kalder UseProgram
            //GL.UseProgram(_shaderProgramHandle); //
            Use();

            int location = GL.GetUniformLocation(_shaderProgramHandle, uniformName);
            if (location == -1)
                throw new Exception($"Uniform '{uniformName}' not found.");

            GL.Uniform1(location, boolStatus ? 1 : 0);
        }

        protected void SetUniformVector3(string uniformName, Vector3 vector)
        {
            // ikke nødvendig da den bliver kaldt i figurens opdatering der allerede kalder UseProgram
            //GL.UseProgram(_shaderProgramHandle); //

            int location = GL.GetUniformLocation(_shaderProgramHandle, uniformName);
            if (location == -1)
                throw new Exception($"Uniform '{uniformName}' not found.");

            GL.Uniform3(location, vector);
        }

        protected void SetUniformVector4(string uniformName, Vector4 vector)
        {
            // ikke nødvendig da den bliver kaldt i figurens opdatering der allerede kalder UseProgram
            //GL.UseProgram(_shaderProgramHandle); //

            int location = GL.GetUniformLocation(_shaderProgramHandle, uniformName);
            if (location == -1)
                throw new Exception($"Uniform '{uniformName}' not found.");

            GL.Uniform4(location, vector);
        }


        public static void UpdateProjection(Matrix4 projection)
        {
            foreach (Shader shader in shaders)
            {
                shader.Projection = projection;
            }
        }

        public static void UpdateModelSpace(Matrix4 modelSpace)
        {
            foreach (Shader shader in shaders)
            {
                shader.Model = modelSpace;

                //TODO fix normals matrix / modelView
                 shader.CalculateModelView();

            }
        }



        public void CalculateModelView()
        {
            ModelView = _view.Matrix * _model.Matrix;
        }

   

        public enum textureUnits { ALBEDO, LIGHTMAP, SPECULARMAP, NORMALMAP }

        public void SetTextureUniform(textureUnits textureUnit)
        {
            string uniformName = string.Empty;
            switch (textureUnit)
            {
                case textureUnits.ALBEDO:
                    uniformName = "albedoTexture";
                    break;

                case textureUnits.LIGHTMAP:
                    uniformName = "lightmapTexture";
                    break;

                case textureUnits.SPECULARMAP:
                    uniformName = "specularTexture";
                    break;

                case textureUnits.NORMALMAP:
                    uniformName = "normalTexture";
                    break;

            }

            // overflødig, bliver kaldt fra figurens update
            //GL.UseProgram(_shaderProgramHandle);
            int location = GL.GetUniformLocation(_shaderProgramHandle, uniformName);
            if (location == -1)
                throw new Exception($"Uniform '{uniformName}' not found.");

            GL.Uniform1(location, (int)textureUnit);
        }

        public void Use()
        {
            GL.UseProgram(_shaderProgramHandle);
        }

        protected void DetachAndDeleteShaderParts()
        {
            foreach (ShaderPart part in parts)
            {
                GL.DetachShader(_shaderProgramHandle, part.shaderPartHandle);
                GL.DeleteShader(part.shaderPartHandle);
            }
        }


        protected void SetUpShaderParts()
        {
            foreach (ShaderPart part in parts)
            {
                int shaderToSetUp = GL.CreateShader(part.type);
                GL.ShaderSource(shaderToSetUp, part.shaderSource);

                GL.CompileShader(shaderToSetUp);
                GL.GetShader(shaderToSetUp, ShaderParameter.CompileStatus, out var code);
                if (code != 1)
                {
                    var infoLog = GL.GetShaderInfoLog(shaderToSetUp);
                    throw new Exception($"Fejl i shader ({shaderToSetUp}).\n\n{infoLog}");
                }

                GL.AttachShader(_shaderProgramHandle, shaderToSetUp);
            }
        }
    }

    public struct ShaderPart
    {
        public ShaderType type;
        public int shaderPartHandle;
        public string shaderSource;

        public ShaderPart(ShaderType type, string shaderSource)
        {
            this.type = type;
            this.shaderSource = shaderSource;
        }
    }


}
