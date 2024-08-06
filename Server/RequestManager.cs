using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Specter.Debug.Prism.Client;
using Specter.Debug.Prism.Commands;
using Specter.Debug.Prism.Exceptions;
using System.Threading.Tasks;


namespace Specter.Debug.Prism.Server;


public class ClientRequestEventArgs(PrismClient? client, DataTransferStructure? requestData) : EventArgs
{
    public PrismClient? Client { get; init; } = client;
    public DataTransferStructure? RequestData { get; init; } = requestData;
}


public class ClientRequestListenerEventArgs(RequestListener listener) : EventArgs
{
    public RequestListener Listener { get; init; } = listener;
}


public class ClientRequestListenerFailedEventArgs(RequestListener listener, Exception exception)
    : ClientRequestListenerEventArgs(listener)
{
    public Exception Exception { get; init; } = exception;
}


public delegate void ClientRequestEventHandler(object? sender, ClientRequestEventArgs args);
public delegate void ClientRequestListenerEventHandler(object? sender, ClientRequestListenerEventArgs args);


/// <summary>
/// A Thread that keeps running in a loop waiting for a client request.
/// </summary>
public class RequestListener
{
    public PrismClient Client { get; init; }
    public Thread Thread { get; init; }
    public CancellationTokenSource CancellationTokenSource { get; init; }


    public delegate void RequestListenerFailedEventHandler(object? sender, ClientRequestListenerFailedEventArgs args);


    public event ClientRequestEventHandler? ValidRequestListened;
    public event ClientRequestEventHandler? InvalidRequestListened;
    public event RequestListenerFailedEventHandler? RequestListenerFailed;


    public virtual void OnValidRequestListened(ClientRequestEventArgs args)
        => ValidRequestListened?.Invoke(this, args);

    public virtual void OnInvalidRequestListened(ClientRequestEventArgs args)
        => InvalidRequestListened?.Invoke(this, args);

    public virtual void OnRequestListenerFailed(ClientRequestListenerFailedEventArgs args)
        => RequestListenerFailed?.Invoke(this, args);



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
                OnRequestListenerFailed(new(this, e));
                return;
            }

            if (CancellationTokenSource.Token.IsCancellationRequested)
                break;

            if (requestData is not null)
                OnValidRequestListened(new(Client, requestData));
            else
                OnInvalidRequestListened(new(Client, requestData));
        }
    }
}


public class RequestManager
{
    public ConcurrentDictionary<string, DataTransferStructure> Requests { get; init; } = [];
    protected Dictionary<string, RequestListener> ClientRequestListeners { get; init; } = [];


    public event ClientRequestEventHandler? ClientRequestProcessed;
    public event ClientRequestEventHandler? ClientRequestAdded;
    public event ClientRequestEventHandler? ClientRequestRemoved;

    public event ClientRequestListenerEventHandler? ClientRequestListenerAdded;
    public event ClientRequestListenerEventHandler? ClientRequestListenerRemoved;



    public virtual void OnClientRequestProcessed(ClientRequestEventArgs args)
        => ClientRequestProcessed?.Invoke(this, args);

    public virtual void OnClientRequestAdded(ClientRequestEventArgs args)
        => ClientRequestAdded?.Invoke(this, args);

    public virtual void OnClientRequestRemoved(ClientRequestEventArgs args)
        => ClientRequestRemoved?.Invoke(this, args);


    public virtual void OnClientRequestListenerAdded(ClientRequestListenerEventArgs args)
        => ClientRequestListenerAdded?.Invoke(this, args);

    public virtual void OnClientRequestListenerRemoved(ClientRequestListenerEventArgs args)
        => ClientRequestListenerRemoved?.Invoke(this, args);



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
            OnClientRequestProcessed(new(null, requestData));
        }
    }



    public bool AddClientRequest(DataTransferStructure requestData)
    {
        if (Requests.ContainsKey(requestData.ClientName))
            throw new TooMuchRequestsException(requestData.ClientName, "Client has already sent a request.");

        bool success = Requests.TryAdd(requestData.ClientName, requestData);
        OnClientRequestAdded(new(null, requestData));

        return success;
    }


    public bool RemoveClientRequest(string clientName)
    {
        if (!Requests.ContainsKey(clientName))
            throw new ClientRequestDoesNotExistsException(clientName, "Could not find the request made by client.");

        bool success = Requests.Remove(clientName, out DataTransferStructure requestData);
        OnClientRequestRemoved(new(null, requestData));

        return success;
    }



    public bool AddClientRequestListener(PrismClient client, out RequestListener listener)
    {
        if (ClientRequestListeners.ContainsKey(client.Name))
            throw new ClientRequestListenerAlreadyExistsException(client.Name, "Can't add new client request listener, it already exists.");

        listener = new(client);

        bool success = ClientRequestListeners.TryAdd(client.Name, listener);
        OnClientRequestListenerAdded(new(listener));

        return success;
    }

    public bool RemoveClientRequestListener(PrismClient client)
    {
        bool success = ClientRequestListeners.Remove(client.Name, out RequestListener? listener);
        listener?.CancellationTokenSource.Cancel();

        if (success)
            OnClientRequestListenerRemoved(new(listener!));

        return success;
    }
}
