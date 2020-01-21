/*
   Copyright 2017-2020 MSE Omnifinity AB
   The code below is a simple example of using a standard Unity CharacterController
   attached to the SteamVR CameraRig for moving the Omnideck user around based on
   position data arriving from Omnitrack.

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 */

using UnityEngine;
using System.Collections;
using Omnifinity.Omnitrack;

public class OmnitrackCharacterController_Example : MonoBehaviour {

	// debugging stuff
	public enum LogLevel {None, Terse, Verbose}
	public LogLevel debugLevel = LogLevel.Verbose;

	// Assign platform that moves around
	public 	GameObject playerSteamVR = null;

	// Assign the actual camera eye
	public 	GameObject cameraEyeSteamVR = null;

	// our interface of interest
	OmnitrackInterface omnitrackInterface;

	// Camera eye transform for positioning of head collider
	Transform cameraTransform = null;

	// The standard Unity CharacterController
	CharacterController characterController = null;

	#region MonoBehaviorMethods
	// setup various things
	void Start () {
		// get hold of the Omnitrack interface component
		omnitrackInterface = GetComponent<OmnitrackInterface> ();
		if (omnitrackInterface) {
			if (debugLevel != LogLevel.None)
				Debug.Log("OmnitrackInterface object: " + omnitrackInterface);
		} else {
			if (debugLevel != LogLevel.None)
				Debug.Log("Unable to find OmnitrackInterface component on object. Please add an OmnitrackInterface component.", gameObject);
			return;
		}

		// get hold of the steamvr camera and its transform
		if (cameraEyeSteamVR) {
			if (debugLevel != LogLevel.None)
				Debug.Log("SteamVR Camera (eye): " + cameraEyeSteamVR, cameraEyeSteamVR);
			cameraTransform = cameraEyeSteamVR.transform;
		} else {
			if (debugLevel != LogLevel.None)
				Debug.LogError("Unable to find SteamVR Eye Camera object");
			return;
		}

		// Get hold of the Unity Character Controller. This object is what we move.
		characterController = transform.GetComponent<CharacterController>();
		if (characterController) {
			if (debugLevel != LogLevel.None)
				Debug.Log("Unity Character Controller: ", characterController);                     
		} else {
			if (debugLevel != LogLevel.None)
				Debug.LogError("Unable to find Character Controller object");
			return;
		}
	}


	// move the object
	void Update () {
		// escape if we have not gotten hold of the interface component
		if (!omnitrackInterface)
			return;

		if (characterController == null) {
			if (debugLevel != LogLevel.None)
				Debug.LogError ("Unable to move charactercontroller");
			return;
		}

		// calculate movement vector since last pass [m/s]
		Vector3 newMovementVector = omnitrackInterface.GetCurrentOmnideckCharacterMovementVector();

		// disregard height changes
		Vector3 currMovementVector = new Vector3 (newMovementVector.x, 0, newMovementVector.z);

		// first move the character controller based on the movement vector [m/s] ...
		characterController.SimpleMove (currMovementVector);

		// ...and secondly move the center of the capsule collider along with the head
		// so that the user cannot move through walls
		if (cameraTransform != null)
			characterController.center = new Vector3 (cameraTransform.localPosition.x, 0, cameraTransform.localPosition.z);

		// Call some prototype code
		// ATTN: this can change anytime
		//PrototypeCodeSubjectToChange();
	}
	#endregion MonoBehaviorMethods

	#region OmnitrackMethods_Beta
	// various prototype code in development below
	void PrototypeCodeSubjectToChange() {
		omnitrackInterface.DevRequestChangeOfOmnideckOperationMode();
	}
	#endregion OmnitrackMethods_Beta
}
