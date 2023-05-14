using Google.Protobuf.WellKnownTypes;

using Grpc.Server.Services;

using static Grpc.Server.Services.Chat;

namespace Grpc.Client;

public class ChatHandler : IDisposable
{
    private readonly ChatClient _client;
    private Task? _currentChannelMemberWatch;
    private CancellationTokenSource? _currentChannelMemberWatchCancel;
    private Task? _currentChannelMessageWatch;
    private CancellationTokenSource? _currentChannelMessageWatchCancel;

    public ChatHandler(ChatClient client)
    {
        _client = client;
    }

    public UserInfo? CurrentUser { get; private set; }
    public Channel? CurrentChannel { get; private set; }

    public event EventHandler<UserInfo> ChannelUserJoined;
    public event EventHandler<Message> MessageReceived;

    public UserInfo LoginUser(string userName)
    {
        var user = _client.Login(new LoginRequest()
        {
            Name = userName,
        });

        CurrentUser = user;

        return user;
    }

    public Channel CreateChannel()
    {
        if (CurrentUser is null)
        {
            throw new InvalidOperationException("User not login");
        }

        var channel = _client.CreateChannel(new Empty());

        JoinChannel(channel.Id);

        CurrentChannel = channel;

        return channel;
    }

    public IEnumerable<Channel> GetChannels()
    {
        if (CurrentUser is null)
        {
            throw new InvalidOperationException("User not login");
        }

        var list = _client
            .GetChannelList(CurrentUser.Id);

        return list.Channels;
    }

    public Channel JoinChannel(ChannelId channelId)
    {
        if (CurrentUser is null)
        {
            throw new InvalidOperationException("User not login");
        }

        var channel = _client.JoinChannel(new JoinChannelRequest()
        {
            UserId = CurrentUser.Id,
            ChannelId = channelId,
        });

        WatchChannelMemberChange(channel.Id);
        WatchChannelMessageChange(channel.Id);

        CurrentChannel = channel;

        return channel;
    }

    public Channel JoinChannel(string code)
    {
        if (CurrentUser is null)
        {
            throw new InvalidOperationException("User not login");
        }

        var channel = _client.JoinChannelByCode(new JoinChannelByCodeRequest()
        {
            UserId = CurrentUser.Id,
            Code = code,
        });

        WatchChannelMemberChange(channel.Id);
        WatchChannelMessageChange(channel.Id);

        CurrentChannel = channel;

        return channel;
    }

    public void SendMessage(string message)
    {
        if (CurrentUser is null)
        {
            throw new InvalidOperationException("User not login");
        }

        if (CurrentChannel is null)
        {
            throw new InvalidOperationException("Channel not joined");
        }

        _client.SendMessage(new SendMessageRequest()
        {
            Id = CurrentChannel.Id,
            Message = new Message()
            {
                SenderName = CurrentUser.Name,
                SenderId = CurrentUser.Id,
                Content = message,
                Timestamp = Timestamp.FromDateTimeOffset(
                    DateTimeOffset.Now
                )
            },
        });
    }

    private void WatchChannelMemberChange(ChannelId channelId)
    {
        _currentChannelMemberWatchCancel?.Cancel();
        _currentChannelMemberWatch?.Dispose();

        var cancellationTokenSource = new CancellationTokenSource();
        var subscribe = async (ChannelId channelId, CancellationToken cancellationToken) =>
        {
            var stream = _client
                .ReceiveChannelMemberChange(channelId, cancellationToken: cancellationToken)
                .ResponseStream;

            while (await stream.MoveNext(cancellationToken))
            {
                var user = stream.Current;
                ChannelUserJoined?.Invoke(this, user);
            }
        };

        _currentChannelMemberWatch = subscribe(channelId, cancellationTokenSource.Token);
        _currentChannelMemberWatchCancel = cancellationTokenSource;
    }

    private void WatchChannelMessageChange(ChannelId channelId)
    {
        _currentChannelMessageWatchCancel?.Cancel();
        _currentChannelMessageWatch?.Dispose();

        var cancellationTokenSource = new CancellationTokenSource();
        var subscribe = async (ChannelId channelId, CancellationToken cancellationToken) =>
        {
            var stream = _client
                .ReceiveMessage(channelId, cancellationToken: cancellationToken)
                .ResponseStream;

            while (await stream.MoveNext(cancellationToken))
            {
                var message = stream.Current;
                MessageReceived?.Invoke(this, message);
            }
        };

        _currentChannelMessageWatch = subscribe(channelId, cancellationTokenSource.Token);
        _currentChannelMessageWatchCancel = cancellationTokenSource;
    }

    public void Dispose()
    {
        _currentChannelMemberWatchCancel?.Cancel();
        GC.SuppressFinalize(this);
    }
}