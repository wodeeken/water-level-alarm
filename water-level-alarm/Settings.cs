public class Settings
{
    public int WaterSensorGPIOPinNumber { get; set; }
    public int ChipNumber { get; set; }
    public List<Email> Emails { get; set; }
    public List<PhoneNumber> PhoneNumbers { get; set; }
    public EmailConfiguration EmailConfiguration { get; set; }
    public SMSConfiguration SMSConfiguration { get; set; }
    public EmailMessageConfiguration EmailMessageConfiguration { get; set; }
    public SMSMessageConfiguration SMSMessageConfiguration { get; set; }
}
public class EmailConfiguration {

}
public class EmailMessageConfiguration
{

}
public class SMSMessageConfiguration
{
    
}
public class SMSConfiguration {

}
public class Email {
    public string Name { get; set; }
    public string Address { get; set; }
}
public class PhoneNumber {
    public string Name { get; set; }
    public string Number { get; set; }
}