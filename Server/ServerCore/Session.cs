using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    // Session: 클라이언트와 서버 간 연결이 종료되기 전의 상태
    internal class Session
    {
        Socket _socket;
        int _disconnected = 0;

        object _lock = new object();
        Queue<byte[]> _sendQueue = new Queue<byte[]>();
        bool _pending = false;
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();

        public void Start(Socket socket)
        {
            _socket = socket;
            SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
            // 비동기 작업을 완료했을 때 callback 형태로 OnRecvComplete 함수 실행
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvComplete);
            recvArgs.SetBuffer(new byte[1024], 0, 1024);

            // 언제 Send가 발생할지 모르므로 _sendArgs를 미리 만들어놓고, Send 발생 시 사용
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv(recvArgs);
        }

        public void Send(byte[] sendBuff)
        {
            //_socket.Send(sendBuff);

            // 비동기 작업을 완료했을 때 callback 형태로 OnRecvComplete 함수 실행
            //_sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
            //_sendArgs.SetBuffer(sendBuff, 0, sendBuff.Length);

            //RegisterSend();

            lock (_lock)
            {
                // 패킷을 보낼 때마다 queue에 저장
                _sendQueue.Enqueue(sendBuff);
                // 다른 프로세스가 Send 작업을 하지 않는 상태
                if (_pending == false)
                    RegisterSend();
            }
        }

        public void Disconnect()
        {
            // 한 번만 소켓 통신을 종료할 수 있도록 함
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        #region 네트워크 통신
        void RegisterSend()
        {
            _pending = true;
            byte[] buff = _sendQueue.Dequeue();
            _sendArgs.SetBuffer(buff, 0, buff.Length);

            // SendAsync: MMO 서버에서 부하가 큰 부분
            // 운영체제가 커널 모드에서 처리하는 부분
            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
                OnSendCompleted(null, _sendArgs);
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                // 성공적으로 패킷 전송
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        if (_sendQueue.Count > 0)
                            RegisterSend();
                        else // 아무도 _sendQueue에 패킷을 추가하지 않음
                            _pending = false;

                        // Receive 때처럼 RegisterSend(args)를 할 경우 Send를 두 번하는 꼴이 됨
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed {e}"); ;
                    }
                }
                else
                {
                    Disconnect();
                }
            }
            
        }

        void RegisterRecv(SocketAsyncEventArgs args)
        {
            bool pending = _socket.ReceiveAsync(args);

            // 바로 성공했을 때는 직접 호출
            // 이후에 성공했을 때는 recvArg에서 등록한 OnRecvComplete 함수를 콜백 형태로 호출
            if (pending == false)
                OnRecvComplete(null, args);
        }

        void OnRecvComplete(object sender, SocketAsyncEventArgs args) 
        { 
            // 성공적으로 데이터를 받음
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    string recvData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
                    Console.WriteLine($"[From Client] {recvData}");
                    RegisterRecv(args);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Failed {e}"); ;
                }
            }
            else
            {
                Disconnect();
            }
        }
        #endregion
    }
}
