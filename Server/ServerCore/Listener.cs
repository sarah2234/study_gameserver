using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class Listener
    {
        /*
         * C#에서는 대리자(Delegate)로 callback 함수를 구현한다.
         * 
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
        Func<Session> _sessionFactory; /* 세션을 어떤 방식으로 만들지 결정할 수 있음 */

        public void Init(IPEndPoint endPoint, Func<Session> sessionFactory)
        {
            /* TCP 소켓 객체 생성 */
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory += sessionFactory;

            /* 소켓에 필요한 정보(ip 주소, 포트 번호) 바인딩 */
            _listenSocket.Bind(endPoint);

            /* Listen: 소켓을 listen(클라이언트의 연결 요청을 기다리는 상태)로 만든다. */
            /* backlog: 최대 대기 수 */
            _listenSocket.Listen(10);

            /* 서버를 여러 개 만들어 동시다발적으로 클라이언트의 요청들을 받음 */
            for (int i = 0; i < 10; i++)
            {
                /* SocketAsyncEventArgs: 소켓 이벤트(한 번 만들어놓으면 재사용 가능) */
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                /* Event: 특정 상황이 발생했을 때, 그것을 외부에 알리고자 하는 용도. delegate를 기반으로 함 */
                /* EventHandler: event를 어떻게 다룰지 정의한 함수 포인터 */

                /* 프로그램 실행 시 OnAcceptComplete 콜백 함수는 별도의 스레드에서 동작하게 된다. */
                /* => 해당 스레드와 주 스레드 간 race condition이 발생할 수 있는 문제! */
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptComplete);
                RegisterAccept(args);
            }
        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            /* 초기화 */
            args.AcceptSocket = null;

            /* AcceptAsync: 클라이언트의 연결 요청을 비동기적으로 받을 수 있음(non-blocking) */
            bool pending = _listenSocket.AcceptAsync(args);
            if (pending == false) /* 클라이언트의 연결 요청이 바로 수락됨 */
                OnAcceptComplete(null, args);
        }

        void OnAcceptComplete(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                /* 세션 생성(Program.cs에서 어떤 종류의 세션인지 결정) */
                Session session = _sessionFactory.Invoke();
                /* Start()는 외부가 아닌 내부에서 실행되는 것이 좋음 */
                session.Start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            }
            else
                Console.WriteLine(args.SocketError.ToString());

            /* 클라이언트의 연결 요청을 받기 위해 사용하는 SocketAsyncEventArgs 재사용 */
            RegisterAccept(args);
        }
    }
}
