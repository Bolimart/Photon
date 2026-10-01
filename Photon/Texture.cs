namespace Photon;

using System;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

public class Texture : IDisposable
{
    public int Handle;
    private bool _disposedValue = false;

    public Texture(string fileName)
    {
        Handle = GL.GenTexture();
        
        // stb_image loads from the top-left pixel, whereas OpenGL loads from the bottom-left, causing the texture to be flipped vertically.
        // This will correct that, making the texture display properly.
        StbImage.stbi_set_flip_vertically_on_load(1);
        
        // Load the image
        string path = Path.Combine(AppContext.BaseDirectory, "Asset", "Texture", fileName);
        ImageResult image = ImageResult.FromStream(File.OpenRead(path), ColorComponents.RedGreenBlueAlpha);
        GL.BindTexture(TextureTarget.Texture2D, Handle);
        
        // Upload the texture to the GPU
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
        
        // Generate the mipmap
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
    }

    public void Use(TextureUnit unit = TextureUnit.Texture0)
    {
        GL.ActiveTexture(unit);
        GL.BindTexture(TextureTarget.Texture2D, Handle);
    }
    
    #region - IDisposable Handling
    protected virtual void Dispose(bool disposing) // protected function that delete the shader from VRAM to prevent leaking
    {
        if (!_disposedValue)
        {
            GL.DeleteProgram(Handle);
            _disposedValue = true;
        }
    }
    
    ~Texture() // Destructor to prevent GPU resource leak
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