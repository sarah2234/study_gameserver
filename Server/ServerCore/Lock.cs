using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    // 재귀적 lock : write lock을 같은 스레드가 다시 획득할 수 있는지
    // 재귀적 lock 허용 O (WriteLock -> WriteLock / WriteLock -> ReadLock)
    // Spinlock 정책: 5000번 spin -> yield
    class Lock
    {
        const int EMPTY_FLAG = 0x00000000;
        const int WRITE_MASK = 0x7FFF0000; // 32-bit: 0111 1111 1111 1111 0000 0000 0000 0000
        const int READ_MASK = 0x0000FFFF; // 32-bit: 0000 0000 0000 0000 1111 1111 1111 1111
        const int MAX_SPIN_COUNT = 5000;

        // int 32비트 : [Unused(1-bit)] [WriteThreadID(15-bit)] [ReadCount(16-bit)]
        // Unused: 맨 왼쪽 비트 사용 시 flag 값이 음수가 될 수 있으므로 사용하지 않음
        int _flag = EMPTY_FLAG;
        int _writeCount = 0;

        public void WriteLock()
        {
            // 동일 스레드가 WriteLock을 이미 획득하고 있는지 확인
            int lockThreadID = (_flag & WRITE_MASK) >> 16;
            if (Thread.CurrentThread.ManagedThreadId == lockThreadID)
            {
                _writeCount++;
                return;
            }

            // 아무도 WriteLock 또는 ReadLock을 획득하고 있지 않을 때, 경합해서 소유권을 얻는다.
            int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;
            while(true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++) 
                {
                    // WriteThreadID(15)에 해당하는 부분만 저장
                    if (Interlocked.CompareExchange(ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                    {
                        _writeCount = 1;
                        return;
                    }
                }

                Thread.Yield();
            }
        }

        public void WriteUnlock()
        {
            int lockCount = --_writeCount;
            if (lockCount == 0)
                Interlocked.Exchange(ref _flag, EMPTY_FLAG);
        }

        public void ReadLock() 
        {
            // 동일 스레드가 WriteLock을 이미 획득하고 있는지 확인
            int lockThreadID = (_flag & WRITE_MASK) >> 16;
            if (Thread.CurrentThread.ManagedThreadId == lockThreadID)
            {
                Interlocked.Increment(ref _flag);
                return;
            }

            // 아무도 WriteLock을 획득하고 있지 않으면, ReadCount를 1 늘린다.
            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    int expected = (_flag & READ_MASK); // WriteThreadID 부분이 0000 이므로 아무도 WriteLock을 획득하고 있지 않음
                    if (Interlocked.CompareExchange(ref _flag, expected + 1, expected) == expected)
                        return;
                    
                }

                Thread.Yield();
            }
        }

        public void ReadUnlock()
        {
            Interlocked.Decrement(ref _flag);
        }
    }
}
