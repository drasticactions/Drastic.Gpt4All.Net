using Gpt4All;
using Gpt4All.LibraryLoader;

NativeLibraryLoader.LoadNativeLibrary();

var modelFactory = new Gpt4AllModelFactory();
if (args.Length < 2)
{
    Console.WriteLine($"Usage: Drastic.Gpt4All.Net.CLISample <model-path> <prompt>");
    return;
}

var modelPath = args[0];
var prompt = args[1];

using var model = modelFactory.LoadModel(modelPath);

var result = await model.GetStreamingPredictionAsync(
    prompt,
    PredictRequestOptions.Defaults);

await foreach (var token in result.GetPredictionStreamingAsync())
{
    Console.Write(token);
}