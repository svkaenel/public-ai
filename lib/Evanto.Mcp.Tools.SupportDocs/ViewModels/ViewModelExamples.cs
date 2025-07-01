using System;
using System.Collections.Generic;
using System.Linq;

namespace Evanto.Mcp.Tools.SupportDocs.ViewModels;

/// <summary>
/// Beispiele für die Verwendung der ProductDocumentation ViewModels.
/// Created: 30.05.2025
/// </summary>
public static class ViewModelExamples
{
    /// <summary>
    /// Beispiel für die Verwendung des ProductDocumentationRepository.
    /// </summary>
    public static void SupportDocSearchExample()
    {
        // Annahme: Repository ist über DI verfügbar
        // var repository = serviceProvider.GetRequiredService<IProductDocumentationRepository>();

        // Beispiel-Suchanfrage
        // var query = "Wie installiere ich eine Brunner Heizung?";
        
        // Suche durchführen
        // var results = await repository.GetProductDocumentationAsync(query, limit: 5);

        // Beispiel-Ausgabe der Ergebnisse
        var exampleResults = new List<EvSupportDocViewModel>
        {
            new EvSupportDocViewModel
            {
                FileName     = "Installationsanleitung_Brunner_2024.pdf",
                Content      = "Die Installation einer Brunner Heizung erfordert zunächst die Vorbereitung des Aufstellortes...",
                Score        = 0.89f,
                ChunkIndex   = 3,
                TotalChunks  = 15,
                ChunkId      = "Installationsanleitung_Brunner_2024_3",
                BaseFileName = "Installationsanleitung_Brunner_2024"
            },
            new EvSupportDocViewModel
            {
                FileName     = "Wartungshandbuch_Brunner.pdf",
                Content      = "Vor der Installation sollten alle Sicherheitsbestimmungen beachtet werden...",
                Score        = 0.82f,
                ChunkIndex   = 1,
                TotalChunks  = 8,
                ChunkId      = "Wartungshandbuch_Brunner_1",
                BaseFileName = "Wartungshandbuch_Brunner"
            }
        };

        foreach (var result in exampleResults)
        {
            Console.WriteLine($"Datei: {result.FileName}");
            Console.WriteLine($"Score: {result.Score:F2}");
            Console.WriteLine($"Chunk: {result.ChunkIndex}/{result.TotalChunks}");
            Console.WriteLine($"Inhalt: {result.Content.Substring(0, Math.Min(100, result.Content.Length))}...");
            Console.WriteLine("---");
        }
    }

    /// <summary>
    /// Beispiel für die Filterung von Ergebnissen nach Score.
    /// </summary>
    public static void FilterByScoreExample()
    {
        var allResults = new List<EvSupportDocViewModel>
        {
            new EvSupportDocViewModel { FileName = "Doc1.pdf", Score = 0.95f, Content = "Sehr relevanter Inhalt" },
            new EvSupportDocViewModel { FileName = "Doc2.pdf", Score = 0.75f, Content = "Mäßig relevanter Inhalt" },
            new EvSupportDocViewModel { FileName = "Doc3.pdf", Score = 0.55f, Content = "Weniger relevanter Inhalt" }
        };

        // Nur Ergebnisse mit Score > 0.7 anzeigen
        var highQualityResults = allResults
            .Where(r => r.Score > 0.7f)
            .OrderByDescending(r => r.Score)
            .ToList();

        Console.WriteLine($"Gefilterte Ergebnisse: {highQualityResults.Count} von {allResults.Count}");
    }

    /// <summary>
    /// Beispiel für die Gruppierung von Ergebnissen nach Datei.
    /// </summary>
    public static void GroupByFileExample()
    {
        var searchResults = new List<EvSupportDocViewModel>
        {
            new EvSupportDocViewModel { FileName = "Manual.pdf", ChunkIndex = 1, Content = "Chunk 1" },
            new EvSupportDocViewModel { FileName = "Manual.pdf", ChunkIndex = 2, Content = "Chunk 2" },
            new EvSupportDocViewModel { FileName = "Guide.pdf", ChunkIndex = 1, Content = "Guide Chunk 1" }
        };

        var groupedByFile = searchResults
            .GroupBy(r => r.FileName)
            .ToDictionary(g => g.Key, g => g.OrderBy(r => r.ChunkIndex).ToList());

        foreach (var fileGroup in groupedByFile)
        {
            Console.WriteLine($"Datei: {fileGroup.Key}");
            foreach (var chunk in fileGroup.Value)
            {
                Console.WriteLine($"  Chunk {chunk.ChunkIndex}: {chunk.Content}");
            }
        }
    }
}
