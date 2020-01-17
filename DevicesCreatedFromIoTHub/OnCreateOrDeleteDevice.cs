using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Opisense.Client;

namespace Opisense.Samples.Azure.IoTHub.DevicesCreatedFromIoTHub
{
    public static class OnCreateOrDeleteDevice
    {
        private static readonly int DefaultSiteIdForNewSources = int.Parse(Environment.GetEnvironmentVariable("DefaultSiteIdForNewSources", EnvironmentVariableTarget.Process));

        [FunctionName("OnCreateOrDeleteDevice")]
        public static async Task RunCreateDeleteDevice([EventHubTrigger("createdelete", Connection = "EventHub")]
            EventData[] messages, ILogger log)
        {
            foreach (var message in messages.Deserialize<dynamic>())
            {
                if (message.eventType == "Microsoft.Devices.DeviceCreated")
                {
                    log.LogInformation("Received DeviceCreated message. Will create source and variables");

                    var sourceId = await OpisenseClient.CreateOpisenseSource(new
                    {
                        siteId = DefaultSiteIdForNewSources,
                        serialNumber = message.data.twin.deviceId,
                        energyTypeId = 17,
                        name = message.data.twin.deviceId,
                        sourceTypeId = 72,
                        timeZoneId = "UTC"
                    }, new List<object>
                    {
                        new
                        {
                            name = "Temperature",
                            unitId = 28, // Celsius Degrees
                            granularity = 15,
                            granularityTimeBase = "Minute",
                            quantityType = "Instantaneous",
                            mappingConfig = "temperature"
                        },
                        new
                        {
                            name = "Humidity",
                            unitId = 31, // Percentages
                            granularity = 15,
                            granularityTimeBase = "Minute",
                            quantityType = "Instantaneous",
                            mappingConfig = "humidity"
                        }
                    });
                    log.LogInformation($"Successfully created source<{sourceId}>");
                }
                else
                {
                    log.LogInformation($"Received <{message.eventType}> message. Ignoring...");
                }
            }
        }
    }
}
