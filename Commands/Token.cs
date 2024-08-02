namespace Specter.Debug.Prism.Commands;


public enum TokenType
{
	Colon, Value
}


public readonly struct TokenPosition(int start, int end)
{
	public int Start { get; init; } = start;
	public int End { get; init; } = end;
}


public readonly struct Token(string lexeme, int start, int end, object? value, TokenType type)
{
	public string Lexeme { get; init; } = lexeme;
	public TokenPosition Position { get; init; } = new(start, end);
	public object? Value { get; init; } = value;
	public TokenType Type { get; init; } = type;
}