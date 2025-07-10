using System.Device.Gpio;
using System.Device.Gpio.Drivers;
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
            int gpioChipNum = settings != null ? settings.ChipNumber : 1;
            Console.WriteLine($"Pin: {sensorPin}");
            Console.WriteLine($"GPIO Chip number: {gpioChipNum}");
            GpioController controller;
            // System must have libgpiod <V2 installed. 
            if (OperatingSystem.IsLinux())
            {
#pragma warning disable SDGPIO0001
                LibGpiodV2Driver driver = new LibGpiodV2Driver(gpioChipNum);
                controller = new GpioController(driver);
            }
            else
            {
                throw new PlatformNotSupportedException("OS not supported. Only Linux is currently supported.");
            }

            
            controller.OpenPin(sensorPin, PinMode.Input);
            
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
