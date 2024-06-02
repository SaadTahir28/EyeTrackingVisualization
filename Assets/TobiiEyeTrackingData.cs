using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.XR;
using TMPro;

public class TobiiEyeTrackingData : MonoBehaviour
{
    public float renderDistance = 9f;
    public GameObject leftEye, rightEye, bothEyes;
    public TMP_Text leftEyeBlinking, rightEyeBlinking;

#if !UNITY_EDITOR
    private void Update()
    {
        //// Get eye tracking data in world space
        //var eyeTrackingData = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.World);

        //// Check if gaze ray is valid
        //if (eyeTrackingData.GazeRay.IsValid)
        //{
        //    // The origin of the gaze ray is a 3D point
        //    var rayOrigin = eyeTrackingData.GazeRay.Origin;

        //    // The direction of the gaze ray is a normalized direction vector
        //    var rayDirection = eyeTrackingData.GazeRay.Direction;
        //}

        // For social use cases, data in local space may be easier to work with
        var eyeTrackingDataLocal = TobiiXR.GetEyeTrackingData(TobiiXR_TrackingSpace.Local);

        // The EyeBlinking bool is true when the eye is closed
        var isLeftEyeBlinking = eyeTrackingDataLocal.IsLeftEyeBlinking;
        var isRightEyeBlinking = eyeTrackingDataLocal.IsRightEyeBlinking;

        leftEye.SetActive(!isLeftEyeBlinking);
        rightEye.SetActive(!isRightEyeBlinking);

        leftEyeBlinking.text = "Left Eye Blinking: " + isLeftEyeBlinking;
        rightEyeBlinking.text = "Right Eye Blinking: " + isRightEyeBlinking;

        // Using gaze direction in local space makes it easier to apply a local rotation
        // to your virtual eye balls.

        // Binocular
        var origin = eyeTrackingDataLocal.GazeRay.Origin; //eyePosition
        var dir = eyeTrackingDataLocal.GazeRay.Direction; //eyeRotation
        var depth = renderDistance - origin.z;
        var eyePosition = origin + depth * (dir / dir.z);
        var binocularVector = new Vector3(eyePosition.x, eyePosition.y, renderDistance);
        bothEyes.transform.localPosition = binocularVector;

        //// Advanced Eye Tracking
        //var eyeTrackingDataAdvanced = TobiiXR.Advanced.LatestData;

        //// Right
        //var originRight = eyeTrackingDataAdvanced.Right.GazeRay.Origin;
        //var dirRight = eyeTrackingDataAdvanced.Right.GazeRay.Direction;
        //var depthRight = renderDistance - originRight.z;
        //var eyePositionRight = originRight + depthRight * (dirRight / dirRight.z);
        //var rightVector = new Vector3(eyePositionRight.x, eyePositionRight.y, renderDistance);
        //rightEye.transform.localPosition = rightVector;

        //// Left
        //var originLeft = eyeTrackingDataAdvanced.Left.GazeRay.Origin;
        //var dirLeft = eyeTrackingDataAdvanced.Left.GazeRay.Direction;
        //var depthLeft = renderDistance - originLeft.z;
        //var eyePositionLeft = originLeft + depthLeft * (dirLeft / dirLeft.z);
        //var leftVector = new Vector3(eyePositionLeft.x, eyePositionLeft.y, renderDistance);
        //leftEye.transform.localPosition = leftVector;
    }
#endif
}
