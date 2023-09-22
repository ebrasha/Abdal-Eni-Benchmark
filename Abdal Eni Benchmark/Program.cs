using Abdal_Eni_Benchmark;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    private static int successfulRequests = 0;
    private static int failedRequests = 0;

    static async Task Main(string[] args)
    {
        Version version = Assembly.GetExecutingAssembly().GetName().Version;

        Console.Title = "Abdal Eni Benchmark " + version.Major + "." + version.Minor;
        Banner.banner();

        Console.WriteLine("---------------------------------------------------------------------------------------------------------");
        Console.WriteLine("Usage: Abdal Eni Benchmark.exe  <url> <connectionCount> <requestPerConnection> <totalDurationInSeconds>");
        Console.WriteLine("Description: A web performance benchmarking tool with customizable connections and requests.");
        Console.WriteLine("---------------------------------------------------------------------------------------------------------");
        Console.WriteLine();

        try
        {
            if (args.Length == 4)
            {
                await RunBenchmark(args[0], int.Parse(args[1]), int.Parse(args[2]), int.Parse(args[3]));
            }
            else
            {
                await RunBenchmark();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred but continuing: {ex.Message}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static async Task RunBenchmark(string? serverUrl = null, int connectionCount = 0, int requestPerConnection = 0, int totalDurationInSeconds = 0)
    {
        if (serverUrl == null)
        {
            Console.WriteLine("Enter the server URL:");
            serverUrl = Console.ReadLine();
        }

        if (connectionCount == 0)
        {
            Console.WriteLine("Enter the number of connections:");
            if (!int.TryParse(Console.ReadLine(), out connectionCount))
            {
                Console.WriteLine("Invalid input. Exiting.");
                return;
            }
        }

        if (requestPerConnection == 0)
        {
            Console.WriteLine("Enter the number of requests per connection:");
            if (!int.TryParse(Console.ReadLine(), out requestPerConnection))
            {
                Console.WriteLine("Invalid input. Exiting.");
                return;
            }
        }

        if (totalDurationInSeconds == 0)
        {
            Console.WriteLine("Enter the total duration in seconds:");
            if (!int.TryParse(Console.ReadLine(), out totalDurationInSeconds))
            {
                Console.WriteLine("Invalid input. Exiting.");
                return;
            }
        }

        var httpClient = new HttpClient();
        var tasks = new ConcurrentBag<Task>();

        Console.WriteLine("Waiting...");

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(totalDurationInSeconds));

        for (int i = 0; i < connectionCount; i++)
        {
            tasks.Add(MakeRequests(httpClient, serverUrl, requestPerConnection, cts.Token));
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Benchmark operation was cancelled after the specified duration.");
        }

       

        // Displaying the summary at the end
        Console.WriteLine("---------------------------------------------------");
        Console.WriteLine($"Successful requests: {successfulRequests}");
        Console.WriteLine($"Failed requests: {failedRequests}");
        Console.WriteLine("# Benchmark Summary:");
        Console.WriteLine($"Server URL: {serverUrl}");
        Console.WriteLine($"Number of Connections: {connectionCount}");
        Console.WriteLine($"Requests per Connection: {requestPerConnection}");
        Console.WriteLine($"Total Duration (seconds): {totalDurationInSeconds}");
        Console.WriteLine("---------------------------------------------------");
    }

    static async Task MakeRequests(HttpClient httpClient, string? serverUrl, int requestPerConnection, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(serverUrl))
        {
            failedRequests += requestPerConnection;
            return;
        }

        for (int i = 0; i < requestPerConnection; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                var response = await httpClient.GetAsync(serverUrl, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    successfulRequests++;
                }
                else
                {
                    failedRequests++;
                }
            }
            catch
            {
                failedRequests++;
            }
        }
    }
}
