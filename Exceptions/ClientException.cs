using System;


namespace Specter.Debug.Prism.Exceptions;


[Serializable]
public class ClientException : Exception
{
	public string? ClientName { get; init; }


	public ClientException()
	{ }

	public ClientException(string clientName, string message) : base(message)
	{
		ClientName = clientName;
	}

	public ClientException(string clientName, string message, Exception inner) : base(message, inner)
	{
		ClientName = clientName;
	}


	public override string ToString()
		=> Message + $@" (""{ClientName}"")";
}



[Serializable]
public class ClientAlreadyExistsException : ClientException
{
	public ClientAlreadyExistsException()
	{ }

	public ClientAlreadyExistsException(string clientName, string message)
		: base(clientName, message)
	{ }
	
	public ClientAlreadyExistsException(string clientName, string message, Exception inner)
		: base(clientName, message, inner)
	{ }
}



[Serializable]
public class ClientDoesNotExistsException : ClientException
{
	public ClientDoesNotExistsException()
	{ }

	public ClientDoesNotExistsException(string clientName, string message)
		: base(clientName, message)
	{ }
	
	public ClientDoesNotExistsException(string clientName, string message, Exception inner)
		: base(clientName, message, inner)
	{ }
}



[Serializable]
public class ClientRequestListenerAlreadyExistsException : ClientException
{
	public ClientRequestListenerAlreadyExistsException()
	{ }
	
	public ClientRequestListenerAlreadyExistsException(string clientName, string message)
		: base(clientName, message)
	{ }
	
	public ClientRequestListenerAlreadyExistsException(string clientName, string message, Exception inner)
		: base(clientName, message, inner)
	{ }
}



[Serializable]
public class TooMuchRequestsException : ClientException
{
	public TooMuchRequestsException()
	{ }

	public TooMuchRequestsException(string clientName, string message)
		: base(clientName, message)
	{ }

	public TooMuchRequestsException(string clientName, string message, Exception inner)
		: base(clientName, message, inner)
	{ }
}



[Serializable]
public class ClientRequestDoesNotExistsException : ClientException
{
	public ClientRequestDoesNotExistsException()
	{ }

	public ClientRequestDoesNotExistsException(string clientName, string message)
		: base(clientName, message)
	{ }

	public ClientRequestDoesNotExistsException(string clientName, string message, Exception inner)
		: base(clientName, message, inner)
	{ }
}



[Serializable]
public class InvalidClientRequestException : ClientException
{
	public InvalidClientRequestException()
	{ }

	public InvalidClientRequestException(string clientName, string message)
		: base(clientName, message)
	{ }

	public InvalidClientRequestException(string clientName, string message, Exception inner)
		: base(clientName, message, inner)
	{ }
}