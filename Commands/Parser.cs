using System;
using System.Collections.Generic;


namespace Specter.Debug.Prism.Commands;


public class Parser
{
	private List<Token> _tokens = [];
	private int _index;


	public CommandData Parse(List<Token> tokens)
	{
		if (tokens.Count == 0)
			throw new ArgumentException("Don't pass a empty list for parsing.");

		_tokens = tokens;
		_index = 0;

		Location location = ParseLocation();
		AdvanceNoReturn();
		List<object?> arguments = ParseArguments();

		return new(location, arguments);
	}


	private Location ParseLocation()
	{
		Location location = new();

		do
		{
			location.Names.Add(Advance().Lexeme);

			if (!AtEnd() && Peek().Type == TokenType.Colon)
				break;
		}
		while (!AtEnd());

		return location;
	}


	private List<object?> ParseArguments()
	{
		List<object?> arguments = [];

		while (!AtEnd())
			arguments.Add(Advance().Value);

		return arguments;
	}


	private bool AtEnd() => _index >= _tokens.Count;

	private Token Advance() => _tokens[_index++];
	private void AdvanceNoReturn() => _index++;
	private Token Peek() => _tokens[_index];
}