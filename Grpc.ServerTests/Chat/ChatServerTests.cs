using Grpc.Server;
using Grpc.Server.Chat;

namespace Grpc.ServerTests.Chat;

[TestFixture]
public class ChatServerTests
{
#pragma warning disable 8618

    private ChatServer _server;
    private IDatetimeProvider _datetimeProvider;

#pragma warning restore 8618

    [SetUp]
    public void Setup()
    {
        _datetimeProvider = new FakeDatetimeProvider();
        _server = new ChatServer(_datetimeProvider);
    }

    [Test]
    public void CanCreateRoom()
    {
        var room = _server.CreateRoom("test");
        Assert.That(_server.GetRoom(room.Id), Is.Not.Null);
    }

    [Test]
    public void CanAddUser()
    {
        var user = ChatUser.CreateByName("test user");

        _server.AddUser(user);

        Assert.That(_server.GetUser(user.Id), Is.Not.Null);
    }

    [Test]
    public void CanJoinRoom()
    {
        var room = _server.CreateRoom("test");
        var user = ChatUser.CreateByName("test user");

        _server.AddUser(user);
        _server.JoinUserInZoom(user, room.Id);

        Assert.That(room.Users.Any(cu => cu.Key == user.Id), Is.True);
    }

    [Test]
    public void JoinRoomEventBeActived()
    {
        var joinedUsers = new List<ChatRoomUser>();
        var room = _server.CreateRoom("test");
        var user = ChatUser.CreateByName("test user");

        room.UserJoined += (_, roomUser) => joinedUsers.Add(roomUser);

        _server.AddUser(user);
        _server.JoinUserInZoom(user, room.Id);

        Assert.Multiple(() =>
        {
            CollectionAssert.IsNotEmpty(joinedUsers);
            Assert.That(joinedUsers, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void UserCanNotJoinRoomWhenNotAdded()
    {
        var room = _server.CreateRoom("test");
        var user = ChatUser.CreateByName("test user");

        Assert.Catch<ChatUserNotFoundException>(
            () => _server.JoinUserInZoom(user, room.Id));
    }

    [Test]
    public void CanSendMessage()
    {
        var room = _server.CreateRoom("test");
        var user = ChatUser.CreateByName("test user");
        var message = ChatMessage.NewMessage(user.Id, "test message", _datetimeProvider.Now);

        _server.AddUser(user);
        _server.JoinUserInZoom(user, room.Id);
        _server.SendMessage(message, room.Id);

        Assert.Multiple(() =>
        {
            CollectionAssert.IsNotEmpty(room.Messages);
            CollectionAssert.Contains(room.Messages, message);
        });
    }

    [Test]
    public void SendMessageEventBeActived()
    {
        var receivedMessages = new List<ChatMessage>();
        var room = _server.CreateRoom("test");
        var user = ChatUser.CreateByName("test user");
        var message = ChatMessage.NewMessage(user.Id, "test message", _datetimeProvider.Now);

        room.MessageReceived += (_, message) => receivedMessages.Add(message);

        _server.AddUser(user);
        _server.JoinUserInZoom(user, room.Id);
        _server.SendMessage(message, room.Id);

        Assert.Multiple(() =>
        {
            CollectionAssert.IsNotEmpty(receivedMessages);
            Assert.That(receivedMessages, Has.Count.EqualTo(1));
            CollectionAssert.Contains(receivedMessages, message);
        });
    }

    [Test]
    public void UserCanNotSendMessageWhenNotJoinedRoom()
    {
        var room = _server.CreateRoom("test");
        var user = ChatUser.CreateByName("test user");
        var message = ChatMessage.NewMessage(user.Id, "test message", _datetimeProvider.Now);

        _server.AddUser(user);

        Assert.Catch<UserNotInRoomException>(
            () => _server.SendMessage(message, room.Id));
    }

}