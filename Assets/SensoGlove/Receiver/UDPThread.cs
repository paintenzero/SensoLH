using UnityEngine;
using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

namespace Senso
{
    public class UDPThread
    {
        ///
        /// @brief States of the network thread
        public enum NetworkState
        {
            SENSO_DISCONNECTED, SENSO_CONNECTING, SENSO_CONNECTED, SENSO_FAILED_TO_CONNECT, SENSO_ERROR, SENSO_FINISHED, SENSO_STATE_NUM
        };

        public NetworkState State { get; private set; }
        private Thread netThread;
        private bool m_isStarted = false;

        private UdpClient m_sock;
        private IPAddress m_ip;
        private Int32 m_port;
        private Int32 m_localPort;
        private IPEndPoint ep;

        private Stack<NetData> pendingPackets;
        private System.Object packetsLock = new System.Object();

        private int SEND_BUFFER_SIZE = 4096; //!< Size of the buffer to send
        private Byte[] outBuffer;
        private int outBufferOffset = 0;

        ///
        /// @brief Default constructor
        ///
        public UDPThread(string host, Int32 port, Int32 localPort)
        {
            outBuffer = new Byte[SEND_BUFFER_SIZE];

            m_port = port;
            m_localPort = localPort;
            if (!IPAddress.TryParse(host, out m_ip))
            {
                State = NetworkState.SENSO_ERROR;
                Debug.LogError("SensoManager: can't parse senso driver host");
            } else
            {
                ep = new IPEndPoint(m_ip, m_port);
            }
            State = NetworkState.SENSO_DISCONNECTED;

            pendingPackets = new Stack<NetData>();
        }

        ~UDPThread()
        {
            StopThread();
        }

        ///
        /// @brief starts the thread that reads from socket
        ///
        public void StartThread()
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
        public void StopThread()
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
            m_sock = new UdpClient(m_localPort);

            while (m_isStarted && State != NetworkState.SENSO_ERROR)
            {
                try
                {
                    var now = DateTime.Now;

                    bool rcvReady = false;
                    while (m_isStarted && !rcvReady)
                    {
                        rcvReady = m_sock.Client.Poll(10, SelectMode.SelectRead);
                        if (!rcvReady && DateTime.Now.Subtract(now).Milliseconds >= 100) break;
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

        ///
        /// @brief Parses JSON packet received from server
        ///
        private NetData processJsonStr(string jsonPacket)
        {
            NetData parsedData = null;
            try
            {
                parsedData = JsonUtility.FromJson<NetData>(jsonPacket);
            }
            catch (Exception ex)
            {
                Debug.LogError("packet " + jsonPacket + " parse error: " + ex.Message);
            }

            if (parsedData != null)
            {
                parsedData.packet = jsonPacket;
            }
            return parsedData;
        }

        public Stack<NetData> UpdateData()
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
        public void VibrateFinger(EPositionType handType, EFingerType fingerType, ushort duration, byte strength)
        {
            if (m_sock != null)
            {
                var str = String.Format("{{\"dst\":\"{0}\",\"type\":\"vibration\",\"data\":{{\"type\":{1},\"dur\":{2},\"str\":{3}}}}}\n", (handType == EPositionType.RightHand ? "rh" : "lh"), (int)fingerType, duration, strength);
                outBufferOffset += Encoding.ASCII.GetBytes(str, 0, str.Length, outBuffer, outBufferOffset);
                m_sock.Send(outBuffer, outBufferOffset, ep);
                outBufferOffset = 0;
            }
        }

        ///
        /// @brief Sends HMD orientation to Senso Server
        ///
        public void SetHeadLocationAndRotation(Vector3 position, Quaternion rotation)
        {
            if (m_sock != null)
            {
                var str = String.Format("{{\"type\":\"orientation\",\"data\":{{\"type\":\"hmd\",\"px\":{0},\"py\":{1},\"pz\":{2}, \"qx\":{3},\"qy\":{4},\"qz\":{5},\"qw\":{6}}}}}\n", position.x, position.z, position.y, rotation.x, rotation.z, rotation.y, rotation.w);
                outBufferOffset += Encoding.ASCII.GetBytes(str, 0, str.Length, outBuffer, outBufferOffset);
                m_sock.Send(outBuffer, outBufferOffset, ep);
                outBufferOffset = 0;
            }
        }
    }
}
