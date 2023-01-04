using System;
using System.Threading;

namespace ServerCore
{
    class Program
    {
        // *상황에 따라 어떤 lock의 성능이 상이하기 때문에 때에 따라 다르게 lock 구현

        // 상호 배제
        static object _lock = new object();
        static SpinLock _lock2 = new SpinLock();

        // ReaderWriterLock의 최신 버전인 Slim
        static ReaderWriterLockSlim _lock3 = new ReaderWriterLockSlim();

        class Reward
        {

        }

        /*
         * 스레드 간에 공유되는 데이터가 있을 때, 항상 모든 스레드가 그 데이터를 읽고 쓰는 것은 아니다.
         * 어떤 스레드는 해당 데이터를 읽기만 하고, 어떤 스레드는 해당 데이터를 쓰기만 할 수 있다.
         * 그리고 소수의 쓰는 스레드가 적은 횟수로 쓰기를 수행하고, 
         * 다수의 읽는 스레드가 빈번하게 읽기를 수행하는 경우가 있을 수 있다.
         * 이런 경우에도 일반적인 lock을 구현하여 읽기/쓰기 수행 시 항상 lock을 설정하고 해제한다면
         * 데이터를 단순히 읽기만 하여 값이 변경되지 않는 상황에도 불필요하게 임계 영역을 만들게 되므로
         * 성능상 굉장히 손해가 될 수 있다.
         * ReaderWriterLock은 데이터에 쓰기 위해 접근할 때는 락을 설정하고,
         * 데이터를 단순히 읽기만 하는 동안에는 락을 설정하지 않도록 비대칭적인 락을 구현함으로써
         * 성능상 이득을 얻을 수 있도록 한다.
         */

        static Reward GetRewardById(int id)
        {
            _lock3.EnterReadLock();

            _lock3.ExitReadLock();
            return null;
        }

        static void AddReward(Reward reward)
        {
            _lock3.EnterWriteLock();

            _lock3.ExitWriteLock();
        }

        static void Main(string[] args)
        {
            // lock 키워드: critical section 제공
            lock (_lock)
            {

            }

            bool lockTaken = false;
            try
            {
                _lock2.Enter(ref lockTaken);
            }
            finally
            {
                if (lockTaken)
                    _lock2.Exit();
            }
        }
    }
}
