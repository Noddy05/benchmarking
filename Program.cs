using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Benchmarking
{
    public delegate void CodeSnippet();

    class BenchmarkData
    {
        public string title;
        public int runs;
        public int tests;
        public BenchmarkRunData testData;

        public BenchmarkData(int runs, int tests) { }

        public BenchmarkData(int runs, int tests, BenchmarkRunData testData)
        {
            this.runs = runs;
            this.tests = tests;
            this.testData = testData;
        }

        public void Print()
        {
            float totalTime = TotalExecutionTime();
            float avg = totalTime / (runs * tests);
            int lowest = LowestExecutionTime();
            int highest = HighestExecutionTime();
            Console.WriteLine($"{runs} runs x {tests} tests.");
            Console.WriteLine("+-----------------------------+");
            Console.WriteLine($"Total execution time: {totalTime}ms.");

            if (avg >= 1)
                Console.WriteLine($"Average execution time: {avg}ms.");
            else
                Console.WriteLine($"Average execution time: < 1ms.");

            if (highest >= 1)
                Console.WriteLine($"Highest execution time: {highest}ms.");
            else
                Console.WriteLine($"Highest execution time: < 1ms.");

            if(lowest >= 1)
                Console.WriteLine($"Lowest execution time: {lowest}ms.");
            else
                Console.WriteLine($"Lowest execution time: < 1ms.");
            Console.WriteLine("+-----------------------------+");
        }

        public int TotalExecutionTime()
        {
            return testData.TotalExecutionTime();
        }

        public int LowestExecutionTime()
        {
            int lowest = int.MaxValue;
            int min = testData.LowestExecutionTime();
            if (lowest > min)
                lowest = min;

            return lowest;
        }

        public int HighestExecutionTime()
        {
            int highest = 0;
            int max = testData.HighestExecutionTime();
            if (highest < max)
                highest = max;

            return highest;
        }
    }

    class BenchmarkRunData
    {
        static Dictionary<int, int> timeDistribution = new Dictionary<int, int>();
        
        public int[] executionTimes;

        public void Distribution()
        {
            List<int> timeKeys = new List<int>();
            List<int> timeValues = new List<int>();
            foreach (KeyValuePair<int, int> time in timeDistribution)
            {
                timeKeys.Add(time.Key);
                timeValues.Add(time.Value);
            }
            int[] timeKeyArray = timeKeys.ToArray();
            int[] timeValueArray = timeValues.ToArray();

            Benchmark.InsertionSort(timeKeyArray);
            Benchmark.InsertionSort(timeValueArray);
        }

        public void Print()
        {
            Console.WriteLine("+-----------------------------+");

            Benchmark.InsertionSort(executionTimes);

            Console.WriteLine("+-----------------------------+");
        }

        public int TotalExecutionTime()
        {
            int output = 0;
            for (int i = 0; i < executionTimes.Length; i++)
            {
                output += executionTimes[i];
            }

            return output;
        }

        public int LowestExecutionTime()
        {
            int lowest = int.MaxValue;
            for (int i = 0; i < executionTimes.Length; i++)
            {
                if (lowest > executionTimes[i])
                    lowest = executionTimes[i];
            }

            return lowest;
        }

        public int HighestExecutionTime()
        {
            int highest = 0;
            for (int i = 0; i < executionTimes.Length; i++)
            {
                if (highest < executionTimes[i])
                    highest = executionTimes[i];
            }

            return highest;
        }
    }

    class Benchmark
    {
        public static void InsertionSort(int[] arr)
        {
            int i, key, j;
            for (i = 1; i < arr.Length; i++)
            {
                key = arr[i];
                j = i - 1;

                // Move elements of arr[0..i-1],  
                // that are greater than key, to one 
                // position ahead of their 
                // current position
                while (j >= 0 && arr[j] > key)
                {
                    arr[j + 1] = arr[j];
                    j = j - 1;
                }
                arr[j + 1] = key;
            }
        }

        private static Stopwatch stopWatch = new Stopwatch();

        /// <summary>
        /// Returns the execution time of your code.
        /// </summary>
        /// <param name="function">Enter the function you want to run or use () => { and enter your code here }</param>
        /// <param name="runs">How many times should your code run</param>
        /// <param name="tests">How many times should we test your code</param>
        public static BenchmarkData Run(string title, CodeSnippet function, int runs, int tests, bool showProgress = false)
        {
            stopWatch = new Stopwatch();
            BenchmarkData data = new BenchmarkData(runs, tests, new BenchmarkRunData());
            data.title = title;

            data.testData.executionTimes = new int[runs];

            stopWatch.Start();
            for (int i = 0; i < runs; i++)
            {
                stopWatch.Restart();
                function();
                stopWatch.Stop();
                data.testData.executionTimes[i] = (int)MathF.Round(stopWatch.ElapsedMilliseconds);
            }
            stopWatch.Restart();

            return data;
        }
    }
}

