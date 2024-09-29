using ChatServer;

class Program
{
    static async Task Main()
    {
        var server = new Server();
        await server.ProcessAsync();
    }
}
