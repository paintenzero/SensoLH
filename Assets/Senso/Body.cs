using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Senso
{
    public abstract class Body : MonoBehaviour
    {
        private System.WeakReference m_bodyController;

        // Use this for initialization
        public void Start()
        {

        }
        
        public void SetBodyController(SensoBodyController aController)
        {
            m_bodyController = new System.WeakReference(aController);
        }

        abstract public void SetSensoPose(BodyData newData);
    }
}

