namespace Mercury.Models.Auth;

public class Device
{
    public Guid DeviceId { get; init; }
    public string? UserAgent { get; set; }
    public string? IPAddress { get; set; }
    public string? DeviceName { get; set; }
}
