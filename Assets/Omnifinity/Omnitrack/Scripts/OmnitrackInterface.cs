/*
   Copyright 2017 MSE Omnifinity AB

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

        

		enum EUserRequestOperateTreadmill {
			EUserRequestOperateTreadmill_NotAllowed = 0,
			EUserRequestOperateTreadmill_NoConnection = 1,
			EUserRequestOperateTreadmill_Stop = 100,
			EUserRequestOperateTreadmill_Start = 200
		}

		enum EUserRequestOperateTreadmillResult {
			EUserRequestOperateTreadmillResult_NotAllowed = 0,
			EUserRequestOperateTreadmillResult_Stopping = 100,
			EUserRequestOperateTreadmillResult_Stopped = 101,
			EUserRequestOperateTreadmillResult_Starting = 200,
			EUserRequestOperateTreadmillResult_Started = 201
		}

        class OmnitrackInterface: MonoBehaviour
        {
            public enum LogLevel { Verbose, Terse, None }
            public LogLevel logLevel;

            // Initialize Omnitrack communication
            [DllImport("OmnitrackDLL")]
            private static extern int InitializeOmnitrack();

            // Establish network connection with Omnitrack
            [DllImport("OmnitrackDLL")]
            private static extern int EstablishOmnitrackConnection(ushort serverPort, char[] trackerName);

            // Close the connection with Omnitrack
            [DllImport("OmnitrackDLL")]
            private static extern int CloseOmnitrackConnection();

            // Run the Omnitrack mainloop each frame to properly receive new data
            // (Subject to change)
            [DllImport("OmnitrackDLL")]
            private static extern bool UpdateOmnitrack();

			// Send a request to Omnitrack that you'd like to stop the Omnideck
			// ATTN: Implementation not finished.
			[DllImport("OmnitrackDLL")]
			private static extern EUserRequestOperateTreadmill UserRequestToStopTreadmill();

			// Send a request to Omnitrack that you'd like to start the Omnideck
			// ATTN: Implementation not finished.
			[DllImport("OmnitrackDLL")]
			private static extern EUserRequestOperateTreadmill UserRequestToStartTreadmill();

			// TODO:
			// Create acknowledge events to tell the user she allowed to start / stop the Omnideck

			// Send a heartbeat to Omnitrack now and then to tell that the game is alive
			[DllImport("OmnitrackDLL")]
			private static extern int SendHeartbeatToOmnitrack();

            // Get X-position, Y and Z position
            [DllImport("OmnitrackDLL")]
            private static extern double getX();

            [DllImport("OmnitrackDLL")]
            private static extern double getY();

            [DllImport("OmnitrackDLL")]
            private static extern double getZ();

            // ATTN: Subject to change
            [DllImport("OmnitrackDLL")]
            private static extern bool hasNewDataArrived(int sensor);

            // Timestamp of last message (second part)
            // ATTN: Subject to change
            [DllImport("OmnitrackDLL")]
            private static extern long getTimeValSecOfLastMessage();

            // Timestamp of last message (millisecond part)
            // ATTN: Subject to change
            [DllImport("OmnitrackDLL")]
            private static extern long getTimeValUSecOfLastMessage();
            private long lastMessageSec, lastMessageUSec;

            // Delta-time since last received message
            // ATTN: Subject to change
            [DllImport("OmnitrackDLL")]
            private static extern double getTimeValDurationOfLastMessage();

            // SteamVR controller manager
            SteamVR_ControllerManager cameraRig = null;
            SteamVR_Camera cameraEye = null;
            Transform cameraTransform = null;
            CharacterController characterController = null;

            // Keep track of current and previous position to be able to calculate a movement vel
            Vector3 currMovementVector;
            Vector3 prevPos;

            // How often to receive motion velocity data from omnitrack. Do not change.
            // ATTN: Subject to change. 
            const float desiredFps_TrackingData = 75f;

            // Get the position (accumulated) of the character walking on the omnideck
            Vector3 getOmnideckCharacterPos()
            {
                return new Vector3((float)getX(), (float)getY(), (float)getZ());
            }

            // Various variables during development
            double timeValOfCurrTrackingMessage, timeValOfPrevTrackingMessage;
            uint numberOfSimilarTrackingDataMessages = 0;
            bool isConnectionEstablished = false;

            // Setup Omnitrack communication, SteamVR connection, Unity Character 
            // Controller component and start various coroutines
            virtual public void Start()
            {
                // Initialize the state of Omnitrack
                InitializeOmnitrack();

                // Establish the connection (uses VRPN)
                ushort port = 3889;
                var trackerName = "AppToOmnitrackTracker0";
                if (EstablishOmnitrackConnection(port, trackerName.ToCharArray()) == 0)
                {
                    // Sync tracking data from the Omnitrack API
                    StartCoroutine(AcquireTrackingData(1.0f / desiredFps_TrackingData));

                    // Send alive to Omnitrack now and then
                    StartCoroutine(SendHeartBeat());

                    if(debugLevel != LogLevel.None)Debug.Log("Successful setup of communication with Omnitrack");
                }
                else
                {
                    Debug.LogError("Unable to setup communication with Omnitrack");
                    //Destroy(gameObject);
                }

                // Initialize access to Steam VR
                cameraRig = FindObjectOfType<SteamVR_ControllerManager>();
                if (cameraRig)
                {
                    if (debugLevel != LogLevel.None) Debug.Log("SteamVR CameraRig: " + cameraRig);
                }
                else
                {
                    Debug.LogError("Unable to find SteamVR_ControllerManager object");
                }

                cameraEye = FindObjectOfType<SteamVR_Camera>();
                if (cameraEye)
                {
                    if (debugLevel != LogLevel.None) Debug.Log("SteamVR Camera (eye): " + cameraEye);
                    cameraTransform = cameraEye.transform;
                }
                else
                {
                    Debug.LogError("Unable to find SteamVR_Camera object");
                }

                // Get hold of the Unity Character Controller. This object is what we move.
                characterController = transform.GetComponent<CharacterController>();
                if (characterController)
                {
                    if (debugLevel != LogLevel.None) Debug.Log("Unity Character Controller: ", characterController);                     
                }
                else
                {
                    Debug.LogError("Unable to find Character Controller object");
                }

            }

            // Get the position of the guy on the omnideck and move the character controller
            // based on a movementment vector (as calculated by Omnitrack)
            void Update()
            {
                // Get current position of the guy walking on the omnideck 
                Vector3 omniguyPosition = getOmnideckCharacterPos();

                // calculate movement vector since last pass
                currMovementVector = (omniguyPosition - prevPos) / Time.deltaTime;
                if (characterController != null)
                {
                    // Not used ATM
                    float moveDistance = Vector3.Distance(omniguyPosition, prevPos);

                    if (debugLevel == LogLevel.Verbose) Debug.Log("OmniguyPosition:" + omniguyPosition);
                    if (debugLevel == LogLevel.Verbose) Debug.Log("movementVector: " + currMovementVector);

                    // this moves the character controller
                    characterController.SimpleMove(currMovementVector);

                    // move the center of the capsule collider along with the head
                    characterController.center = new Vector3(cameraTransform.localPosition.x, 0, cameraTransform.localPosition.z);

                    // Height compensation? Do not use ATM.
                    //cameraRig.transform.localPosition = new Vector3(0, -characterController.height / 2f, 0);

                }
                // store for next pass
                prevPos = omniguyPosition;

                // various test code below
                DevRequestStartStopOfOmnideck();
            }

            // Shut down the connection to Omnitrack
            void OnApplicationQuit() {
                if (CloseOmnitrackConnection() == 0)
                {
                    if (debugLevel != LogLevel.None) Debug.Log("Closed down communication with Omnitrack");
                }
                else
                {
                    Debug.LogWarning("Unable to properly close down ommunication with Omnitrack");
                }
            }

            // Acquire tracking data from Omnitrack
            // ATTN: Subject to change
            IEnumerator AcquireTrackingData(float waitTime)
            {
                while (true)
                {
                    // Must be called each frame at the moment
                    bool res = UpdateOmnitrack();

                    // Various code under heavy development
                    // ATTN: Subject to change
                    DevCheckIncomingDataAgainstConnectionState();

                    yield return new WaitForSeconds(waitTime);
                }
            }

            // Periodically send heatbeat to Omnitrack to notify that we are alive
            IEnumerator SendHeartBeat()
            {
                float waitTime = 1.0f;
                while (true)
                {
                    if (isConnectionEstablished)
                    {
                        SendHeartbeatToOmnitrack();
                        if (debugLevel == LogLevel.Verbose) Debug.Log("Sent heartbeat to Omnitrack");
                    }
                    else
                    {
                        Debug.LogWarning("Unable to send heartbeat to Omnitrack - connection down");
                    }
                    yield return new WaitForSeconds(waitTime);

                }
            }


            // Code in dev, will enable users to disable/enable omnideck upon will 
            // ("forced roomscale")
            // ATTN: Subject to change
            void DevRequestStartStopOfOmnideck()
            {
                if (Input.GetMouseButtonDown(0))
                {
					EUserRequestOperateTreadmill resUserRequest = UserRequestToStopTreadmill();
					switch (resUserRequest) {
					case EUserRequestOperateTreadmill.EUserRequestOperateTreadmill_NoConnection:
                            if (debugLevel != LogLevel.None) Debug.Log ("Not connection to Omnitrack");
						break;
					case EUserRequestOperateTreadmill.EUserRequestOperateTreadmill_NotAllowed:
                            if (debugLevel != LogLevel.None) Debug.Log ("Not allowed to send user request to stop the Omnideck treadmill");
						break;
					case EUserRequestOperateTreadmill.EUserRequestOperateTreadmill_Stop:
                            if (debugLevel != LogLevel.None) Debug.Log ("Sent user request to stop the Omnideck treadmill. Awaiting stopping & stopped events.");
						break;
					}
                }

                if (Input.GetMouseButtonDown(1))
                {
					EUserRequestOperateTreadmill resUserRequest = UserRequestToStartTreadmill();
					switch (resUserRequest) {
					case EUserRequestOperateTreadmill.EUserRequestOperateTreadmill_NoConnection:
                            if (debugLevel != LogLevel.None) Debug.Log ("Not connection to Omnitrack");
						break;
					case EUserRequestOperateTreadmill.EUserRequestOperateTreadmill_NotAllowed:
                            if (debugLevel != LogLevel.None) Debug.Log ("Not allowed to send user request to start the Omnideck treadmill");
						break;
					case EUserRequestOperateTreadmill.EUserRequestOperateTreadmill_Start:
                            if (debugLevel != LogLevel.None) Debug.Log ("Sent user request to start the Omnideck treadmill. Awaiting starting & started events.");
						break;
					}
                }
            }

            // Code in dev
            // ATTN: Subject to change
            void DevCheckIncomingDataAgainstConnectionState()
            {
                // Get time difference (in seconds)
                timeValOfCurrTrackingMessage = getTimeValDurationOfLastMessage() / 1000000;

                // Rapid hack for none/loss of connection based on similar messages
                if (timeValOfCurrTrackingMessage - timeValOfPrevTrackingMessage == 0)
                {
                    numberOfSimilarTrackingDataMessages++;
                    if (numberOfSimilarTrackingDataMessages % desiredFps_TrackingData == 0)
                    {
                        Debug.LogWarning("Probably none/lost connection to Omnitrack");
                        isConnectionEstablished = false;
                    }
                }
                else
                {
                    numberOfSimilarTrackingDataMessages = 0;
                    if (!isConnectionEstablished)
                    {
                        if (debugLevel != LogLevel.None) Debug.Log("Established connection with Omnitrack");
                        isConnectionEstablished = true;
                    }
                    else
                    {
                        // Already connected, everything normal
                    }
                }

                timeValOfPrevTrackingMessage = timeValOfCurrTrackingMessage;
            }
        }
    }
}
