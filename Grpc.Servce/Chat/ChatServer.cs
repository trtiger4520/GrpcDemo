namespace Grpc.Servce.Chat;

public class ChatServer
{
    private readonly ICollection<ChatUser> _users;
    private readonly IDatetimeProvider _provider;

    public ICollection<ChatRoom> Rooms { get; }

    public ChatServer(IDatetimeProvider provider)
    {
        this._users = new List<ChatUser>();
        this.Rooms = new List<ChatRoom>();
        this._provider = provider;
    }

    public void AddUser(ChatUser user)
        => _users.Add(user);

    public ChatRoom CreateRoom(string name = "")
    {
        name = string.IsNullOrEmpty(name) ? RandomZoomName() : name;
        var room = new ChatRoom(Guid.NewGuid(), name, _provider.Now, _provider);
        Rooms.Add(room);
        return room;
    }

    public void JoinUserInZoom(ChatUser user, Guid roomId)
    {
        if (!_users.Any(u => u.Id == user.Id))
        {
            throw new ChatUserNotFoundException();
        }

        GetRoomById(roomId).AddUser(user);
    }

    public void SendMessage(ChatMessage message, Guid roomId)
        => GetRoomById(roomId).AddMessage(message);

    public ChatUser GetUser(Guid userId) => GetUserById(userId);

    public ChatRoom GetRoom(Guid roomId) => GetRoomById(roomId);

    private ChatUser GetUserById(Guid userId)
        => _users.FirstOrDefault(r => r.Id == userId) ?? throw new ChatUserNotFoundException();

    private ChatRoom GetRoomById(Guid roomId)
        => Rooms.FirstOrDefault(r => r.Id == roomId) ?? throw new ChatRoomNotFoundException();

    private static string RandomZoomName() => $"Zoom-{Guid.NewGuid()}";

}
