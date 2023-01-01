using System;
using System.Threading;

namespace ServerCore
{
    class Program
    {
        static void Main(string[] args)
        {
            int[,] arr = new int[10000, 10000];

            // 행 우선 접근이 열 우선 접근보다 빠른 이유:
            // 캐시의 spatial locality 전략으로 행을 캐시에 저장 
            // 행 우선 접근 시 cache hit 발생 -> 빠르게 데이터 접근 가능
            {
                long now = DateTime.Now.Ticks;
                for (int y = 0; y < 10000; y++)
                    for (int x = 0; x < 10000; x++)
                        arr[y, x] = 1;
                long end = DateTime.Now.Ticks;
                Console.WriteLine($"(y, x) 순서 걸린 시간 {end - now}");
            }

            {
                long now = DateTime.Now.Ticks;
                for (int y = 0; y < 10000; y++)
                    for (int x = 0; x < 10000; x++)
                        arr[x, y] = 1;
                long end = DateTime.Now.Ticks;
                Console.WriteLine($"(x, y) 순서 걸린 시간 {end - now}");
            }
        }
    }
}
