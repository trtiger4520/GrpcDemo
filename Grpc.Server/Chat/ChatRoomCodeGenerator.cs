namespace Grpc.Server.Chat
{
    public class ChatRoomCodeGenerator
    {
        public static string Generate()
        {
            return Guid.NewGuid().ToString()[..6];
        }
    }
}