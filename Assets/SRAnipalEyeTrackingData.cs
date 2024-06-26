using UnityEngine;
using ViveSR.anipal.Eye; // Importing the namespace for the Vive Eye Tracker SDK.
using System.Text.RegularExpressions; // For using regular expressions.
using System.Collections; // For using coroutines.
using TMPro;

public class SRAnipalEyeTrackingData : MonoBehaviour // Defining a new class EyeGazeRaycast, inheriting from MonoBehaviour.
{
    public float renderDistance = 9f;
    public GameObject leftEye, rightEye, bothEyes;

    private void Start()
    {
        if (!SRanipal_Eye_Framework.Instance.EnableEye)
        {
            enabled = false;
            return;
        }
    }

    private void Update() // Update is called once per frame.
    {
        CollectEyeTrackingData();
    }

    private void CollectEyeTrackingData()
    {
        Ray gazeRay;

        // Obtaining the combined gaze direction for both eyes and storing it in gazeRay.
        if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out gazeRay))
        {
            // Binocular
            var origin = gazeRay.origin; //eyePosition
            var dir = gazeRay.direction; //eyeRotation
            var depth = renderDistance - origin.z;
            var eyePosition = origin + depth * (dir / dir.z);
            var binocularVector = new Vector3(eyePosition.x, eyePosition.y, renderDistance);
            bothEyes.transform.localPosition = binocularVector;
        }
    }
}