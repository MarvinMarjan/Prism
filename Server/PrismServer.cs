using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

using Specter.Debug.Prism.Client;
using Specter.Debug.Prism.Exceptions;
using System.Threading.Tasks;


namespace Specter.Debug.Prism.Server;


public abstract partial class PrismServer : TcpListener
{
    public ConcurrentDictionary<string, PrismClient> Clients { get; private set; }
    public ILogger Logger { get; set; }
    public RequestManager RequestManager { get; set; }


    public delegate void ClientEventHandler(PrismClient client);
    public delegate void ClientRegistrationStartEventHandler();
    public delegate void ClientRegistrationEndEventHandler(PrismClient client);


    public event ClientEventHandler? ClientAddedEvent;
    public event ClientEventHandler? ClientRemovedEvent;

    public event ClientRegistrationStartEventHandler? ClientRegistrationStartEvent;
    public event ClientRegistrationEndEventHandler? ClientRegistrationEndEvent;



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

        listener.ValidRequestListenedEvent += requestData => RequestManager.AddClientRequest(requestData);
        listener.InvalidRequestListenedEvent += RemoveClientAndRequestListener;
    }

    public void RemoveClientAndRequestListener(PrismClient client)
    {
        RemoveClient(client);
        RequestManager.RemoveClientRequestListener(client);
    }



    public void AddClient(PrismClient client)
    {
        if (Clients.ContainsKey(client.Name))
            throw new ClientAlreadyExistsException(client.Name, "Can't add new client, it already exists.");

        Clients.TryAdd(client.Name, client);
        ClientAddedEvent?.Invoke(client);
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



    public PrismClient AddAndWaitForNewClient()
        => RegisterClient(AcceptTcpClient());

    public async Task<PrismClient> AddAndWaitForNewClientAsync()
        => RegisterClient(await AcceptTcpClientAsync());



    public PrismClient RegisterClient(TcpClient client)
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



    public List<string> GetAllClientNames()
        => [.. Clients.Keys];
}
