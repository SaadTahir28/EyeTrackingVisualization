using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class DataVisualization : MonoBehaviour
{
    public Transform visualizationCamera;
    public Transform eyeTransform, leftEyeTransform, rightEyeTransform;
    public LineRenderer xLineRenderer;
    public LineRenderer yLineRenderer;
    public LineRenderer followLineRenderer;
    public int maxDataPoints = 100;
    public float zSpeed = 0.1f; // Speed at which z moves forward
    public float zMax = 1f;
    public Color xLineColor = Color.red;
    public Color yLineColor = Color.blue;
    public Color followLineColor = Color.yellow;
    public TMP_Text xAxisText, yAxisText, xData, yData;
    public GameObject leftEyeBlink, rightEyeBlink;

    private Queue<Vector3> xDataPoints = new Queue<Vector3>();
    private Queue<Vector3> yDataPoints = new Queue<Vector3>();
    private Queue<Vector3> followDataPoints = new Queue<Vector3>();
    private float currentZ = 0;

    void Start()
    {
        xLineRenderer.positionCount = maxDataPoints;
        yLineRenderer.positionCount = maxDataPoints;

        // Set colors for the line renderers
        xLineRenderer.startColor = xLineColor;
        xLineRenderer.endColor = xLineColor;
        yLineRenderer.startColor = yLineColor;
        yLineRenderer.endColor = yLineColor;
        followLineRenderer.startColor = followLineColor;
        followLineRenderer.endColor = followLineColor;

        xAxisText.color = xLineColor;
        yAxisText.color = yLineColor;
        xData.color = xLineColor;
        yData.color = yLineColor;
    }

    void Update()
    {
        //// Get mouse position relative to the center of the screen
        //Vector3 mousePosition = Input.mousePosition;
        //mousePosition.z = 0; // Set z to 0 for 2D graph
        //Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        //Vector3 relativePosition = mousePosition - screenCenter;

        //// Normalize the position to a reasonable range for plotting
        //relativePosition.x /= (Screen.width / 2);
        //relativePosition.y /= (Screen.height / 2);

        // Add z movement and reset if it reaches 1
        currentZ += zSpeed * Time.deltaTime;
        //if (currentZ > zMax)
        //{
        //    currentZ = 0;
        //}

        var relativePosition = eyeTransform.localPosition;

        xData.text = "X-Data: " + relativePosition.x;
        yData.text = "Y-Data: " + relativePosition.y;

        // Create new points for both x and y graphs
        Vector3 newXPoint = new Vector3(0, relativePosition.x, currentZ);
        Vector3 newYPoint = new Vector3(0, relativePosition.y, currentZ);
        Vector3 newFollowPoint = relativePosition;

        // Add the new data points
        AddDataPoint(xDataPoints, newXPoint);
        AddDataPoint(yDataPoints, newYPoint);
        AddDataPoint(followDataPoints, newFollowPoint);

        // Update line renderers
        UpdateLineRenderer(xLineRenderer, xDataPoints);
        UpdateLineRenderer(yLineRenderer, yDataPoints);
        UpdateLineRenderer(followLineRenderer, followDataPoints);

        UpdateCamera();
        ToggleBlinkState();
    }

    void AddDataPoint(Queue<Vector3> dataPoints, Vector3 newPoint)
    {
        if (dataPoints.Count >= maxDataPoints)
        {
            dataPoints.Dequeue();
        }
        dataPoints.Enqueue(newPoint);
    }

    void UpdateLineRenderer(LineRenderer lineRenderer, Queue<Vector3> dataPoints)
    {
        lineRenderer.positionCount = dataPoints.Count;
        lineRenderer.SetPositions(dataPoints.ToArray());
    }

    void UpdateCamera()
    {
        visualizationCamera.Translate(Vector3.right * Time.deltaTime * zSpeed);
    }

    void ToggleBlinkState()
    {
        leftEyeBlink.SetActive(leftEyeTransform.gameObject.activeSelf);
        rightEyeBlink.SetActive(rightEyeTransform.gameObject.activeSelf);
    }
}
