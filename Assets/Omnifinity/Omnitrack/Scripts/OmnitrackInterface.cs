/*
   Copyright 2017-2018 MSE Omnifinity AB
   The code below is part of the Omnitrack Unity API

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
using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Omnifinity
{
    namespace Omnitrack
    {

		#region OmnitrackEnums
		enum ETreadmillStatus {
			Stopped = 1,
			Running = 2
		};

		enum EUserRequestOperateTreadmill {
			Request_Disable = 100,
			Request_Enable = 200
		}

		enum ESystemReplyOperateTreadmill {
			Result_NotAllowed = 1,
			Result_Disabled_Ok = 100,
			Result_Enabled_Ok = 200
		}
		#endregion OmnitrackEnums

        class OmnitrackInterface: MonoBehaviour
        {

			public enum LogLevel {None, TerseUserPositionAndVector, Verbose}
			public LogLevel debugLevel = LogLevel.Verbose;

			#region OmnitrackAPIImports
			[DllImport("OmnitrackAPI")]
			private static extern UInt16 GetAPIVersionMajor ();
			[DllImport("OmnitrackAPI")]
			private static extern UInt16 GetAPIVersionMinor ();
			[DllImport("OmnitrackAPI")]
			private static extern UInt16 GetAPIVersionPatch ();

            // Initialize Omnitrack API communication
            [DllImport("OmnitrackAPI")]
            private static extern int InitializeOmnitrackAPI();

            // Establish network connection with Omnitrack
			[DllImport("OmnitrackAPI")]
			private static extern int EstablishOmnitrackConnection(ushort serverPort, string trackerName);

            // Close the connection with Omnitrack
            [DllImport("OmnitrackAPI")]
            private static extern int CloseOmnitrackConnection();

            // Run the Omnitrack mainloop each frame to properly receive new data
            [DllImport("OmnitrackAPI")]
			[return:MarshalAs(UnmanagedType.I1)]
            private static extern void UpdateOmnitrack();

			// Check if the treadmill is online and communicating with game
			[DllImport("OmnitrackAPI")]
			[return:MarshalAs(UnmanagedType.I1)]
			private static extern bool IsOmnitrackOnline ();

			[DllImport("OmnitrackAPI")]
			private static extern float GetTreadmillSpeed();

			[DllImport("OmnitrackAPI")]
			private static extern int GetTreadmillState();

			[DllImport("OmnitrackAPI")]
			private static extern int SendHeartbeatToOmnitrack(UInt16 major, UInt16 minor, UInt16 patch);

            // Get X-position, Y and Z position
            [DllImport("OmnitrackAPI")]
            private static extern double getX();

            [DllImport("OmnitrackAPI")]
            private static extern double getY();

            [DllImport("OmnitrackAPI")]
            private static extern double getZ();

			#endregion OmnitrackAPIImports

			#region OmnitrackAPIImports_Beta
			// Handshake between this projects DLL API-version and Omnitrack-version
			[DllImport("OmnitrackAPI")]
			private static extern int PerformOmnitrackHandshake();

			// Has new data arrived or not
			// ATTN: Subject to change
			[DllImport("OmnitrackAPI")]
			[return:MarshalAs(UnmanagedType.I1)]
			private static extern bool hasNewDataArrived(int sensor);

			// Timestamp of last message (second part)
			// ATTN: Subject to change
			[DllImport("OmnitrackAPI")]
			private static extern long getTimeValSecOfLastMessage();

			// Timestamp of last message (millisecond part)
			// ATTN: Subject to change
			[DllImport("OmnitrackAPI")]
			private static extern long getTimeValUSecOfLastMessage();
			private long lastMessageSec, lastMessageUSec;

			// Delta-time since last received message
			// ATTN: Subject to change
			[DllImport("OmnitrackAPI")]
			private static extern double getTimeValDurationOfLastMessage();

			// Send a request to Omnitrack that you'd like to stop the Omnideck
			// ATTN: Implementation not finished.
			[DllImport("OmnitrackAPI")]
			private static extern ESystemReplyOperateTreadmill UserRequestToStopTreadmill();

			// Send a request to Omnitrack that you'd like to start the Omnideck
			// ATTN: Implementation not finished.
			[DllImport("OmnitrackAPI")]
			private static extern ESystemReplyOperateTreadmill UserRequestToStartTreadmill();

			// TODO:
			// Create acknowledge events to tell the user she allowed to start / stop the Omnideck
			[DllImport("OmnitrackAPI")]
			[return:MarshalAs(UnmanagedType.I1)]
			private static extern bool IsAllowedToStartTreadmill();

			[DllImport("OmnitrackAPI")]
			[return:MarshalAs(UnmanagedType.I1)]
			private static extern bool IsAllowedToStopTreadmill();
			#endregion OmnitrackAPIImports_Beta

			#region OmnitrackVariables

            // How often to receive motion velocity data from omnitrack.
            // ATTN: Subject to change.
            const float desiredFps_TrackingData = 60f;

			// Keep track of current and previous position to be able to calculate a movement vel
			bool hasReceivedStartPosition = false;
			static Vector3 currPosition;
			static Vector3 currMovementVector;
			Vector3 prevPos;

            // Various variables during development
			// ATTN: Subject to change
            double timeValOfCurrTrackingMessage, timeValOfPrevTrackingMessage;
            uint numberOfSimilarTrackingDataMessages = 0;

			string strAPIVersion = "";
			string strOmnitrackVersion = "";
			bool isHandShakeFinished = false;

			// Port and trackername. Should normally not be changed.
			public ushort port = 3889;
			public string trackerName = "AppToOmnitrackTracker0";
			#endregion OmnitrackVariables

			#region MonoBehaviourMethods
            // Setup Omnitrack communication, SteamVR connection, Unity Character 
            // Controller component and start various coroutines
            virtual public void Start()
            {
                // Initialize communication with the Omnitrack API
                InitializeOmnitrackAPI();

                // Establish the connection (uses VRPN)
                if (EstablishOmnitrackConnection(port, trackerName) == 0)
                {
                    // Periodically acquire tracking data from Omnitrack
                    StartCoroutine(AcquireTrackingData(1.0f / desiredFps_TrackingData));

                    // Periodically tell Omnitrack we are alive
                    StartCoroutine(SendHeartBeat());

					// Periodically check the state of the Omnideck
					StartCoroutine(CheckOmnideckState());

					if (debugLevel != LogLevel.None)
	                    Debug.Log("Successful setup of communication handlers with Omnitrack");
                }
                else
                {
					if (debugLevel != LogLevel.None)
	                    Debug.LogError("Unable to setup communication handlers with Omnitrack");
                }
            }

			// Shut down the connection to Omnitrack
			void OnApplicationQuit() {
				if (CloseOmnitrackConnection() == 0)
				{
					if (debugLevel != LogLevel.None)
						Debug.Log("Closed down communication with Omnitrack");
				}
				else
				{
					if (debugLevel != LogLevel.None)
						Debug.LogWarning("Unable to properly close down ommunication with Omnitrack");
				}
			}
			#endregion MonoBehaviourMethods

			#region OmnitrackAPIMethods
			// Get the position (accumulated over time) of the character walking on the omnideck
			public static Vector3 GetOmnideckCharacterPos()
			{
				return new Vector3((float)getX(), (float)getY(), (float)getZ());
			}

			// Update the omnideck users position/movement vector
			private void UpdateOmnideckCharacterMovement() {
				currPosition = GetOmnideckCharacterPos();

				// make sure the initial starting position does not result in a large jump
				if (!hasReceivedStartPosition) {
					if (debugLevel != LogLevel.None)
						Debug.Log ("Resetting start position for calculation of the Omnideck character movement");
					hasReceivedStartPosition = !hasReceivedStartPosition;
					// set same previous and current position
					prevPos = currPosition;
				}

				// calculate movement vector since last pass
				currMovementVector = (currPosition - prevPos) / Time.deltaTime;

				// cap the vector if it is very high (e.g. when omnitrack starts and headset goes from
				// lying on the centerplate to being moved to e.g. 1.8m in one frame. With 60 fps a 
				// threshold value of 0.05 means roughly a 3 m/s movement which is way too high.
				float distance = Vector3.Distance (currPosition, prevPos);
				if (distance > 0.05f) {
					Debug.Log ("Received potential high initial movement speed, capping");
					currMovementVector = Vector3.zero;
				}

				// store current pos for next pass
				prevPos = currPosition;

				if (debugLevel == LogLevel.TerseUserPositionAndVector ) {
					Debug.Log ("User Position:" + currPosition);
					Debug.Log ("User movementVector: " + currMovementVector);
				}
			}

			// returns the current accumulated position of the omnideck user.
			// Unit = [m]
			public static Vector3 GetCurrentOmnideckCharacterPosition() {
				return currPosition;
			}

			// returns the current movement vector of the omnideck user.
			// Unit = [m/s]
			public static Vector3 GetCurrentOmnideckCharacterMovementVector() {
				return currMovementVector;
			}

            // Acquire tracking data from Omnitrack
            // ATTN: Subject to change
            IEnumerator AcquireTrackingData(float waitTime)
            {
                while (true)
                {
					// Update against Omnitrack API
                    UpdateOmnitrack();

					// Update Omnideck Character position/movement data
					UpdateOmnideckCharacterMovement ();

                    yield return new WaitForSeconds(waitTime);
                }
            }

            // Periodically send heatbeat to Omnitrack to notify that game is alive
            IEnumerator SendHeartBeat()
            {
                float waitTime = 1.0f;
                while (true)
                {
					if (IsOmnitrackOnline ())
                    {
						SendHeartbeatToOmnitrack(GetAPIVersionMajor(), GetAPIVersionMinor(), GetAPIVersionPatch());
						if (debugLevel != LogLevel.None)
							Debug.Log("Sent heartbeat to Omnitrack, using API v" + GetAPIVersionMajor().ToString () + "." + GetAPIVersionMinor().ToString () + "." + GetAPIVersionPatch().ToString ());
                    }
                    else
                    {
						if (debugLevel != LogLevel.None)
	                        Debug.LogWarning("Unable to send heartbeat to Omnitrack - connection down");
                    }
                    yield return new WaitForSeconds(waitTime);
                }
            }

			// Periodically check the state of the Omnideck
			IEnumerator CheckOmnideckState()
			{
				float waitTime = 1.0f;
				while (true)
				{
					if (IsOmnitrackOnline ()) {
						ETreadmillStatus treadmillState = (ETreadmillStatus)GetTreadmillState ();
						switch (treadmillState) {
						case ETreadmillStatus.Stopped:
							if (debugLevel != LogLevel.None)
								Debug.Log ("Treadmill stopped");
							break;

						case ETreadmillStatus.Running:
							if (debugLevel != LogLevel.None)
								Debug.Log ("Treadmill running");
							break;

						default:
							if (debugLevel != LogLevel.None)
								Debug.Log ("Unsupported treadmill state");
							break;
						}
					} else {
						if (debugLevel != LogLevel.None)
							Debug.LogWarning ("Omnitrack connection offline");
					}
					yield return new WaitForSeconds(waitTime);
				}
			}
			#endregion

			#region OmnitrackAPIMethods_Beta
			// Unfinished/unverified code that is in active development
			// ATTN: Subject to change

			// Handshake version check
			// ATTN: Subject to change
			IEnumerator PerformVersionHandshake()
			{
				float waitTime = 1.0f;
				while (true && !isHandShakeFinished)
				{
					if (debugLevel != LogLevel.None)
						Debug.Log ("Handshake not finished");
					if (IsOmnitrackOnline ())
					{
						if (debugLevel != LogLevel.None)
							Debug.Log ("Connection is enabled");
						if (PerformOmnitrackHandshake () == 0) {

							UInt16 verAPIMajor, verAPIMinor, verAPIPatch;
							verAPIMajor = GetAPIVersionMajor ();
							verAPIMinor = GetAPIVersionMinor ();
							verAPIPatch = GetAPIVersionPatch ();
							strAPIVersion = verAPIMajor.ToString () + "." + verAPIMinor.ToString () + "." + verAPIPatch.ToString ();
							if (debugLevel != LogLevel.None)
								Debug.Log ("Using API Version: " + strAPIVersion);
						}
					}
					else
					{
						if (debugLevel != LogLevel.None)
							Debug.LogWarning("Unable to handshake with Omnitrack - connection down");
					}
					yield return new WaitForSeconds(waitTime);
				}
				yield return null;
			}

            // Code in dev, will enable users to disable/enable omnideck upon will 
            // ("forced roomscale")
            // ATTN: Subject to change
			public  void DevRequestChangeOfOmnideckOperationMode()
            {
				if (Input.GetMouseButtonDown (0)) {
					if (IsOmnitrackOnline ()) {
						ESystemReplyOperateTreadmill resOperateTreadmillRequest = UserRequestToStopTreadmill ();
						switch (resOperateTreadmillRequest) {
						case ESystemReplyOperateTreadmill.Result_NotAllowed:
							if (debugLevel != LogLevel.None)
								Debug.Log ("Not allowed to send user request to stop the Omnideck");
							break;
						case ESystemReplyOperateTreadmill.Result_Disabled_Ok:
							if (debugLevel != LogLevel.None)
								Debug.Log ("Sent user request to stop the Omnideck treadmill. Treadmill disabled.");
							break;
						}
					} else {
						if (debugLevel != LogLevel.None)
						Debug.LogError ("Unable to send request, not connected to Omnitrack");
					}
				}

				if (Input.GetMouseButtonDown (1)) {
					if (IsOmnitrackOnline ()) {
						ESystemReplyOperateTreadmill resOperateTreadmillRequest = UserRequestToStartTreadmill ();
						switch (resOperateTreadmillRequest) {
						case ESystemReplyOperateTreadmill.Result_NotAllowed:
							if (debugLevel != LogLevel.None)
								Debug.Log ("Not allowed to send user request to start the Omnideck");
							break;
						case ESystemReplyOperateTreadmill.Result_Enabled_Ok:
							if (debugLevel != LogLevel.None)
								Debug.Log ("Sent user request to start the Omnideck treadmill. Treadmill enabled.");
							break;
						}
					} else {
						if (debugLevel != LogLevel.None)
							Debug.LogError ("Unable to send request, not connected to Omnitrack");
					}
				}
            }
			#endregion OmnitrackAPICode_Beta
        }
    }
}
