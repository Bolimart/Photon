namespace Photon;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;


public class Shader : IDisposable
{
    #region - Fields and Properties

    private int _handle; // Since shaders are GPU objects, they're represented by an integer (handle)
    private bool _disposedValue = false;

    public int Handle { get => _handle; }

    #endregion
    #region - Constructor
    public Shader(string vertexPath, string fragmentPath)
    {
        // Load the shader source code
        string vertexShaderSource = File.ReadAllText(MakeShaderPath(false, vertexPath));
        string fragmentShaderSource = File.ReadAllText(MakeShaderPath(true, fragmentPath));
        
        // Load the shader source code into the GPU
        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexShaderSource);
        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentShaderSource);
        
        // Compile the vertex shader
        GL.CompileShader(vertexShader);
        GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out int success);
        string infoLog = GL.GetShaderInfoLog(vertexShader);
        Console.WriteLine(infoLog);
        if (success == 0) // If the compilation failed.
        {
            throw new Exception();
        }
        
        // Compile the fragment shader
        GL.CompileShader(fragmentShader);
        GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out success);
        infoLog = GL.GetShaderInfoLog(fragmentShader);
        Console.WriteLine(infoLog);
        if (success == 0) // If the compilation failed.
        {
            throw new Exception();
        }
        
        // link the fragment and vertex shader into a program
        _handle = GL.CreateProgram();
        
        GL.AttachShader(_handle, vertexShader);
        GL.AttachShader(_handle, fragmentShader);
        
        GL.LinkProgram(_handle);

        GL.GetProgram(_handle, GetProgramParameterName.LinkStatus, out success);
        if (success == 0)
        {
            infoLog = GL.GetProgramInfoLog(_handle);
            Console.WriteLine(infoLog);
        }
        
        // Memory cleanup
        GL.DetachShader(_handle, vertexShader);
        GL.DetachShader(_handle, fragmentShader);
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
        
        GL.GetProgram(_handle, GetProgramParameterName.ActiveUniforms, out int count);
        for (int i = 0; i < count; i++)
        {
            string name = GL.GetActiveUniform(_handle, i, out int size, out ActiveUniformType type);
            Console.WriteLine($"uniform {i}: {name} ({type})");
        }
    }
    #endregion
    #region - Methods
    
    private string MakeShaderPath(bool isFragment, string fileName)
        => Path.Combine(AppContext.BaseDirectory, "Shaders", isFragment ? "Fragment" : "Vertex", fileName);
    
    public void Use()
    {
        GL.UseProgram(_handle);
    }
    
    public int GetAttribLocation(string attribName)
    {
        return GL.GetAttribLocation(_handle, attribName);
    }
    
    public void SetInt(string name, int value)
    {
        int location = GL.GetUniformLocation(_handle, name);
        if (location == -1) Console.WriteLine($"Uniform '{name}' introuvable");
        Use();
        GL.Uniform1(location, value);
    }

    public void SetMatrix4(string name, Matrix4 value)
    {
        int location = GL.GetUniformLocation(_handle, name);
        if (location == -1) Console.WriteLine($"Uniform '{name}' introuvable");
        Use();
        GL.UniformMatrix4(location,true, ref value);
    }
    
    public void SetFloat(string name, float value)
    {
        int location = GL.GetUniformLocation(_handle, name);
        Use();
        GL.Uniform1(location, value);
    }

    public void SetVector3(string name, Vector3 value)
    {
        int location = GL.GetUniformLocation(_handle, name);
        Use();
        GL.Uniform3(location, value);
    }
    
    #endregion
    #region - IDisposale Handling
    
    /// <summary>
    /// The function that allows the IDisposable interface to work.
    /// It deletes the texture from the VRAM.
    /// </summary>
    /// <param name="disposing">If the object is already being disposed</param>
    protected virtual void Dispose(bool disposing) // protected function that delete the shader from VRAM to prevent leaking
    {
        if (!_disposedValue)
        {
            GL.DeleteTexture(_handle);
            _disposedValue = true;
        }
    }
    
    /// <summary>
    /// At the end of the program, if the shader haven't been disposed, inform the developper.
    /// </summary>
    ~Shader() // Destructor to prevent GPU resource leak
    {
        if (_disposedValue == false)
        {
            Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
        }
    }
    
    /// <summary>
    /// The function to call to dispose of the texture
    /// Is mandatory to call when you end the program to not have any GPU resource leak.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion
}