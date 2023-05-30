namespace Drastic.Gpt4All.Net;

public static class Gpt4AllStatic
{
    public static string DefaultPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Drastic.Gpt4All");
}