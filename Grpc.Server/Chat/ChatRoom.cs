namespace Grpc.Server.Chat;

public class ChatRoom
{
    private readonly IDatetimeProvider _provider;
    public event EventHandler<ChatMessage>? MessageReceived;
    public event EventHandler<ChatRoomUser>? UserJoined;

    public ChatRoom(Guid id, string name, DateTimeOffset createdAt, IDatetimeProvider provider)
    {
        this.Id = id;
        this.Name = name;
        this.CreatedAt = createdAt;
        this.Users = new Dictionary<Guid, ChatRoomUser>();
        this.Messages = new List<ChatMessage>();
        this._provider = provider;
    }

    public Guid Id { get; }
    public string Name { get; }
    public DateTimeOffset CreatedAt { get; }
    public IDictionary<Guid, ChatRoomUser> Users { get; }
    public ICollection<ChatMessage> Messages { get; }

    public void AddUser(ChatUser user)
    {
        if (user.IsStopAuthorized)
            throw new Exception("user is not authorized to send message");

        var roomUser = new ChatRoomUser(this.Id, user.Id, _provider.Now);
        Users.Add(roomUser.UserId, roomUser);
        UserJoined?.Invoke(this, roomUser);
    }

    public void AddMessage(ChatMessage message)
    {
        if (!this.Users.TryGetValue(message.UserId, out var user))
            throw new UserNotInRoomException();

        if (user.IsStopAuthorized)
            throw new Exception("user is not authorized to send message");

        Messages.Add(message);
        MessageReceived?.Invoke(this, message);
    }
}

public record ChatRoomUser(Guid RoomId, Guid UserId, DateTimeOffset CreatedAt, bool IsStopAuthorized = false);