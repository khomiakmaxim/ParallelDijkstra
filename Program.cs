using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DijkstraParallel
{

//N: 612
//Main Thread: duration 00:00:01.9588199
//Additional threads: 4, duration: 00:00:01.3147311

//N: 612
//Main Thread: duration 00:00:01.9660381
//Additional threads: 2, duration: 00:00:01.5843526

//N: 1024
//Main Thread: duration 00:00:10.4935633
//Additional threads: 2, duration: 00:00:06.8728448

//N: 128
//Main Thread: duration 00:00:00.0249240
//Additional threads: 4, duration: 00:00:00.0300581

    class Program
    {
        public const int IINF = 2 << 32 - 47;

        public static int[][] MakeMatrix(int size, int? value = null)
        {
            var matrix = new int[size][];
            var rng = new Random();
            for (int i = 0; i < size; i++)
            {
                matrix[i] = new int[size];
                for (int j = 0; j < size; j++)
                {
                    if (i != j)
                    {
                        matrix[i][j] = value ?? rng.Next(10);
                    }
                    else
                    {
                        matrix[i][j] = 0;
                    }
                }
            }
            return matrix;
        }

        public static int[] Dijkstra(int[][] graph, int iv, int size)//граф задається матрицею суміжності
        {
            bool[] used = new bool[size];
            for (int i = 0; i < size; ++i) used[i] = false;

            int[] ans = new int[size];
            for (int i = 0; i < size; ++i) ans[i] = IINF;

            used[iv] = true;
            ans[iv] = 0;

            for (int count = 0; count < size-1; ++count)
            {
                for (int i = 0; i < size; ++i)
                {
                    for (int j = 0; j < size; ++j)
                    {
                        if (graph[i][j] != -1)
                        {
                            if (!used[j])
                            {
                                ans[j] = Math.Min(ans[j], ans[i] + graph[i][j]);
                            }
                        }
                    }
                }
                int minI = -1;
                int min = IINF;

                for (int j = 0; j < size; ++j)
                {
                    if (!used[j])
                    {
                        if (ans[j] < min)
                        {
                            minI = j;
                            min = ans[j];
                        }
                    }
                }                
                used[minI] = true;                
            }

            return ans;
        }

        public static void Print(int[]arr, int n)
        {
            for (int i = 0; i < n; ++i)
            {
                Console.Write(arr[i] + " ");
            }
            Console.Write("\n\n");
        }

        public static void TestParallelism(int[][] graph, int start, int size, int numThreads)
        {
            var startTime = DateTime.Now;
            var result = ParallelDijkstra(graph, start, size, numThreads);
            //Console.WriteLine("result: ");
            //Print(result, size);
            var duration = DateTime.Now - startTime;
            Console.WriteLine($"Additional threads: {numThreads}, duration: {duration}");
        }

        static void Main(string[] args)
        {
            #region Quick Check            
            //int[][] graph = new int[][]
            //    {
            //        new int[]{0, 7, -1, -1, 10},
            //        new int[]{-1, 0, 3, -1, 4},
            //        new int[]{-1, -1, 0, 5, -1},
            //        new int[]{-1, -1, -1, 0, -1},
            //        new int[]{-1, -1, -1, 2, 0}
            //    };

            //Console.WriteLine("Main thread:");
            //int[] result = Dijkstra(graph, 0, 5);
            //foreach (var i in result)
            //{
            //    Console.Write(i + " ");
            //}
            //Console.WriteLine("\n");            
            //TestParallelism(graph, 0, 5, 4);
            #endregion

            Console.Write("N: ");
            int N = int.Parse(Console.ReadLine());
            int[][] graph = MakeMatrix(N);

            var startTime = DateTime.Now;
            var result = Dijkstra(graph, 0, N);
            var duration = DateTime.Now - startTime;
            Console.WriteLine($"Main Thread: duration {duration}");

            TestParallelism(graph, 0, N, 4);
        }

        public static int[] ParallelDijkstra(int[][] graph, int iv, int size, int numThreads)
        {
            bool[] used = new bool[size];
            for (int i = 0; i < size; ++i) used[i] = false;

            int[] ans = new int[size];
            for (int i = 0; i < size; ++i) ans[i] = IINF;

            used[iv] = true;
            ans[iv] = 0;

            for (int count = 0; count < size - 1; ++count)
            {
                var tasks = new List<Task>();
                var partRowCount = size / numThreads;
                for (int startRow = 0; startRow < size; startRow += partRowCount)
                {
                    var start = startRow;
                    tasks.Add(Task.Factory.StartNew(() => PartialResult(graph, start, size, partRowCount, used, ans)));
                }

                Task.WaitAll(tasks.ToArray());                
                int minI = -1;
                int min = IINF;

                for (int j = 0; j < size; ++j)
                {
                    if (!used[j])
                    {
                        if (ans[j] < min)
                        {
                            minI = j;
                            min = ans[j];
                        }
                    }
                }                
                used[minI] = true;
            }

            return ans;
        }

        public static void PartialResult(int[][] graph, int start, int size, int rowCount, bool[] used, int[] ans)
        {
            //оцю частину можна паралелити
            for (int i = start; i < start + rowCount; ++i)
            {
                if (i >= size) return;
                for (int j = 0; j < size; ++j)
                {                    
                    if (graph[i][j] != -1)
                    {
                        if (!used[j])
                        {
                            ans[j] = Math.Min(ans[j], ans[i] + graph[i][j]);
                        }
                    }
                }
            }
        }
    }
}
