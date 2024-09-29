using ChatClient;

class Program
{
    static async Task Main()
    {
        Client c = new Client();
        await c.ConnectServerAsync();
    }
}