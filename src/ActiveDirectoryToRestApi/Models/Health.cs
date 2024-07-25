namespace ActiveDirectoryToRestApi.Models;

public class Health
{
    public HealthData Data { get; set; } = new();
}

public class HealthData
{
    public bool Health { get; set; } = true;
}