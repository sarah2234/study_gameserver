using System;
using System.Threading;

namespace ServerCore
{
    // 메모리 배리어
    // 1) 코드 재배치 억제
    // 2) 가시성

    // 메모리 배리어 종류
    // 1) Full Memory Barrier(어셈블리 명령어 ASM: MFENCE / C#: Thread.MemoryBarrier) : Store/Load 모두 막음
    // 2) Store Memory Barrier (ASM: SFENCE) : Store만 막음
    // 3) Load Memory Barrier (ASM: LFENCE) : Load만 막음

    // Store 후에는 반드시 Thread.MemoryBarrier를 해야 함
    // Load 전에 Thread.MemoryBarrier를 쓰면 데이터의 최신값 보장

    class Program
    {
        static int x = 0;
        static int y = 0;
        static int r1 = 0;
        static int r2 = 0;

        static void Thread_1()
        {
            y = 1; // Store y

            // Thread.MemoryBarrier():
            // 컴파일러에게 barrier 명령문 전후의 메모리 연산을 순서에 맞게 실행하도록 강제
            // 가시성 측면에서: 실제 메모리에 y = 1을 올림 & x의 값을 최신 값으로 가져와 r1에 저장함을 의미 => volatile을 대신함

            Thread.MemoryBarrier();

            r1 = x; // Load x
        }

        static void Thread_2()
        {
            x = 1; // Store x

            Thread.MemoryBarrier();

            r2 = y; // Load y
        }
        
        static void Main(string[] args)
        {
            int count = 0;

            while (true)
            {
                count++;
                x = y = r1 = r2 = 0;

                Task t1 = new Task(Thread_1);
                Task t2 = new Task(Thread_2);
                t1.Start();
                t2.Start();

                // WaitAll(): 지정된 스레드가 모두 끝날 때까지 메인 스레드 대기
                Task.WaitAll(t1, t2);

                if (r1 == 0 && r2 == 0)
                    break;

                // r1 == 0 && r2 == 0 조건식이 성립하는 이유(Thread.MemeoryBarrier()가 없을 시):
                // Thread_1에서 y = 1, r1 = x 두 코드는 연관성이 없으므로 컴파일러가 속도 향상을 위해 임의로 자리를 바꿀 수 있음
                // 따라서 r1 = x이 y = 1보다 먼저 실행됐을 때 r1에는 0이 저장됨
                // Thread_2에서도 마찬가지
            }

            Console.WriteLine($"{count}번만에 빠져나옴!");
        }
    }
}
