using System;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

using Specter.Debug.Prism.Client;
using Specter.Debug.Prism.Commands;
using Specter.Debug.Prism.Exceptions;


namespace Specter.Debug.Prism.Server;


/// <summary>
/// A Thread that keeps running in a loop waiting for a client request.
/// </summary>
public struct RequestListener(Thread thread, CancellationTokenSource cancellationTokenSource)
{
	public Thread Thread { get; set; } = thread;
	public CancellationTokenSource CancellationTokenSource { get; set; } = cancellationTokenSource;
}


public abstract partial class PrismServer : TcpListener, ILogWriter
{
	public ConcurrentDictionary<string, PrismClient> Clients { get; private set; }
	public ConcurrentDictionary<string, DataTransferStructure> Requests { get; private set; }

	protected readonly Dictionary<string, RequestListener> ClientRequestListeners;



	public PrismServer(int port)
		: base(IPAddress.Loopback, port)
	{
		ServerState.Server = this;

		Clients = [];
		Requests = [];

		ClientRequestListeners = [];

		// start server
		Start();
	}




	public void ProcessRequests()
	{
		foreach (var (_, requestData) in Requests)
			ProcessRequest(requestData);
	}


	public void ProcessRequest(DataTransferStructure requestData)
	{
		if (!Requests.ContainsKey(requestData.ClientName))
			throw new ClientRequestDoesNotExists(requestData.ClientName, "Could not find the request to process.");

		try
		{
			CommandRunner.Run(requestData);
		}
		finally
		{
			RemoveClientRequest(requestData.ClientName);
		}
	}




	public void AddClientAndRequestListener(PrismClient client)
	{
		AddClient(client);
		AddClientRequestListener(client);
	}

	public void AddClient(PrismClient client)
	{
		if (Clients.ContainsKey(client.Name))
			throw new ClientAlreadyExistsException(client.Name, "Can't add new client, it already exists.");

		Clients.TryAdd(client.Name, client);
		ClientAddedEvent?.Invoke(client);
	}




	public void RemoveClientAndRequestListener(PrismClient client)
	{
		RemoveClient(client);
		RemoveClientRequestListener(client);
	}

	public void RemoveClient(PrismClient client)
	{
		if (!Clients.ContainsKey(client.Name))
			throw new ClientDoesNotExistsException(client.Name, "Could not find client.");

		Clients.TryRemove(client.Name, out _);
		ClientRemovedEvent?.Invoke(client);
	}

	public void RemoveClient(string clientName)
	{
		if (!Clients.TryGetValue(clientName, out PrismClient? client))
			throw new ClientDoesNotExistsException(clientName, "Could not find client.");

		RemoveClient(client);
	}




	public void AddClientRequestListener(PrismClient client)
	{
		CancellationTokenSource tokenSource = new();

		if (ClientRequestListeners.ContainsKey(client.Name))
			throw new ClientRequestListenerAlreadyExistsException(client.Name, "Can't add new client request listener, it already exists.");

		Thread requestListenerThread = new(new ThreadStart(
			() => AddAndListenForClientRequestThread(client, tokenSource.Token)
		));

		requestListenerThread.Start();

		ClientRequestListeners.TryAdd(client.Name, new(requestListenerThread, tokenSource));
		ClientRequestListenerAddedEvent?.Invoke(client);
	}

	public void RemoveClientRequestListener(PrismClient client)
	{
		ClientRequestListeners.Remove(client.Name, out RequestListener listener);
		listener.CancellationTokenSource.Cancel();

		ClientRequestListenerRemovedEvent?.Invoke(client);
	}




	public void AddClientRequest(DataTransferStructure requestData)
	{
		if (Requests.ContainsKey(requestData.ClientName))
			throw new TooMuchRequestsException(requestData.ClientName, "Client has already sent a request.");

		Requests.TryAdd(requestData.ClientName, requestData);
		ClientSentRequestEvent?.Invoke(requestData);
	}


	public void RemoveClientRequest(string clientName)
	{
		if (!Requests.ContainsKey(clientName))
			throw new ClientRequestDoesNotExists(clientName, "Could not find the request made by client.");
	
		Requests.Remove(clientName, out DataTransferStructure requestData);
		ClientRequestProcessedEvent?.Invoke(requestData);
	}




	public PrismClient AddAndWaitForNewClient()
		=> RegisterClient(AcceptTcpClient());




	public List<string> GetAllClientNames()
		=> [.. Clients.Keys];




	protected PrismClient RegisterClient(TcpClient client)
	{
		try
		{
			ClientRegistrationStartEvent?.Invoke();

			DataTransferStructure? registrationData = new StreamReader(client.GetStream()).ReadDataTransfer();
		
			if (registrationData is not DataTransferStructure validRegistrationData)
				throw new InvalidRegistrationDataException("Invalid registration data.");

			PrismClient prismClient = new(validRegistrationData.ClientName, client);

			ClientRegistrationEndEvent?.Invoke(prismClient);

			AddClientAndRequestListener(prismClient);
			return prismClient;
		}
		catch (JsonException e)
		{
			throw new InvalidRegistrationDataException("JSON data is invalid.", e);
		}
	}


	private void AddAndListenForClientRequestThread(PrismClient client, CancellationToken cancellationToken)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			try
			{
				AddAndListenForClientRequestAsync(client, cancellationToken).Wait(CancellationToken.None);
			}
			catch (Exception e)
			{
				RequestListenerFailedEvent?.Invoke(client, e);
			}
		}
	}


	private async Task AddAndListenForClientRequestAsync(PrismClient client, CancellationToken cancellationToken)
	{
		DataTransferStructure? requestData = await client.Reader.ReadDataTransferAsync(cancellationToken);


		if (cancellationToken.IsCancellationRequested)
			return;

		if (requestData is DataTransferStructure validRequestData)
			AddClientRequest(validRequestData);
		else
			RemoveClientAndRequestListener(client);
	}





	public abstract void ServerMessage(string message);
	public abstract void ServerWarning(string message);
	public abstract void ServerError(string message);

	public abstract void Message(string message, DataTransferStructure requestData);
	public abstract void Warning(string message, DataTransferStructure requestData);
	public abstract void Error(string message, DataTransferStructure requestData);
}