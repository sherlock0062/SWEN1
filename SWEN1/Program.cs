using System.Net;
using System.Net.Sockets;
using System.Text;
using Npgsql;

using SWEN1.Http;
using SWEN1.Routing;

namespace SWEN1;

public static class Program
{
    private const string ConnectionString = "Host=localhost;Username=postgres;Password=postgres;Database=mtcg";
    private static TcpListener? _listener;
    private static bool _running = false;
    private static readonly Queue<string> BattleQueue = new();
    private static readonly object BattleQueueLock = new();

    public static async Task Main()
    {
        Console.WriteLine($"{DateTime.Now}: Starting MTCG Server");
        try
        {
            using var conn = new NpgsqlConnection(ConnectionString);
            conn.Open();
            Console.WriteLine($"{DateTime.Now}: Database connection successful.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{DateTime.Now}: Failed to connect to database: {ex.Message}");
            return;
        }

        int port = 10001;
        bool started = false;
        while (!started && port <= 65535)
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, port);
                _listener.Start();
                started = true;
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
            {
                port++;
            }
        }

        if (!started)
        {
            Console.WriteLine($"{DateTime.Now}: No available ports found.");
            return;
        }

        Console.WriteLine(@"
 /_/\  
( o.o ) 
 > ^ <
");

        _running = true;
        Console.WriteLine($"{DateTime.Now}: MTCG Server started on port {port}. Press Enter to stop.");
        var serverTask = AcceptLoop();
        Console.ReadLine();
        _running = false;
        _listener.Stop();
        await serverTask;
        Console.WriteLine($"{DateTime.Now}: Server stopped.");
    }

    private static async Task AcceptLoop()
    {
        while (_running)
        {
            try
            {
                var client = await _listener!.AcceptTcpClientAsync();
                Console.WriteLine($"{DateTime.Now}: New client connected.");
                _ = Task.Run(() => HandleClientAsync(client));
            }
            catch (Exception ex) when (_running)
            {
                Console.WriteLine($"{DateTime.Now}: Accept error: {ex.Message}");
            }
        }
    }

    private static async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            using (client)
            using (var stream = client.GetStream())
            {
                var buffer = new byte[8192];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead <= 0)
                {
                    Console.WriteLine($"{DateTime.Now}: Received empty request.");
                    return;
                }

                var requestString = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"{DateTime.Now}: Received request:\n{requestString}");
                var request = HttpServer.ParseHttpRequest(requestString);
                var response = Router.HandleRequest(request, ConnectionString, BattleQueue, BattleQueueLock);
                var responseBytes = Encoding.ASCII.GetBytes(response);
                await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                Console.WriteLine($"{DateTime.Now}: Response sent.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{DateTime.Now}: Error handling client: {ex.Message}");
        }
    }
}