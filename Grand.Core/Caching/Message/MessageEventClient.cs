
namespace Grand.Core.Caching.Message
{
    public class MessageEventClient : IMessageEventClient
    {
        public string ClientId { get; set; }
        public string Key { get; set; }
        public int MessageType { get; set; }
    }
}
