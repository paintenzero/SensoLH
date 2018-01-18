using UnityEngine;

namespace Senso
{
    [System.Serializable]
    public class BodyJoint : System.Object
    {
        public enum Axis { Pitch, Roll, Yaw };
        public enum AngleApplyOrder { XYZ, ZYX }

        public Transform jointGameObject;

        public Axis X = Axis.Yaw;
        public Axis Y = Axis.Pitch;
        public Axis Z = Axis.Roll;

        public bool PitchInverted = false;
        public bool YawInverted = false;
        public bool RollInverted = false;

        private Quaternion startRot;
        private Quaternion currentRot;

        public void OnStart()
        {
            if (jointGameObject != null)
            {
                startRot = jointGameObject.localRotation;
            }
        }

        public void ApplyQuaternion(Quaternion quat)
        {
            Quaternion newQuat = tranformQuat(quat);
            if (jointGameObject != null)
                jointGameObject.localRotation = startRot * newQuat;
            currentRot = quat;
        }

        public void ApplyQuaternion(Quaternion quat, Senso.BodyJoint substractJoint)
        {
            var substractRotation = new Quaternion(-substractJoint.currentRot.x, substractJoint.currentRot.y, -substractJoint.currentRot.z, substractJoint.currentRot.w);

            Quaternion newQuat = tranformQuat(quat * Quaternion.Inverse(substractRotation));
            if (jointGameObject != null)
                jointGameObject.localRotation = startRot * newQuat;
            currentRot = quat;
        }

        public void ApplyQuaternion(Quaternion quat, AngleApplyOrder ord)
        {
            Quaternion newQuat = tranformQuat(quat);
            jointGameObject.localRotation = startRot;
            Vector3 angles = newQuat.eulerAngles;

            if (ord == AngleApplyOrder.ZYX)
            {
                jointGameObject.Rotate(Vector3.forward, angles.z, Space.Self);
                jointGameObject.Rotate(Vector3.up, angles.y, Space.Self);
                jointGameObject.Rotate(Vector3.right, angles.x, Space.Self);
            }
            else if (ord == AngleApplyOrder.XYZ)
            {
                jointGameObject.Rotate(Vector3.right, angles.x, Space.Self);
                jointGameObject.Rotate(Vector3.up, angles.y, Space.Self);
                jointGameObject.Rotate(Vector3.forward, angles.z, Space.Self);
            }
            currentRot = quat;
        }

        private Quaternion tranformQuat(Quaternion quat)
        {
            Quaternion newQuat = new Quaternion();
            newQuat.w = quat.w;

            newQuat.x = GetAxisVal(quat, X, GetAxisInverted(X));
            newQuat.y = GetAxisVal(quat, Y, GetAxisInverted(Y));
            newQuat.z = GetAxisVal(quat, Z, GetAxisInverted(Z));
            return newQuat;
        }

        private float GetAxisVal(Quaternion quat, Axis ax, bool inverted)
        {
            float val = 0.0f;
            if (ax == Axis.Pitch) val = quat.x;
            else if (ax == Axis.Roll) val = quat.z;
            else if (ax == Axis.Yaw) val = quat.y;
            return inverted ? -val : val;
        }

        private bool GetAxisInverted(Axis ax)
        {
            if (ax == Axis.Pitch && PitchInverted) return true;
            else if (ax == Axis.Roll && RollInverted) return true;
            else if (ax == Axis.Yaw && YawInverted) return true;
            return false;
        }
    }

}
