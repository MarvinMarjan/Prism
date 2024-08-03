using System;
using System.Threading;

using Specter.Color;
using Specter.Color.Paint;
using Specter.Debug.Prism.Client;
using Specter.Debug.Prism.Server;
using Specter.String;


namespace ExampleProgram;


public class Logger : ILogger
{
	private string BuildMessageInformation(DataTransferStructure requestData)
		=> $"[{requestData.ClientName}]";


	public void ServerMessage(string message)
	{
		Console.WriteLine(Painter.Paint($"* {message}", ColorValue.Bold));
	}

	public void ServerWarning(string message)
	{
		Console.WriteLine(Painter.Paint($"* {message}", ColorValue.FGBYellow + ColorValue.Bold));
	}

	public void ServerError(string message)
	{
		Console.WriteLine(Painter.Paint($"* {message}", ColorValue.FGBRed+ ColorValue.Bold));
	}



	public void Message(string message, DataTransferStructure requestData)
	{
		Console.WriteLine(BuildMessageInformation(requestData) + $" {message}");
	}

	public void Warning(string message, DataTransferStructure requestData)
	{
		Console.WriteLine(ColorValue.FGBYellow.Paint(BuildMessageInformation(requestData) + $" {message}"));

	}

	public void Error(string message, DataTransferStructure requestData)
	{
		Console.WriteLine(ColorValue.FGBRed.Paint(BuildMessageInformation(requestData) + $" {message}"));
	}
}


public class Server : PrismServer
{
	private readonly Thread _waitForNewClientsThread;


	public Server(int port)
		: base(port, new Logger())
	{
		// stay listening for new clients in a separated thread
		_waitForNewClientsThread = new(new ThreadStart(WaitForNewClientsThread));
		_waitForNewClientsThread.Start();

		// events
		ClientAddedEvent += OnClientAdded;
		ClientRemovedEvent += OnClientRemoved;
		ClientRegistrationStartEvent += OnClientRegistrationStart;
		ClientRegistrationEndEvent += OnClientRegistrationEnd;

		RequestManager.ClientRequestListenerAddedEvent
			+= listener => listener.RequestListenerFailedEvent += OnRequestListenerFail;

		Logger.ServerMessage($"Server running at port {ColorValue.Underline.Paint(port.ToString())}.");
	}


	private void WaitForNewClientsThread()
	{
		while (true)
		{
			try
			{
				AddAndWaitForNewClient();
			}
			catch (Exception e)
			{
				Logger.ServerError(e.ToString());
			}
		}
	}



	private void OnRequestListenerFail(RequestListener listener, Exception exception)
		=> Logger.ServerError(exception.Message);


	private void OnClientRegistrationStart()
		=> Logger.ServerMessage("New client connected, waiting for registration...");

	private void OnClientRegistrationEnd(PrismClient client)
		=> Logger.ServerMessage($"Client registrated as {client.Name.FGBGreen()}");

	private void OnClientAdded(PrismClient client)
		=> Logger.ServerMessage($"Added client {client.Name.FGBGreen()}");

	private void OnClientRemoved(PrismClient client)
		=> Logger.ServerMessage($"Removed client {client.Name.FGBGreen()}");
}


public class PrismServerTesting
{
	public static void Main()
	{
		Server server = new(ServerState.PORT);

		while (true)
		{
			try
			{
				server.RequestManager.ProcessRequests();
			}
			catch (Exception e)
			{
				server.Logger.ServerError(e.ToString());
			}
		}
	}
}