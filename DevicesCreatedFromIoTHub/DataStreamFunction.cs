using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Opisense.Client;

namespace Opisense.Samples.Azure.IoTHub.DevicesCreatedFromIoTHub
{
    public static class DataStreamFunction
    {
        [FunctionName("OnDataStream")]
        public static async Task RunDataStream([EventHubTrigger("datastream", Connection = "EventHub")]
            EventData[] messages, ILogger log)
        {
            var data = new List<OpisenseData>();
            foreach (var eventData in messages)
            {
                var message =
                    JsonConvert.DeserializeObject<SimulatorMessage>(Encoding.UTF8.GetString(eventData.Body.Array));

                data.Add(new OpisenseData
                {
                    MeterNumber = message.DeviceId,
                    MappingConfig = "Temperature",
                    Data = new List<OpisenseDataPoint>
                    {
                        new OpisenseDataPoint
                        {
                            Date = eventData.SystemProperties.EnqueuedTimeUtc,
                            Value = message.Temperature
                        }
                    }
                });
                data.Add(new OpisenseData
                {
                    MeterNumber = message.DeviceId,
                    MappingConfig = "Humidity",
                    Data = new List<OpisenseDataPoint>
                    {
                        new OpisenseDataPoint
                        {
                            Date = eventData.SystemProperties.EnqueuedTimeUtc,
                            Value = message.Humidity
                        }
                    }
                });
            }

            await OpisenseClient.SendData(data.Merge());
        }
    }
}