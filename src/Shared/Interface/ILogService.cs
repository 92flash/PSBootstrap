#nullable enable
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PSBootstrap.Shared.Entity;
using PSBootstrap.Shared.Enum;
using PSBootstrap.Shared.Value_object;

namespace PSBootstrap.Shared.Interface;

public interface ILogService : IDisposable
{
    public void Clear();
    public void Close();
    public void Delete();
    internal bool FindMatch(string[] matches, int tailBytes = 4096);
    public List<Log> Get(Regex headerMatch, Regex logEntryMatch);
    internal bool IsFileEmpty();
    public void Append(string message, LogType logType, bool addDetails = true);
}