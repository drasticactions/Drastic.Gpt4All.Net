using System.Text.Json.Serialization;

namespace Drastic.Gpt4All.Net.UI.Models;

public class Gpt4AllWebModel
{
    [JsonPropertyName("md5sum")]
    public string Md5Sum { get; set; }

    [JsonPropertyName("filename")]
    public string Filename { get; set; }

    [JsonPropertyName("filesize")]
    public string Filesize { get; set; }

    [JsonPropertyName("isDefault")]
    public string IsDefault { get; set; }

    [JsonPropertyName("bestGPTJ")]
    public string BestGPTJ { get; set; }

    [JsonPropertyName("bestLlama")]
    public string BestLlama { get; set; }

    [JsonPropertyName("bestMPT")]
    public string BestMPT { get; set; }

    [JsonPropertyName("requires")]
    public string Requires { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }
    
    public string FileLocation { get; set; } = string.Empty;

    public string DownloadUrl => "https://gpt4all.io/models/" + this.Filename;
    
    public bool Exists => !string.IsNullOrEmpty(this.FileLocation) && System.IO.Path.Exists(this.FileLocation);
}