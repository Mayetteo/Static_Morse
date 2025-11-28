using UnityEngine;
using UnityEngine.Splines;

public class ParallaxSplineFollower : MonoBehaviour
{
    [Header("Settings")]
    public SplineContainer spline;
    public Transform cam;
    
    [Range(0f, 1f)]
    public float parallaxEffect = 0.5f;
    
    [Tooltip("1920px = 19.2)")]
    public float tileWidth = 19.2f; 

    [Header("Offset")]
    [Tooltip("Se 0 parte dall'inizio dello spline se 0.5 da in mezzo")]
    [Range(0f, 1f)]
    public float splineStartOffset = 0.5f;

    private float startPosX;

    void Start()
    {
        startPosX = transform.position.x;
    }

    void FixedUpdate()
    {
        float dist = cam.position.x * parallaxEffect;
        float xPos = startPosX + dist;

        float positionInTile = Mathf.Repeat(xPos, tileWidth);
        float rawT = positionInTile / tileWidth;
        
        float finalT = Mathf.Repeat(rawT + splineStartOffset, 1f);

        Vector3 splinePos = spline.EvaluatePosition(finalT);

        transform.position = new Vector3(
            xPos,
            splinePos.y,
            transform.position.z
        );
    }
}