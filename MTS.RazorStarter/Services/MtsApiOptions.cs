namespace MTS.RazorStarter.Services;

public class MtsApiOptions
{
    public string BaseUrl { get; set; } = "http://192.168.75.100:80/";
    public int TimeoutSeconds { get; set; } = 30;
}
