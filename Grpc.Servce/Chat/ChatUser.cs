namespace Grpc.Servce.Chat;

public record ChatUser(
    Guid Id,
    string Name,
    DateTimeOffset LastSeenAt,
    DateTimeOffset CreatedAt,
    bool IsStopAuthorized)
{
    public static ChatUser CreateByName(string name)
        => new(
            Guid.NewGuid(),
            name,
            DateTimeOffset.MinValue,
            DateTimeOffset.Now,
            false);
}
