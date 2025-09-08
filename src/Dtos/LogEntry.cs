namespace Soenneker.Serilog.Sinks.Cache.Dtos;

internal readonly struct Entry
{
    public readonly string Line;
    public readonly int Bytes; // approx UTF-16

    internal Entry(string line)
    {
        Line = line;
        Bytes = checked(line.Length * 2);
    }
}