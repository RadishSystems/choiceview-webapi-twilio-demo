namespace WebApiInterface.Models
{
    public interface ISession
    {
        /// <summary>
        /// The unique id the ChoiceView switch assigns to the session.
        /// Each call from the same client gets a different session id.
        /// </summary>
        int sessionId { get; set; }
        /// <summary>
        /// Uniquely identifies the client. Usually the phone number of the mobile device.
        /// </summary>
        string callerId { get; set; }
        /// <summary>
        /// Associates this client with an automated agent (IVR).
        /// </summary>
        string callId { get; set; }
        /// <summary>
        /// Current state of the session with the client.
        /// </summary>
        string status { get; set; }
        /// <summary>
        /// A string describing the quality of the network connection between the client and the switch.
        /// </summary>
        string networkQuality { get; set; }
        /// <summary>
        /// A string value - 'wifi' indicates a fast wifi type connection, 'mobile' indicates a mobile network connection.
        /// </summary>
        string networkType { get; set; }
        Payload properties { get; set; }
    }
}