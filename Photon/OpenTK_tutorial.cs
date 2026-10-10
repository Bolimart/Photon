using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Photon
{
    public class Game(int width, int height, string title) : GameWindow(GameWindowSettings.Default,
        new NativeWindowSettings() { ClientSize = (width, height), Title = title })
    {
        #region - Fields & Propreties
        
        private CubeData[] _cubes;
        
        float[] _vertices = {
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
            0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,

            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
            0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,

            -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

            0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            0.5f, -0.5f, -0.5f,  1.0f, 1.0f,
            0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
            0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,

            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
            0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
            0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,  0.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f
        };

        // Why is vertices an array of float and not an array of coordinates ?
        // Because the GPU is designed for reading long string of simple data
        
        private int _vertexBufferObject; // Store the data (pos) of the vertices
        private int _vertexArrayObject; // Store the configuration of the Vertex Format
        private int _elementBufferObject; // Strore the indexes of each vertex of each triangle
        private Stopwatch _timer = new Stopwatch();
        
        // Camera
        private Camera _camera = new Camera(new Vector3(0.0f, 0.0f,  10.0f));
        private float _speed = 15f;
        private float _fov = 45f;
        // Mouse
        private float _sensibility = 0.3f;
        
        private Shader _shader;
        private Texture _textureCrate;
        private Texture _textureFace;
        private int _width = width;
        private int _height = height;

        #endregion
        #region - Methods override

        /// <summary>
        /// Called at the initialization of the window
        /// </summary>
        protected override void OnLoad()
        {
            Console.WriteLine(GL.GetString(StringName.Version));
            base.OnLoad();
            Console.WriteLine($"Context: {Context != null}, current: {Context?.IsCurrent}");
            Console.WriteLine($"GL error: {GL.GetError()}");
            Console.WriteLine($"Version: [{GL.GetString(StringName.Version)}]");
            
            // The color used by the Color buffer when the window is cleared
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            // Enable Z-Testing
            GL.Enable(EnableCap.DepthTest);
            
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
            _shader.SetVector3("fogColor", new Vector3(0.0f, 0.0f, 0.0f)); // même valeur que GL.ClearColor
            _shader.SetFloat("fogStart", 5f);
            _shader.SetFloat("fogEnd", 120f);
            
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
            GL.BufferData(BufferTarget.ElementArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
            
            // Set the uniforms:
            _shader.SetInt("texture0", 0);
            _shader.SetInt("texture1", 1);
            
            // Load the scene:
            var rng = new Random(42); // fixed seed
            _cubes = new CubeData[4800];

            for (int i = 0; i < _cubes.Length; i++)
            {
                Vector3 axis = new Vector3(
                    (float)rng.NextDouble() * 2 - 1,
                    (float)rng.NextDouble() * 2 - 1,
                    (float)rng.NextDouble() * 2 - 1);
                if (axis.LengthSquared < 0.01f) axis = Vector3.UnitY; // évite un axe nul

                _cubes[i] = new CubeData
                {
                    Position = new Vector3(
                        (float)rng.NextDouble() * 240 - 120,
                        (float)rng.NextDouble() * 240 - 120,
                        (float)rng.NextDouble() * 240 - 120),
                    Axis  = Vector3.Normalize(axis),
                    Speed = 0.3f + (float)rng.NextDouble() * 2.2f,
                    Phase = (float)rng.NextDouble() * MathHelper.TwoPi,
                    Scale = 0.5f + (float)rng.NextDouble() * 1.8f
                };
            }
        }
        
        /// <summary>
        /// Called when a new frame is being rendered.
        /// Where the render code goes.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            
            // Used to clear the screen.
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            // Load Ressources
            _shader.Use();
            _textureCrate.Use(TextureUnit.Texture0);
            _textureFace.Use(TextureUnit.Texture1);
            GL.BindVertexArray(_vertexArrayObject);
            
            // The view matrix is just translating and rotating the whole world
            Matrix4 view = _camera.GetView();
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_fov), (float)_width / _height, 0.1f, 120.0f);
            _shader.SetMatrix4("view", view);
            _shader.SetMatrix4("projection", projection);
            
            float t = (float)_timer.Elapsed.TotalSeconds;
            
            foreach (var cube in _cubes)
            {
                float angle = cube.Phase + cube.Speed * t;
                
                // Convention ligne : échelle -> rotation -> translation
                Matrix4 model = Matrix4.CreateScale(cube.Scale)
                                * Matrix4.CreateFromAxisAngle(cube.Axis, angle)
                                * Matrix4.CreateTranslation(cube.Position);
                
                _shader.SetMatrix4("model", model);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            }
            
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
            
            if (!IsFocused) // check to see if the window is focused
            {
                return;
            }
            if (CursorState != CursorState.Grabbed) CursorState = CursorState.Grabbed;
            
            KeyboardState input = KeyboardState;

            #region - Displacement Input

            if (input.IsKeyDown(Keys.W))
            {
                _camera.Position += _camera.Front * _speed * (float)args.Time; //Forward 
            }

            if (input.IsKeyDown(Keys.S))
            {
                _camera.Position -= _camera.Front * _speed * (float)args.Time; //Backwards
            }

            if (input.IsKeyDown(Keys.A))
            {
                _camera.Position -= _camera.Right * _speed * (float)args.Time; //Left
            }

            if (input.IsKeyDown(Keys.D))
            {
                _camera.Position += _camera.Right * _speed * (float)args.Time; //Right
            }

            if (input.IsKeyDown(Keys.E))
            {
                _camera.Position += _camera.Up * _speed * (float)args.Time; //Up 
            }

            if (input.IsKeyDown(Keys.Q))
            {
                _camera.Position -= _camera.Up * _speed * (float)args.Time; //Down
            }

            #endregion
            #region - Rotation Input

            const float sensitivity = 0.1f;
            var mouse = MouseState;
            _camera.Yaw   += mouse.Delta.X * sensitivity;
            _camera.Pitch -= mouse.Delta.Y * sensitivity; // screen Y goes down

            #endregion
            if (KeyboardState.IsKeyDown(Keys.Escape)) Close();
        }
        
        /// <summary>
        /// called when the window (and therefore the buffer) is resized.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnFramebufferResize(FramebufferResizeEventArgs args)
        {
            base.OnFramebufferResize(args);

            _width = args.Width;
            _height = args.Height;
            
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
        
        
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            var value = _fov - e.OffsetY;
            
            if (value >= 45.0f)
            {
                _fov = 45.0f;
            }
            else if (value <= 1.0f)
            {
                _fov = 1.0f;
            }
            else
            {
                _fov -= e.OffsetY;
            }
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
        private Matrix4 GetTransformMatrix(Vector3 trans, Vector3 scale, Vector3 rot)
        {
            Matrix4 mRot = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rot.Z));
            mRot *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rot.X));
            mRot *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rot.Y));
            Matrix4 mScale = Matrix4.CreateScale(scale);
            Matrix4 mTrans = Matrix4.CreateTranslation(trans);
            return mRot * mScale * mTrans;
        }

        #endregion
    }
}