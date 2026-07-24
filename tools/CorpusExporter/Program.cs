using System.Text;
using CompanionDesktopPet.Services;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: CorpusExporter <output-path>");
    return 2;
}

var outputPath = Path.GetFullPath(args[0]);
Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
await using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
await using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
foreach (var line in PersonaCorpus.All)
{
    var text = line.Text.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
    await writer.WriteLineAsync($"{line.Category}\t{text}");
}

Console.WriteLine($"Exported {PersonaCorpus.All.Count:N0} lines to {outputPath}");
return 0;
