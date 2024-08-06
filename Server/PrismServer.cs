using System;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Specter.Debug.Prism.Client;
using Specter.Debug.Prism.Exceptions;


namespace Specter.Debug.Prism.Server;


public class PrismServerClientEventArgs(PrismClient? client) : EventArgs
{
    public PrismClient? Client { get; init; } = client;
}


public abstract partial class PrismServer : TcpListener
{
    public ConcurrentDictionary<string, PrismClient> Clients { get; private set; }
    public ILogger Logger { get; set; }
    public RequestManager RequestManager { get; set; }


    public delegate void ClientEventHandler(object? sender, PrismServerClientEventArgs args);


    public event ClientEventHandler? ClientAdded;
    public event ClientEventHandler? ClientRemoved;

    public event ClientEventHandler? ClientRegistrationStart;
    public event ClientEventHandler? ClientRegistrationEnd;



    protected virtual void OnClientAdded(PrismServerClientEventArgs args)
        => ClientAdded?.Invoke(this, args);
    
    protected virtual void OnClientRemoved(PrismServerClientEventArgs args)
        => ClientRemoved?.Invoke(this, args);

    protected virtual void OnClientRegistrationStart(PrismServerClientEventArgs args)
        => ClientRegistrationStart?.Invoke(this, args);

    protected virtual void OnClientRegistrationEnd(PrismServerClientEventArgs args)
        => ClientRegistrationEnd?.Invoke(this, args);



    public PrismServer(int port, ILogger logger, RequestManager requestManager)
        : base(IPAddress.Loopback, port)
    {
        ServerState.Server = this;

        Clients = [];
        Logger = logger;
        RequestManager = requestManager;

        // start server
        Start();
    }


    public PrismServer(int port, ILogger logger)
        : this(port, logger, new()) { }



    public void AddClientAndRequestListener(PrismClient client)
    {
        AddClient(client);
        RequestManager.AddClientRequestListener(client, out RequestListener listener);

        // ValidRequestListened event is not raised if a null request is received,
        // so no problem with using "!"
        listener.ValidRequestListened += (_, args)
            => RequestManager.AddClientRequest(args.RequestData!.Value);

        // InvalidRequestListened event is always raised with the Client property
        // initialized, so no problem with using "!"
        listener.InvalidRequestListened += (_, args)
            => RemoveClientAndRequestListener(args.Client!);
    }

    public void RemoveClientAndRequestListener(PrismClient client)
    {
        RemoveClient(client);
        RequestManager.RemoveClientRequestListener(client);
    }

    public void RemoveClientAndRequestListener(string clientName)
    {
        RemoveClient(clientName, out PrismClient client);
        RequestManager.RemoveClientRequestListener(client); // FIXME: removing client by command throws AggregateException...
    }



    public void AddClient(PrismClient client)
    {
        if (Clients.ContainsKey(client.Name))
            throw new ClientAlreadyExistsException(client.Name, "Can't add new client, it already exists.");

        Clients.TryAdd(client.Name, client);
        OnClientAdded(new(client));
    }

    public void RemoveClient(PrismClient client)
    {
        if (!Clients.ContainsKey(client.Name))
            throw new ClientDoesNotExistsException(client.Name, "Could not find client.");

        Clients.TryRemove(client.Name, out _);
        OnClientRemoved(new(client));
    }

    public void RemoveClient(string clientName, out PrismClient removedClient)
    {
        if (!Clients.TryGetValue(clientName, out PrismClient? client))
            throw new ClientDoesNotExistsException(clientName, "Could not find client.");

        removedClient = client;
        RemoveClient(client);
    }



    public PrismClient AddAndWaitForNewClient()
        => RegisterClient(AcceptTcpClient());

    public async Task<PrismClient> AddAndWaitForNewClientAsync()
        => RegisterClient(await AcceptTcpClientAsync());



    public PrismClient RegisterClient(TcpClient client)
    {
        try
        {
            OnClientRegistrationStart(new(null));

            DataTransferStructure? registrationData = new StreamReader(client.GetStream()).ReadDataTransfer();

            if (registrationData is not DataTransferStructure validRegistrationData)
                throw new InvalidRegistrationDataException("Invalid registration data.");

            PrismClient prismClient = new(validRegistrationData.ClientName, client);

            OnClientRegistrationEnd(new(prismClient));

            AddClientAndRequestListener(prismClient);
            return prismClient;
        }
        catch (JsonException e)
        {
            throw new InvalidRegistrationDataException("JSON data is invalid.", e);
        }
    }



    public List<string> GetAllClientNames()
        => [.. Clients.Keys];
}
