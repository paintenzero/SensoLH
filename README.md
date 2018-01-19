## Trying things out

You will need to checkout submodule [Senso plugin](https://github.com/paintenzero/senso-unity-plugin). It is already in the project just make sure you checked it out with this project. Also you need to add [SteamVR plugin](https://www.assetstore.unity3d.com/en/#!/content/32647) to the project. After that it should run perfectly. Just don't forget to open main.scene in the root of the project.

## Description

You will see _SensoManager_ object in the scene. There is Senso Hands Controller component in it. Senso Host and Senso Port are needed to be change only if you run SENSO_UI driver on another host - just put the proper IP and port. Default settings will suit most of the users.
In case you running your project in VR (and you probably are, why else you need SteamVR tracking?) you will want to add Senso Head Position to your HMD camera. This will not interfere with headset's default tracking but helps us to proper position your glove.
The most interesting option of Senso Hands Controller is _Hands_ - this is an array of hands that should be controlled. It takes Senso.Hand derivatives.

### Senso.Hand derivative

For your 3d model of a hand you will want to write your own class to set a pose received from Senso Glove. To do that you derive class from Senso.Hand and implement `SetSensoPose(Senso.HandData newData)`. Example implementation of Senso.Hand you can find in the project `Assets/Senso/Examples/SensoHandExample.cs`. You need to find positions of X,Y,Z in quaternions and vectors that suit your hand's model on your own as Unity doesn't provide any default rig for this.
