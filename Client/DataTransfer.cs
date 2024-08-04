using System.Text.Json;


namespace Specter.Debug.Prism.Client;


public struct DataTransferStructure()
{
    public string ClientName { get; set; } = "";
    public string Command { get; set; } = "";


    public readonly string ToJson()
        => JsonSerializer.Serialize(this);

    public static DataTransferStructure FromJson(string json)
        => JsonSerializer.Deserialize<DataTransferStructure>(json);
}
