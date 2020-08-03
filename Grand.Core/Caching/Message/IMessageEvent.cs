
namespace Grand.Core.Caching.Message
{
    public interface IMessageEvent
    {
        string Key { get; set; }
        int MessageType { get; set; }
    }
}
