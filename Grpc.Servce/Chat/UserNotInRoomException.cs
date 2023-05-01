using System.Runtime.Serialization;

namespace Grpc.Servce.Chat;

[Serializable]
public class UserNotInRoomException : Exception
{
    public UserNotInRoomException() : base("User not in room")
    {
    }

    public UserNotInRoomException(string? message) : base(message)
    {
    }

    public UserNotInRoomException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected UserNotInRoomException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}