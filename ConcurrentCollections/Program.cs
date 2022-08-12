// See https://aka.ms/new-console-template for more information

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

ConcurrentDictionary<string, string> capitals = new();
ConcurrentQueue<string> capitalsQueue = new ConcurrentQueue<string>();
ConcurrentStack<int> numbersStack = new ConcurrentStack<int>();
//ConcurrentDictionary<string, string> capitals = new(new InsensitiveComparer());
ConcurrentBag<int> numbersBag = new ConcurrentBag<int>();
BlockingCollection<int> messages = new BlockingCollection<int>(
      new ConcurrentBag<int>(), 10); /* max 10 elements to block thread */
CancellationTokenSource cts = new CancellationTokenSource();
Random random = new Random();

Console.WriteLine("Hello, World!");
//ConCurrentDict();
//ConcurrentQueue();
//ConcurrentBags();
BlockingCollections();

void BlockingCollections()
{
    Task.Factory.StartNew(ProduceAndConsume, cts.Token);

    Console.ReadKey();
    cts.Cancel();
}

void ProduceAndConsume()
{
    var producer = Task.Factory.StartNew(RunProducer);
    var consumer = Task.Factory.StartNew(RunConsumer);

    try
    {
        Task.WaitAll(new[] { producer, consumer }, cts.Token);
    }
    catch (AggregateException ae)
    {
        ae.Handle(e => true);
    }
}
void RunConsumer()
{
    foreach (var item in messages.GetConsumingEnumerable())
    {
        cts.Token.ThrowIfCancellationRequested();
        Console.WriteLine($"-{item}");
        Thread.Sleep(random.Next(3000));
    }
}

void RunProducer()
{

    while (true)
    {
        cts.Token.ThrowIfCancellationRequested();
        int i = random.Next(100);
        messages.Add(i);
        Console.WriteLine($"+{i}\t");
        Thread.Sleep(random.Next(1000));
    }
}
void ConcurrentBags()
{
    var tasks = new List<Task>();
    for (int i = 0; i < 20; i++)
    {
        tasks.Add(Task.Factory.StartNew(() => { numbersBag.Add(i); }));
        tasks.Add(Task.Factory.StartNew(() => {
            int n = -1;
            if (numbersBag.TryPeek(out n))
            {
                Console.Write(Task.CurrentId);
                Console.WriteLine($" Peek n= {n}");
            }
                
        }));
    }

    Task.WaitAll(tasks.ToArray());
}

void ConCurrentDict()
{
    var addedFirst = capitals.TryAdd("USA", "Washington");
    FailSilentlyToAdd();
    capitals["rusia"] = "leningrado";
    AddOrUpdate();
    TryToDelete("Wrong");
    TryToDelete("usa");
    TryToDelete("USA");
    
    foreach (var key in capitals.Keys)
    {
        Console.WriteLine($"Capital of {key} is {capitals[key]}");
    }
}

void TryToDelete(string KeyToDelete)
{
    var deletedValue = string.Empty;
    bool success = capitals.TryRemove(KeyToDelete, out deletedValue);
    Console.WriteLine(success ? $"Deleted Key {KeyToDelete} with output {deletedValue}" : $"Not able to delete {KeyToDelete}");
}

void AddOrUpdate()
{
    var result = capitals.AddOrUpdate("Rusia", "Moscow", (key, oldValue) => {
        Console.Write($"{oldValue} Updated -->");
        return "Moscow+";
    });
    Console.WriteLine(result);
}

void FailSilentlyToAdd()
{ 
    Task.Factory.StartNew(AddParis).Wait();
    AddParis();
}
void AddParis() {

    bool success = capitals.TryAdd("France", "Paris");
    string whoAdded = Task.CurrentId.HasValue ? $"Task {Task.CurrentId}" : "Main Thread";
    Console.WriteLine(whoAdded + " Added New Key");
}
void ConcurrentQueue()
{
    capitalsQueue.Enqueue("Paris");
    capitalsQueue.Enqueue("Washington");
    capitalsQueue.Enqueue("Lima");
    capitalsQueue.Enqueue("Bogota");
    capitalsQueue.Enqueue("Madrid");
    capitalsQueue.Enqueue("Lisboa");
    List<Task> tasks = new List<Task>();
    for (int i = 0; i < 8; i++)
    {
        tasks.Add(Task.Factory.StartNew(MultiThreadDeQueue));
    }
    Task.WhenAll(tasks).Wait();
}
void MultiThreadDeQueue()
{
    string capital;
    var success = capitalsQueue.TryDequeue(out capital);
    string nextCapital;
    var topElement = capitalsQueue.TryPeek(out nextCapital);
    Console.WriteLine($"deque returned {success} with value {capital} new active element is {nextCapital}");
}
void ConcurrentStack()
{
    foreach (var number in Enumerable.Range(1,10))
    {
        numbersStack.Push(number);
    }
    List<Task> tasks = new List<Task>();
    for (int i = 0; i < 8; i++)
    {
        tasks.Add(Task.Factory.StartNew(MultiThreadPop));
    }
    Task.WhenAll(tasks).Wait();
}
void MultiThreadPop()
{
    int number = -1;
    if (numbersStack.TryPop(out number))
        Console.WriteLine($"Pop Number {number}");
}
public class InsensitiveComparer : IEqualityComparer<string>
{
    public bool Equals(string? x, string? y)
    {
        return x!= null && y!= null && x.ToLowerInvariant() == y.ToLower();
    }

    public int GetHashCode([DisallowNull] string obj)
    {
        return obj.ToLowerInvariant().GetHashCode();
    }
}