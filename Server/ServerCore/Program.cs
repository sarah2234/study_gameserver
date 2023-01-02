using System;
using System.Threading;

namespace ServerCore
{
    // DeadLock을 완전히 막을 수는 없다.
    // DeadLock이 발생한 것을 확인 후 코드 수정이 효율적

    // DeadLock 예제
    class SessionManager
    {
        static object _lock = new object();

        static public void TestSession()
        {
            lock (_lock)
            {

            }
        }

        static public void Test()
        {
            lock (_lock)
            {
                UserManager.TestUser();
            }
        }
    }

    class UserManager
    {
        static object _lock = new object();

        static public void Test()
        {
            lock (_lock)
            {
                SessionManager.TestSession();
            }
        }

        static public void TestUser()
        {
            lock (_lock)
            {

            }
        }
    }

    class Program
    {
        static int number = 0;
        static object _obj = new object();

        static void Thread_1()
        {
            for (int i = 0; i < 10000; i++)
            {
                SessionManager.Test();
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 10000; i++)
            {
                UserManager.Test();
            }
        }

        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);
            t1.Start();
            //Thread.Sleep(1000); // 실행 시작 시간이 다르면 deadlock이 발생하지 않음
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(number);
        }
    }
}
