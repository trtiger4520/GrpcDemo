using System.Threading.Channels;

using Google.Protobuf.WellKnownTypes;

using Grpc.Server.Services;

using Microsoft.Extensions.Logging;

namespace Grpc.Client;

public class Worker : BackgroundService
{
    private readonly ChatHandler _chatHandler;
    private readonly ILogger<Worker> _logger;
    private readonly ChannelReader<Command> _command;

    public Worker(
        ChatHandler chatHandler,
        ILogger<Worker> logger,
        ChannelReader<Command> command)
    {
        chatHandler.ChannelUserJoined += ChannelUserJoined;
        chatHandler.MessageReceived += MessageReceived;

        _chatHandler = chatHandler;
        _logger = logger;
        _command = command;
    }

    private void CreateUser(string name)
    {
        _chatHandler.LoginUser(name);
    }

    private void SendMessage(string message)
    {
        try
        {
            _chatHandler.SendMessage(message);
        }
        catch (Exception error)
        {
            _logger.LogError(error, "Send message failed");
        }
    }

    private void JoinChannel(string commandValue)
    {
        try
        {
            _logger.LogInformation("Try join channel by code:{code}...", commandValue);

            var channel = _chatHandler.JoinChannel(commandValue);

            _logger.LogInformation(
                "Channel created, name:{name}, code:{code}",
                channel.Name, channel.Code);
        }
        catch (Exception error)
        {
            _logger.LogError(error, "Join channel failed");
        }
    }

    private void GetChannels()
    {
        try
        {
            var channels = _chatHandler.GetChannels();
            var msg = channels.Select(c => $"name:{c.Name} code:{c.Code}");
            _logger.LogInformation("{command}", string.Join("\n", msg));
        }
        catch (Exception error)
        {
            _logger.LogError(error, "Get channels failed");
        }
    }

    private void NewChannel()
    {
        try
        {
            var channel = _chatHandler.CreateChannel();
            _logger.LogInformation(
                "Channel created, name:{name}, code:{code}",
                channel.Name, channel.Code);
        }
        catch (Exception error)
        {
            _logger.LogError(error, "Create channels failed");
        }
    }

    private void ChannelUserJoined(object? sender, UserInfo e)
    {
        _logger.LogInformation("User {name} joined", e.Name);
    }

    private void MessageReceived(object? sender, Message e)
    {
        _logger.LogInformation(
            "[{time}] {name}: {message}",
            e.Timestamp.ToDateTime().ToShortTimeString(),
            e.SenderName,
            e.Content);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (await _command.WaitToReadAsync(cancellationToken))
        {
            var command = await _command.ReadAsync(cancellationToken);
            _logger.LogInformation("Received command: {command}", command);
            HandleCommand(command.Value);
        }
    }

    private void HandleCommand(string cmd)
    {
        try
        {
            var isCommand = cmd.StartsWith("/");

            if (isCommand)
            {
                var commandParts = cmd.Split(" ");
                var commandType = commandParts[0][1..];
                var commandValue = commandParts.ElementAtOrDefault(1)?.ToString();

                switch (commandType)
                {
                    case "login":
                        if (commandValue is null)
                        {
                            _logger.LogError("need user name");
                            break;
                        }
                        CreateUser(commandValue);
                        break;
                    case "newchannel":
                        NewChannel();
                        break;
                    case "channels":
                        GetChannels();
                        break;
                    case "joinchannel":
                        if (commandValue is null)
                        {
                            _logger.LogError("need channel code");
                            break;
                        }
                        JoinChannel(commandValue);
                        break;
                }
            }
            else
            {
                SendMessage(cmd);
            }
        }
        catch (Exception error)
        {
            _logger.LogError(error, "Handle command failed");
        }
    }
}
