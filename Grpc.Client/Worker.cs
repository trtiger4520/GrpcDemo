using System.Threading.Channels;

using Grpc.Server.Services;

using Microsoft.Extensions.Logging;

namespace Grpc.Client;

public class Worker : IHostedService
{
    private readonly ILogger<Worker> _logger;
    private readonly ChannelReader<Command> _command;
    private readonly Chat.ChatClient _chatClient;
    private readonly UserInfo _userInfo;

    private Task _commandReader;

    public Worker(
        Chat.ChatClient chatClient,
        ILogger<Worker> logger,
        ChannelReader<Command> command)
    {
        _chatClient = chatClient;
        _logger = logger;
        _command = command;

        _userInfo = chatClient.Login(new LoginRequest()
        {
            Name = "User1",
        });
    }

    public async Task CommandReader(CancellationToken cancellationToken)
    {
        while (await _command.WaitToReadAsync(cancellationToken))
        {
            var command = await _command.ReadAsync(cancellationToken);
            _logger.LogInformation("Received command: {command}", command);
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _commandReader = CommandReader(cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
