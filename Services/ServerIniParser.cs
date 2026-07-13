using System.Text;
using TSProxyCapture.Models;

namespace TSProxyCapture.Services;

/// <summary>
/// Reads and parses Server.ini file.
/// Format: ServerName*ServerIP (one entry per line, Windows-874 encoding)
/// </summary>
public static class ServerIniParser
{
    public static List<ServerEntry> Parse(string filePath)
    {
        var entries = new List<ServerEntry>();

        if (!File.Exists(filePath))
            return entries;

        try
        {
            var lines = File.ReadAllLines(filePath, Encoding.GetEncoding(874));
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Use LastIndexOf in case the name contains '*'
                int starIndex = line.LastIndexOf('*');
                if (starIndex < 0 || starIndex >= line.Length - 1)
                    continue;

                string name = line[..starIndex].Trim();
                string ip = line[(starIndex + 1)..].Trim();

                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(ip))
                    entries.Add(new ServerEntry(name, ip));
            }
        }
        catch (Exception)
        {
            // Silent failure - return whatever was parsed so far
        }

        return entries;
    }
}
