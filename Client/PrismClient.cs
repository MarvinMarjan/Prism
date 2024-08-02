using System.IO;
using System.Net.Sockets;


namespace Specter.Debug.Prism.Client;


public class PrismClient
{
	public string Name { get; init; }

	public StreamWriter Writer { get; init; }
	public StreamReader Reader { get; init; }
	public TcpClient Tcp { get; init; }


	public PrismClient(string name, int port)
		: this(name, new TcpClient("localhost", port))
	{
		// registration data
		WriteDataTransfer();	
	}


	public PrismClient(string name, TcpClient client)
	{
		Name = name;
		Tcp = client;
		Writer = new(Tcp.GetStream());
		Reader = new(Tcp.GetStream());

		Writer.AutoFlush = true;
	}


	public DataTransferStructure ToDataTransferStructure()
		=> new() {
			ClientName = Name,
		};


	public void WriteDataTransfer()
		=> Writer.WriteDataTransfer(ToDataTransferStructure());

	public void WriteCommandRequest(string command)
		=> Writer.WriteDataTransfer(ToDataTransferStructure() with { Command = command });



	public void Info(string message)
		=> WriteCommandRequest($@"msg info: ""{message}""");

	public void Warning(string message)
		=> WriteCommandRequest($@"msg warn: ""{message}""");

	public void Error(string message)
		=> WriteCommandRequest($@"msg error: ""{message}""");
}