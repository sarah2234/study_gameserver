using System;
using System.Threading;

namespace ServerCore
{
    class SpinLock
    {
        volatile int _locked = 0;
        public void Acquire()
        {
            while (true)
            {
                // 1.
                // original 변수 위치: stack / _locked 변수 위치: data
                // Interlocked.Exchange: location1을 value 값으로 설정
                // 리턴: location1의 원래 값
                //int original = Interlocked.Exchange(ref _locked, 1);
                //if (original == 0)
                //    break;

                // 2.
                // 위의 코드처럼 조건 없이 변수 값을 바꾸는 것은 위험할 수 있음
                // Interlocked.CompareExchange: location1과 comparand의 값이 같으면 location1의 값을 value 값으로 바꿈
                // 리턴: location1의 원래 값
                // if (_locked == 1) _locked = 0;과 동일
                int expected = 0;
                int desired = 1;
                if (Interlocked.CompareExchange(ref _locked, desired, expected) == expected)
                    break;
            }
        }

        public void Release() 
        {
            // 잠금 해제
            _locked = 0;
        }
    }

    class Program
    {
        static int num = 0;
        static SpinLock _lock = new SpinLock();

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
