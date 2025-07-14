public class Settings
{
    public int WaterSensorGPIOPinNumber { get; set; }
    public int ChipNumber { get; set; }
    public int PinSampleIntervalMS { get; set; }
    public required List<EmailRecipient> EmailRecipients { get; set; }
    public required SMTPConfiguration SMTPConfiguration { get; set; }
    public required EmailMessageConfiguration EmailMessageConfiguration { get; set; }
}
public class SMTPConfiguration
{
    public required string SMTPServerAddress { get; set; }
    public int SMTPServerPort { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string SenderAddress { get; set; }
    public int MaxRetryCount { get; set; }
}
public class EmailMessageConfiguration
{
    public required EmailMessageConfiguration_EventSpecificConfig AlarmTriggeredMessage { get; set; }
    public required EmailMessageConfiguration_EventSpecificConfig AlarmCanceledMessage { get; set; }
}
public class EmailRecipient {
    public required string Name { get; set; }
    public required string Address { get; set; }
}
public class PhoneNumber
{
    public required string Name { get; set; }
    public required string Number { get; set; }
}
public class EmailMessageConfiguration_EventSpecificConfig
{
    public required string Subject { get; set; }
    public required string Body { get; set; }
}