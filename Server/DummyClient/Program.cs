using ServerCore;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DummyClient
{
    class GameSession : Session
    {
        /* 클라이언트의 연결 요청이 성공적으로 받아들여졌을 때 수행하는 함수 */
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");

            for (int i = 0; i < 5; i++)
            {
                /* 서버에게 버퍼 송신 */
                byte[] sendBuffer = Encoding.UTF8.GetBytes($"Hello World! {i}");
                Send(sendBuffer);
            }
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        public override void OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Server] {recvData}");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transfer bytes: {numOfBytes}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            Connector connector = new Connector();

            connector.Connect(endPoint, () => { return new GameSession(); });

            while (true)
            {
                try
                {
                   
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                /* 3초마다 위의 작업 반복 */
                Thread.Sleep(3000);
            }
        }
    }
}
