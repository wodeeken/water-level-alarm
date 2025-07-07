using System.Device.Gpio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
namespace LoraArduCAMHostApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Get configuration.
            IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
            
            Settings? settings = config.GetRequiredSection("Settings").Get<Settings>();


            int sensorPin = settings != null ? settings.WaterSensorGPIOPinNumber : 262;
            Console.WriteLine($"Pin: {sensorPin}");
            GpioController controller = new GpioController();
            controller.OpenPin(sensorPin, PinMode.InputPullDown);
            while (true)
            {
                PinValue buttonState = controller.Read(sensorPin);

                if (buttonState == PinValue.Low)
                {
                    Console.WriteLine("0");
                }
                else
                {
                    Console.WriteLine("1");
                }

                Thread.Sleep(100); 
            }
        }

    }
}
