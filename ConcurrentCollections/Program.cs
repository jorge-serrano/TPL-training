// See https://aka.ms/new-console-template for more information

using System.Collections.Concurrent;

Console.WriteLine("Hello, World!");

void ConCurrentDict()
{
    ConcurrentDictionary<string, string> capitals = new();
    var addedFirst = capitals.TryAdd("USA", "Washington");
}