using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.WebPages;
using System.Web.SessionState;
using System.Xml;
using Twilio;
using WebApiInterface.Models;

namespace CVClassLib
{
    /// <summary>
    /// Summary description for ClassName
    /// </summary>
    public class cvWorkThread
    {
        private string ANI;
        private string CallSid;
        private string AccountSid;
        private string signalURI;
        public static readonly ILog log = LogManager.GetLogger(typeof(cvWorkThread));
        FileInfo log4netConfig;
        private Uri BaseUri;
        private HttpClient Client;
        private Uri SessionsUri;
        private readonly MediaTypeFormatter jsonFormatter;
        private Uri SessionUri;
        private Uri SessionPropertiesUri;
        private Uri SessionControlMessageUri;
        private Session cvSession;
        private string stateChangeURI;
        private string newMessageURI;
        private string notificationType;
        private bool CVStartFailed;
        private string transferAccount;
        private string filePath;

        public Uri SessionURI
        {
            get
            {
                log.Info(String.Format("{0} - SessionURI property retrieved - {1}", ANI, SessionUri));
                return SessionUri;
            }

            set
            {
                log.Info(String.Format("{0} - SessionURI property set to - {1}", ANI, value));
                SessionUri = value;
            }
        }

        public bool CVStartUpFailed
        {
            get
            {
                log.Info(String.Format("{0} - CVStartUpfailed property retrieved - {1}", ANI, CVStartFailed));
                return CVStartFailed;
            }

            set
            {
                log.Info(String.Format("{0} - CVStartUpFailed property set to - {1}", ANI, value));
                CVStartFailed = value;
            }
        }

        public cvWorkThread(string PhoneNumber, string CallID, string Account, Uri SessionUri)
        {
            ANI = PhoneNumber;
            CallSid = CallID;
            AccountSid = Account;
            filePath = HttpContext.Current.Server.MapPath("~/App_Data/");
            string log4NetPath = filePath + "log4net.config";
            log4netConfig = new FileInfo(log4NetPath);
            XmlConfigurator.Configure(log4netConfig);
            log.Info(String.Format("\n\n{0} - Instantiating cvWorkThread. PhoneNumber:{1} CallID:{2} Account:{3} SessionUri:{4}\n", ANI, PhoneNumber, CallID, Account, SessionUri));
            log.Info(String.Format("{0} - filePath is {1}", ANI, filePath));
            string configPath = filePath + "TwilioIVR.config";

            try
            {
                XmlDocument xDoc = new XmlDocument();
                log.Info(String.Format("{0} - configPath is {1}", ANI, configPath));
                xDoc.Load(configPath);
//                log.Info(String.Format("{0} - after xml config loaded", ANI));
                XmlNodeList signalURINode = xDoc.GetElementsByTagName("signalURI");
                signalURI = signalURINode[0].InnerText;
                log.Info(String.Format("{0} - signalURI is {1}", ANI, signalURI));
                XmlNodeList transferAccountNode = xDoc.GetElementsByTagName("transferAccount");
                transferAccount = transferAccountNode[0].InnerText;
                log.Info(String.Format("{0} - transferAccount is {1}", ANI, transferAccount)); 
                XmlNodeList baseURINode = xDoc.GetElementsByTagName("baseURI");
                string baseURIString = baseURINode[0].InnerText;
                log.Info(String.Format("{0} - baseURIString is {1}", ANI, baseURIString));
                BaseUri = new Uri(baseURIString, UriKind.Absolute);
            }
            catch (Exception ex)
            {
                log.Error(String.Format("{0} - error getting config info - " + ex.Message, ANI));
            }



            SessionURI = SessionUri;
            jsonFormatter = new JsonMediaTypeFormatter();

        }

        public void StartCVSession()
        {
            try
            {
                log.Info(String.Format("{0} - StartCVSession started.\n", ANI));
                CVStartUpFailed = false;
                stateChangeURI = signalURI + "/state_change.cshtml?sessionid=" + ANI;
                newMessageURI = signalURI + "/new_message.cshtml?sessionid=" + ANI;
                SessionsUri = new Uri(BaseUri, "/ivr/api/sessions");
                notificationType = "CCXML";
                Client = new HttpClient { BaseAddress = BaseUri };
                Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var param =
                    Convert.ToBase64String(
                        System.Text.Encoding.ASCII.GetBytes(String.Format("{0}:{1}",
                                                                            "demo",
                                                                            "radisH1!")));

                Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", param);
            }
            catch (UriFormatException ex)
            {
                log.Error(String.Format("{0} - Bad API server address - " + ex.Message, ANI));
            }

            var newSession = new NewSession
                            {
                                callerId = ANI,
                                callId = CallSid,
                                newMessageUri = newMessageURI,
                                stateChangeUri = stateChangeURI,
                                notificationType = notificationType
                            };
            try
            {
                log.Info(String.Format("{0} - StartCVSession inside try block ", ANI));
                log.Info(String.Format("{0} - stateChangeURI is {1}", ANI, newSession.stateChangeUri));
                log.Info(String.Format("{0} - newMessageURI is {1}", ANI, newSession.newMessageUri));
                log.Info(String.Format("{0} - callerId is {1}", ANI, newSession.callerId));
                log.Info(String.Format("{0} - callId is {1}", ANI, newSession.callId));
                log.Info(String.Format("{0} - notificationType is {1}", ANI, newSession.notificationType));
                log.Info(String.Format("{0} - SessionsUri.AbsoluteUri is {1}", ANI, SessionsUri.AbsoluteUri));

                Client.PostAsJsonAsync<NewSession>(SessionsUri.AbsoluteUri, newSession).ContinueWith(
                    responseTask =>
                    {
                        log.Info(String.Format("{0} - StartCVSession inside PostAsJsonAsync task ", ANI));
                        if (responseTask.Exception != null)
                        {
                            log.Info(String.Format("{0} - Post failed - {1}", ANI, responseTask.Exception.InnerException.Message));
                            log.Info(String.Format("{0} - responseTask.Exception.Message is {1}", ANI, responseTask.Exception.Message));
                            log.Info(String.Format("{0} - responseTask.Exception.InnerException.InnerException.Source is {1}", ANI, responseTask.Exception.InnerException.InnerException.Message));
                            log.Info(String.Format("{0} - responseTask.Exception.InnerException.InnerException.StackTrace is {1}", ANI, responseTask.Exception.InnerException.InnerException.StackTrace));
                            CVStartUpFailed = true;
                            RedirectTwilio(signalURI + "/main-menu.xml");
                        }
                        else
                        {
                            if (responseTask.Result.StatusCode == HttpStatusCode.Created)
                            {
                                log.Info(String.Format("{0} - StartCVSession status code created ", ANI));
                                SessionUri = responseTask.Result.Headers.Location;
                                this.SessionURI = SessionUri;
                                log.Info(String.Format("{0} - SessionUri is {1}", ANI, SessionUri));
                                string fullPath = "";
                                try
                                {
//                                    log.Info(String.Format("{0} - filePath is {1}", ANI, filePath));
                                    fullPath = filePath + ANI + ".txt";
                                    log.Info(String.Format("{0} - fullPath is {1}", ANI, fullPath));
                                }
                                catch (Exception ex)
                                {
                                    log.Info(String.Format("{0} - exception is {1}", ANI, ex.Message));
                                }

                                StreamWriter swFile;
                                if (File.Exists(fullPath))
                                {
                                    File.Delete(fullPath);  // remove old file
                                }

                                // Create a file to write to.
                                swFile = File.CreateText(fullPath);
                                swFile.WriteLine(String.Format("{0}", SessionUri));
                                swFile.Close();

                                responseTask.Result.Content.ReadAsAsync<Session>(
                                    new List<MediaTypeFormatter> { jsonFormatter }).ContinueWith(
                                        contentTask =>
                                        {
                                            log.Info(String.Format("{0} - StartCVSession async read completed ", ANI));
                                            if (contentTask.Exception == null)
                                            {
                                                log.Info(String.Format("{0} - StartCVSession inside contentTask.Exception = null ", ANI));
                                                cvSession = contentTask.Result;
                                                if (cvSession != null)
                                                {
                                                    log.Info(String.Format("{0} - StartCVSession session created ", ANI));
                                                    ContinueCVSession();
                                                }
                                                else
                                                {
                                                    log.Info(String.Format("{0} - StartCVSession POST {1} did not return recognizable content!", ANI, SessionsUri.AbsoluteUri));
                                                    CVStartUpFailed = true;
                                                }
                                            }
                                            else
                                            {
                                                log.Info(String.Format("{0} - StartCVSession Cannot read the content from POST {1}\nError: {2}",
                                                        ANI, SessionsUri.AbsoluteUri, contentTask.Exception.InnerException.Message));
                                                CVStartUpFailed = true;
                                            }
                                        }
                                    );
                            }
                            else
                            {
                                var msg = responseTask.Result.Content.ReadAsStringAsync().Result;
                                log.Info(String.Format("{0} - StartCVSession POST {1} failed! Reason - {2}, {3}, {4}",
                                                                ANI, 
                                                                SessionsUri.AbsoluteUri,
                                                                responseTask.Result.StatusCode,
                                                                responseTask.Result.ReasonPhrase,
                                                                msg));
                                CVStartUpFailed = true;
                            }
                        }
                    });
                log.Info(String.Format("{0} - StartCVSession after Post task", ANI));
            }
            catch (Exception ex)
            {
                log.Info(String.Format("{0} - error in StartCVSession - " + ex.Message, ANI));
                CVStartUpFailed = true;
            }
            log.Info(String.Format("{0} - leaving StartCVSession ", ANI));
        }

        public void HandleControlMessageNew()
        {
            HandleControlMessage("New");
        }

        public void HandleControlMessageState()
        {
            HandleControlMessage("State");
        }
        
        private void HandleControlMessage(string type)
        {
            try
            {
                log.Info(String.Format("{0} - HandleControlMessage started - {1}\n\n", ANI, type));
                SessionsUri = new Uri(BaseUri, "/ivr/api/sessions");
                Client = new HttpClient { BaseAddress = BaseUri };
                Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var param =
                    Convert.ToBase64String(
                        System.Text.Encoding.ASCII.GetBytes(String.Format("{0}:{1}",
                                                                            "demo",
                                                                            "radisH1!")));
                Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", param);
            }
            catch (UriFormatException ex)
            {
                log.Error(String.Format("{0} - Bad API server address - " + ex.Message, ANI));
            }

            log.Info(String.Format("{0} - SessionURI in HandleControlMessage is {1}", ANI, SessionURI));
            Client.GetAsync(SessionURI).ContinueWith(
            responseTask =>
            {
                log.Info(String.Format("{0} - continuing async get", ANI));
                if (responseTask.Result.IsSuccessStatusCode)
                {
                    responseTask.Result.Content.ReadAsAsync<Session>(
                        new List<MediaTypeFormatter> { jsonFormatter }).ContinueWith(
                        contentTask =>
                        {
                            cvSession = contentTask.Result;
                            if (cvSession != null)
                            {
                                ANI = cvSession.callerId;
                                CallSid = cvSession.callId;
                                log.Info(String.Format("{0} - ANI is {1}, CallSid is {2}", ANI, ANI, CallSid));
                                if (cvSession.status.Equals("connected"))
                                {
                                    log.Info(String.Format("{0} - session connected", ANI));
                                    if (SessionPropertiesUri == null ||
                                        !SessionPropertiesUri.AbsolutePath.StartsWith(
                                            SessionUri.AbsolutePath))
                                    {
                                        var link = cvSession.links.Find(l => l.rel.EndsWith(Link.PayloadRel));
                                        if (link != null)
                                        {
                                            SessionPropertiesUri = new Uri(link.href);
//                                            log.Info(String.Format("{0} - Ready to log properties uri for {1}", ANI, link.href));
                                            GetProperties();
                                            log.Info(String.Format("{0} - After logging properties uri", ANI));
                                        }
                                        else
                                        {
                                            log.Info(String.Format("{0} - Cannot find the properties uri!", ANI));
                                        }
                                        log.Info(String.Format("{0} - looking for control message link", ANI));
                                        var cmlink = cvSession.links.Find(l => l.rel.EndsWith(Link.ControlMessageRel));
                                        if (cmlink != null)
                                        {
//                                            log.Info(String.Format("{0} - control message link is not null", ANI));
                                            SessionControlMessageUri = new Uri(cmlink.href);
                                            log.Info(String.Format("{0} - control message URI is {1}", ANI, SessionControlMessageUri));
                                            GetControlMessage();
                                        }
                                        else
                                        {
                                            log.Info(String.Format("{0} - Cannot find the control message uri!", ANI));
                                        }
                                    }
                                    log.Info(String.Format("{0} - Last update: {1}", ANI, DateTime.Now));
                                }
                                else
                                {
                                    if (cvSession.status.Equals("interrupted"))
                                    {
                                        log.Info(String.Format("{0} - session interrupted", ANI));
                                    }
                                    else
                                        if (cvSession.status.Equals("suspended"))
                                        {
                                            log.Info(String.Format("{0} - session suspended", ANI));
                                        }
                                        else
                                        {
                                            log.Info(String.Format("{0} - session disconnected", ANI));
                                        }
                                    SendTwilioInterruptStatus(cvSession.status);
                                }
                            }
                            else
                            {
                                log.Info(String.Format("{0} - GET {1} did not return recognizable content!", ANI, SessionUri.AbsoluteUri));
                            }
                        });
                }
                else
                {
                    log.Info(String.Format("{0} - switching on StatusCode", ANI));
                    string statusMsg;
                    switch (responseTask.Result.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                            log.Info(String.Format("{0} - HttpStatusCode.NotFound detected", ANI));
                            statusMsg = "disconnected";
                            break;
                        case HttpStatusCode.NotModified:
                            log.Info(String.Format("{0} - HttpStatusCode.NotModified detected", ANI));
                            statusMsg = "no change";
                            break;
                        default:
                            log.Info(String.Format("{0} - default case got {1}", responseTask.Result.StatusCode));
                            var msg = responseTask.Result.Content.ReadAsStringAsync().Result;
                            log.Info(String.Format("{0} - GET {1} failed! Reason - {2}, {3}, {4}",
                                                            ANI,
                                                            SessionUri.AbsoluteUri,
                                                            responseTask.Result.StatusCode,
                                                            responseTask.Result.ReasonPhrase,
                                                            msg));
                            statusMsg = "disconnected";
                            break;
                    }
                    SendTwilioInterruptStatus(statusMsg);
                }
            });
            log.Info(String.Format("{0} - after async get", ANI));
        }

        private void ContinueCVSession()
        {
            try
            {
                log.Info(String.Format("{0} - in ContinueCVSession", ANI));

                var clientUrl = new ClientUrl();
                clientUrl.url = "http://cvnet.radishsystems.com/choiceview/ivr/twilio_main_menu.html";
                log.Info(String.Format("{0} - Going to post {1} to {2}", ANI, clientUrl.url, SessionsUri.AbsoluteUri));
                Client.PostAsJsonAsync<ClientUrl>(SessionUri.AbsoluteUri, clientUrl).ContinueWith(
                    task =>
                    {
                        log.Info(String.Format("{0} - inside ContinueCVSession task", ANI));
                        if (task.Exception != null)
                        {
                            log.Info(String.Format("{0} - POST {1} application/json failed!\nError: {2}",
                                                          ANI,
                                                          SessionUri.AbsoluteUri,
                                                          task.Exception.InnerException.Message));
                            RedirectTwilio(signalURI + "/main-menu.xml");
                        }
                        else if (!task.Result.IsSuccessStatusCode)
                        {
                            var msg = task.Result.Content.ReadAsStringAsync().Result;
                            log.Info(String.Format("{0} - Post {1} application/json failed! Reason - {2}, {3}, {4}",
                                ANI,
                                SessionUri.AbsoluteUri,
                                task.Result.StatusCode,
                                task.Result.ReasonPhrase,
                                msg));
                            RedirectTwilio(signalURI + "/main-menu.xml");
                        }
                        else
                        {
                            RedirectTwilio(signalURI + "/CVMain.xml");
                        }
                    });
            }
            catch (UriFormatException ex)
            {
                log.Error(String.Format("{0} - Bad API server address - " + ex.Message, ANI));
            }
            log.Info(String.Format("{0} - leaving ContinueCVSession", ANI));
        }

        // Just here as an example of getting the properties
        private void GetProperties()
        {
            if (SessionPropertiesUri != null)
            {
                Client.GetAsync(SessionPropertiesUri).ContinueWith(
                    responseTask =>
                    {
                        if (responseTask.Result.IsSuccessStatusCode)
                        {
                            responseTask.Result.Content.ReadAsAsync<WebApiInterface.Models.Properties>(
                                new List<MediaTypeFormatter> { jsonFormatter }).ContinueWith(
                                contentTask =>
                                {
                                    WebApiInterface.Models.Properties clientInfo = contentTask.Result;
                                    if (clientInfo != null && clientInfo.properties != null)
                                    {
                                        ShowProperties(clientInfo.properties);
                                    }
                                    else
                                    {
                                        log.Info(String.Format("{0} - GET {1} did not return recognizable content!", ANI,
                                                                      SessionPropertiesUri.AbsoluteUri));
                                    }
                                });
                        }
                        else
                        {
                            switch (responseTask.Result.StatusCode)
                            {
                                case HttpStatusCode.NotFound:
                                    log.Info(String.Format("{0} - {1} was not found!", ANI, SessionPropertiesUri.AbsoluteUri));
                                    break;
                                case HttpStatusCode.NotModified:
                                    if (cvSession != null && cvSession.properties != null)
                                    {
                                        ShowProperties(cvSession.properties);
                                    }
                                    else
                                    {
                                        log.Info(String.Format("{0} - No properties to show!", ANI));
                                    }
                                    break;
                                default:
                                    var msg = responseTask.Result.Content.ReadAsStringAsync().Result;
                                    log.Info(String.Format("{0} - GET {1} failed! Reason - {2}, {3}, {4}",
                                                                  ANI,
                                                                  SessionPropertiesUri.AbsoluteUri,
                                                                  responseTask.Result.StatusCode,
                                                                  responseTask.Result.ReasonPhrase,
                                                                  msg));
                                    break;
                            }
                        }
                    });
            }
        }

        //  Just here as an example of logging the properties
        private void ShowProperties(Payload pairs)  
        {
            foreach (var pair in pairs)
            {
                log.Info(String.Format("{0} - " + pair.ToString(), ANI));
            }
        }

        private void GetControlMessage()
        {
            if (SessionControlMessageUri != null)
            {
                log.Info(String.Format("{0} - in GetControlMessage - {1}", ANI, SessionControlMessageUri));
//                String sessionID = SessionControlMessageUri.ToString();
//                int lastPos = sessionID.LastIndexOf("/");
//                sessionID = sessionID.Substring(lastPos + 1);
                
                Client.GetAsync(SessionControlMessageUri).ContinueWith(
                    responseTask =>
                    {
                        if (responseTask.Result.IsSuccessStatusCode)
                        {
                            log.Info(String.Format("{0} - in GetControlMessage - successful response received", ANI));
                            responseTask.Result.Content.ReadAsStringAsync().ContinueWith(
                                 contentTask =>
                                {
                                    log.Info(String.Format("{0} - in GetControlMessage - response read", ANI));
                                    String cmData = contentTask.Result;
                                    if (cmData != null)
                                    {
                                        log.Info(String.Format("{0} - control message data is {1}", ANI, cmData));
                                        ControlMessage CM = (ControlMessage)Newtonsoft.Json.JsonConvert.DeserializeObject(cmData, typeof(ControlMessage));
                                        log.Info(String.Format("{0} - after deserialize of CM", ANI));
                                        try
                                        {
                                            SendInterruptControl(CM.buttonName);
                                        }
                                        catch (Exception ex)
                                        {
                                            log.Info(String.Format("{0} - Exception with CM.buttonName - {1}", ANI, ex.Message));
                                            SendInterruptControl("No CM data");
                                        }
                                    }
                                    else
                                    {
                                        log.Info(String.Format("{0} - GET {1} did not return recognizable content!", ANI, SessionControlMessageUri.AbsoluteUri));
                                    }
                                });
                        }
                        else
                        {
                            log.Info(String.Format("{0} - in GetControlmessage - error response received", ANI));
                            switch (responseTask.Result.StatusCode)
                            {
                                case HttpStatusCode.NotFound:
                                    log.Info(String.Format("{0} - {1} was not found!", ANI, SessionControlMessageUri.AbsoluteUri));
                                    break;
                                case HttpStatusCode.NotModified:
                                    log.Info(String.Format("{0} - No control message to show!", ANI));
                                    break;
                                default:
                                    var msg = responseTask.Result.Content.ReadAsStringAsync().Result;
                                    log.Info(String.Format("{0} - GET {1} failed! Reason - {2}, {3}, {4}",
                                                                  ANI,
                                                                  SessionControlMessageUri.AbsoluteUri,
                                                                  responseTask.Result.StatusCode,
                                                                  responseTask.Result.ReasonPhrase,
                                                                  msg));
                                    break;
                            }
                        }
                    });
            }
        }

        private void SendInterruptControl(string option)
        {
            log.Info(String.Format("{0} - option for Interrupt is {1}", ANI, option));

            switch (option)
            {
                case "Visual_Response":
                    {
                        SendURLToCV("http://cvnet.radishsystems.com/choiceview/samples/generic_order_status.html", "CVOrderStatus.xml");
                        Thread.Sleep(10000);
                        ContinueCVSession();
                        break;
                    }
                case "Radish_Website":
                    {
                        SendURLToCV("http://www.radishsystems.com", "CVWebsite.xml");
//                        log.Info(String.Format("{0} - sleeping for 5", ANI));
                        Thread.Sleep(5000);
                        log.Info(String.Format("{0} - going to  DeleteCVSession", ANI));
                        DeleteCVSession();
//                        log.Info(String.Format("{0} - back from  DeleteCVSession", ANI));
                        break;
                    }
                case "Customer_Support":
                    {
                        SendURLToCV("http://cvnet.radishsystems.com/choiceview/samples/radish_transfer.html", "AnnounceTransfer.xml");
                        var newProperty = new ClientProperty
                        {
                            name = "transfer",
                            value = "twilio_demo"
                        };

                        log.Info(String.Format("{0} - ready to Post property", ANI));
                        PostCVProperty(newProperty);
                        log.Info(String.Format("{0} - back from Post of property", ANI));
//                        log.Info(String.Format("{0} - waiting 1", ANI));
                        Thread.Sleep(1000);
                        TransferCVSession();
                        break;
                    }
                    default:
                    {
                        log.Info(String.Format("{0} - taking default in SendInterruptControl", ANI));
                        ContinueCVSession();
                        break;
                    }
            }
        }

        private void SendTwilioInterruptStatus(string status)
        {
            log.Info(String.Format("{0} - status for Twilio Interrupt is {1}", ANI, status));

            switch (status)
            {
                case "suspended":
                    {
                        RedirectTwilio(signalURI + "/Suspended.xml");
                        break;
                    }
                case "interrupted":
                    {
                        RedirectTwilio(signalURI + "/Interrupted.xml");
                        break;
                    }
                case "connected":
                    {
                        RedirectTwilio(signalURI + "/CVMain.xml");
                        break;
                    }
                case "disconnected":
                    {
                        RedirectTwilio(signalURI + "/CVGoodBye.xml");
                        break;
                    }
                default:        // not supposed to happen per Darryl
                    {
                        DeleteCVSession();
                        log.Info(String.Format("{0} - fell into default - status is " + status, ANI));
                        RedirectTwilio(signalURI + "/CVTrouble.xml");
                        break;
                    }
            }
        }

        private void SendURLToCV(string URL, string xmlPage)
        {
            try
            {
                log.Info(String.Format("{0} - in SendURLToCV", ANI));

                var clientUrl = new ClientUrl();
                clientUrl.url = URL;
                log.Info(String.Format("{0} - Going to post {1} to {2}", ANI, clientUrl.url, SessionUri.AbsoluteUri));
                Client.PostAsJsonAsync<ClientUrl>(SessionUri.AbsoluteUri, clientUrl).ContinueWith(
                    task =>
                    {
                        log.Info(String.Format("{0} - inside SendURLToCV task", ANI));
                        if (task.Exception != null)
                        {
                            log.Info(String.Format("{0} - POST {1} application/json failed! Error: {2}",
                                                          ANI,
                                                          SessionUri.AbsoluteUri,
                                                          task.Exception.InnerException.Message));
                            RedirectTwilio(signalURI + "/main-menu.xml");
                        }
                        else if (!task.Result.IsSuccessStatusCode)
                        {
                            var msg = task.Result.Content.ReadAsStringAsync().Result;
                            log.Info(String.Format("{0} - Post {1} application/json failed! Reason - {2}, {3}, {4}",
                                ANI,
                                SessionUri.AbsoluteUri,
                                task.Result.StatusCode,
                                task.Result.ReasonPhrase,
                                msg));
                            RedirectTwilio(signalURI + "/main-menu.xml");
                        }
                        else
                        {
                            RedirectTwilio(signalURI + "/" + xmlPage);
                        }
                    });
            }
            catch (UriFormatException ex)
            {
                log.Error(String.Format("{0} - Bad API server address - " + ex.Message, ANI));
                RedirectTwilio(signalURI + "/main-menu.xml");
            }
            log.Info(String.Format("{0} - leaving SendURLToCV", ANI));
        }

        private void DeleteCVSession()
        {
            log.Info(String.Format("{0} - Starting DeleteCVSession", ANI));
            Client.DeleteAsync(SessionUri).ContinueWith(
                task =>
                    {
                        log.Info(String.Format("{0} - continuing task in DeleteCVSession", ANI));
                        if (task.Exception != null)
                        {
                            log.Info(String.Format("{0} - DELETE {1} failed! Error: {2}",
                                ANI, SessionUri.AbsoluteUri, task.Exception.InnerException.Message));
                        }
                        else if (task.Result.IsSuccessStatusCode)
                        {
                            log.Info(String.Format("{0} - CV session disconnected", ANI));
                            SendTwilioInterruptStatus("disconnected");
                        }
                        else
                        {
                            var msg = task.Result.Content.ReadAsStringAsync().Result;
                            log.Info(String.Format("{0} - DELETE {1} failed! Reason - {2}, {3}, {4}",
                                ANI,
                                SessionUri.AbsoluteUri,
                                task.Result.StatusCode,
                                task.Result.ReasonPhrase,
                                msg));
                        }
                    });

            string fullPath = "";
            try
            {
                log.Info(String.Format("{0} - filePath is {1}", ANI, filePath));
                fullPath = filePath + ANI + ".txt";
                log.Info(String.Format("{0} - fullPath is {1}", ANI, fullPath));
            }
            catch (Exception ex)
            {
                log.Info(String.Format("{0} - exception is {1}", ANI, ex.Message));
            }
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);  // remove old file
            }
            log.Info(String.Format("{0} - After Client.DeleteAsync in DeleteCVSession", ANI));
        }

        private void TransferCVSession()
        {
            string uriString = (String.Format("{0}", SessionURI));
            log.Info(String.Format("{0} - uriString is {1}", ANI, uriString)); 
            String combinedUriString = uriString + "/transfer/" + transferAccount;
            log.Info(String.Format("{0} - combinedURIString is {1}", ANI, combinedUriString));
            Uri combinedUri = new Uri(combinedUriString);
            Client.PostAsync(combinedUri, new StringContent("")).ContinueWith(
                responseTask =>
                {
                    log.Info(String.Format("{0} - inside TransferCVSession task ", ANI));
                    if (responseTask.Exception != null)
                    {
                        log.Info(String.Format("{0} - Transfer {1} failed! Error: {2}",
                            ANI, combinedUri.AbsoluteUri, responseTask.Exception.InnerException.Message));
                    }
                    else if (responseTask.Result.IsSuccessStatusCode)
                    {
                        log.Info(String.Format("{0} - CV session transferred", ANI));
                        Thread.Sleep(2000); // allow prompt to finish. otherwise, it gets interrupted by dial
                        RedirectTwilio(signalURI + "/CVTransfer.xml");
                    }
                });
                log.Info(String.Format("{0} - after Post Transfer task", ANI));
            }
    
        private void PostCVProperty(ClientProperty newProperty)
        {
            Client.PostAsJsonAsync(SessionPropertiesUri.AbsoluteUri, newProperty).ContinueWith(
                responseTask =>
                {
                    log.Info(String.Format("{0} - inside PostCVProperty task ", ANI));
                    if (responseTask.Exception != null)
                    {
                        log.Info(String.Format("{0} - Post failed - {1}", ANI, responseTask.Exception.InnerException.Message));
                    }
                    else if (responseTask.Result.IsSuccessStatusCode)
                    {
                        log.Info("CV session properties updated");
                    }
                    else
                    {
                        var msg = responseTask.Result.Content.ReadAsStringAsync().Result;
                        log.Info(String.Format("{0} - Property Update to {1} failed! Reason - {2}, {3}, {4}",
                            ANI,
                            SessionPropertiesUri.AbsoluteUri,
                            responseTask.Result.StatusCode,
                            responseTask.Result.ReasonPhrase,
                            msg));
                    }
                });
            log.Info(String.Format("{0} - leaving PostCVProperty", ANI));
        }

        private bool RedirectTwilio(string xmlPage)
        {
            TwilioRestClient Tclient;
            CallListRequest options;
            Tclient = new TwilioRestClient(AccountSid, "803c5c172df9c39096770ae982e0cefe");
            options = new CallListRequest();
            options.Count = 1;
            options.Status = "in-progress";
            var call = Tclient.RedirectCall(CallSid, xmlPage, "GET");
            if (call.RestException != null)
            {
                log.Info(String.Format("{0} - RedirectTwilio sendError Code: " + call.RestException.Code + " Error Message: " + call.RestException.Message, ANI));
                return false;
            }
            else
            {
                log.Info(String.Format("{0} - Sent " + xmlPage + " to Twilio. CallSid is " + CallSid + " AccountSid is " + AccountSid, ANI));
                return true;
            }
        }
    }
}
