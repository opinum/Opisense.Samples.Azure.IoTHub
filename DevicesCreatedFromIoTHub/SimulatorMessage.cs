namespace Opisense.Samples.Azure.IoTHub.DevicesCreatedFromIoTHub
{
    public class SimulatorMessage
    {
        public string MessageId { get; set; }
        public string DeviceId { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public string Date { get; set; }
    }
}