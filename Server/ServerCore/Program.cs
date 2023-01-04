using System;
using System.Threading;

namespace ServerCore
{
    class Lock
    {
        // 1. AutoResetEvent
        // AutoResetEvent를 기다리는 스레드들에게 신호를 보내 하나의 스레드만 통과시키고 나머지 스레드들은 다음 신호를 기다리게 한다.
        AutoResetEvent _available = new AutoResetEvent(true);

        // 2. ManualResetEvent
        // ManualResetEvent _available = new ManualResetEvent(true);

        public void Acquire()
        {
            // 스레드 A가 AutoResetEvent 객체의 WaitOne() 메소드를 써서 대기하고 있다가, 
            // 다른 스레드 B에서 이 AutoResetEvent 객체의 Set() 메소드를 호출하면
            // 스레드 A는 대기 상태를 해제하고 계속 다음 문장을 실행할 수 있게 된다.

            _available.WaitOne(); // 입장 시도
        }

        public void Release() 
        {
            // Set(): flag == true(AutoResetEvent(true))으로 만든다.
            _available.Set();
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
