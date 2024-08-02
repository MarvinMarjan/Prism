using System;


namespace Specter.Debug.Prism.Exceptions;


[Serializable]
public class CommandException : Exception
{
	public CommandException() { }
	public CommandException(string message) : base(message) { }
	public CommandException(string message, Exception inner) : base(message, inner) { }


	public override string ToString()
		=> Message;
}



[Serializable]
public class InvalidTokenException : CommandException
{
	public InvalidTokenException() { }
	public InvalidTokenException(string message) : base(message) { }
	public InvalidTokenException(string message, Exception inner) : base(message, inner) { }
}



[Serializable]
public class UnclosedStringException : CommandException
{
	public UnclosedStringException() { }
	public UnclosedStringException(string message) : base(message) { }
	public UnclosedStringException(string message, Exception inner) : base(message, inner) { }
}



[Serializable]
public class CommandNotFoundException : CommandException
{
	public string? Command { get; init; }


	public CommandNotFoundException() { }
	public CommandNotFoundException(string command, string message) : base(message)
	{
		Command = command;
	}

	public CommandNotFoundException(string command, string message, Exception inner) : base(message, inner)
	{
		Command = command;
	}


	public override string ToString()
		=> Message + $@" (""{Command}"")";
}