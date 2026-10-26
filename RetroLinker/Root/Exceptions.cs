namespace RetroLinker;

public class CommandLineParserException : ArgumentException
{
    private const string DefaultMessage = "An error ocurred while parsing command line arguments.";
    
    public CommandLineParserException() : base(DefaultMessage) { }
    public CommandLineParserException(string message) : base(message) { }
    public CommandLineParserException(string message, string? paramName) : base(message,  paramName) { }
    public CommandLineParserException(string message, Exception innerException) : base(message, innerException) { }
    public CommandLineParserException(string message, string? paramName, Exception innerException) : base(message, paramName, innerException) { }
}