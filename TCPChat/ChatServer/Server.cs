using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatServer;

public class Server
{
    private const string ClientsFile = "clients.json";
    private TcpListener _tcpListener = new TcpListener(IPAddress.Any, 8000);
    private Dictionary<string, Client> _clients = new Dictionary<string, Client>();
    private Dictionary<string, string> _registeredClients = new Dictionary<string, string>();

    public IEnumerable<Client> Clients => _clients.Values;

    public Server()
    {
        ClientsLoad();
    }

    private void ClientsLoad()
    {
        if (File.Exists(ClientsFile))
        {
            string json = File.ReadAllText(ClientsFile);
            _registeredClients = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
        }
        else
        {
            SaveClients();
        }
    }

    private void SaveClients()
    {
        string json = JsonSerializer.Serialize(_registeredClients);
        File.WriteAllText(ClientsFile, json);
    }

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
        return _registeredClients.ContainsKey(userName);
    }

    public bool ConfirmPassword(string userName, string password)
    {
        return _registeredClients.TryGetValue(userName, out var storedPassword) && storedPassword == password;
    }

    public void RegisterClient(string userName, string password)
    {
        _registeredClients[userName] = password;
        SaveClients();
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