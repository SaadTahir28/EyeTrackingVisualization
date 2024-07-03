using UnityEngine;
using ViveSR.anipal.Eye; // Importing the namespace for the Vive Eye Tracker SDK.
using System.Text.RegularExpressions; // For using regular expressions.
using System.Collections; // For using coroutines.
using TMPro;

public class SRAnipalEyeTrackingData : MonoBehaviour // Defining a new class EyeGazeRaycast, inheriting from MonoBehaviour.
{
    public bool collectIndividualData;
    public float renderDistance = 9f;
    public GameObject leftEye, rightEye, bothEyes;
    public LocalTrailRenderer localTrailRenderer;

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
        CombinedGazeData();

        CheckEyeBlinks();

        if (collectIndividualData)
        {
            LeftEyeGazeData();
            RightEyeGazeData();
        }
    }

    void CombinedGazeData()
    {
        // Obtaining the combined gaze direction for both eyes and storing it in gazeRay.
        if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out var gazeRay))
        {
            bothEyes.SetActive(true);

            // Binocular
            var origin = gazeRay.origin; //eyePosition
            var dir = gazeRay.direction; //eyeRotation
            var depth = renderDistance - origin.z;
            var eyePosition = origin + depth * (dir / dir.z);
            var binocularVector = new Vector3(eyePosition.x, eyePosition.y, renderDistance);
            bothEyes.transform.localPosition = binocularVector;
        } 
        else
        {
            bothEyes.SetActive(false);
            localTrailRenderer.ResetLine();
        }
    }

    void LeftEyeGazeData()
    {
        if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out var leftGazeRay))
        {
            // Left
            var origin = leftGazeRay.origin; //eyePosition
            var dir = leftGazeRay.direction; //eyeRotation
            var depth = renderDistance - origin.z;
            var eyePosition = origin + depth * (dir / dir.z);
            var leftEyeVector = new Vector3(eyePosition.x, eyePosition.y, renderDistance);
            leftEye.transform.localPosition = leftEyeVector;
        }
    }

    void RightEyeGazeData()
    {
        if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out var rightGazeRay))
        {
            // Right
            var origin = rightGazeRay.origin; //eyePosition
            var dir = rightGazeRay.direction; //eyeRotation
            var depth = renderDistance - origin.z;
            var eyePosition = origin + depth * (dir / dir.z);
            var rightEyeVector = new Vector3(eyePosition.x, eyePosition.y, renderDistance);
            leftEye.transform.localPosition = rightEyeVector;
        }
    }

    void CheckEyeBlinks()
    {
        if (SRanipal_Eye_v2.GetEyeOpenness(EyeIndex.LEFT, out var leftEyeOpenness))
        {
            Debug.Log("leftEyeOpenness: " + leftEyeOpenness);
            leftEye.SetActive(leftEyeOpenness == 1);
        }

        if (SRanipal_Eye_v2.GetEyeOpenness(EyeIndex.RIGHT, out var rightEyeOpenness))
        {
            Debug.Log("rightEyeOpenness: " + rightEyeOpenness);
            rightEye.SetActive(rightEyeOpenness == 1);
        }
    }
}