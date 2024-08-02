using System.Collections.Generic;
using Specter.Debug.Prism.Client;

namespace Specter.Debug.Prism.Commands;


public interface ICommand
{
	void Execute(DataTransferStructure clientData, List<object?> args);
}


public readonly struct CommandData(Location location, List<object?> arguments)
{
	public Location Location { get; init; } = location;
	public List<object?> Arguments { get; init; } = arguments;
}