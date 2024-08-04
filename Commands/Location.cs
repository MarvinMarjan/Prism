using System.Collections.Generic;


namespace Specter.Debug.Prism.Commands;


public struct Location(List<string> names)
{
    public List<string> Names { get; set; } = names;


    public Location() : this([]) { }


    public static Location FromString(string source)
        => new([.. source.Split('.')]);


    public override readonly string ToString()
        => string.Join('.', Names);
}
