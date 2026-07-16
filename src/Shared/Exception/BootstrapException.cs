namespace PSBootstrap.Shared.Exception;

public class BootstrapException : System.Exception
{
    public BootstrapException(string message) : base(message)
    {
    }

    public BootstrapException(string message, System.Exception innerException) : base(message, innerException)
    {
    }
}