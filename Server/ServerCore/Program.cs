using System;
using System.Threading;

namespace ServerCore
{
    class Program
    {
        static int number = 0;
        static object _obj = new object();

        static void Thread_1()
        {

            for (int i = 0; i < 10000; i++)
            {
                // 상호 배제(Mutual Exclusive): 임계 구역을 단 한 개의 프로세스만이 접근할 수 있도록 함
                // 코드의 가독성을 위해 try-finally 사용
                // 하지만 좀 더 편하게 lock 키워드 사용!!
                //try
                //{
                //    Monitor.Enter(_obj); // 문을 잠그는 행위
                //    number++;

                //    return;
                //}
                //finally
                //{
                //    Monitor.Exit(_obj); // 잠금을 푸는 행위
                //}

                lock (_obj)
                {
                    number++;
                }
            }
        }

        // Deadlock: 둘 이상의 프로세스가 다른 프로세스가 점유하고 있는 자원을 서로 기다릴 때 무한 대기에 빠지는 상황
        // deadlock을 피하기 위해 Monitor.Enter 후 반드시 Monitor.Exit 추가
        static void Thread_2()
        {
            for (int i = 0; i < 10000; i++)
            {
                //Monitor.Enter(_obj);
                //number--;
                //Monitor.Exit(_obj);

                lock(_obj)
                {
                    number--;
                }
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
