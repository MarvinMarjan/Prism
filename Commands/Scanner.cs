using System.Collections.Generic;
using Specter.Debug.Prism.Exceptions;


namespace Specter.Debug.Prism.Commands;


public class Scanner
{
    private string _source = "";
    private int _start, _end;

    private readonly List<Token> _tokens = [];


    public List<Token> Scan(string source)
    {
        _tokens.Clear();

        _source = source;
        _start = _end = 0;

        while (!AtEnd())
        {
            _start = _end;
            ScanToken();
        }

        return _tokens;
    }


    private void ScanToken()
    {
        char ch = Advance();

        switch (ch)
        {
            case ' ':
            case '\n':
            case '\t':
                break;

            case ':': AddToken(TokenType.Colon); break;

            case '"':
                StringValue();
                break;

            default:
                if (char.IsLetter(ch))
                    Identifier();

                else if (char.IsDigit(ch))
                    NumberValue();

                else
                    throw new InvalidTokenException("Invalid token.");

                break;
        }
    }


    private void Identifier()
    {
        while (!AtEnd() && char.IsLetterOrDigit(Peek()))
            Advance();

        AddToken(TokenType.Value, _source[_start.._end]);
    }


    private void StringValue()
    {
        while (!AtEnd() && Peek() != '"')
            Advance();

        if (AtEnd())
            throw new UnclosedStringException("String not closed.");

        Advance();

        string value = _source[(_start + 1)..(_end - 1)];

        AddToken(TokenType.Value, value);
    }


    private void NumberValue()
    {
        while (!AtEnd() && char.IsDigit(Peek()))
            Advance();

        _ = int.TryParse(_source[_start.._end], out int result);

        AddToken(TokenType.Value, result);
    }


    private void AddToken(TokenType type)
        => _tokens.Add(new Token(_source[_start.._end], _start, _end, null, type));

    private void AddToken(TokenType type, object value)
        => _tokens.Add(new Token(_source[_start.._end], _start, _end, value, type));


    private bool AtEnd() => _end >= _source.Length;

    private char Advance() => _source[_end++];
    private char Peek() => _source[_end];
}
