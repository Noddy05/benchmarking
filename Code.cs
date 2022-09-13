using System;
using System.Collections.Generic;
using Benchmarking;

class Code
{
    static void Main()
    {
        //Testing the speed of arrays, vs lists

        BenchmarkData data = Benchmark.Run(() => {

            List<int> list = new List<int>();

            int iL = 100;
            int jL = 1000;
            for (int i = 0; i < iL; i++)
            {
                for (int j = 0; j < jL; j++)
                    list.Add(j + i * jL);
            }

        }, 20000, 1);

        data.Print();

        data = Benchmark.Run(() => {

            int iL = 100;
            int jL = 1000;

            int[] array = new int[jL];
            for (int i = 0; i < iL; i++)
            {
                array = new int[(i + 1) * jL];
                for (int j = 0; j < array.Length; j++)
                    array[j] = j;
            }

        }, 20000, 1);

        data.Print();

        //PDF.Generate(data);

        while (true) ;
    }
}
