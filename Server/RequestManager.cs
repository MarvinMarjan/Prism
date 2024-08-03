using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Specter.Debug.Prism.Client;
using Specter.Debug.Prism.Commands;
using Specter.Debug.Prism.Exceptions;


namespace Specter.Debug.Prism.Server;


/// <summary>
/// A Thread that keeps running in a loop waiting for a client request.
/// </summary>
public class RequestListener
{
	public PrismClient Client { get; init; }
	public Thread Thread { get; init; }
	public CancellationTokenSource CancellationTokenSource { get; init; }


	public delegate void ValidRequestListenedEventHandler(DataTransferStructure requestData);
	public delegate void InvalidRequestListenedEventHandler(PrismClient client);
	public delegate void RequestListenerFailedEventHandler(RequestListener listener, Exception exception);


	public event ValidRequestListenedEventHandler? ValidRequestListenedEvent;
	public event InvalidRequestListenedEventHandler? InvalidRequestListenedEvent;
	public event RequestListenerFailedEventHandler? RequestListenerFailedEvent;



	public RequestListener(PrismClient client)
	{
		Client = client;
		CancellationTokenSource = new();

		Thread = new(ListenForRequestThread);
		Thread.Start();
	}



	private void ListenForRequestThread()
	{
		while (!CancellationTokenSource.Token.IsCancellationRequested)
		{
			DataTransferStructure? requestData = null;

			try
			{
				requestData = Client.Reader.ReadDataTransferAsync(CancellationTokenSource.Token).Result;
			}
			catch (Exception e)
			{
				RequestListenerFailedEvent?.Invoke(this, e);
				return;
			}

			if (requestData is null)
				InvalidRequestListenedEvent?.Invoke(Client);
			else
				ValidRequestListenedEvent?.Invoke(requestData.Value);
		}
	}
}


public class RequestManager
{
	public ConcurrentDictionary<string, DataTransferStructure> Requests { get; init; } = [];
	protected Dictionary<string, RequestListener> ClientRequestListeners { get; init; } = [];


	public delegate void ClientRequestEventHandler(DataTransferStructure requestData);
	public delegate void ClientRequestListenerEventHandler(RequestListener listener);

	public event ClientRequestEventHandler? ClientRequestProcessedEvent;
	public event ClientRequestEventHandler? ClientRequestAddedEvent;
	public event ClientRequestEventHandler? ClientRequestRemovedEvent;

	public event ClientRequestListenerEventHandler? ClientRequestListenerAddedEvent;
	public event ClientRequestListenerEventHandler? ClientRequestListenerRemovedEvent;



	public void ProcessRequests()
	{
		foreach (var (_, requestData) in Requests)
			ProcessRequest(requestData);
	}


	public void ProcessRequest(DataTransferStructure requestData)
	{
		if (!Requests.ContainsKey(requestData.ClientName))
			throw new ClientRequestDoesNotExistsException(requestData.ClientName, "Could not find the request to process.");

		try
		{
			CommandRunner.Run(requestData);
		}
		finally
		{
			RemoveClientRequest(requestData.ClientName);
			ClientRequestProcessedEvent?.Invoke(requestData);
		}
	}



	public bool AddClientRequest(DataTransferStructure requestData)
	{
		if (Requests.ContainsKey(requestData.ClientName))
			throw new TooMuchRequestsException(requestData.ClientName, "Client has already sent a request.");

		bool success = Requests.TryAdd(requestData.ClientName, requestData);
		ClientRequestAddedEvent?.Invoke(requestData);

		return success;
	}


	public bool RemoveClientRequest(string clientName)
	{
		if (!Requests.ContainsKey(clientName))
			throw new ClientRequestDoesNotExistsException(clientName, "Could not find the request made by client.");
	
		bool success = Requests.Remove(clientName, out DataTransferStructure requestData);
		ClientRequestRemovedEvent?.Invoke(requestData);
	
		return success;
	}



	public bool AddClientRequestListener(PrismClient client, out RequestListener listener)
	{
		if (ClientRequestListeners.ContainsKey(client.Name))
			throw new ClientRequestListenerAlreadyExistsException(client.Name, "Can't add new client request listener, it already exists.");

		listener = new(client);

		bool success = ClientRequestListeners.TryAdd(client.Name, listener);
		ClientRequestListenerAddedEvent?.Invoke(listener);

		return success;
	}

	public bool RemoveClientRequestListener(PrismClient client)
	{
		bool success = ClientRequestListeners.Remove(client.Name, out RequestListener? listener);
		listener?.CancellationTokenSource.Cancel();

		if (success)
			ClientRequestListenerRemovedEvent?.Invoke(listener!);
	
		return success;
	}
}