using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    /* Session: 클라이언트와 서버 간 연결이 종료되기 전의 상태 */
    public abstract class Session
    {
        Socket _socket;
        int _disconnected = 0;

        RecvBuffer _recvBuffer = new RecvBuffer(1024);

        object _lock = new object();
        Queue<byte[]> _sendQueue = new Queue<byte[]>();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

        public void Start(Socket socket)
        {
            _socket = socket;
            /* 비동기 작업을 완료했을 때 callback 형태로 OnRecvComplete 함수 실행 */
            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvComplete);

            /* 언제 Send가 발생할지 모르므로 _sendArgs를 미리 만들어놓고, Send 발생 시 사용 */
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv();
        }

        public void Send(byte[] sendBuff)
        {
            lock (_lock)
            {
                /* 패킷을 보낼 때마다 queue에 저장 */
                _sendQueue.Enqueue(sendBuff);
                /* 다른 프로세스가 Send 작업을 하지 않는 상태 */
                if (_pendingList.Count() == 0)
                    RegisterSend();
            }
        }

        public void Disconnect()
        {
            /* 한 번만 소켓 통신을 종료할 수 있도록 함 */
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            OnDisconnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        #region 네트워크 통신
        void RegisterSend()
        {
            /* SocketAsyncEventArgs.BufferList: 데이터 버퍼의 배열을 가져오거나 설정 */
            /* SetBuffer로 설정한 Buffer도 null이 아니고 BufferList도 null이 아니면 에러 발생 */
            while (_sendQueue.Count > 0)
            {
                byte[] buff = _sendQueue.Dequeue();
                _pendingList.Add(new ArraySegment<byte>(buff, 0, buff.Length)); 
            }
            _sendArgs.BufferList = _pendingList;

            /* SendAsync: MMO 서버에서 부하가 큰 부분 */
            /* 운영체제가 커널 모드에서 처리하는 부분 */
            /* 여러 데이터들을 한 번에 보냄 */
            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
                OnSendCompleted(null, _sendArgs);
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                /* 성공적으로 패킷 전송 */
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        _sendArgs.BufferList = null;
                        _pendingList.Clear();

                        OnSend(_sendArgs.BytesTransferred);

                        /* 도중에 누군가가 패킷을 보냄 */
                        if (_sendQueue.Count > 0)
                            RegisterSend();

                        /* Receive 때처럼 RegisterSend(args)를 할 경우 Send를 두 번하는 꼴이 됨 */
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

        void RegisterRecv()
        {
            _recvBuffer.Clean(); // 커서가 너무 뒤로 가는 것을 방지
            ArraySegment<byte> segment = _recvBuffer.WriteSegment;
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            bool pending = _socket.ReceiveAsync(_recvArgs);

            /* 바로 성공했을 때는 직접 호출 */
            /* 이후에 성공했을 때는 recvArg에서 등록한 OnRecvComplete 함수를 콜백 형태로 호출 */
            if (pending == false)
                OnRecvComplete(null, _recvArgs);
        }

        void OnRecvComplete(object sender, SocketAsyncEventArgs args) 
        {
            /* 성공적으로 데이터를 받음 */
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    // Write 커서 이동
                    if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }
                    // 컨텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리했는지 받음
                    int processLen = OnRecv(_recvBuffer.ReadSegment);
                    if (processLen < 0 || _recvBuffer.DataSize < processLen)
                    {
                        Disconnect();
                        return;
                    }
                    
                    // Read 커서 이동
                    if (_recvBuffer.OnRead(processLen) == false)
                    {
                        Disconnect();
                        return;
                    }

                    RegisterRecv();
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
