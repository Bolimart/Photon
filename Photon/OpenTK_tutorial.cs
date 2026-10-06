using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Photon
{
    public class Game(int width, int height, string title) : GameWindow(GameWindowSettings.Default,
        new NativeWindowSettings() { ClientSize = (width, height), Title = title })
    {
        #region - Fields & Propreties
        
        float[] _vertices = {
            //Position          Texture coordinates
             0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // top right
             0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
            -0.5f,  0.5f, 0.0f, 0.0f, 1.0f, // top left
        };

        // Why is vertices an array of float and not an array of coordinates ?
        // Because the GPU is designed for reading long string of simple data
        
        private uint[] _indices = {
            0, 1, 3,
            1, 2, 3
        };
        
        private int _vertexBufferObject; // Store the data (pos) of the vertices
        private int _vertexArrayObject; // Store the configuration of the Vertex Format
        private int _elementBufferObject; // Strore the indexes of each vertex of each triangle
        private Stopwatch _timer = new Stopwatch();
        
        private Shader _shader;
        private Texture _textureCrate;
        private Texture _textureFace;
        
        #endregion
        #region - Methods override

        /// <summary>
        /// Called at the initialization of the window
        /// </summary>
        protected override void OnLoad()
        {
            Console.WriteLine(GL.GetString(StringName.Version));
            base.OnLoad();
            
            // The color used by the Color buffer when the window is cleared
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            
            _timer.Start();
            
            _vertexBufferObject = GL.GenBuffer(); // Create a Buffer object
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject); // Set the buffer "type" to ArrayBuffer (kinda)
            // From that point on any buffer calls we make (on the BufferTarget.ArrayBuffer target) will be used to configure the currently bound buffer
            
            // Configure the buffer bound to ArrayBuffer
            GL.BufferData(BufferTarget.ArrayBuffer, 
                _vertices.Length * sizeof(float) /*Set the size of the buffer (number of vertices * size of a float)*/,
                _vertices, BufferUsageHint.StaticDraw);
            
            // Load the shader
            _shader = new Shader("shader.vert", "shader.frag");
            
            // Load the texture and modify settings
            _textureCrate = new Texture("container.jpg");
            _textureFace = new Texture("awesomeface.png");
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat); // Set the Texture Wrap Parameters
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest); // Set the Texture filtering parameters
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            
            // Create the VAO and configure it
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float)/*24*/, 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float)/*24*/, 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
            
            // Create the EBO
            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);
            
            // Set the uniforms:
            _shader.SetInt("texture0", 0);
            _shader.SetInt("texture1", 1);
        }
        
        /// <summary>
        /// Called when a new frame is being rendered.
        /// Where the render code goes.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            
            Matrix4 transform = GetTransformMatrix(new Vector3(0, 0, 0),new Vector3(1 + (float)Math.Cos(_timer.Elapsed.TotalSeconds * 2f) * 0.3f, 1, 1), (float)_timer.Elapsed.TotalSeconds * 50);
            _shader.SetMatrix4("transfrom", transform);
            
            // Used to clear the screen.
            GL.Clear(ClearBufferMask.ColorBufferBit);
            
            // The render code
            _shader.Use();
            _textureCrate.Use(TextureUnit.Texture0);
            _textureFace.Use(TextureUnit.Texture1);
            
            // Create the uniforms:
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
            
            // Swap the old frame with the newly rendered frame.
            SwapBuffers();
        }
        
        /// <summary>
        /// Called when the render has ended and is ready to update the screen.
        /// Where the logic code goes.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            
            if (KeyboardState.IsKeyDown(Keys.Escape)) Close();
        }
        
        /// <summary>
        /// called when the window (and therefore the buffer) is resized.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnFramebufferResize(FramebufferResizeEventArgs args)
        {
            base.OnFramebufferResize(args);
            
            GL.Viewport(0, 0, args.Width, args.Height);
        }
        
        /// <summary>
        /// Called at the closing of the window
        /// </summary>
        protected override void OnUnload()
        {
            base.OnUnload();
            
            _shader.Dispose();
            _textureCrate.Dispose();
            _textureFace.Dispose();
            
            // Not required since all data is automatically freed at closure, but can be used to free VRAM at runtime
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteBuffer(_elementBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Generate a transform matrix.
        /// </summary>
        /// <param name="trans">The transform vector.</param>
        /// <param name="scale">The scale vector.</param>
        /// <param name="rot">Currently the rot is only on the Z axis.</param>
        /// <returns>The transform matrix.</returns>
        private Matrix4 GetTransformMatrix(Vector3 trans, Vector3 scale, float rot)
        {
            Matrix4 mRot = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rot));
            Matrix4 mScale = Matrix4.CreateScale(scale);
            Matrix4 mTrans = Matrix4.CreateTranslation(trans);
            return mRot * mScale * mTrans;
        }

        #endregion
    }
}