using System;
using System.Threading;

namespace ServerCore
{
    class Program
    {
        static int number = 0;

        static void Thread_1()
        {
            // atomic(원자성) : 동작은 모두 성공하거나 실패해야 함

            // 어셈블리어에서 number++;는
            // int temp = number;
            // temp += 1;
            // number = temp;로 쪼개짐

            for (int i = 0; i < 10000; i++)
            {
                // Interlocked 내부에 간접적으로 메모리 배리어를 사용하므로 가시성 문제 해결 (volatile 필요 x) 
                int value = Interlocked.Increment(ref number); // 성능 손해 but atomic

                //int temp = number; // 0
                //temp += 1; // 1
                //number = temp; // Thread_1의 코드가 먼저 실행됐을 때: number = 1;
                //// 위의 세 코드가 한번에 실행되지 않아 number의 값이 정확해지지 않는 문제 발생
            }
        }

        static void Thread_2()
        {
            // 어셈블리어에서 number--;는
            // int temp = number;
            // temp -= 1;
            // number = temp;로 쪼개짐

            for (int i = 0; i < 10000; i++)
            {
                Interlocked.Decrement(ref number);

                //int temp = number; // 0
                //temp -= 1; // -1
                //number = temp; // Thread_2의 코드가 먼저 실행됐을 때: number = -1;
            }
        }

        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);
            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(number);
        }
    }
}
