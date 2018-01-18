using UnityEngine;

public class SensoHandsController : SensoBaseController
{

    // Variables for hands objects
    public Senso.Hand[] Hands;
    private int m_rightHandInd = -1;
    private int m_leftHandInd = -1;

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
        base.Start();
    }

    // Every frame
    void Update ()
    {
        base.Update();
		if (sensoThread != null)
        {
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
                            //Debug.Log(handData.data.handType);
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
                }
            }
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
