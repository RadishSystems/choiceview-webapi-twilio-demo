namespace WebApiInterface.Models
{
    public class NewSession
    {
        public string callerId { get; set; }
        public string callId { get; set; }
        public string stateChangeUri { get; set; }
        public string newMessageUri { get; set; }
        public string notificationType { get; set; }
    }
}