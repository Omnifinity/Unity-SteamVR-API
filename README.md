#  What is this?
This is an early Beta of our 'Omnifinity Unity SteamVR API' making it possible for developers to use the HTC Vive together with the Omnideck. The Omnideck is basically the worlds largest analog joystick.

We are working on native integration with VRTK. More news to come later.

Currently you as a developer can emulate movement of a person walking on an Omnideck using a) the WASD keys on the keyboard or b) an XBOX 360 controller. In-game touchpad movement emulation is in the works. 

If you like to build support in your game for the Omnideck please contact us.   

##  How do I use it?
To build support for the Omnideck in your experience you need to first prepare your Unity project to use our API and second - if you do not have access to an Omnideck - you need to run our Omnideck Emulator software.

### Using our API Code
1. Import the SteamVR plugin (v1.2.0 or higher) from the Asset Store.
2. Import the VRTK Unity plugin (v2.2.1 or higher) from the Asset Store.
3. Import our Omnifinity .unitypackage into Unity (Assets > Import Package > Custom Package). It contains a DLL that is built for 64-bit.
4. Open the scene named "Scene 1" (residing in the drawer Omnifinity > Omnitrack > Scenes) and play the game. Now Unity will try to connect to Omnitrack. 
5. Start the Omnideck Emulator software - it is a console application.
6. To move the character a) while having the console active use they WASD keys on your keyboard to move the character or b) use the left thumbstick on an XBox 360 gamepad. Please look inside the batchscript (referenced later in this document below) and change the parameter '--usexbox360gamepad 0' to '--usexbox360gamepad 1'. 

#### Note
If you do not use VRTK you can skip that step but you have to remove the missing script components in the following gameobjects

1. "OmnideckPlatform" - one missing script
2. "OmnideckPlatform > [CameraRig] > Controller (left)" - two missing scripts

### Using our *Omnideck Emulator* executable
Inside the Binary-folder you will find an executable (win32) together with a batch script. The default settings in the batch script is setup to communicate over VRPN using a tracker named 'OmnitrackHeadtracker0' on port 3887 (cannot be changed at the moment). 

When you run the batch file the Omindeck Emulator will start communicating using our API and the DLL that you just imported into Unity. 

If you run the executable without any arguments you will see a usage example. The options is undocumented at the moment - it is best if you do not modify any parameters as of now.

## Building an executable
Since our DLL is built for 64-bit you need to build for the x86_64 architecture in the build settings. Contact us if you do not want/can build a 64-bit version of your experience. 

## How does my Unity character actually move?

*The "OmnideckPlatform" GameObject*

This gameobject contains a Unity Character Controller component. It will move around based on the users physical movements on the Omnideck. This movement is calculated in our software and is sent over VRPN in the form of a position vector (unit: [m]) that is the accumulated movement on the Omnideck over time. This position vector is further calculated into a velocity vector (unit: [m]/[s]) inside the unity script and sent to the Unity "SimpleMove()" function of the Unity Character Controller component.

*The "[CameraRig]" GameObject*

This gameobject is the standard SteamVR gameobject. Since this is a child to the OmnideckPlatform GameObject your controllers and camera will move around locally in relation to the CameraRig (as usual) and locally in relation to the OmnideckPlatform GameObject. 


## TODO
Make is possible for the player to disable/enable virtual locomotion in game just as it works in games with touchpad/sliding movement.    

Apologies for the lack of documentation and an incomplete software suite at the moment. Look in the issues section for various bugs. 