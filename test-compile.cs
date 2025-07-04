// Simple test to check compilation
using System;
using Evanto.Mcp.Qdrant.Models;
using Evanto.Mcp.Qdrant.Contracts;

public class TestCompile
{
    public static void Main()
    {
        var doc = new EvDocument
        {
            Id = "test",
            FileName = "test.txt",
            Content = "test content"
        };
        
        Console.WriteLine($"Created document with ID: {doc.Id}");
    }
}