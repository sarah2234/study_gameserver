using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
    class Program
    {
        static void Main(string[] args) 
        {
            // DNS (Domain Name System)
            string host = Dns.GetHostName();
            // IPHostEntry: 호스트 이름, 등록되어 있는 ip 주소들 등 호스트에 대한 DNS 정보 포함
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            // ip 주소는 여러 개 존재 가능 (트래픽이 많을 시 각 이용자에게 다른 ip 주소 할당)
            IPAddress ipAddr = ipHost.AddressList[0];
            // endPoint: 최종 주소
            // address(ipAddr): 인터넷 호스트의 IP 주소
            // port(7777): address와 연결된 포트 번호이거나, 사용할 수 있는 포트가 있을 시 0으로 지정 
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            // TCP 소켓 객체 생성
            Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // 소켓에 필요한 정보(ip 주소, 포트 번호) 바인딩
                listenSocket.Bind(endPoint);

                // Listen: 소켓을 listen(클라이언트의 연결 요청을 기다리는 상태)로 만든다.
                // backlog: 최대 대기 수
                listenSocket.Listen(10);

                while (true)
                {
                    Console.WriteLine("Listening...");

                    // 클라이언트의 연결 요청을 받음
                    Socket clientSocket = listenSocket.Accept();

                    // 메세지를 받음
                    byte[] recvBuffer = new byte[1024];
                    int recvBytes = clientSocket.Receive(recvBuffer);
                    // Encoding.GetString: 지정한 바이트 배열의 모든 바이트를 문자열로 디코딩
                    // index에서 시작하여 count 길이만큼의 바이트 시퀀스를 문자열로 디코딩
                    string recvData = Encoding.UTF8.GetString(recvBuffer, 0, recvBytes);
                    Console.WriteLine($"[From Client] {recvData}");

                    // 메세지를 보냄
                    byte[] sendBuffer = Encoding.UTF8.GetBytes("Welcome to MMORPG Server!");
                    clientSocket.Send(sendBuffer);

                    // 통신 종료
                    // SocketShutdown.Both: 소켓의 송수신을 모두 사용하지 않음
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
