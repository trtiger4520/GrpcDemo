using System.Runtime.Serialization;

namespace Grpc.Servce.Chat;

[Serializable]
public class ChatUserNotFoundException : Exception
{
    public ChatUserNotFoundException() : base("Chat user not found")
    {
    }

    public ChatUserNotFoundException(string? message) : base(message)
    {
    }

    public ChatUserNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected ChatUserNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}