using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;  
using MimeKit; 
using System.Timers;
using System.Linq.Expressions;
namespace WaterLevelAlarm
{
    class Program
    {
        public static Settings settings { get; set; }
        public static GpioController controller { get; set; }
        public static PinValue previousPinValue { get; set; }
        public static bool firstPoll { get; set; }
        public static System.Timers.Timer timer { get; set; }
        static void Main(string[] args)
        {
            // Get configuration.
            IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

            settings = config.GetRequiredSection("Settings").Get<Settings>();

            int sensorPin = settings.WaterSensorGPIOPinNumber;
            int gpioChipNum = settings.ChipNumber;
            Console.WriteLine("--GPIO and Email Configuration--");
            Console.WriteLine($"Pin: {sensorPin}");
            Console.WriteLine($"GPIO Chip number: {gpioChipNum}");
            Console.WriteLine($"Pin Sample Interval (Ms): {settings.PinSampleIntervalMS}");
            Console.WriteLine($"SMTP Server: {settings.SMTPConfiguration.SMTPServerAddress}");
            Console.WriteLine($"SMTP Server Port: {settings.SMTPConfiguration.SMTPServerPort}");
            Console.WriteLine($"SMTP Server Username: {settings.SMTPConfiguration.Username}");
            Console.WriteLine($"Sender Address: {settings.SMTPConfiguration.SenderAddress}");
            Console.WriteLine($"Email Send Max Retry Count: {settings.SMTPConfiguration.MaxRetryCount}");
            Console.WriteLine($"Alert Triggered Message subject: {settings.EmailMessageConfiguration.AlarmTriggeredMessage.Subject}");
            Console.WriteLine($"Alert Triggered Message body: {settings.EmailMessageConfiguration.AlarmTriggeredMessage.Body}");
            Console.WriteLine($"Alert Canceled Message subject: {settings.EmailMessageConfiguration.AlarmCanceledMessage.Subject}");
            Console.WriteLine($"Alert Canceled Message body: {settings.EmailMessageConfiguration.AlarmCanceledMessage.Body}");
            Console.WriteLine("Recipients");
            foreach (EmailRecipient recipient in settings.EmailRecipients)
            {
                Console.WriteLine($"\tName: {recipient.Name}, Address: {recipient.Address}");
            }
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
            // read pin.
            firstPoll = true;
            previousPinValue = controller.Read(sensorPin);
            timer = new System.Timers.Timer();
            timer.Interval = settings.PinSampleIntervalMS;
            timer.Elapsed += PollPinEvent;
            timer.Start();
            Console.WriteLine("Press any key to close.");
            // wait forever.
            Console.Read();
            controller.ClosePin(sensorPin);

        }
        private static void SendEmail(SMTPConfiguration serverConfig, List<EmailRecipient> recipients, EmailMessageConfiguration_EventSpecificConfig message)
        {

            try
            {

                using (SmtpClient client = new SmtpClient())
                {
                    client.Connect(serverConfig.SMTPServerAddress, serverConfig.SMTPServerPort, false);
                    client.Authenticate(serverConfig.Username, serverConfig.Password);
                    foreach (EmailRecipient recipient in recipients)
                    {
                        int curRetryCount = 0;
                        while (curRetryCount < serverConfig.MaxRetryCount)
                        {

                            try
                            {

                                Console.WriteLine($"Retry Count {curRetryCount}/{serverConfig.MaxRetryCount} - Sending email to {recipient.Address}.");
                                MimeMessage mailMessage = new MimeMessage();
                                mailMessage.From.Add(new MailboxAddress("Water Level Meter", serverConfig.SenderAddress));
                                mailMessage.To.Add(new MailboxAddress(recipient.Name, recipient.Address));
                                mailMessage.Subject = message.Subject;
                                mailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = $"{recipient.Name}, \n {message.Body}" };
                                client.Send(mailMessage);
                                Console.WriteLine("Success!");
                                break;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"Failure! Exception message {e}");
                                Thread.Sleep(250);
                                curRetryCount++;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"SMTP Connection Error. Exception message: {e}");
            }
        }

        private static void PollPinEvent(object? sender, ElapsedEventArgs eventArgs)
        {
            timer.Stop();
            if (firstPoll)
            {
                firstPoll = false;
                if (previousPinValue == PinValue.High)
                {
                    // send an alert immediately.
                    Console.WriteLine("Alarm Triggered. Sending email.");
                    SendEmail(settings.SMTPConfiguration, settings.EmailRecipients, settings.EmailMessageConfiguration.AlarmTriggeredMessage);
                }
            }
            else
            {
                PinValue currentPollPinValue = controller.Read(settings.WaterSensorGPIOPinNumber);
                if (currentPollPinValue != previousPinValue)
                {
                    if (currentPollPinValue == PinValue.Low)
                    {
                        Console.WriteLine("Alarm Canceled. Sending email.");
                        SendEmail(settings.SMTPConfiguration, settings.EmailRecipients, settings.EmailMessageConfiguration.AlarmCanceledMessage);
                    }
                    else if (currentPollPinValue == PinValue.High)
                    {
                        Console.WriteLine("Alarm Triggered. Sending email.");
                        SendEmail(settings.SMTPConfiguration, settings.EmailRecipients, settings.EmailMessageConfiguration.AlarmTriggeredMessage);
                    }
                    previousPinValue = currentPollPinValue;
                }
            }
            timer.Start();
            
        }
    }
}
