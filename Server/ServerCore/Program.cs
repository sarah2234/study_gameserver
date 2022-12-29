using System;
using System.Threading;

namespace ServerCore
{
    class Program
    {
        static void MainThread(object state)
        {
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine("Hello Thread!");
            }
        }

        static void Main(string[] args)
        {
            // SetMinThreads: ThreadPool이 만들 수 있는 스레드의 최소 개수 설정
            // Max 개수를 넘어갈 시 나머지 스레드는 대기 상태
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(5, 5);

            for (int i = 0; i < 5; i++) 
            {
                // TaskCreationOptions.LongRunning: 오래 걸리는 작업이라는 것을 알림
                Task task = new Task(() => { while (true) { } }, TaskCreationOptions.LongRunning);
                task.Start(); // ThreadPool에 넣어서 스레드 관리
            }

            //for (int i = 0; i < 3; i++)
            //{
            //    ThreadPool.QueueUserWorkItem((obj) => { while (true) { Console.WriteLine("."); } });
            //}

            // QueueUserWorkItem: 실행을 위해 메서드를 큐에 대기시킴
            // 가급적 짧게 일하는 스레드를 ThreadPool에 사용하는 것이 cpu 독점 문제 예방 => 이를 해결하기 위해서 Task에서 LongRunning 사용
            ThreadPool.QueueUserWorkItem(MainThread);

            //for (int i = 0; i < 1000; i++)
            //{
            //    Thread t = new Thread(MainThread);
            //    t.Name = "Test Thread";
            // C#에서 스레드는 기본적으로 foreground(isBackground == false)
            //    t.IsBackground = true;
            //    t.Start();
            //}

            //Console.WriteLine("Waiting for Thread!"); 

            // Join(): 스레드가 끝날 때까지 기다렸다가 아래줄 실행
            //t.Join();
            //Console.WriteLine("Hello World!");

            while (true)
            {

            }
        }
    }
}
