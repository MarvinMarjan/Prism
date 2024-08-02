using System;

using Specter.Debug.Prism.Client;


namespace Specter.Debug.Prism.Server;


public abstract partial class PrismServer
{
	public delegate void RequestListenerFailedEventHandler(PrismClient client, Exception exception);
	public delegate void ClientEventHandler(PrismClient client);
	public delegate void ClientRequestEventHandler(DataTransferStructure requestData);
	public delegate void ClientRegistrationStartEventHandler();
	public delegate void ClientRegistrationEndEventHandler(PrismClient client);


	public event RequestListenerFailedEventHandler? RequestListenerFailedEvent;

	public event ClientEventHandler? ClientAddedEvent;
	public event ClientEventHandler? ClientRemovedEvent;
	public event ClientEventHandler? ClientRequestListenerAddedEvent;
	public event ClientEventHandler? ClientRequestListenerRemovedEvent;

	public event ClientRequestEventHandler? ClientSentRequestEvent;
	public event ClientRequestEventHandler? ClientRequestProcessedEvent;

	public event ClientRegistrationStartEventHandler? ClientRegistrationStartEvent;
	public event ClientRegistrationEndEventHandler? ClientRegistrationEndEvent;
}