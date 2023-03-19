namespace ProtaBot_v1._0
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var protaBot = new ProtaBot();
            protaBot.RunAsync().GetAwaiter().GetResult();

        }
    }
}
