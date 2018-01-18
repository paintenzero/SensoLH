using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

namespace Senso
{
    ///
    /// @brief States of the network thread
    public enum NetworkState
    {
        SENSO_DISCONNECTED, SENSO_CONNECTING, SENSO_CONNECTED, SENSO_FAILED_TO_CONNECT, SENSO_ERROR, SENSO_FINISHED, SENSO_STATE_NUM
    };

    ///
    /// @brief Class that connects to the Senso Server and provides pose samples
    ///
    abstract public class NetworkThread
    {
        public NetworkState State { get; protected set; }
        protected bool m_isStarted = false;

        protected IPAddress m_ip;
        protected Int32 m_port;

        ///
        /// @brief Default constructor
        ///
        public NetworkThread(string host, Int32 port)
        {
            m_port = port;
            State = NetworkState.SENSO_DISCONNECTED;

            if (!IPAddress.TryParse(host, out m_ip))
            {
                State = NetworkState.SENSO_ERROR;
                Debug.LogError("SensoManager: can't parse senso driver host");
            }
        }

        ~NetworkThread()
        {
            StopThread();
        }

        abstract public void StartThread();
        abstract public void StopThread();
        abstract public Stack<NetData> UpdateData();
        abstract public void SetHeadLocationAndRotation(Vector3 position, Quaternion rotation);
        abstract public void VibrateFinger(EPositionType handType, EFingerType fingerType, ushort duration, byte strength);
        abstract public void SendPing();

        ///
        /// @brief Parses JSON packet received from server
        ///
        protected NetData processJsonStr(string jsonPacket)
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

        ///
        /// @brief Send vibrating command to the server
        ///
        protected String GetVibrateFingerJSON (EPositionType handType, EFingerType fingerType, ushort duration, byte strength)
        {
            return String.Format("{{\"dst\":\"{0}\",\"type\":\"vibration\",\"data\":{{\"type\":{1},\"dur\":{2},\"str\":{3}}}}}\n", (handType == EPositionType.RightHand ? "rh" : "lh"), (int)fingerType, duration, strength);
        }

        ///
        /// @brief Sends HMD orientation to Senso Server
        ///
        public String GetHeadLocationAndRotationJSON (Vector3 position, Quaternion rotation)
        {
            return String.Format("{{\"type\":\"orientation\",\"data\":{{\"type\":\"hmd\",\"px\":{0},\"py\":{1},\"pz\":{2}, \"qx\":{3},\"qy\":{4},\"qz\":{5},\"qw\":{6}}}}}\n", position.x, position.z, position.y, rotation.x, rotation.z, rotation.y, rotation.w);
        }

        public String GetPingJSON()
        {
            return "{\"type\":\"ping\"}";
        }
    }
}