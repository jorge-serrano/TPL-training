// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using ParallelTasks;

Console.WriteLine("Hello, World!");
long wordsCount = 0;
long charsCount = 0;
long sum = 0;
//Basics();
//PFor();
//PForEachSimple();
//PForEachWithAcumulation();
//PForEachWithAcumulationInternal();
BenchMarkingCode();

void BenchMarkingCode()
{
    var sumary = BenchmarkRunner.Run<SquareCalculator>();
}
void PForEachWithAcumulationInternal()
{
    Parallel.ForEach(Enumerable.Range(1, 200),
                ()=>0,
                (currentVal, state, localStorage) => //delegate for acumulation
                {
                    localStorage += currentVal;
                    return localStorage;
                },
                partialTotal => //delegate for getting current local storage
                {
                    Console.WriteLine($"ACumulated {partialTotal}");
                    Interlocked.Add(ref sum, partialTotal);
                }
                );
    Console.WriteLine($"Total Sum is {sum}");
}

//Cancellation();

void Cancellation()
{
    var words = new[] { "This", "Is", "A","", "Test", "With", "More", "Words" };
    var result = Parallel.ForEach(words, (word, state, counter) => {
        if(string.IsNullOrEmpty(word))
            state.Break();
        Interlocked.Increment(ref wordsCount);
        Interlocked.Add(ref charsCount, word.Length);
    });
    Console.WriteLine($"Total Words before empty {wordsCount} and {charsCount} characters break Iteration {result.LowestBreakIteration}");
}

void PForEachWithAcumulation()
{
    var words = new[] { "This", "Is", "A", "Test","With", "More","Words" };
    var result = Parallel.ForEach(words, (word,state,counter) => {
        Interlocked.Increment(ref wordsCount);
        Interlocked.Add(ref charsCount,word.Length);
    });
    Console.WriteLine($"Total Words {wordsCount} and {charsCount} characters");
}

void PForEachSimple()
{
    Parallel.ForEach(Enumerable.Range(1, 10), (number) => Console.WriteLine(number * number));
}

void PFor()
{
    Parallel.For(1, 10, (number) => Console.WriteLine(number * number));
    
}

void Basics()
{
    var first = new Action(() => {
        Console.WriteLine("First Sequence");
        for (int i = 0; i < 100; i++)
        {
            Console.WriteLine($"1- {i}");
        }
    });
    var second = new Action(() => {
        Console.WriteLine("Second Sequence");
        for (int i = 0; i < 100; i++)
        {
            Console.WriteLine($"2- {i}");
        }
    });
    Parallel.Invoke(first,second);
}