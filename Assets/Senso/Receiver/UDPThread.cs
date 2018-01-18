using UnityEngine;
using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

namespace Senso
{
    public class UDPThread : NetworkThread
    {
        private Thread netThread;

        private UdpClient m_sock;
        private IPEndPoint ep;

        private Stack<NetData> pendingPackets;
        private System.Object packetsLock = new System.Object();

        private int SEND_BUFFER_SIZE = 4096; //!< Size of the buffer to send
        private Byte[] outBuffer;
        private int outBufferOffset = 0;

        ///
        /// @brief Default constructor
        ///
        public UDPThread(string host, Int32 port) : base(host, port)
        {
            outBuffer = new Byte[SEND_BUFFER_SIZE];
            
            if (State != NetworkState.SENSO_ERROR)
            {
                ep = new IPEndPoint(m_ip, m_port);
            }

            pendingPackets = new Stack<NetData>();
        }

        ~UDPThread()
        {
            StopThread();
        }

        ///
        /// @brief starts the thread that reads from socket
        ///
        public override void StartThread()
        {
            if (!m_isStarted)
            {
                m_isStarted = true;
                netThread = new Thread(Run);
                netThread.Start();
            }
        }

        ///
        /// @brief Stops the thread that reads from socket
        ///
        public override void StopThread()
        {
            if (m_isStarted)
            {
                m_isStarted = false;
                netThread.Join();
            }
        }

        private void Run()
        { 
            Byte[] inBuffer;
            m_sock = new UdpClient();

            while (m_isStarted && State != NetworkState.SENSO_ERROR)
            {
                try
                {
                    var now = DateTime.Now;

                    bool rcvReady = false;
                    while (m_isStarted && !rcvReady)
                    {
                        rcvReady = m_sock.Client.Poll(10, SelectMode.SelectRead);
                        if (!rcvReady && DateTime.Now.Subtract(now).Milliseconds >= 200) break;
                    }
                    if (rcvReady)
                    {
                        inBuffer = m_sock.Receive(ref ep);
                        int packetStart = 0;
                        for (int i = 0; i < inBuffer.Length; ++i)
                        {
                            if (inBuffer[i] == '\n')
                            {
                                if (State == NetworkState.SENSO_CONNECTING) State = NetworkState.SENSO_CONNECTED;
                                var packet = processJsonStr(Encoding.ASCII.GetString(inBuffer, packetStart, i - packetStart));
                                if (packet != null)
                                {
                                    lock (packetsLock)
                                        pendingPackets.Push(packet);
                                }
                                packetStart = i + 1;
                            }
                        }
                    }
                }
                catch (SocketException ex)
                {
                    Debug.LogError("(Socket) Unable to get packet from Senso with code " + ex.ErrorCode + ": " + ex.Message);
                    State = NetworkState.SENSO_ERROR;
                }
                catch (Exception ex)
                {
                    Debug.LogError("(General) Unable to get packet from Senso: " + ex.Message);
                }
            }
            Debug.Log("(Socket) end");
            m_sock.Close();
            m_sock = null;
            State = NetworkState.SENSO_DISCONNECTED;
        }

        public override Stack<NetData> UpdateData()
        {
            Stack<NetData> result = null;
            lock (packetsLock)
            {
                result = new Stack<NetData>(pendingPackets);
                pendingPackets.Clear();
            }
            return result;
        }


        ///
        /// @brief Send vibrating command to the server
        ///
        public override void VibrateFinger(EPositionType handType, EFingerType fingerType, ushort duration, byte strength)
        {
            if (m_sock != null)
            {
                sendToServer(GetVibrateFingerJSON(handType, fingerType, duration, strength));
            }
        }

        ///
        /// @brief Sends HMD orientation to Senso Server
        ///
        public override void SetHeadLocationAndRotation(Vector3 position, Quaternion rotation)
        {
            if (m_sock != null)
            {
                sendToServer(GetHeadLocationAndRotationJSON(position, rotation));
            }
        }

        ///
        /// @brief Sends ping to Senso Server
        ///
        public override void SendPing()
        {
            if (m_sock != null)
            {
                sendToServer(GetPingJSON());
            }
        }

        private void sendToServer(String str)
        {
            outBufferOffset += Encoding.ASCII.GetBytes(str, 0, str.Length, outBuffer, outBufferOffset);
            m_sock.Send(outBuffer, outBufferOffset, ep);
            outBufferOffset = 0;
        }
    }
}
