using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;

namespace ServerCore
{
    class GameSession : Session
    {
        /* 클라이언트의 연결 요청이 성공적으로 받아들여졌을 때 수행하는 함수 */
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");

            byte[] sendBuffer = Encoding.UTF8.GetBytes("Welcome to MMORPG Server!");
            Send(sendBuffer);
            Thread.Sleep(1000);
            Disconnect();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        public override void OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Client] {recvData}");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transfer bytes: {numOfBytes}");
        }
    }

    class Program
    {
        static Listener _listener = new Listener();

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
            /* 어떤 세션을 만들지 여기서 결정 */
            _listener.Init(endPoint, () => { return new GameSession(); });
            Console.WriteLine("Listening...");

            while (true)
            {
               
            }
        }
    }
}
