namespace Photon;
using OpenTK.Graphics.OpenGL4;

public class Shader : IDisposable
{
    public int Handle; // Since shaders are GPU objects, they're represented by an integer (handle)
    private bool _disposedValue = false;

    private string MakeShaderPath(bool isFragment, string fileName)
        => Path.Combine(AppContext.BaseDirectory, "Shaders", isFragment ? "Fragment" : "Vertex", fileName);

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
        if (success == 0) // If the compilation failed.
        {
            string infoLog = GL.GetShaderInfoLog(vertexShader);
            Console.WriteLine(infoLog);
        }
        
        // Compile the fragment shader
        GL.CompileShader(fragmentShader);
        GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out success);
        if (success == 0) // If the compilation failed.
        {
            string infoLog = GL.GetShaderInfoLog(fragmentShader);
            Console.WriteLine(infoLog);
        }
        
        // link the fragment and vertex shader into a program
        Handle = GL.CreateProgram();
        
        GL.AttachShader(Handle, vertexShader);
        GL.AttachShader(Handle, fragmentShader);
        
        GL.LinkProgram(Handle);

        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out success);
        if (success == 0)
        {
            string infoLog = GL.GetProgramInfoLog(Handle);
            Console.WriteLine(infoLog);
        }
        
        // Memory cleanup
        GL.DetachShader(Handle, vertexShader);
        GL.DetachShader(Handle, fragmentShader);
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
    }

    public void Use()
    {
        GL.UseProgram(Handle);
    }
    
    public int GetAttribLocation(string attribName)
    {
        return GL.GetAttribLocation(Handle, attribName);
    }
    
    public void SetInt(string name, int value)
    {
        int location = GL.GetUniformLocation(Handle, name);
        Use();
        GL.Uniform1(location, value);
    }

    #region - IDisposavleHandling
    protected virtual void Dispose(bool disposing) // protected function that delete the shader from VRAM to prevent leaking
    {
        if (!_disposedValue)
        {
            GL.DeleteProgram(Handle);
            _disposedValue = true;
        }
    }

    ~Shader() // Destructor to prevent GPU resource leak
    {
        if (_disposedValue == false)
        {
            Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
        }
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion
}