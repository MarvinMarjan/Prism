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

		RequestManager.ClientRequestListenerAdded
			+= (_, args)
                => args.Listener.RequestListenerFailed += OnRequestListenerFail;

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



	private void OnRequestListenerFail(object? sender, ClientRequestListenerFailedEventArgs args)
		=> Logger.ServerError(args.Exception.Message);


    protected override void OnClientRegistrationStart(PrismServerClientEventArgs args)
    {
        base.OnClientRegistrationStart(args);

        Logger.ServerMessage("New client connected, waiting for registration...");
    }

    protected override void OnClientRegistrationEnd(PrismServerClientEventArgs args)
    {
        base.OnClientRegistrationEnd(args);

        Logger.ServerMessage($"Client registrated as {args.Client?.Name.FGBGreen()}");
    }

    protected override void OnClientAdded(PrismServerClientEventArgs args)
    {
        base.OnClientAdded(args);

        Logger.ServerMessage($"Added client {args.Client?.Name.FGBGreen()}");
    }

    protected override void OnClientRemoved(PrismServerClientEventArgs args)
    {
        base.OnClientRemoved(args);

        Logger.ServerMessage($"Removed client {args.Client?.Name.FGBGreen()}");
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
				server.RequestManager.ProcessRequests();
			}
			catch (Exception e)
			{
				server.Logger.ServerError(e.ToString());
			}
		}
	}
}