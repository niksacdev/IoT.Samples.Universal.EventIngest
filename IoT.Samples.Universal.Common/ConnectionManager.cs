using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Web.Http;
using Amqp;
using Amqp.Framing;

namespace IoT.Samples.Universal.Common
{
    public class ConnectionManager
    {
        // App Settings variables
        public Settings Settings = new Settings();

        // Http connection string, SAS tokem and client
        Uri _uri;
        private string _sas;
        readonly HttpClient _httpClient = new HttpClient();
        bool _eventHubConnectionInitialized = false;
        private Address _address;
        private Connection _connection;
        private Session _session;
        private SenderLink _sender;

        public Protocol Protocol { get; set; }

        public ConnectionManager(Settings settings)
        {
            // load settings from json

            Settings.ServicebusNamespace = settings.ServicebusNamespace;
            Settings.EventHubName = settings.EventHubName;
            Settings.KeyName = settings.KeyName;
            Settings.Key = settings.Key;

            SaveSettings();
        }
        public ConnectionManager(string serviceBusNamespace = "",
            string eventHubName = "",
            string keyName = "",
            string key = "")
        {
            // load settings from json

            Settings.ServicebusNamespace = serviceBusNamespace;
            Settings.EventHubName = eventHubName;
            Settings.KeyName = keyName;
            Settings.Key = key;

            SaveSettings();
        }

        /// <summary>
        /// Validate the settings 
        /// </summary>
        /// <returns></returns>
        private bool ValidateSettings()
        {
            if ((Settings.ServicebusNamespace == "") ||
                (Settings.EventHubName == "") ||
                (Settings.KeyName == "") ||
                (Settings.Key == ""))
            {
                this.Settings.SettingsSet = false;
                return false;
            }

            this.Settings.SettingsSet = true;
            return true;

        }

        /// <summary>
        /// Apply new settings to sensors collection
        /// </summary>
        private bool SaveSettings()
        {
            if (ValidateSettings())
            {
                this.InitializeEventHubConnection(Protocol.Amqp);
                return true;
            } else {
                return false;
            }
        }

        public async Task<bool> SendEvent(Event eventStream)
        {
            eventStream.Timecreated = DateTime.UtcNow.ToString("mm:dd:yyyy hh:mm:ss");
            return await SendMessage(eventStream.ToJson());
        }

        private async Task<bool> SendMessage(string message)
        {
            var protocol = Protocol;
            switch (protocol)
            {
                    case Protocol.Amqp:
                    return await SendMessageAmqp(message);
                case Protocol.Https:
                    return await SendMessageHttps(message);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Send message to Azure Event Hub using HTTP/REST API
        /// </summary>
        /// <param name="message"></param>
        private async Task<bool> SendMessageHttps(string message)
        {
            if (!this._eventHubConnectionInitialized) return false;
            try
            {
                var content = new HttpStringContent(message, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");
                var postResult = await _httpClient.PostAsync(_uri, content);

                if (postResult.IsSuccessStatusCode)
                {
                    Debug.WriteLine("Message Sent: {0}", content);
                }
                else
                {
                    Debug.WriteLine("Failed sending message: {0}", postResult.ReasonPhrase);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception when sending message:" + e.Message);
                return false;
            }
        }

        private async Task<bool> SendMessageAmqp(string message)
        {
            //TODO: figure out if AMQP.NET lite support async method calls
            // construct message
            var messageValue = Encoding.UTF8.GetBytes(message);

            // here, AMQP supports 3 types of body, here we use Data.
            var formattedMessage = new Message { BodySection = new Data { Binary = messageValue } };
            _sender.Send(formattedMessage, null, null); // Send the message 
            // _connection.Close(); // close connection
            return true;
        }

        /// <summary>
        /// Helper function to get SAS token for connecting to Azure Event Hub
        /// </summary>
        /// <returns></returns>
        private string SasTokenHelper()
        {
            int expiry = (int)DateTime.UtcNow.AddMinutes(20).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string stringToSign = WebUtility.UrlEncode(this._uri.ToString()) + "\n" + expiry.ToString();
            string signature = HmacSha256(this.Settings.Key, stringToSign);
            string token = $"sr={WebUtility.UrlEncode(this._uri.ToString())}&sig={WebUtility.UrlEncode(signature)}&se={expiry}&skn={this.Settings.KeyName.ToString()}";
            return token;
        }

        /// <summary>
        /// Because Windows.Security.Cryptography.Core.MacAlgorithmNames.HmacSha256 doesn't
        /// exist in WP8.1 context we need to do another implementation
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private string HmacSha256(string key, string value)
        {
            var keyStrm = CryptographicBuffer.ConvertStringToBinary(key, BinaryStringEncoding.Utf8);
            var valueStrm = CryptographicBuffer.ConvertStringToBinary(value, BinaryStringEncoding.Utf8);

            var objMacProv = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha256);
            var hash = objMacProv.CreateHash(keyStrm);
            hash.Append(valueStrm);

            return CryptographicBuffer.EncodeToBase64String(hash.GetValueAndReset());
        }

        /// <summary>
        /// Initialize Event Hub connection
        /// </summary>
        public bool InitializeEventHubConnection(Protocol protocol)
        {
            try
            {
                var ehName = this.Settings.EventHubName;
                var hostName = this.Settings.ServicebusNamespace + ".servicebus.windows.net";
                var ehAddress = hostName + "/" + ehName;
                var publisher = this.Settings.Id;
                switch (protocol)
                {
                    case Protocol.Https:
                        this._uri = new Uri("https://" + ehAddress + "/publishers/" + publisher + "/messages");
                        this._sas = SasTokenHelper();
                        this._httpClient.DefaultRequestHeaders.Authorization = new Windows.Web.Http.Headers.HttpCredentialsHeaderValue("SharedAccessSignature", _sas);
                        this._eventHubConnectionInitialized = true;
                        return true;
                    case Protocol.Amqp:
                        // create address
                        this._address = new Address(hostName, 5671, this.Settings.KeyName, this.Settings.Key);

                        // create connection
                        this._connection = new Connection(this._address);

                        // create session
                        _session = new Session(_connection);

                        // create sendlink
                        _sender = new SenderLink(_session,"send-link:" + ehName,string.Format(CultureInfo.InvariantCulture, "{0}/Publishers/{1}", ehName, publisher));
                        // this._sas = SasTokenHelper();

                        this._eventHubConnectionInitialized = true;

                        return true;
                    default:
                        return false;

                }
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
