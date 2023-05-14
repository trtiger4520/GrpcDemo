using Google.Protobuf.WellKnownTypes;

using Grpc.Core;
using Grpc.Server.Chat;

namespace Grpc.Server.Services;

public class ChatService : Chat.ChatBase
{
    private readonly ChatServer _server;
    private readonly IDatetimeProvider _provider;

    public ChatService(ChatServer server, IDatetimeProvider provider)
    {
        _server = server;
        _provider = provider;
    }

    public override Task<ChannelList> GetChannelList(UserId request, ServerCallContext context)
    {
        var channels = _server.GetRooms().Select(room => new Channel()
        {
            Id = new() { Id = room.Id.ToString() },
            Code = room.Code,
            Name = room.Name,
        });

        var result = new ChannelList();

        result.Channels.AddRange(channels);

        return Task.FromResult(result);
    }

    public override Task<Channel> CreateChannel(Empty request, ServerCallContext context)
    {
        var room = _server.CreateRoom();

        return Task.FromResult(new Channel()
        {
            Id = new() { Id = room.Id.ToString() },
            Code = room.Code,
            Name = room.Name,
        });
    }

    public override Task<Channel> JoinChannel(JoinChannelRequest request, ServerCallContext context)
    {
        var user = _server.GetUser(Guid.Parse(request.UserId.Id));
        var room = _server.GetRoom(Guid.Parse(request.ChannelId.Id));

        _server.JoinUserInZoom(user, room.Id);

        return Task.FromResult(new Channel()
        {
            Id = new() { Id = room.Id.ToString() },
            Code = room.Code,
            Name = room.Name,
        });
    }

    public override Task<Channel> JoinChannelByCode(JoinChannelByCodeRequest request, ServerCallContext context)
    {
        var user = _server.GetUser(Guid.Parse(request.UserId.Id));
        var room = _server.GetRoomByCode(request.Code);

        _server.JoinUserInZoom(user, room.Id);

        return Task.FromResult(new Channel()
        {
            Id = new() { Id = room.Id.ToString() },
            Code = room.Code,
            Name = room.Name,
        });
    }

    public override Task<ChannelMembers> GetChannelMembers(ChannelId request, ServerCallContext context)
    {
        var room = _server.GetRoom(Guid.Parse(request.Id));
        var members = room.Users.Select((roomUser) =>
        {
            var user = _server.GetUser(roomUser.Key);

            return new UserInfo()
            {
                Id = new() { Id = user.Id.ToString() },
                Name = user.Name,
            };
        });

        var result = new ChannelMembers();

        result.Members.AddRange(members);

        return Task.FromResult(result);
    }

    public override Task<UserInfo> Login(LoginRequest request, ServerCallContext context)
    {
        var user = new ChatUser(Guid.NewGuid(), request.Name, _provider.Now, _provider.Now, false);

        _server.AddUser(user);

        return Task.FromResult(new UserInfo()
        {
            Id = new UserId() { Id = user.Id.ToString() },
            Name = user.Name,
        });
    }

    public override Task ReceiveChannelMemberChange(ChannelId request, IServerStreamWriter<UserInfo> responseStream, ServerCallContext context)
    {
        var room = _server.GetRoom(Guid.Parse(request.Id));

        room.UserJoined += OnUserJoined();

        context.CancellationToken.WaitHandle.WaitOne();

        room.UserJoined -= OnUserJoined();

        return Task.CompletedTask;

        EventHandler<ChatRoomUser> OnUserJoined() => (_, roomUser) =>
        {
            var user = _server.GetUser(roomUser.UserId);
            responseStream.WriteAsync(new UserInfo()
            {
                Id = new UserId() { Id = user.Id.ToString() },
                Name = user.Name,
            });
        };
    }

    public override Task ReceiveMessage(ChannelId request, IServerStreamWriter<Message> responseStream, ServerCallContext context)
    {
        var room = _server.GetRoom(Guid.Parse(request.Id));

        room.MessageReceived += OnMessageReceived(context.CancellationToken);

        context.CancellationToken.WaitHandle.WaitOne();

        room.MessageReceived -= OnMessageReceived(context.CancellationToken);

        return Task.CompletedTask;

        EventHandler<ChatMessage> OnMessageReceived(CancellationToken cancellationToken) => (_, message) =>
        {
            var user = _server.GetUser(message.UserId);
            responseStream.WriteAsync(new Message()
            {
                SenderId = new UserId() { Id = user.Id.ToString() },
                SenderName = user.Name,
                Content = message.Content,
                Timestamp = Timestamp.FromDateTimeOffset(message.CreatedAt),
            }, cancellationToken);
        };
    }

    public override Task<Empty> SendMessage(SendMessageRequest request, ServerCallContext context)
    {
        var room = _server.GetRoom(Guid.Parse(request.Id.Id));

        room.AddMessage(new ChatMessage(
            Guid.NewGuid(),
            Guid.Parse(request.Message.SenderId.Id),
            request.Message.Content,
            request.Message.Timestamp.ToDateTimeOffset(),
            _provider.Now));

        return Task.FromResult(new Empty());
    }
}