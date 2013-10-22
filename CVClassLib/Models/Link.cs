namespace WebApiInterface.Models
{
    public class Link
    {
        public static string StateNotificationRel = "/rels/statenotification";
        public static string MessageNotificationRel = "/rels/messagenotification";
        public static string SessionRel = "/rels/session";
        public static string PayloadRel = "/rels/properties";
        public static string ControlMessageRel = "/rels/controlmessage";

        public string rel { get; set; }
        public string href { get; set; }
    }
}