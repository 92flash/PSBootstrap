using PSBootstrap.Shared.Exception;
using PSBootstrap.Shared.Value_object;

namespace PSBootstrap.Shared.Context;

// Allow saving preferences extracted from the Bootstrap script config so it can be used in other cmdlets in the same session
internal static class BootstrapContext
{
    private static readonly object _lock = new();
    private static LogPath _logPath;

    internal static LogPath LogPath 
    {
        get =>_logPath;
        set
        {
            lock (_lock)
            {
                if (_logPath != null && _logPath.Path != value.Path)
                {
                    throw new BootstrapException($"LogPath was already set to '{_logPath}' by another bootstrap process running in this session. Cannot set a different LogPath ('{value}') while it's in use.");
                }

                _logPath = value;
            }
        }
    }
    internal static bool ShowShellOutput { get; set; }
}