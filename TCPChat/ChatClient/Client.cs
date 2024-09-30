using System.Net.Sockets;
using System.Text.Json;

namespace ChatClient;

public class Client
{
    private const string ServerPath = "serverData.json";
    private const string UserPath = "userData.json";

    public async Task ConnectServerAsync()
    {
        string? ip = null;
        int port = 0;
        
        if (File.Exists(ServerPath))
        {
            try
            {
                var serverData = JsonSerializer.Deserialize<ServerData>(File.ReadAllText(ServerPath));
                if (serverData != null)
                {
                    ip = serverData.IP;
                    port = serverData.Port;
                    Console.WriteLine($"Connecting to saved server...");
                    if (await CheckConnectAsync(ip, port))
                    {
                        Console.WriteLine($"Successful connection to the server! \n(IP-address = \"{ip}\", Port = \"{port}\") \n");
                        await RunAsync(ip, port);
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Unfortunately, were unable to connect to the saved server :с");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while reading server data: {ex.Message}");
            }
        }
        
        while (true)
        {
            Console.WriteLine("Enter the server IP-address:");
            ip = Console.ReadLine();

            Console.WriteLine("Enter server port:");
            string? connectionPort = Console.ReadLine();

            if (int.TryParse(connectionPort, out port))
            {
                bool connected = await CheckConnectAsync(ip, port);

                if (connected)
                {
                    Console.WriteLine("Successful connection to the server! c:");
                    SaveServerData(ip, port);
                    
                    await RunAsync(ip, port);
                    return;
                }
                else
                {
                    Console.WriteLine("Failed to connect to the server! :с \nIf you want to exit, type \"stop\". \nIf you want to try connecting again, type \"more\".");
                    string? gg = Console.ReadLine();
                    if (gg?.ToLower() == "stop")
                    {
                        return;
                    }
                    else if (gg?.ToLower() == "more")
                    {
                        continue;
                    }
                }
            }
            else
            {
                Console.WriteLine("Invalid IP address or port! Be more attentive =з");
            }
        }
    }

    private async Task<bool> CheckConnectAsync(string? host, int port)
    {
        using var client = new TcpClient();
        try
        {
            var connectTask = client.ConnectAsync(host, port);
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10));

            var completedTask = await Task.WhenAny(connectTask, timeoutTask);
            if (completedTask == connectTask && client.Connected)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task RunAsync(string host, int port)
    {
        using var client = new TcpClient();
        try
        {
            await client.ConnectAsync(host, port);
            var stream = client.GetStream();
            var reader = new StreamReader(stream);
            var writer = new StreamWriter(stream);

            Console.WriteLine("Enter your name:");
            string userName = Console.ReadLine();
            await writer.WriteLineAsync(userName);
            await writer.FlushAsync();

            string password;
            while (true)
            {
                Console.WriteLine("Enter your password:");
                password = Console.ReadLine();
                if (string.IsNullOrEmpty(password)) continue;

                await writer.WriteLineAsync(password);
                await writer.FlushAsync();
                string? passwordResponse = await reader.ReadLineAsync();

                if (passwordResponse == "Welcome")
                {
                    Console.WriteLine($"Welcome to the chat, {userName}!");
                    SaveUserData(userName, password);
                    break;
                }
                else if (passwordResponse == "IncorrectPassword")
                {
                    Console.WriteLine("Incorrect password!");
                }
            }

            var receiveTask = ReceiveMessageAsync(reader);
            var sendTask = SendMessageAsync(writer, userName);
            await Task.WhenAny(receiveTask, sendTask);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            client.Close();
        }
    }

    private async Task SendMessageAsync(StreamWriter writer, string? userName)
    {
        Console.WriteLine("Enter your message:");
        while (true)
        {
            string? message = Console.ReadLine();
            if (string.IsNullOrEmpty(message)) continue;
            await writer.WriteLineAsync(message);
            await writer.FlushAsync();
        }
    }

    private async Task ReceiveMessageAsync(StreamReader reader)
    {
        while (true)
        {
            try
            {
                string? message = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(message)) continue;
                Console.WriteLine(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                break;
            }
        }
    }
    
    private void SaveServerData(string ip, int port)
    {
        var serverData = new ServerData
        {
            IP = ip,
            Port = port
        };
        string json = JsonSerializer.Serialize(serverData);
        File.WriteAllText(ServerPath, json);
    }
    
    private void SaveUserData(string userName, string password)
    {
        var userData = new UserData
        {
            UserName = userName,
            Password = password
        };
        string json = JsonSerializer.Serialize(userData);
        File.WriteAllText(UserPath, json);
    }
}

public class ServerData
{
    public string IP { get; set; }
    public int Port { get; set; }
}

public class UserData
{
    public string UserName { get; set; }
    public string Password { get; set; }
}