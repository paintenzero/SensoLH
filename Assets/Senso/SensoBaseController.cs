using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensoBaseController : MonoBehaviour
{
    public Transform HeadPositionSource;
    private System.DateTime orientationNextSend;
    public double orientationSendEveryMS = 100.0f;

    // Where to connect to
    public string SensoHost = "127.0.0.1"; //!< IP address of the Senso Server instane
    public int SensoPort = 53450; //!< Port of the Senso Server instance
    protected Senso.NetworkThread sensoThread;

    public bool StartOnLaunch = true;
    public bool UseUDP = false;

    // Use this for initialization
    public void Start () {
        if (StartOnLaunch) StartTracking();
    }

    // Update is called once per frame
    public void Update()
    {
        if (sensoThread != null)
        {
            var now = System.DateTime.Now;
            if (now >= orientationNextSend)
            {
                if (HeadPositionSource != null)
                {
                    sensoThread.SetHeadLocationAndRotation(HeadPositionSource.transform.localPosition, HeadPositionSource.transform.localRotation);
                }
                else
                {
                    sensoThread.SendPing();
                }
                orientationNextSend = now.AddMilliseconds(orientationSendEveryMS);
            }
        }
    }

    public void OnDestroy()
    {
        StopTracking();
    }

    public void StartTracking()
    {
        if (sensoThread == null)
        {
            if (UseUDP)
            {
                sensoThread = new Senso.UDPThread(SensoHost, SensoPort);
            }
            else
            {
                sensoThread = new Senso.TCPThread(SensoHost, SensoPort);
            }
            sensoThread.StartThread();
        }
    }

    public void StopTracking()
    {
        if (sensoThread != null)
        {
            sensoThread.StopThread();
            sensoThread = null;
        }
    }
}
