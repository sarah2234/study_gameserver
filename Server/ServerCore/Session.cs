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

        public void Start(Socket socket)
        {
            _socket = socket;
            SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
            // 비동기 작업을 완료했을 때 callback 형태로 OnRecvComplete 함수 실행
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvComplete);
            recvArgs.SetBuffer(new byte[1024], 0, 1024);

            RegisterRecv(recvArgs);
        }

        public void Send(byte[] sendBuff)
        {
            _socket.Send(sendBuff);
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
