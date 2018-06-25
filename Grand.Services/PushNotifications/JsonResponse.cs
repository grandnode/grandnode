namespace Grand.Services.PushNotifications
{
    public class JsonResponse
    {
        public long multicast_id { get; set; }
        public int success { get; set; }
        public int failure { get; set; }
        public int canonical_ids { get; set; }
        public Message[] results { get; set; }
    }

    public class Message
    {
        public string message_id { get; set; }
    }
}
