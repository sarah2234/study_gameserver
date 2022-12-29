using System;
using System.Threading;

namespace ServerCore
{
    class Program
    {
        // volatile: 언제 바뀔지 모르는 값 => 최적화하지 않음
        volatile static bool _stop = false;

        static void ThreadMain()
        {
            Console.WriteLine("스레드 시작!");

            // Release Mode에서 while (_stop == false) 코드 (최적화)
            // if (_stop == false)
            //     while (true)
            // 따라서 아래의 코드는 최적화 대상이 되면 안됨(_stop의 값이 도중에 바뀔 수 있으므로)

            while(_stop == false)
            {
                // 누군가가 stop 신호를 해주기를 기다린다.
            }
            Console.WriteLine("스레드 종료!");
        }

        static void Main(string[] args)
        {
            Task t = new Task(ThreadMain);
            t.Start();

            Thread.Sleep(1000);

            _stop = true;

            Console.WriteLine("Stop 호출");
            Console.WriteLine("종료 대기 중");

            t.Wait();

            Console.WriteLine("종료 성공");
        }
    }
}
