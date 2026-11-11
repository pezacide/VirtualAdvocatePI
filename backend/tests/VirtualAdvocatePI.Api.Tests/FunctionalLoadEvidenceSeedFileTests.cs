using System.Text.Json;

namespace VirtualAdvocatePI.Api.Tests;

public sealed class FunctionalLoadEvidenceSeedFileTests
{
    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "document-templates")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("Repo root with document-templates was not found.");
    }

    private static JsonElement LoadSeed()
    {
        var path = Path.Combine(
            FindRepoRoot(), "backend", "src", "VirtualAdvocatePI.Api",
            "KnowledgeBase", "admin", "functional-load-evidence.seed.json");

        return JsonDocument.Parse(File.ReadAllText(path)).RootElement;
    }

    [Fact]
    public void Seed_TemplateKeysAreUnique()
    {
        var keys = LoadSeed().GetProperty("templates").EnumerateArray()
            .Select(t => t.GetProperty("templateKey").GetString()!)
            .ToList();

        Assert.Equal(keys.Count, keys.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void Seed_SourceTemplates_PointAtExistingFiles_AndAreValidJson()
    {
        var root = FindRepoRoot();
        var sources = LoadSeed().GetProperty("templates").EnumerateArray()
            .Where(t => t.GetProperty("templateKey").GetString()!.StartsWith("FLE_SRC_"))
            .ToList();

        Assert.NotEmpty(sources);

        foreach (var template in sources)
        {
            using var body = JsonDocument.Parse(template.GetProperty("templateBody").GetString()!);
            var sourceFile = body.RootElement.GetProperty("sourceFile").GetString()!;

            Assert.True(
                File.Exists(Path.Combine(root, sourceFile.Replace('/', Path.DirectorySeparatorChar))),
                $"Source file not found: {sourceFile}");
            Assert.True(body.RootElement.GetProperty("columns").GetArrayLength() > 0);
        }
    }
}
