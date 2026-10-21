using System;

namespace RetroLinker;

public class CommandLineParserExceptions : ArgumentException
{
    private const string DefaultMessage = "An error ocurred while parsing command line arguments.";
    
    public CommandLineParserExceptions() : base(DefaultMessage) { }
    public CommandLineParserExceptions(string message) : base(message) { }
    public CommandLineParserExceptions(string message, string? paramName) : base(message,  paramName) { }
    public CommandLineParserExceptions(string message, Exception innerException) : base(message, innerException) { }
    public CommandLineParserExceptions(string message, string? paramName, Exception innerException) : base(message, paramName, innerException) { }
}