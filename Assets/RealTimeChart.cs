using UnityEngine;
using System.Collections.Generic;

public class RealTimeChart : MonoBehaviour
{
    public LineRenderer alphaLineRenderer;
    public LineRenderer betaLineRenderer;
    public int maxDataPoints = 100;
    public float zSpeed = 0.1f; // Speed at which z moves forward
    public Color alphaLineColor = Color.red;
    public Color betaLineColor = Color.green;
    private Queue<Vector3> alphaDataPoints = new Queue<Vector3>();
    private Queue<Vector3> betaDataPoints = new Queue<Vector3>();
    private float currentZ = 0;

    void Start()
    {
        alphaLineRenderer.positionCount = maxDataPoints;
        betaLineRenderer.positionCount = maxDataPoints;

        // Set colors for the line renderers
        alphaLineRenderer.startColor = alphaLineColor;
        alphaLineRenderer.endColor = alphaLineColor;
        betaLineRenderer.startColor = betaLineColor;
        betaLineRenderer.endColor = betaLineColor;

        // Initialize with empty points
        for (int i = 0; i < maxDataPoints; i++)
        {
            alphaDataPoints.Enqueue(new Vector3(0, 0, 0));
            betaDataPoints.Enqueue(new Vector3(0, 0, 0));
        }
    }

    void Update()
    {
        // Get mouse position relative to the center of the screen
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 0; // Set z to 0 for 2D graph
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Vector3 relativePosition = mousePosition - screenCenter;

        // Normalize the position to a reasonable range for plotting
        relativePosition.x /= (Screen.width / 2);
        relativePosition.y /= (Screen.height / 2);

        // Add z movement and reset if it reaches 1
        currentZ += zSpeed * Time.deltaTime;
        if (currentZ > 1)
        {
            currentZ = 0;
        }

        // Create new points for both x and y graphs
        Vector3 newAlphaDataPoint = new Vector3(0, relativePosition.x, currentZ);
        Vector3 newBetaDataPoint = new Vector3(0, relativePosition.y, currentZ);

        // Add the new data points
        AddDataPoint(alphaDataPoints, newAlphaDataPoint);
        AddDataPoint(betaDataPoints, newBetaDataPoint);

        // Update line renderers
        UpdateLineRenderer(alphaLineRenderer, alphaDataPoints);
        UpdateLineRenderer(betaLineRenderer, betaDataPoints);
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
}
