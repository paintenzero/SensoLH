using UnityEngine;

public class SensoHandExample : Senso.Hand {

    public Transform Palm;

	public Transform[] thumbBones;
	public Transform[] indexBones;
	public Transform[] middleBones;
	public Transform[] thirdBones;
	public Transform[] littleBones;

    private Quaternion[] thumbInitialRotations;

    public new void Start ()
	{
		base.Start();
        thumbInitialRotations = new Quaternion[thumbBones.Length];
        for (int i = 0; i < thumbInitialRotations.Length; ++i)
            thumbInitialRotations[i] = thumbBones[i].localRotation;
	}


    public override void SetSensoPose (Senso.HandData aData)
	{
        Palm.localRotation = /*(Quaternion.Inverse(wq) */ aData.PalmRotation;
        Palm.localPosition = aData.PalmPosition;

		//Fingers
        if (aData.AdvancedThumb)
        {
            Quaternion thumbQ = new Quaternion(aData.ThumbQuaternion.y / 3.0f, aData.ThumbQuaternion.x, -aData.ThumbQuaternion.z, aData.ThumbQuaternion.w);
            thumbBones[0].localRotation = thumbInitialRotations[0] * thumbQ;
            thumbBones[1].localRotation = thumbInitialRotations[1] * Quaternion.Euler(aData.ThumbQuaternion.y / 3.0f, 0.0f, 0.0f);
            thumbBones[2].localRotation = thumbInitialRotations[2] * Quaternion.Euler(0.0f, 0.0f, -aData.ThumbBend * 1.5f * Mathf.Rad2Deg);
        }
        else // old method
		      setFingerBones(ref thumbBones, aData.ThumbAngles, Senso.EFingerType.Thumb);
		setFingerBones(ref indexBones, aData.IndexAngles, Senso.EFingerType.Index);
		setFingerBones(ref middleBones, aData.MiddleAngles, Senso.EFingerType.Middle);
		setFingerBones(ref thirdBones, aData.ThirdAngles, Senso.EFingerType.Third);
		setFingerBones(ref littleBones, aData.LittleAngles, Senso.EFingerType.Little);
	}

	private static void setFingerBones(ref Transform[] bones, Vector2 angles, Senso.EFingerType fingerType)
	{
		if (fingerType == Senso.EFingerType.Thumb) setThumbBones(ref bones, ref angles);
		else {
            var vec = new Vector3(0, angles.x, -angles.y);
			if (vec.z > 0.0f) {
				bones[0].localEulerAngles = vec;
			} else {
                vec.z /= 2.0f;
				for (int j = 0; j < bones.Length; ++j) {
					bones[j].localEulerAngles = vec;
					if (j == 0) vec.y = 0.0f;
				}
			}
		}
	}

	private static void setThumbBones(ref Transform[] bones, ref Vector2 angles) {
		bones[0].localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
		float t = angles.x;
		angles.x = -angles.y;
		angles.y = t;

		angles.y += 30.0f;
		bones[0].Rotate(angles);
	}
}
