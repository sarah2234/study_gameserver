using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
    class Program
    {
        static Listener _listener = new Listener();

        /* 클라이언트의 연결 요청이 성공적으로 받아들여졌을 때 수행하는 함수 */
        static void OnAcceptHandler(Socket clientSocket)
        {
            try
            {
                /* _listener가 성공적으로 연결 요청을 받았을 때 세션 생성 */
                Session session = new Session();
                session.Start(clientSocket);

                byte[] sendBuffer = Encoding.UTF8.GetBytes("Welcome to MMORPG Server!");
                session.Send(sendBuffer);

                Thread.Sleep(1000);

                session.Disconnect();
                session.Disconnect(); /* 연결 종료를 두 번 했을 시 오류가 뜨는지 확인 */
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void Main(string[] args) 
        {
            /* DNS (Domain Name System) */
            string host = Dns.GetHostName();
            /* IPHostEntry: 호스트 이름, 등록되어 있는 ip 주소들 등 호스트에 대한 DNS 정보 포함 */
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            /* ip 주소는 여러 개 존재 가능 (트래픽이 많을 시 각 이용자에게 다른 ip 주소 할당) */
            IPAddress ipAddr = ipHost.AddressList[0];
            /* endPoint: 최종 주소 */
            /* address(ipAddr): 인터넷 호스트의 IP 주소 */
            /* port(7777): address와 연결된 포트 번호이거나, 사용할 수 있는 포트가 있을 시 0으로 지정 */
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            /* Listen 서버 생성 */
            _listener.Init(endPoint, OnAcceptHandler);
            Console.WriteLine("Listening...");

            while (true)
            {
               
            }
        }
    }
}
