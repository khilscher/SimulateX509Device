using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using System.Security.Cryptography.X509Certificates;

namespace SimulateX509Device
{
    class Program
    {
        private static int MESSAGE_COUNT = 5000;
        private static int DELAY_BETWEEN_SENDS = 2000;
        private const int TEMPERATURE_THRESHOLD = 30;
        private static float temperature;
        private static float humidity;
        private static Random rnd = new Random();

        // Device ID registered in IoT Hub.
        private static string deviceId = "device4302021";

        // Path to pfx file with device cert. Generate this using openssl
        // CN in cert must match deviceId above and signed by root or intermediate cert uploaded to IoT Hub.
        // e.g. openssl pkcs12 -export -out device123.pfx -inkey device123.key -in device123.cer 
        private static string pfxPath = @"c:\openssl_stuff\device4302021.pfx"; 

        // IoT Hub FQDN. Ensure root or intermediate cert has been uploaded to IoT Hub and verified.
        private static string iotHubFqdn = "youriothub.azure-devices.net";

        // Based on https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-security-x509-get-started#create-an-x509-device-for-your-iot-hub

        static void Main(string[] args)
        {
            try
            {

                var cert = new X509Certificate2(pfxPath);

                Console.WriteLine("Certificate Info:");
                Console.WriteLine($"Subject: {cert.Subject}");
                Console.WriteLine($"Expires: {cert.NotAfter}");

                var auth = new DeviceAuthenticationWithX509Certificate(deviceId, cert);

                var deviceClient = DeviceClient.Create(iotHubFqdn, auth, TransportType.Mqtt_Tcp_Only);

                if (deviceClient == null)
                {

                    Console.WriteLine("Failed to create DeviceClient!");

                }
                else
                {

                    Console.WriteLine("Successfully created DeviceClient using x.509 certificate.");
                    SendEvent(deviceClient).Wait();

                }

                Console.WriteLine("Exiting...\n");

            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error: {ex.Message}");

            }

            Console.ReadLine();
        }

        static async Task SendEvent(DeviceClient deviceClient)
        {

            string dataBuffer;
            Console.WriteLine("Device sending {0} messages to IoTHub...\n", MESSAGE_COUNT);

            for (int count = 0; count < MESSAGE_COUNT; count++)
            {

                temperature = rnd.Next(20, 35);
                humidity = rnd.Next(60, 80);
                dataBuffer = string.Format("{{\"deviceId\":\"{0}\",\"messageId\":{1},\"temperature\":{2},\"humidity\":{3}}}", deviceId, count, temperature, humidity);
                Message eventMessage = new Message(Encoding.UTF8.GetBytes(dataBuffer));
                eventMessage.Properties.Add("temperatureAlert", (temperature > TEMPERATURE_THRESHOLD) ? "true" : "false");
                Console.WriteLine("\t{0}> Sending message: {1}, Data: [{2}]", DateTime.Now.ToLocalTime(), count, dataBuffer);

                try
                {

                    await deviceClient.SendEventAsync(eventMessage);

                    await Task.Delay(DELAY_BETWEEN_SENDS);

                }
                catch(Exception e)
                {

                    Console.WriteLine(e.Message);

                }

            }
        }
    }
}
