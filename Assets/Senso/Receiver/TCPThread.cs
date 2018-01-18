using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

namespace Senso
{
    ///
    /// @brief Class that connects to the Senso Server and provides pose samples
    ///
    public class TCPThread : NetworkThread
    {
        private TcpClient m_sock;
        private NetworkStream m_stream;
        private IAsyncResult m_connectRes;

        private int RECV_BUFFER_SIZE = 4096; //!< Size of the buffer for read operations
        private int SEND_BUFFER_SIZE = 4096; //!< Size of the buffer to send
        private Byte[] inBuffer;
        private int inBufferOffset = 0;
        private Byte[] outBuffer;
        private int outBufferOffset = 0;

        ///
        /// @brief Default constructor
        ///
        public TCPThread(string host, Int32 port) : base(host, port)
        {
            inBuffer = new Byte[RECV_BUFFER_SIZE];
            outBuffer = new Byte[SEND_BUFFER_SIZE];
        }

        public override void StartThread()
        {
            if (!m_isStarted)
            {
                m_isStarted = true;
                connect();
            }
        }

        public override void StopThread()
        {
            if (m_isStarted)
            {
                m_isStarted = false;
                disconnect();
            }
        }

        private void connect()
        {
            if (State == NetworkState.SENSO_DISCONNECTED)
            {
                m_sock = new TcpClient(AddressFamily.InterNetwork);
                m_sock.NoDelay = true;
                var connectCB = new AsyncCallback(ProcessConnectResult);
                m_connectRes = m_sock.BeginConnect(m_ip, m_port, connectCB, null);
                State = NetworkState.SENSO_CONNECTING;
            }
        }

        private void disconnect()
        {
            if (State == NetworkState.SENSO_CONNECTING)
            {
                m_sock.EndConnect(m_connectRes);
            }
            else if (State == NetworkState.SENSO_CONNECTED)
            {
                m_sock.Close();
            }
            m_stream = null;
            State = NetworkState.SENSO_DISCONNECTED;
        }

        // The following method is called when each asynchronous operation completes.
        void ProcessConnectResult(IAsyncResult result)
        {
            try
            {
                m_sock.EndConnect(result);
            }
            catch (Exception ex)
            {
                // Eat the exception
            }
            if (m_sock.Connected)
            {
                Debug.Log("Connected to Senso Server");
                m_stream = m_sock.GetStream();
                State = NetworkState.SENSO_CONNECTED;
            }
            else
            {
                Debug.LogError("Unable to connect to Senso Server");
                m_stream = null;
                State = NetworkState.SENSO_DISCONNECTED;
            }
        }

        public override Stack<NetData> UpdateData()
        {
            if (State == NetworkState.SENSO_DISCONNECTED && m_isStarted)
            {
                connect();
            }
            else if (State == NetworkState.SENSO_CONNECTED)
            {
                return ReadNetwork();
            }
            return null;
        }

        private Stack<NetData> ReadNetwork()
        {
            var result = new Stack<NetData>();
            if (!m_sock.Connected) disconnect();
            if (m_stream.DataAvailable)
            {
                // send out messages
                if (outBufferOffset > 0)
                {
                    //Debug.Log(Encoding.ASCII.GetString(outBuffer, 0, outBufferOffset));
                    try
                    {
                        m_stream.Write(outBuffer, 0, outBufferOffset);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Write error: " + ex.Message);
                        disconnect();
                        return result;
                    }
                    outBufferOffset = 0;
                }

                // Read incoming messages
                int readSz = -1;
                try
                {
                    readSz = m_stream.Read(inBuffer, inBufferOffset, RECV_BUFFER_SIZE - inBufferOffset);
                    if (inBufferOffset > 0)
                    {
                        readSz += inBufferOffset;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error reading socket: " + ex.Message);
                }
                int packetStart = 0;
                if (readSz > 0)
                {
                    for (int i = 0; i < readSz; ++i)
                        if (inBuffer[i] == '\n')
                        {
                            var packet = processJsonStr(Encoding.ASCII.GetString(inBuffer, packetStart, i - packetStart));
                            if (packet != null) result.Push(packet);
                            packetStart = i + 1;
                        }
                    if (readSz > packetStart)
                    {
                        Array.Copy(inBuffer, packetStart, inBuffer, 0, readSz - packetStart);
                        inBufferOffset = readSz - packetStart;
                    } else
                    {
                        inBufferOffset = 0;
                    }
                }
                else
                {
                    disconnect();
                }
            }
            return result;
        }

        ///
        /// @brief Send vibrating command to the server
        ///
        public override void VibrateFinger (EPositionType handType, EFingerType fingerType, ushort duration, byte strength)
        {
            sendToServer(GetVibrateFingerJSON(handType, fingerType, duration, strength));
        }

        ///
        /// @brief Sends HMD orientation to Senso Server
        ///
        public override void SetHeadLocationAndRotation(Vector3 position, Quaternion rotation) 
        {
            sendToServer(GetHeadLocationAndRotationJSON(position, rotation));
        }

        ///
        /// @brief Sends ping to Senso Server
        ///
        public override void SendPing()
        {
            sendToServer(GetPingJSON());
        }

        private void sendToServer(String str)
        {
            if (str.Length > outBuffer.Length - outBufferOffset) return;
            outBufferOffset += Encoding.ASCII.GetBytes(str, 0, str.Length, outBuffer, outBufferOffset);
        }
    }
}