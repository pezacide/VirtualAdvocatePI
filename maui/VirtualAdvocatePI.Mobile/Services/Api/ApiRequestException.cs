namespace VirtualAdvocatePI.Mobile.Services.Api;

public sealed class ApiRequestException : Exception
{
    public ApiRequestException(string message) : base(message)
    {
    }
}
