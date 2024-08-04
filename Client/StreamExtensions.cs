using System.IO;
using System.Threading;
using System.Threading.Tasks;


namespace Specter.Debug.Prism.Client;


public static class StreamExtensions
{
    public static DataTransferStructure? ReadDataTransfer(this StreamReader stream)
    {
        string? data = stream.ReadLine();
        return data is null ? null : DataTransferStructure.FromJson(data);
    }

    public static async Task<DataTransferStructure?> ReadDataTransferAsync(this StreamReader stream, CancellationToken cancellationToken)
    {
        string? data = await stream.ReadLineAsync(cancellationToken);
        return data is null ? null : DataTransferStructure.FromJson(data);
    }

    public static async Task<DataTransferStructure?> ReadDataTransferAsync(this StreamReader stream)
    {
        string? data = await stream.ReadLineAsync();
        return data is null ? null : DataTransferStructure.FromJson(data);
    }


    public static void WriteDataTransfer(this StreamWriter stream, DataTransferStructure data)
        => stream.WriteLine(data.ToJson());
}
