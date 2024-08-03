using System.Collections.Generic;

using Specter.Debug.Prism.Client;
using Specter.Debug.Prism.Server;


namespace Specter.Debug.Prism.Commands.Definition;


public class ClientList : ICommand
{
	public void Execute(DataTransferStructure requestData, List<object?> args)
	{
		if (ServerState.Server is null)
			return;

		PrismServer server = ServerState.Server;

		server.Logger.ServerMessage("Clients connected: " + string.Join(", ", server.GetAllClientNames()));
	}
}


public class ClientRemove : ICommand
{
	public void Execute(DataTransferStructure requestData, List<object?> args)
		=> ServerState.Server?.RemoveClient(args[0] as string ?? "");
}