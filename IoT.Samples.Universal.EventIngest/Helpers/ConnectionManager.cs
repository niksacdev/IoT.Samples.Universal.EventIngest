using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Web.Http;
using IoT.Samples.EventIngest.Entity;

namespace IoT.Samples.Universal.EventIngest.Helpers
{
    public class ConnectionManager
    {
        // App Settings variables
        public AppSettings Settings = new AppSettings();

        // Http connection string, SAS tokem and client
        Uri _uri;
        private string _sas;
        readonly HttpClient _httpClient = new HttpClient();
        bool _eventHubConnectionInitialized = false;

        public ConnectionManager(string serviceBusNamespace = "",
            string eventHubName = "",
            string keyName = "",
            string key = "")
        {
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
        bool ValidateSettings()
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
        public bool SaveSettings()
        {
            if (ValidateSettings())
            {
                this.InitEventHubConnection();
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

        /// <summary>
        /// Send message to Azure Event Hub using HTTP/REST API
        /// </summary>
        /// <param name="message"></param>
        private async Task<bool> SendMessage(string message)
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

        /// <summary>
        /// Helper function to get SAS token for connecting to Azure Event Hub
        /// </summary>
        /// <returns></returns>
        private string SasTokenHelper()
        {
            int expiry = (int)DateTime.UtcNow.AddMinutes(20).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string stringToSign = WebUtility.UrlEncode(this._uri.ToString()) + "\n" + expiry.ToString();
            string signature = HmacSha256(this.Settings.Key.ToString(), stringToSign);
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
        public string HmacSha256(string key, string value)
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
        public bool InitEventHubConnection()
        {
            try
            {
                // TODO: use AMQP.net instead
                this._uri = new Uri("https://" + this.Settings.ServicebusNamespace +
                              ".servicebus.windows.net/" + this.Settings.EventHubName +
                              "/publishers/" + this.Settings.Id + "/messages");

                this._sas = SasTokenHelper();
                this._httpClient.DefaultRequestHeaders.Authorization = new Windows.Web.Http.Headers.HttpCredentialsHeaderValue("SharedAccessSignature", _sas);
                this._eventHubConnectionInitialized = true;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
