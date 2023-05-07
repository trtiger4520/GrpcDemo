namespace Grpc.Server.Chat;

public record ChatMessage(
    Guid Id,
    Guid UserId,
    string Content,
    DateTimeOffset SentAt,
    DateTimeOffset CreatedAt)
{
    public static ChatMessage NewMessage(Guid userId, string content, DateTimeOffset sentAt)
    => new(
        Guid.NewGuid(),
        userId,
        content,
        sentAt,
        DateTimeOffset.Now
    );
}
