using UnityEngine;

public class SensoHandsController : MonoBehaviour {

    // Variables for hands objects
    public Senso.Hand[] Hands;
    public Transform HeadPositionSource;
    private System.DateTime orientationNextSend;
    public double orientationSendEveryMS = 100.0f;
    private int m_rightHandInd = -1;
    private int m_leftHandInd = -1;

    // Where to connect to
    public string SensoHost = "127.0.0.1"; //!< IP address of the Senso Server instane
    public int SensoPort = 53450; //!< Port of the Senso Server instance
    private Senso.UDPThread sensoThread;

    public bool StartOnLaunch = true;

    // Initialization
    void Start () {
        if (Hands != null && Hands.Length > 0) {
            for (int i = 0; i < Hands.Length; ++i)
            {
                if (m_rightHandInd == -1 && Hands[i].HandType == Senso.EPositionType.RightHand)
                {
                    m_rightHandInd = i;
                    Hands[i].SetHandsController(this);
                }
                else if (m_leftHandInd == -1 && Hands[i].HandType == Senso.EPositionType.LeftHand)
                {
                    m_leftHandInd = i;
                    Hands[i].SetHandsController(this);
                }
            }
        }

        if (StartOnLaunch) StartTracking();
    }

    private void OnDestroy()
    {
        StopTracking();
    }

    // Every frame
    void Update ()
    {
		if (sensoThread != null)
        {
            if (HeadPositionSource != null)
            {
                var now = System.DateTime.Now;
                if (now >= orientationNextSend)
                {
                    sensoThread.SetHeadLocationAndRotation(HeadPositionSource.transform.localPosition, HeadPositionSource.transform.localRotation);
                    orientationNextSend = now.AddMilliseconds(orientationSendEveryMS);
                }
            }

            var datas = sensoThread.UpdateData();
            if (datas != null)
            {
                bool rightUpdated = false, leftUpdated = false;
                while (datas.Count > 0)
                {
                    var parsedData = datas.Pop();
                    if (parsedData.type.Equals("position"))
                    {
                        if ((m_rightHandInd != -1 && !rightUpdated) || (m_leftHandInd != -1 && !leftUpdated))
                        {
                            var handData = JsonUtility.FromJson<Senso.HandDataFull>(parsedData.packet);
                            if (handData.data.handType == Senso.EPositionType.RightHand && m_rightHandInd != -1 && !rightUpdated)
                            {
                                setHandPose(ref handData, m_rightHandInd);
                                rightUpdated = true;
                            }
                            else if (handData.data.handType == Senso.EPositionType.LeftHand && m_leftHandInd != -1 && !leftUpdated)
                            {
                                setHandPose(ref handData, m_leftHandInd);
                                leftUpdated = true;
                            }
                        }
                    }
                    else if (parsedData.type.Equals("gesture"))
                    {

                    }
                    else if (parsedData.type.Equals("battery"))
                    {
                        // do nothing
                    }
                    else
                    {
                        Debug.Log("Received unknown type: " + parsedData.type);
                    }
                }
            }
        }
	}

    public void StartTracking() {
        if (sensoThread == null) {
            sensoThread = new Senso.UDPThread(SensoHost, SensoPort, 53459);
            sensoThread.StartThread();
        }
    }

    public void StopTracking() {
        if (sensoThread != null) {
            sensoThread.StopThread();
            sensoThread = null;
        }
    }

    public void SendVibro(Senso.EPositionType handType, Senso.EFingerType finger, ushort duration, byte strength)
    {
        sensoThread.VibrateFinger(handType, finger, duration, strength);
    }

    private void setHandPose(ref Senso.HandDataFull handData, int ind)
    {
        if (Hands[ind].MacAddress == null)
        {
            Hands[ind].SetMacAddress(handData.src);
        }
        Hands[ind].SetSensoPose(handData.data);
    }
}
