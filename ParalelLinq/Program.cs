using System.Linq;
// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
//Basics();
//OrderedParallelQueries();
void OrderedParallelQueries()
{
    var range = Enumerable.Range(0, 20);
    var cubes = new int[range.Count()];
    range.AsParallel().AsOrdered().ForAll(
        n => {
            cubes[n] = n * n * n;
            Console.WriteLine($"Cube of {n} is {cubes[n]}");
        }
        );
}
//ProducerConsumer();
Aggregation();

void Aggregation()
{
    var numbers = ParallelEnumerable.Range(1, 10);
    var customAgg = numbers.Aggregate(
            0,//initial value
            (partial, current) =>partial+=current, //acumulation function
            (acumlate,total) => total + acumlate, // merge (subtotal) function
            item=> {
                if (item > 20)
                    return "High";
                else if (item > 10)
                    return "Medium";
                return "low";
            } //final return (select) function
        );
    Console.WriteLine(customAgg);
}

void ProducerConsumer()
{
    var numbers = Enumerable.Range(0, 100);
    var logs = numbers.AsParallel()
        .WithMergeOptions(ParallelMergeOptions.NotBuffered)
        .Select(
            n => {
                var l = Math.Log10(n);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Produced {l}");
                //Console.ForegroundColor = ConsoleColor.White;
                return l;
            }
        );

    foreach (var l in logs)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($" Consumed {l}");
        //Console.ForegroundColor = ConsoleColor.White;
    }
    Console.ForegroundColor = ConsoleColor.White;
}

void Basics()
{
    var range = Enumerable.Range(0, 500);
    var cubes = new int[range.Count()];
    range.AsParallel().ForAll(
        n => {
            cubes[n] = n * n * n;
            Console.WriteLine($"Cube of {n} is {cubes[n]}");
            } 
        ) ;
}