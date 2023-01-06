using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    internal class Listener
    {
        /*
         * Func: Func 대리자(Delegate)는 결과를 반환하는 메소드를 참조한다.
         * Func 대리자의 형식 매개변수 중 가장 마지막에 있는 것이 반환 형식이다.
         * ex) delegate TResult Func<out TResult>() => 그 자체가 반환 형식
         * delegate TResult Func<in T, out TResult>(T arg) => 두 번째가 반환 형식
         * delegate TResult Func<in T1, in T2, out TResult>(T1 arg1, T2 arg2) => 세 번째가 반환 형식
         * 
         * Action: Action 대리자(Delegate)는 반환 형식이 없다.
         * Func와 달리 어떤 결과를 반환하는 것이 목적이 아닌, 일련의 작업 수행이 목적이다.
         * Action에 등록된 함수를 실행하기 위해선 Invoke()를 사용한다.
         * ex) delegate void Action<in T1, ..., in T16>(T1 arg1, ..., T16 arg16)
         */

        Socket _listenSocket;
        Action<Socket> _onAcceptHandler; // Socket을 인자로 사용하는 메소드 참조

        public void Init(IPEndPoint endPoint, Action<Socket> onAcceptHandler)
        {
            // TCP 소켓 객체 생성
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _onAcceptHandler += onAcceptHandler;

            // 소켓에 필요한 정보(ip 주소, 포트 번호) 바인딩
            _listenSocket.Bind(endPoint);

            // Listen: 소켓을 listen(클라이언트의 연결 요청을 기다리는 상태)로 만든다.
            // backlog: 최대 대기 수
            _listenSocket.Listen(10);

            // SocketAsyncEventArgs: 소켓 이벤트(한 번 만들어놓으면 재사용 가능)
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            // Event: 특정 상황이 발생했을 때, 그것을 외부에 알리고자 하는 용도. delegate를 기반으로 함
            // EventHandler: event를 어떻게 다룰지 정의한 함수 포인터
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptComplete);
            RegisterAccept(args);
        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            // 초기화
            args.AcceptSocket = null;

            // AcceptAsync: 클라이언트의 연결 요청을 비동기적으로 받을 수 있음(non-blocking)
            bool pending = _listenSocket.AcceptAsync(args);
            if (pending == false) // 클라이언트의 연결 요청이 바로 수락됨
                OnAcceptComplete(null, args);
        }

        void OnAcceptComplete(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                // AcceptSocket: 비동기 소켓 메소드를 통해 연결을 허용하기 위해 만들었거나 사용할 소켓을 가져오거나 설정
                // _onAcceptHandler에 등록된 ServerCore의 Program.OnAcceptHandler()에게 인수로 소켓 전달 (args.AcceptSocket의 반환값: Socket)
                _onAcceptHandler.Invoke(args.AcceptSocket);
            }
            else
                Console.WriteLine(args.SocketError.ToString());

            // 클라이언트의 연결 요청을 받기 위해 사용하는 SocketAsyncEventArgs 재사용
            RegisterAccept(args);
        }
    }
}
