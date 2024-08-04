using System.Collections.Generic;

using Specter.Debug.Prism.Client;
using Specter.Debug.Prism.Commands.Definition;
using Specter.Debug.Prism.Exceptions;


namespace Specter.Debug.Prism.Commands;


public static class CommandRunner
{
    public static void Run(DataTransferStructure requestData)
    {
        List<Token> tokens = new Scanner().Scan(requestData.Command);
        CommandData commandData = new Parser().Parse(tokens);

        if (!CommandDefinition.Commands.ContainsKey(commandData.Location.ToString()))
            throw new CommandNotFoundException(commandData.Location.ToString(), "Could not find command.");

        Run(requestData, commandData);
    }


    public static void Run(DataTransferStructure requestData, CommandData commandData)
        => CommandDefinition.Commands[commandData.Location.ToString()]
            .Execute(requestData, commandData.Arguments);
}
