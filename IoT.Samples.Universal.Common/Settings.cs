using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Newtonsoft.Json;

namespace IoT.Samples.Universal.Common
{
    /// <summary>
    ///  Class for managing app settings
    /// </summary>
    public class Settings
    {
        // Our settings
        readonly ApplicationDataContainer _localSettings;

        // The key names of our settings
        const string SettingsSetKeyname = "settingsset";
        const string ServicebusNamespaceKeyname = "namespace";
        const string EventHubNameKeyname = "eventhubname";
        const string KeyNameKeyname = "keyname";
        const string KeyKeyname = "key";
        const string IdKeyname = "id";

        // The default value of our settings
        const bool SettingsSetDefault = false;
        const string ServicebusNamespaceDefault = "";
        const string EventHubNameDefault = "";
        const string KeyNameDefault = "";
        const string KeyDefault = "";
        string IdDefault = Guid.NewGuid().ToString();

        /// <summary>
        /// Constructor that gets the application settings.
        /// </summary>
        public Settings()
        {
            // Get the settings for this application.
            _localSettings = ApplicationData.Current.LocalSettings;
        }

        /// <summary>
        /// Update a setting value for our application. If the setting does not
        /// exist, then add the setting.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool AddOrUpdateValue(string Key, Object value)
        {
            bool valueChanged = false;

            // If the key exists
            if (_localSettings.Values.ContainsKey(Key))
            {
                // If the value has changed
                if (_localSettings.Values[Key] != value)
                {
                    // Store the new value
                    _localSettings.Values[Key] = value;
                    valueChanged = true;
                }
            }
            // Otherwise create the key.
            else
            {
                _localSettings.Values.Add(Key, value);
                valueChanged = true;
            }
            return valueChanged;
        }

        /// <summary>
        /// Get the current value of the setting, or if it is not found, set the 
        /// setting to the default setting.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetValueOrDefault<T>(string Key, T defaultValue)
        {
            T value;

            // If the key exists, retrieve the value.
            if (_localSettings.Values.ContainsKey(Key))
            {
                value = (T)_localSettings.Values[Key];
            }
            // Otherwise, use the default value.
            else
            {
                value = defaultValue;
            }
            return value;
        }

        /// <summary>
        /// Save the settings.
        /// </summary>
        public void Save()
        {
        }


        /// <summary>
        /// Property to get and set a Username Setting Key.
        /// </summary>
        public bool SettingsSet
        {
            get
            {
                return GetValueOrDefault<bool>(SettingsSetKeyname, SettingsSetDefault);
            }
            set
            {
                if (AddOrUpdateValue(SettingsSetKeyname, value))
                {
                    Save();
                }
            }
        }

        /// <summary>
        /// Property to get and set a Username Setting Key.
        /// </summary>
        public string Id
        {
            get
            {
                return GetValueOrDefault(IdKeyname, IdDefault);
            }
            set
            {
                if (AddOrUpdateValue(IdKeyname, value))
                {
                    Save();
                }
            }
        }


        /// <summary>
        /// Property to get and set a Username Setting Key.
        /// </summary>
        public string ServicebusNamespace
        {
            get
            {
                return GetValueOrDefault<string>(ServicebusNamespaceKeyname, ServicebusNamespaceDefault);
            }
            set
            {
                if (AddOrUpdateValue(ServicebusNamespaceKeyname, value))
                {
                    Save();
                }
            }
        }

        /// <summary>
        /// Property to get and set a Username Setting Key.
        /// </summary>
        public string EventHubName
        {
            get
            {
                return GetValueOrDefault<string>(EventHubNameKeyname, EventHubNameDefault);
            }
            set
            {
                if (AddOrUpdateValue(EventHubNameKeyname, value))
                {
                    Save();
                }
            }
        }
        /// <summary>
        /// Property to get and set a Username Setting Key.
        /// </summary>
        public string KeyName
        {
            get
            {
                return GetValueOrDefault<string>(KeyNameKeyname, KeyNameDefault);
            }
            set
            {
                if (AddOrUpdateValue(KeyNameKeyname, value))
                {
                    Save();
                }
            }
        }
        /// <summary>
        /// Property to get and set a Username Setting Key.
        /// </summary>
        public string Key
        {
            get
            {
                return GetValueOrDefault<string>(KeyKeyname, KeyDefault);
            }
            set
            {
                if (AddOrUpdateValue(KeyKeyname, value))
                {
                    Save();
                }
            }
        }

        public static Settings GetSettings(string json)
        {
            return JsonConvert.DeserializeObject<Settings>(json);
        }
    }
}
