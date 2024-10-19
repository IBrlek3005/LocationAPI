using Microsoft.AspNetCore.SignalR.Client;

class Program
{
    static async Task Main(string[] args)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5048/searchHub")
            .Build();

        connection.On<string>("ReceiveMessage", (message) =>
        {
            Console.WriteLine($"Primljena poruka: {message}");
        });

        try
        {
            await connection.StartAsync();
            Console.WriteLine("SignalR klijent je povezan.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Greška prilikom povezivanja: {ex.Message}");
            return;
        }

        Console.WriteLine("Pritisni bilo koju tipku za izlaz...");
        Console.ReadKey();

        await connection.StopAsync();
        await connection.DisposeAsync();
    }
}