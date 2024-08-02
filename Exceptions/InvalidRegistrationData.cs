using System;


namespace Specter.Debug.Prism.Exceptions;


[Serializable]
public class InvalidRegistrationDataException : Exception
{
	public InvalidRegistrationDataException() { }
	public InvalidRegistrationDataException(string message) : base(message) { }
	public InvalidRegistrationDataException(string message, Exception inner) : base(message, inner) { }


	public override string ToString()
		=> Message;
}