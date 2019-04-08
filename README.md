#  What is this?
This is Omnifinity's 'Unity SteamVR API' (beta) making it possible for developers to use the HTC Vive together with the Omnideck. The Omnideck is basically an analog joystick that feeds a game/simulation with a 3D vector representing how the character/camera controller should move around. 

There are various ways to control a game/simulation using the Omnideck based on third party emulators and drivers. You can find an overview of these methods here: [Controlling applications using the Omnideck.pdf](https://github.com/Omnifinity/Unity-SteamVR-API/blob/master/Controlling%20applications%20using%20the%20Omnideck.pdf). 

If you would like to implement "trackpad locomotion/smooth locomotion" using SteamVR and the HTC Vive trackpad button instead of our Unity API, please look here for an introduction:
[https://medium.com/omnifinity/steamvr-omnideck-driver-2da32f79602d](https://medium.com/omnifinity/steamvr-omnideck-driver-2da32f79602d "The Omnideck OpenVR Treadmill device driver")

Please contact us if you would like to know more about how to integrate.

### Version notice
This API v1.17.7p1-Beta has been tested on SteamVR plugin v1.2.0 and v2.0.1. It has also been tested on Unity 5.4.2f2 and Unity 2018.3.0f2 with success. There should be no reason why it should not work with the latest version of Unity. If not, please contact us. 

### Emulating an Omnideck
If you do not already own or have access to an Omnideck, you can emulate movement of a person walking on an Omnideck using a) the WASD keys on the keyboard or b) an XBOX 360 controller.

If you press the "W" key the character will move along the +Z (forward) axis.

If you press the "A" key the character will mgitkove along the -X (left) axis.

Note: When using a keyboard and running the console emulator please make sure that you've got the console window activated so that it receives the key presses. Also make sure you allow Unity to run in the background.

### Let us help you
If you like to get help building support in your game or simulation for the Omnideck please contact us by visiting [http://omnifinity.se](http://omnifinity.se) and finding our contact details.

## What is an Omnideck and how does it communicate with a game?
The Omnideck is a 360 degree treadmill for full freedom of movement in VR. The Omnideck is controlled by a program called "Omnitrack" which is responsible for tracking the user and controlling the speed of the Omnideck. Omnitrack also communicates with games that have implemented support of our API. 

Below is a basic overview of the architecture:

	Omnideck
		360 degree motorized treadmill

	Tracking system
		A system that tracks objects, e.g. HTC Vive

	VR System
		A system providing a HMD and various controllers/tracked objects, E.g. HTC Vive

	User
		A person using the Omnideck. Equipped with a VR System such as the HTC Vive.

	Omnitrack
		Software tracking the User via the Tracking system/VR System. Responsible for controlling the speed of the Omnideck and the corresponding movement path in the Game.

	Game
		A game/simulation which has implemented the Omnitrack API. Bi-directional communication with Omnitrack providing access to various states of the Omnideck.


##  How do I use it?
You first need to implement our API in your Unity project and run our Omnideck Emulator software. Basically you need to move whatever character/camera controller you have around in the scene using a movement vector (Vector3 Unit: m/s) that is being supplied via the Omnitrack API.

### Using our API Code in the example scenes
1. Import the SteamVR plugin (v1.2.0 or higher) from the Asset Store. Note that there is a bug in version 1.2.3 hindering the controllers to update. Check this thread for a solution provided by aleiby: [https://github.com/ValveSoftware/steamvr_unity_plugin/issues/49](https://github.com/ValveSoftware/steamvr_unity_plugin/issues/49)

2. Import the latest Omnifinity .unitypackage into Unity (Assets > Import Package > Custom Package). Find the highest number in the root of the reposity. The unitypackage contains source code, example scenes together with our API/DLL, built for 64-bit. **Note that the source code in this repository will be ahead of the .unitypackage at an given time. The most stable implementation relies on the latest .unitypackage**.

3. Open one of the two scenes named (residing in the drawer Omnifinity > Omnitrack > Scenes):
	1. "Example - Move SteamVR Camerarig using a standard Unity CharacterController"
	2. "Example - Move SteamVR Camerarig using a Unity Vector3"

	The first example demonstrates the use of a standard Unity CharacterController (as a parent to the HTC Vive Camerarig) to enable object collision.
	The second example demonstrates the use of a standard Vector3 and a call to transform.Translate() to move the HTC Vive CameraRig around the scene. There is no collision detection in this example. Note that your game/simulation will most likely use a custom way of detecting collision with the ground/walls so you should adapt your codebase to use the movement vector from Omnitrack.

4. Now, if you Play the game Unity will try to connect to Omnitrack. This happens through the "OmnitrackInterface" component attached to the "OmnideckPlatform" object. You will also find another component attached to the same object that shows how you can communicate with the API to move either a CharacterController or a gameobject via its transform. 
5. Start the Omnideck Emulator batch file - it is a console application that you can find in the Binary-folder (see the explanation further down this document).
6. While having the console window activated (important), move the character using they WASD keys on your keyboard. An alternative is to use the left thumbstick on an XBox 360 gamepad. Please look inside the batchscript (referenced later in this document below) and change the parameter '--usexbox360gamepad 0' to '--usexbox360gamepad 1'. 

### Using our *Omnideck Emulator* executable
Inside the Binary-folder you will find an executable (win32) together with a batch script. The default settings in the batch script is setup to communicate over VRPN using a tracker named 'OmnitrackHeadtracker0' on port 3887 (you can not change this at the moment). 

When you run the batch file the Omindeck Emulator starts communicating with Omnitrack via the API/DLL. 

If you run the executable without any arguments you will see a usage example. The options is undocumented at the moment - it is best if you do not modify any parameters as of now.

## Coordinate systems
The physical orientation (rotation) of the calibrated playspace of the HTC Vive and the rotation of your game [CameraRig] should always be aligned. You should never rotate the [CameraRig] (or any character controller controlling the [CameraRig]). If you do change the rotation of the game objects the actual physical movement of the user will translate to movement in the wrong direction in the game - leading to disorientation. 

For the person to move in the correct direction in the game the physical orientation (rotation) of the Omnideck platform/character controller/gameobject must be aligned with Unity's coordinate system.

For information - by default - if you would add/drag the SteamVR [CameraRig] into a new scene its forward direction (+Z) is aligned with Unity's forward direction (+Z).

Thus, when you add the Omnideck platform you should not rotate it or make it a child of a rotated gameobject. If you do this will lead to the user moving in the wrong direction (e.g. 90 degrees to the left instead of straight ahead).

## Building an executable
Since our DLL is built for 64-bit you need to build for the x86_64 architecture in the build settings. Contact us if you do not want/can build a 64-bit version of your experience. 

When you have built an executable and is able to control it via the emulator you can contact us and we can test the experience and give you feedback on the implementation.

## How does my Unity character actually move?

In the first example scene this is how it works:

*The "OmnideckPlatform" GameObject*

This gameobject contains a Unity Character Controller component. It will move around based on the users physical movements on the Omnideck. This movement is calculated in our software and is sent over VRPN in the form of a position vector (unit: [m]) that is the accumulated movement on the Omnideck over time. This position vector is further calculated into a velocity vector (unit: [m]/[s]) inside the unity script and sent to the Unity "SimpleMove()" function of the Unity Character Controller component.

*The "[CameraRig]" GameObject*

This gameobject is the standard SteamVR gameobject. Since this is a child to the OmnideckPlatform GameObject your controllers and camera will move around locally in relation to the CameraRig (as usual) and locally in relation to the OmnideckPlatform GameObject. 