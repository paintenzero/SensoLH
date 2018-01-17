using UnityEngine;

namespace Senso
{
    public abstract class Hand : MonoBehaviour
    {
        public EPositionType HandType;
        private System.WeakReference m_sensoHandsController;
        public int BatteryLevel { get; private set; }
        public int Temperature { get; private set; }
        public string MacAddress { get; private set; }

        public void Start()
        {
            MacAddress = null;
        }

        public void SetBattery(int newLevel)
        {
            BatteryLevel = newLevel;
        }
        public void SetTemperature(int newTemp)
        {
            Temperature = newTemp;
        }

        public void SetMacAddress(string newAddress)
        {
            MacAddress = newAddress;
        }

        public void SetHandsController(SensoHandsController aController)
        {
            m_sensoHandsController = new System.WeakReference(aController);
        }

        public void VibrateFinger(Senso.EFingerType finger, ushort duration, byte strength)
        {
            if (m_sensoHandsController.IsAlive)
            {
                SensoHandsController man = m_sensoHandsController.Target as SensoHandsController;
                man.SendVibro(HandType, finger, duration, strength);
            }
        }

        abstract public void SetSensoPose(HandData newData);
    }
}
