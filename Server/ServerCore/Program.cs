using System;
using System.Threading;

namespace ServerCore
{
    class Lock
    {
        // 1. AutoResetEvent
        // AutoResetEvent를 기다리는 스레드들에게 신호를 보내 하나의 스레드만 통과시키고 나머지 스레드들은 다음 신호를 기다리게 한다.
        //AutoResetEvent _available = new AutoResetEvent(true);

        // 2. ManualResetEvent
        // 한 번 열리면 대기 중이던 모든 스레드를 실행하게 하고 코드에서 수동으로 Reset()을 호출하여 문을 닫고 이후 도착한 스레드들을 다시 대기토록 한다. 
        ManualResetEvent _available = new ManualResetEvent(true);

        public void Acquire()
        {
            // AutoResetEvent에서는 WaitOne에서 Reset까지 한 번에 실행되어 0이 제대로 출력됐으나,
            // ManualResetEvent에서는 두 번에 걸쳐서 문이 닫히기 때문에 0이 제대로 출력되지 않는다.
            // => 여러 스레드들이 동시에 진행되어야 할 때 ManualResetEvent 사용
            _available.WaitOne(); // 입장 시도
            _available.Reset(); // 문을 닫음
        }

        public void Release()
        {
            // Set(): flag == true(AutoResetEvent(true))으로 만든다.
            _available.Set(); // 문을 열어줌
        }
    }

    class Program
    {
        static int num = 0;
        static Lock _lock = new Lock();

        static void Thread_1()
        {
            for (int i = 0; i < 100000; i++)
            {
                _lock.Acquire();
                num++;
                _lock.Release();
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 100000; i++)
            {
                _lock.Acquire();
                num--;
                _lock.Release();
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
