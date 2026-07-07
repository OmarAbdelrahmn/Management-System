namespace Express_Service;

public class SeedOptions
{
    public bool Enabled { get; set; }
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
    public string AdminFullName { get; set; } = "System Administrator";
    public bool CreateDemoBoard { get; set; }
}
