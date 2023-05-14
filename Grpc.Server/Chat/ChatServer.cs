namespace Grpc.Server.Chat;

public class ChatServer
{
    private readonly ICollection<ChatUser> _users;
    private readonly IDatetimeProvider _provider;
    private readonly ICollection<ChatRoom> _rooms;

    public ChatServer(IDatetimeProvider provider)
    {
        this._users = new List<ChatUser>();
        this._rooms = new List<ChatRoom>();
        this._provider = provider;
    }

    public void AddUser(ChatUser user)
        => _users.Add(user);

    public ChatRoom CreateRoom(string name = "")
    {
        name = string.IsNullOrEmpty(name) ? RandomZoomName() : name;
        var id = Guid.NewGuid();
        var code = ChatRoomCodeGenerator.Generate();

        while (_rooms.Any(r => r.Code == code))
            code = ChatRoomCodeGenerator.Generate();

        var room = new ChatRoom(id, code, name, _provider.Now, _provider);
        _rooms.Add(room);
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

    public ICollection<ChatRoom> GetRooms()
    {
        return _rooms;
    }

    public ChatRoom GetRoomByCode(string code)
        => _rooms.FirstOrDefault(r => r.Code == code) ?? throw new ChatRoomNotFoundException();

    private ChatUser GetUserById(Guid userId)
        => _users.FirstOrDefault(r => r.Id == userId) ?? throw new ChatUserNotFoundException();

    private ChatRoom GetRoomById(Guid roomId)
        => _rooms.FirstOrDefault(r => r.Id == roomId) ?? throw new ChatRoomNotFoundException();

    private static string RandomZoomName() => $"Zoom-{Guid.NewGuid()}";

}
