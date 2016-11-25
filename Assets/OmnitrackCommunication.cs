using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

class OmnitrackCommunication : MonoBehaviour
{
    // Initialize Omnitrack communication
    [DllImport("OmnitrackDLL")]
    private static extern int IInitializeOmnitrack();

    // Establish network connection with Omnitrack
    [DllImport("OmnitrackDLL")]
    private static extern int IEstablishOmnitrackCommunication(ushort serverPort, char[] trackerName);

    [DllImport("OmnitrackDLL")]
    private static extern int ICloseOmnitrackConnection();

    [DllImport("OmnitrackDLL")]
    private static extern bool IMainloopOmnitrack();

    [DllImport("OmnitrackDLL")]
    private static extern double getX();

    [DllImport("OmnitrackDLL")]
    private static extern double getY();

    [DllImport("OmnitrackDLL")]
    private static extern double getZ();


    [DllImport("OmnitrackDLL")]
    private static extern bool hasNewDataArrived(int sensor);

    [DllImport("OmnitrackDLL")]
    private static extern long getTimeValSecOfLastMessage();

    [DllImport("OmnitrackDLL")]
    private static extern long getTimeValUSecOfLastMessage();
    private long lastMessageSec, lastMessageUSec;

    [DllImport("OmnitrackDLL")]
    private static extern double getTimeValDurationOfLastMessage();

    // Send a request to Omnitrack that you'd like to stop the Omnideck
    [DllImport("OmnitrackDLL")]
    private static extern int IRequestToStopOmnideck();

    // Send a request to Omnitrack that you'd like to start the Omnideck
    [DllImport("OmnitrackDLL")]
    private static extern int IRequestToStartOmnideck();

    // Send a heartbeat to Omnitrack now and then to tell that your gime is alive
    [DllImport("OmnitrackDLL")]
    private static extern int ISendHeartbeatToOmnitrack();

    Vector3 getHeadPos()
    {
        //return new Vector3((float)getX(), (float)getY(), (float)getZ());
        return Vector3.zero;
    }

    double timeValOfCurrTrackingMessage, timeValOfPrevTrackingMessage;
    uint numberOfSimilarTrackingData = 0;
    bool hasLostConnection = false;

    // Setup Omnitrack communication and various coroutines
    virtual public void Start()
    {
        // Initialize the state of Omnitrack
        IInitializeOmnitrack();

        // Establish the connection
        ushort port = 3889;
        var trackerName = "AppToOmnitrackTracker0";
        if (IEstablishOmnitrackCommunication(port, trackerName.ToCharArray()) == 0)
        {
            float desiredFps_TrackingData = 75f;
            StartCoroutine(AcquireTrackingData(1.0f / desiredFps_TrackingData));

            StartCoroutine(SendHeartBeat());
            Debug.Log("Successful setup of communication with Omnitrack");
        }
        else
        {
            Debug.Log("Unable to setup communication with Omnitrack");
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            IRequestToStartOmnideck();
        }

        if (Input.GetMouseButtonDown(1))
        {
            IRequestToStopOmnideck();
        }
    }

    // Shut down communication with Omnitrack
    void OnApplicationQuit() {
        if (ICloseOmnitrackConnection() == 0)
        {
            Debug.Log("Successful shutdown of ommunication with Omnitrack");
        }
        else
        {
            Debug.Log("Unable to properly shutdown ommunication with Omnitrack");
        }
    }

    // Acquire tracking data from Omnitrack
    IEnumerator AcquireTrackingData(float waitTime)
    {
        while (true)
        {
            // TODO: This should be ran automatically internally in the library
            // and not be demanded like this
            IMainloopOmnitrack();

            // Get time difference (in seconds)
            double deltaTime = getTimeValDurationOfLastMessage() / 1000000;

            timeValOfCurrTrackingMessage = deltaTime;
            if (timeValOfCurrTrackingMessage == timeValOfPrevTrackingMessage)
            {
                numberOfSimilarTrackingData++;
                if (numberOfSimilarTrackingData > 10)
                {
                    Debug.Log("Probably no/lost connection to Omnitrack");
                    hasLostConnection = true;
                }
            }
            else {
                numberOfSimilarTrackingData = 0;
                if (hasLostConnection)
                {
                    Debug.Log("Recovered connection to Omnitrack");
                }
                hasLostConnection = false;
            }

            timeValOfPrevTrackingMessage = timeValOfCurrTrackingMessage;
            //Debug.Log("new data at dt: " + deltaTime + " x: " + getX() + " y: " + getY() + " z: " + getZ());

            transform.position = new Vector3((float)getX(), (float)getY(), (float)getZ());

            yield return new WaitForSeconds(waitTime);
        }
    }

    // Periodically send heatbeat to Omnitrack
    IEnumerator SendHeartBeat()
    {
        float waitTime = 1.0f;
        while (true)
        {
            ISendHeartbeatToOmnitrack();
            Debug.Log("Sent heartbeat to Omnitrack");
            yield return new WaitForSeconds(waitTime);
        }
    }
}
