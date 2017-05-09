using namevk;

namespace ypravlenie
{
    class Program
    {
        static void Main(string[] args)
        {
            vk cmd = new vk(args);
            cmd.mainLoop();
        }
    }
}