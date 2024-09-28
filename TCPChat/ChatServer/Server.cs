using System.Net;
using System.Net.Sockets;

namespace ChatServer;

public class Server
{
    private TcpListener _tcpListener = new TcpListener(IPAddress.Any, 8000);
    private Dictionary<string, Client> _clients = new Dictionary<string, Client>();
    public IEnumerable<Client> Clients => _clients.Values;

    public void AddClient(Client client)
    {
        _clients.Add(client.Id, client);
    }

    public Client? GetClientName(string userName)
    {
        return _clients.Values.FirstOrDefault(c => c.UserName == userName);
    }

    public bool UserNameTaken(string userName)
    {
        return _clients.Values.Any(c => c.UserName == userName);
    }

    public async Task BroadCastMessage(string message, string id)
    {
        foreach (var (_, client) in _clients)
        {
            if (client.Id != id)
            {
                await client.Writer.WriteLineAsync(message);
                await client.Writer.FlushAsync();
            }
        }
    }

    public void RemoveClient(string id)
    {
        if (!_clients.Remove(id, out var client))
            return;
        client.Close();
    }

    public async Task ProcessAsync()
    {
        _tcpListener.Start();
        Console.WriteLine("Server online.");
        while (true)
        {
            TcpClient cl = await _tcpListener.AcceptTcpClientAsync();
            Client client = new Client(cl, this);
            Task.Run(client.ProcessAsync);
        }
    }
}