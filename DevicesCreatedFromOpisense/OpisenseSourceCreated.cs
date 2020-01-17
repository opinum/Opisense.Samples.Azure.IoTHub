using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Opisense.Client;

namespace Opisense.Samples.Azure.IoTHub.DevicesCreatedFromOpisense
{
    public static class OpisenseSourceCreated
    {
        static RegistryManager registryManager;
        static readonly string connString = Environment.GetEnvironmentVariable("AzureIotHub", EnvironmentVariableTarget.Process);

        [FunctionName("OpisenseSourceCreated")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, ILogger log)
        {
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic source = JsonConvert.DeserializeObject(requestBody);
            if (source == null)
            {
                log.LogError("Request Body cannot be empty");
                return new BadRequestObjectResult("Request Body cannot be empty");
            }

            string meterNumber = source.MeterNumber;
            int? id = source.Id;

            if (string.IsNullOrWhiteSpace(meterNumber))
            {
                log.LogError("Meter Number cannot be empty");
                return new BadRequestObjectResult("Meter Number cannot be empty");
            }

            if (!id.HasValue)
            {
                log.LogError("Id cannot be empty");
                return new BadRequestObjectResult("Id cannot be empty");
            }

            try
            {
                registryManager = RegistryManager.CreateFromConnectionString(connString);

                log.LogInformation("Creating device");

                var primaryKey = await CreateDeviceIdentity(meterNumber, log);

                var patchDocument = new JsonPatchDocument();
                patchDocument.Add("/clientData/IoT Hub Info/Security/Primary Key", primaryKey);

                log.LogInformation("Patching Source");
                await OpisenseClient.PatchOpisenseSource(id.Value, patchDocument);

                return new OkObjectResult($"Device {meterNumber} created.");
            }
            catch (Exception e)
            {
                log.LogError(e.Message, e);
                return new ExceptionResult(e, true);
            }
        }

        private static async Task<string> CreateDeviceIdentity(string deviceId, ILogger log)
        {
            var device = new Device(deviceId);
            var newDevice = new Device();

            var primaryKey = Guid.NewGuid();
            var secondaryKey = Guid.NewGuid();

            var bytes = Encoding.UTF8.GetBytes(primaryKey.ToString());
            var base64PrimaryKey = Convert.ToBase64String(bytes);

            bytes = Encoding.UTF8.GetBytes(secondaryKey.ToString());
            var base64SecondaryKey = Convert.ToBase64String(bytes);

            try
            {
                device.Authentication = new AuthenticationMechanism
                {
                    SymmetricKey = new SymmetricKey
                    {
                        PrimaryKey = base64PrimaryKey,
                        SecondaryKey = base64SecondaryKey
                    }
                };

                newDevice = await registryManager.AddDeviceAsync(device);
            }
            catch (Exception e)
            {
                log.LogError(e.Message, e);
            }

            return newDevice.Authentication.SymmetricKey.PrimaryKey;
        }
    }
}
