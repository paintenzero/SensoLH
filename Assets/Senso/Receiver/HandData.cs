using System;
using UnityEngine;

namespace Senso
{
    ///
    /// @brief Enumeration for fingers
    public enum EFingerType
    {
        Thumb, Index, Middle, Third, Little
    };

    /*
     {
	    "src": "cc:78:ab:ad:ac:6f",
	    "name": "",
	    "fullname": "",
	    "type": "position",
	    "data": {
		    "type": "rh",
		    "palm": {
			    "pos": [0.000000, 0.000000, 0.000000],
			    "quat": [0.99907601, -0.00936431, -0.00414328, -0.04174160]
		    },
		    "wrist": {
			    "quat": [0.94954628, 0.11741631, 0.16499953, -0.23947972]
		    },
		    "shoulder": {
			    "quat": [1.00000000, 0.00000000, 0.00000000, 0.00000000]
		    },
		    "fingers": [{
			    "ang": [0.029587, -1.186242],
			    "quat": [0.96821135, -0.00132843, 0.24999560, 0.00819642],
			    "bend": 0.000
		    }, {
			    "ang": [0.015035, -0.344085]
		    }, {
			    "ang": [0.018008, -0.002090]
		    }, {
			    "ang": [0.006028, -0.004824]
		    }, {
			    "ang": [0.007790, 0.160404]
		    }],
		    "m0": [72, 70, -168],
		    "m1": [75, 95, 19],
		    "m2": [0, 0, 0],
		    "battery": 100,
		    "temperature": 32
	    }
    }
    */

    ///
    /// @brief Implements a container for Senso hand pose information.
    ///
    [Serializable]
    public class HandData
    {
        public string type;

        public PosQuat palm;
        public Quat wrist;
        public Quat shoulder;
        public JsonFinger[] fingers;

        public int[] m0, m1, m2;
        public int battery;
        public int temperature;
        
        // Main positions and rotation
        public EPositionType handType { get
            {
                var _type = EPositionType.Unknown;
                if (type.Equals("rh")) _type = EPositionType.RightHand;
                else if (type.Equals("lh")) _type = EPositionType.LeftHand;
                return _type;
            }
        }
        public Vector3 PalmPosition { get { return arrToVec3(palm.pos); } }
        public Quaternion PalmRotation { get { return arrToQuat(palm.quat); } }
        public Quaternion WristRotation { get { return arrToQuat(wrist.quat); } }
        public bool WristPresent { get { return wrist.quat != null; } }
        public Quaternion ShoulderRotation { get { return arrToQuat(shoulder.quat); } }
        public bool ShoulderPresent { get { return shoulder.quat != null; } }

        // Fingers
        public Vector2 ThumbAngles { get { return arrToFingerAngles(fingers[0].ang); } }
        public Quaternion ThumbQuaternion { get { return arrToQuat(fingers[0].quat); } }
        public float ThumbBend { get { return fingers[0].bend; } }
        public bool AdvancedThumb { get { return fingers[0].quat != null; } }
        public Vector2 IndexAngles { get { return arrToFingerAngles(fingers[1].ang); } }
        public Vector2 MiddleAngles { get { return arrToFingerAngles(fingers[2].ang); } }
        public Vector2 ThirdAngles { get { return arrToFingerAngles(fingers[3].ang); } }
        public Vector2 LittleAngles { get { return arrToFingerAngles(fingers[4].ang); } }
       
        // Utilities
        static private Quaternion arrToQuat(float[] arr)
        {
            if (arr == null) return Quaternion.identity;
            return new Quaternion(arr[1], -arr[3], arr[2], arr[0]);
        }
        static private Vector3 arrToVec3(float[] arr)
        {
            if (arr == null) return Vector3.zero;
            return new Vector3(arr[0], arr[2], arr[1]);
        }
        static private Vector2 arrToFingerAngles(float[] arr)
        {
            if (arr == null) return Vector2.zero;
            return new Vector2(arr[1] * Mathf.Rad2Deg, arr[0] * Mathf.Rad2Deg);
        }
    }

    [Serializable]
    public class HandDataFull : NetData
    {
        public HandData data;
    }

    [Serializable]
    public class JsonFinger
    {
        public float[] ang;
        public float[] quat;
        public float bend;
    }
}

