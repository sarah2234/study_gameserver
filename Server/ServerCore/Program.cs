using System;
using System.Threading;

namespace ServerCore
{
    /*
     * Thread Local Storage
     * TLS란 스레드가 각각 가지고 있는 저장소이다.
     * 이 곳에 저장되는 값들은 전역 변수이지만, 한 스레드 내에서만 유효한 전역 변수이다.
     */
    class Program
    {
        // ThreadLocal<> 인자로 func delegate를 받으면 TLS를 새로 만들 때 ThreadLocal.Value에 return 값을 넣어준다.
        static ThreadLocal<string> ThreadName = new ThreadLocal<string>(() => { return $"My name is {Thread.CurrentThread.ManagedThreadId}"; });

        static void WhoAmI()
        {
            // 이미 만들어진 ThreadName을 재사용함
            bool repeat = ThreadName.IsValueCreated;
            if (repeat)
                Console.WriteLine(ThreadName.Value + "(repeat)");
            else // ThreadName.Value == null일 때 인자로 받은 func delegate 실행
                Console.WriteLine(ThreadName.Value); 
        }
        
        static void Main(string[] args) 
        {
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(3, 3);
            // Parallel.Invoke(): 여러 작업들을 병렬로 처리하는 기능을 제공한다.
            Parallel.Invoke(WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI);

            // ThreadLocal 인스턴스가 사용하는 리소스들을 해제함
            ThreadName.Dispose();
        }
    }
}
