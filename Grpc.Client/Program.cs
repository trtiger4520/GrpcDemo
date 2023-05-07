using Grpc.Client;
using Grpc.Server.Services;

var canceller = new CancellationTokenSource();
var builder = Host.CreateApplicationBuilder(args);
var commandChannel = System.Threading.Channels.Channel.CreateUnbounded<Command>(new System.Threading.Channels.UnboundedChannelOptions()
{
    SingleReader = false,
    SingleWriter = true,
});

builder.Services.AddGrpcClient<Chat.ChatClient>(
    o => o.Address = new Uri("http://localhost:5134"));

builder.Services.AddSingleton<System.Threading.Channels.ChannelReader<Command>>(commandChannel);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.StartAsync(canceller.Token).Wait();

Console.CancelKeyPress += (sender, e) => canceller.Cancel();
Console.CancelKeyPress += (sender, e) => commandChannel.Writer.Complete();
Console.CancelKeyPress += (sender, e) => Console.WriteLine("key enter to exit");

while (!canceller.IsCancellationRequested)
{
    var command = Console.ReadLine();
    if (command is not null)
        commandChannel.Writer.TryWrite(command);
}

host.StopAsync().Wait();
