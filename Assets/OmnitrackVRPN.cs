using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

class OmnitrackVRPN : MonoBehaviour
{
    [DllImport("OmnitrackDLL")]
    private static extern int init();

    [DllImport("OmnitrackDLL")]
    private static extern int setup();

    [DllImport("OmnitrackDLL")]
    private static extern int cleanup();

    [DllImport("OmnitrackDLL")]
    private static extern bool runIt();

    /*
    // Pointers need the unsafe keyword and allowing build of unsafe code
    [DllImport("OmnitrackDLL")]
    private static extern unsafe double* getPos();

    [DllImport("OmnitrackDLL")]
    private static extern unsafe double* getQuat();
    */

    // safe mode pointer
    [DllImport("OmnitrackDLL")]
    private static extern IntPtr getPosition();

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


    [DllImport("OmnitrackDLL")]
    private static extern int SendSignal_RequestToStopOmnideck();

    [DllImport("OmnitrackDLL")]
    private static extern int SendSignal_RequestToStartOmnideck();

    [DllImport("OmnitrackDLL")]
    private static extern int SendSignal_HeartbeatToOmnitrack();

    IntPtr pos, rot;

    Vector3 getHeadPos()
    {
        //return new Vector3((float)getX(), (float)getY(), (float)getZ());
        return Vector3.zero;


        //IntPtr pArr;
        //bool res = getPosition(out pArr);
        //return new Vector3((float)pArr[0], (float)getY(), (float)getZ());
    }


    void Start()
    {
        Debug.Log("Initializing...");
        init();
        Debug.Log("Done");

        Debug.Log("Setting up...");
        int res;
        res = setup();
        Debug.Log("Result: " + res);

        float desiredFps_TrackingData= 1.0f / 75f;
        StartCoroutine(AcquireTrackingData(desiredFps_TrackingData));

        StartCoroutine(SendHeartBeat());
    }

    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SendSignal_RequestToStartOmnideck();
        }

        if (Input.GetMouseButtonDown(1))
        {
            SendSignal_RequestToStopOmnideck();
 
        }
    }

    //void OnDestroy()
    void OnApplicationQuit()
    {
        Debug.Log("Cleaning up...");
        int res = cleanup();
        Debug.Log("Done: " + res);
    }

    IEnumerator AcquireTrackingData(float waitTime)
    {
        while (true)
        {
            // TODO: This should be ran automatically internally in the library 
            // and not be demanded like this
            runIt();


            // Get time difference (in seconds)
            double deltaTime = getTimeValDurationOfLastMessage() / 1000000;

            //Debug.Log("new data at dt: " + deltaTime + " x: " + getX() + " y: " + getY() + " z: " + getZ());

            transform.position = new Vector3((float)getX(), (float)getY(), (float)getZ());

            /*
            //if (lastMessageSec != getTimeValSecOfLastMessage() && lastMessageUSec != getTimeValUSecOfLastMessage()) {
            Debug.Log("time: " + getTimeValSecOfLastMessage() + " " + getTimeValUSecOfLastMessage() + " x: " + getX() + " y: " + getY() + " z: " + getZ());
            lastMessageSec = getTimeValSecOfLastMessage();
            lastMessageUSec = getTimeValUSecOfLastMessage();
            */

            /*
                Debug.Log("Sec: " + lastMessageSec);
                Debug.Log("USec: " + lastMessageUSec);
                */
            //}

            /*
            unsafe
            {
                double* pos;
                pos = getPos();
                Debug.Log("Pos: " + pos[0]);
            }
            */

            /*
            Debug.Log("Sending...");
            int val = SendSignal_RequestToStopOmnideck();
            Debug.Log("Sent it: " + val);
            //SendMessage();
                */

            yield return new WaitForSeconds(waitTime);
        }
    }

    IEnumerator SendMessage(float waitTime)
    {
        while (true) {
            Debug.Log("Send Stop Request");
            SendSignal_RequestToStopOmnideck();
            yield return new WaitForSeconds(waitTime);
        }
    }

    // Periodically send heatbeat to Omnitrack
    IEnumerator SendHeartBeat()
    {
        float waitTime = 1.0f;
        while (true)
        {
            Debug.Log("Send heartbeat to Omnitrack");
            SendSignal_HeartbeatToOmnitrack();
            yield return new WaitForSeconds(waitTime);
        }
    }

}