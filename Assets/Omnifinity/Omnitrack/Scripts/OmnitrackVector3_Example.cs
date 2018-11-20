/*
   Copyright 2017-2018 MSE Omnifinity AB
   The code below is a simple example of moving a transform based on 
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

public class OmnitrackVector3_Example : MonoBehaviour {

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
		//cameraEye = FindObjectOfType<SteamVR_Camera>();
		if (cameraEyeSteamVR) {
			if (debugLevel != LogLevel.None)
				Debug.Log("SteamVR Camera (eye): " + cameraEyeSteamVR, cameraEyeSteamVR);
			cameraTransform = cameraEyeSteamVR.transform;
		} else {
			if (debugLevel != LogLevel.None)
				Debug.LogError("Unable to find SteamVR Eye Camera object");
			return;
		}
	}

	// move the object
	void Update () {
		// escape if we have not gotten hold of the interface component
		if (!omnitrackInterface)
			return;

		// calculate movement vector since last pass [m/s]
		Vector3 currMovementVector = omnitrackInterface.GetCurrentOmnideckCharacterMovementVector();

		// disregard height changes
		Vector3 bodyMovementVector = new Vector3 (currMovementVector.x, 0, currMovementVector.z);

		// Simply translate the transform ([m/s] * [s] = [m])
		// (in a normal use case you'd have some code/raycasting for ground/object collision)
		transform.Translate (bodyMovementVector * Time.deltaTime);

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
