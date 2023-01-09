using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DummyClient
{
    class Program
    {
        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            while (true)
            {
                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    /* 서버에게 연결 요청 */
                    socket.Connect(endPoint);
                    Console.WriteLine($"Connected to {socket.RemoteEndPoint.ToString()}");

                    for (int i = 0; i < 5; i++)
                    {
                        /* 서버에게 버퍼 송신 */
                        byte[] sendBuffer = Encoding.UTF8.GetBytes($"Hello World! {i}");
                        int sendBytes = socket.Send(sendBuffer);
                    }

                    /* 서버에게서 버퍼 수신 */
                    byte[] recvBuffer = new byte[1024];
                    int recvBytes = socket.Receive(recvBuffer);
                    string recvData = Encoding.UTF8.GetString(recvBuffer, 0, recvBytes);
                    Console.WriteLine($"[From Server] {recvData}");

                    /* 소켓 통신 종료 */
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
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
