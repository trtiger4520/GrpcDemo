using System.Runtime.Serialization;

namespace Grpc.Servce.Chat;

[Serializable]
public class ChatRoomNotFoundException : Exception
{
    public ChatRoomNotFoundException() : base("Room not found")
    {
    }

    public ChatRoomNotFoundException(string? message) : base(message)
    {
    }

    public ChatRoomNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected ChatRoomNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}