using System;
using System.Threading;

namespace ServerCore
{
    class Program
    {
        // Mutex: 커널 모드로 동작하기 때문에 속도가 느리다. (유저 모드에서 커널 모드로 바꿀 때 속도 느림)
        static int num = 0;
        static Mutex _lock = new Mutex();

        static void Thread_1()
        {
            for (int i = 0; i < 100000; i++)
            {
                _lock.WaitOne();
                num++;
                _lock.ReleaseMutex();
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 100000; i++)
            {
                _lock.WaitOne();
                num--;
                _lock.ReleaseMutex();
            }
        }

        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);
            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(num);
        }
    }
}
