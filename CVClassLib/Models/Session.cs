namespace WebApiInterface.Models
{
    using System.Collections.Generic;

    public class Session : ISession
    {
        public int sessionId { get; set; }
        public string callerId { get; set; }
        public string callId { get; set; }
        public string status { get; set; }
        public string networkQuality { get; set; }
        public string networkType { get; set; }
        public Payload properties { get; set; }

        public List<Link> links { get; set; }

        public Session()
        {
            properties = new Payload();
            links = new List<Link>();
        }
    }
}