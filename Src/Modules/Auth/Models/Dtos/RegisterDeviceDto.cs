namespace Mercury.Models.Auth;

public class RegisterDeviceDto
{
    public required string UserAgent { get; init; }
    public required string IpAddress { get; init; }
}
