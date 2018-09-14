using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;

namespace Opisense.Samples.Azure.IoTHub.DevicesCreatedFromIoTHub
{
    public static class EventDataExtensions
    {
        public static List<T> Deserialize<T>(this EventData[] messages)
        {
            return messages.SelectMany(eventData => JsonConvert.DeserializeObject<List<T>>(Encoding.UTF8.GetString(eventData.Body.Array))).ToList();
        }
    }
}