using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace IoT.Samples.EventIngest.Entity
{
    /// <summary>
    /// Class to manage sensor data and attributes 
    /// </summary>
    public class Event
    {
        public string Id { get; set; }

        public string Timecreated { get; set; }

        public string Value { get; set; }

        /// <summary>
        /// ToJson function is used to convert sensor data into a JSON string to be sent to Azure Event Hub
        /// </summary>
        /// <returns>JSon String containing all info for sensor data</returns>
        public string ToJson()
        {
            var jsonSerializer = new DataContractJsonSerializer(typeof(Event)); // using .NET serializer to avoid adding JSON.net on device
            string output;
            using (var stream = new MemoryStream())
            {
                jsonSerializer.WriteObject(stream, this);
                output = Encoding.UTF8.GetString(stream.ToArray(), 0, (int)stream.Length);

            }

            return output;
        }
    }
}
