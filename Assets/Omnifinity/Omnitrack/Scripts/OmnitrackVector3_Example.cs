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

	public enum LogLevel {None, Terse, Verbose}
	public LogLevel debugLevel = LogLevel.Verbose;

	OmnitrackInterface omnitrackInterface;

	// SteamVR controller manager
	SteamVR_ControllerManager cameraRig = null;
	SteamVR_Camera cameraEye = null;
	Transform cameraTransform = null;

	#region MonoBehaviorMethods
	// setup various things
	void Start () {
		omnitrackInterface = GetComponent<OmnitrackInterface> ();
		if (omnitrackInterface) {
			if (debugLevel != LogLevel.None)
				Debug.Log("OmnitrackInterface object: " + omnitrackInterface);
		} else {
			if (debugLevel != LogLevel.None)
				Debug.Log("Unable to find OmnitrackInterface component on object. Please add an OmnitrackInterface component.", gameObject);
		}

		// Initialize access to Steam VR
		cameraRig = FindObjectOfType<SteamVR_ControllerManager>();
		if (cameraRig)
		{
			if (debugLevel != LogLevel.None)
				Debug.Log("SteamVR CameraRig: " + cameraRig);
		}
		else
		{
			if (debugLevel != LogLevel.None)
				Debug.LogError("Unable to find SteamVR_ControllerManager object");
		}

		cameraEye = FindObjectOfType<SteamVR_Camera>();
		if (cameraEye)
		{
			if (debugLevel != LogLevel.None)
				Debug.Log("SteamVR Camera (eye): " + cameraEye);
			cameraTransform = cameraEye.transform;
		}
		else
		{
			if (debugLevel != LogLevel.None)
				Debug.LogError("Unable to find SteamVR_Camera object");
		}
	}

	// move the object
	void Update () {

		// calculate movement vector since last pass
		Vector3 currMovementVector = OmnitrackInterface.GetCurrentOmnideckCharacterMovementVector();

		// Simply translate the transform ([m/s] * [1/s] = [m])
		// (in a normal use case you'd have some code for ground/object collision)
		transform.Translate (currMovementVector * Time.deltaTime);

		// Call some prototype code, this can change anytime
		PrototypeCodeSubjectToChange();
	}
	#endregion MonoBehaviorMethods

	#region OmnitrackMethods_Beta
	// various prototype code in development below
	void PrototypeCodeSubjectToChange() {
		omnitrackInterface.DevRequestChangeOfOmnideckOperationMode();
	}
	#endregion OmnitrackMethods_Beta
}
