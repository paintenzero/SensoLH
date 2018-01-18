using UnityEngine;

public class SensoBodyController : SensoBaseController {

    // Variables for hands objects
    public Senso.Body Avatar;

    // Initialization
    void Start ()
    {
        if (Avatar == null)
        {
            Debug.LogError("Avatar is not set! What am I supposed to control?");
            return;
        }
        Avatar.SetBodyController(this);

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
                bool positionSet = false;
                while (datas.Count > 0)
                {
                    var parsedData = datas.Pop();
                    if (parsedData.type.Equals("avatar"))
                    {
                        if (!positionSet) {
                            var bodyData = JsonUtility.FromJson<Senso.BodyDataFull>(parsedData.packet);
                            if (bodyData != null)
                            {
                                Avatar.SetSensoPose(bodyData.data);
                                positionSet = true;
                            }
                        }
                    }
                }
            }
        }
	}

}
