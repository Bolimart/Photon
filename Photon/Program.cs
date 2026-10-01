namespace Photon;

internal class Program
{
    public static void Main(string[] args)
    {
        using (Game game = new Game(800, 600, "tutorial"))
        {
            game.Run();
        }
    }
}