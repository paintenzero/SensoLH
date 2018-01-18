using System;
using UnityEngine;

namespace Senso
{
    [Serializable]
    public class BodyData
    {
        public String type;
        public float[] position;
        public float[] campos;

        public Quat pelvis;
        public Quat spine;
        public Quat neck;
        public LeftRightQuat clavicle;
        public LeftRightQuat shoulder;
        public LeftRightQuat elbow;
        public LeftRightQuat palm;

        public LeftRightQuat hip;
        public LeftRightQuat knee;
        public LeftRightQuat foot;

        public Vector3 PelvisPosition { get { return arrToVec3(position); } }
        public Vector3 CameraPosition { get { return arrToVec3(campos); } }

        public Quaternion PelvisRotation { get { return arrToQuat(pelvis.quat); } }
        public Quaternion SpineRotation { get { return arrToQuat(spine.quat); } }
        public Quaternion NeckRotation { get { return arrToQuat(neck.quat); } }

        public Quaternion ClavicleLeftRotation { get { return arrToQuat(clavicle.left.quat); } }
        public Quaternion ClavicleRightRotation { get { return arrToQuat(clavicle.right.quat); } }

        public Quaternion ShoulderLeftRotation { get { return arrToQuat(shoulder.left.quat); } }
        public Quaternion ShoulderRightRotation { get { return arrToQuat(shoulder.right.quat); } }

        public Quaternion ElbowLeftRotation { get { return arrToQuat(elbow.left.quat); } }
        public Quaternion ElbowRightRotation { get { return arrToQuat(elbow.right.quat); } }

        public Quaternion PalmLeftRotation { get { return arrToQuat(palm.left.quat); } }
        public Quaternion PalmRightRotation { get { return arrToQuat(palm.right.quat); } }

        public Quaternion HipLeftRotation { get { return arrToQuat(hip.left.quat); } }
        public Quaternion HipRightRotation { get { return arrToQuat(hip.right.quat); } }

        public Quaternion KneeLeftRotation { get { return arrToQuat(knee.left.quat); } }
        public Quaternion KneeRightRotation { get { return arrToQuat(knee.right.quat); } }

        public Quaternion FootLeftRotation { get { return arrToQuat(foot.left.quat); } }
        public Quaternion FootRightRotation { get { return arrToQuat(foot.right.quat); } }

        // Utilities
        static private Quaternion arrToQuat(float[] arr)
        {
            if (arr == null) return Quaternion.identity;
            return new Quaternion(arr[1], arr[3], arr[2], arr[0]);
        }
        static private Vector3 arrToVec3(float[] arr)
        {
            if (arr == null) return Vector3.zero;
            return new Vector3(arr[0], arr[2], arr[1]);
        }
    }

    [Serializable]
    public class BodyDataFull : NetData
    {
        public BodyData data;
    }

    [Serializable]
    public class LeftRightQuat
    {
        public Quat left;
        public Quat right;
    }
}
