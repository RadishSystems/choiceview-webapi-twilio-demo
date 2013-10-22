namespace WebApiInterface.Models
{
    using System.Collections.Generic;

    public class Properties
    {
        public int sessionId { get; set; }
        public Payload properties { get; set; }
        public List<Link> links { get; set; }

        public Properties()
        {
            properties = new Payload();
            links = new List<Link>();
        }
    }
}