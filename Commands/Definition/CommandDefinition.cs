using System.Collections.Generic;


namespace Specter.Debug.Prism.Commands.Definition;


public static class CommandDefinition
{
    public readonly static Dictionary<string, ICommand> Commands = new([
        new("client.list", new ClientList()),
        new("client.remove", new ClientRemove()),

        new("msg.info", new MsgInfo()),
        new("msg.warn", new MsgWarn()),
        new("msg.error", new MsgError())
    ]);
}
