#  What is this?
This is a very early alpha of the upcoming new Omnifinity Unity SteamVR API that uses the HTC Vive as a VR solution. It is not yet complete since it is lacking the Omnideck emulator software that you need to emulate an actual Omnideck. It is currently only for developers whom we are in contact with.

If you like to build support in your game for the Omnideck please contact us.   

##  How do I use it?
1. Import the SteamVR plugin (v1.2.0 or higher) from the Asset Store.
2. Import the VRTK Unity plugin (v2.2.1 or higher) from the Asset Store.
3. Import our Omnifinity .unitypackage into Unity (Assets > Import Package > Custom Package).
4. Open the scene named "Scene 1" (residing in the drawer Omnifinity > Omnitrack > Scenes).
5. Start the Omnideck emulator software (Not included here yet thus you can only study the source code)
6. Start your game, moving the mouse around the edges of your screen will now emulate human movement around the Omnideck. 

### Note
If you do not use VRTK you can skip that step but you have to remove the missing script components in the following gameobjects

1. "OmnideckPlatform" - one missing script
2. "OmnideckPlatform > [CameraRig] > Controller (left)" - two missing scripts

## How does my Unity character actually move?

*The "OmnideckPlatform" GameObject*

This gameobject contains a Unity Character Controller component. It will move around based on the users physical movements on the Omnideck. This movement is calculated in our software and is sent over VRPN in the form of velocity vector. The vector is used in Unity using the "SimpleMove()" function included with the Unity Character Controller component. The vector is in [m/s].  

*The "[CameraRig]" GameObject*

This gameobject is the standard SteamVR gameobject. Since this is a child to the OmnideckPlatform GameObject your controllers and camera will move around locally in relation to the CameraRig (as usual) and locally in relation to the OmnideckPlatform GameObject. You can consider this to be room-scale VR on steroids. 

If the Omnideck is deactivated (e.g. the velocity vector is (0, 0, 0)) you'll have the normal room-scale VR as commonly known from the HTC Vive system.


## TODO
Loads :) Apologies for the lack of documentation and an incomplete software suite at the moment.