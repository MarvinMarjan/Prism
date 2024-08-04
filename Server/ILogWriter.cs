using Specter.Debug.Prism.Client;

namespace Specter.Debug.Prism.Server;


public interface ILogger
{
    public void ServerMessage(string message);
    public void ServerWarning(string message);
    public void ServerError(string message);


    public void Message(string message, DataTransferStructure requestData);
    public void Warning(string message, DataTransferStructure requestData);
    public void Error(string message, DataTransferStructure requestData);
}
