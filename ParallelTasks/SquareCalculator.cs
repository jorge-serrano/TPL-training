using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParallelTasks
{
    public class SquareCalculator
    {
        //Just a comment
        [Benchmark]
        public void SquareEachValue() { 
            const int count = 1000;
            int[] squares = new int[count];
            Parallel.ForEach(Enumerable.Range(0, count), (x) => squares[x] = x * x);
        }
        [Benchmark]
        public void SquareInChunks()
        {
            const int count = 1000;
            int[] squares = new int[count];
            var partition = Partitioner.Create(1, count , 1000);
            Parallel.ForEach(partition, (range) => {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    squares[i] = i * i;
                }
            });
        }
    }
}
