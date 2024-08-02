using System;
using System.Threading;

using Specter.Color;
using Specter.Color.Paint;
using Specter.Debug.Prism.Client;
using Specter.Debug.Prism.Server;
using Specter.String;


namespace ExampleProgram;


public class Server : PrismServer
{
	private readonly Thread _waitForNewClientsThread;


	public Server(int port)
		: base(port)
	{
		// stay listening for new clients in a separated thread
		_waitForNewClientsThread = new(new ThreadStart(WaitForNewClientsThread));
		_waitForNewClientsThread.Start();

		// events
		RequestListenerFailedEvent += OnRequestListenerFail;
		ClientAddedEvent += OnClientAdded;
		ClientRemovedEvent += OnClientRemoved;
		ClientRegistrationStartEvent += OnClientRegistrationStart;
		ClientRegistrationEndEvent += OnClientRegistrationEnd;

		ServerMessage($"Server running at port {ColorValue.Underline.Paint(port.ToString())}.");
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
				ServerError(e.ToString());
			}
		}
	}



	private void OnRequestListenerFail(PrismClient client, Exception exception)
		=> ServerError(exception.Message);


	private void OnClientRegistrationStart()
		=> ServerMessage("New client connected, waiting for registration...");

	private void OnClientRegistrationEnd(PrismClient client)
		=> ServerMessage($"Client registrated as {client.Name.FGBGreen()}");

	private void OnClientAdded(PrismClient client)
		=> ServerMessage($"Added client {client.Name.FGBGreen()}");

	private void OnClientRemoved(PrismClient client)
		=> ServerMessage($"Removed client {client.Name.FGBGreen()}");



	private string BuildMessageInformation(DataTransferStructure requestData)
		=> $"[{requestData.ClientName}]";



	public override void ServerMessage(string message)
	{
		Console.WriteLine(Painter.Paint($"* {message}", ColorValue.Bold));
	}

	public override void ServerWarning(string message)
	{
		Console.WriteLine(Painter.Paint($"* {message}", ColorValue.FGBYellow + ColorValue.Bold));
	}

	public override void ServerError(string message)
	{
		Console.WriteLine(Painter.Paint($"* {message}", ColorValue.FGBRed+ ColorValue.Bold));
	}



	public override void Message(string message, DataTransferStructure requestData)
	{
		Console.WriteLine(BuildMessageInformation(requestData) + $" {message}");
	}

	public override void Warning(string message, DataTransferStructure requestData)
	{
		Console.WriteLine(ColorValue.FGBYellow.Paint(BuildMessageInformation(requestData) + $" {message}"));

	}

	public override void Error(string message, DataTransferStructure requestData)
	{
		Console.WriteLine(ColorValue.FGBRed.Paint(BuildMessageInformation(requestData) + $" {message}"));
	}
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
				server.ProcessRequests();
			}
			catch (Exception e)
			{
				server.ServerError(e.ToString());
			}
		}
	}
}