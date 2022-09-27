using System;
using System.Collections.Generic;
using Benchmarking;
using PDF;

struct Graph
{
    public float xSum;
    public float ySum;
    public float xySum;
    public float xSqSum;
    public float ySqSum;
    public float n;
    public float a;
    public float b;
    public float r;
    public float r2;
}

class Code
{
    static void Main()
    {
        BenchmarkData data = Benchmark.Run("Console.WriteLine", () => {

            Console.WriteLine("a");

        }, 200, 1);

        BenchmarkData data2 = Benchmark.Run("Console.Clear", () => {

            Console.Clear();

        }, 20, 1);

        PDFHandler.NewPDF();
        PDFHandler.Generate(new BenchmarkData[]{ data, data2 });
        
        while (true) ;
    }

    
}
