using System;
using PSBootstrap.Shared.Enum;

namespace PSBootstrap.Shared.Entity;

public sealed record class Log(DateTime Timestamp, LogType LogType, string Message, string ComputerName, string Username)
{
    public DateTime Timestamp { get; } = Timestamp;
    public LogType LogType { get; } = LogType;
    public string Message { get; } = Message;
    public string ComputerName { get; } = ComputerName;
    public string Username { get; } = Username;
}