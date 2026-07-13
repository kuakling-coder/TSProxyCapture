namespace TSProxyCapture.Models;

/// <summary>
/// Represents a server entry parsed from Server.ini
/// Pattern: ServerName*ServerIP
/// </summary>
public record ServerEntry(string Name, string IpAddress)
{
    public override string ToString() => Name;
}
