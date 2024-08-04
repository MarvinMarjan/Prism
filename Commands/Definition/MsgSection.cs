using System.Collections.Generic;
using Specter.Debug.Prism.Client;
using Specter.Debug.Prism.Server;


namespace Specter.Debug.Prism.Commands.Definition;


public class MsgInfo : ICommand
{
    public void Execute(DataTransferStructure requestData, List<object?> args)
        => ServerState.Server?.Logger.Message(args[0] as string ?? "", requestData);
}

public class MsgWarn : ICommand
{
    public void Execute(DataTransferStructure requestData, List<object?> args)
        => ServerState.Server?.Logger.Warning(args[0] as string ?? "", requestData);
}

public class MsgError : ICommand
{
    public void Execute(DataTransferStructure requestData, List<object?> args)
        => ServerState.Server?.Logger.Error(args[0] as string ?? "", requestData);
}
