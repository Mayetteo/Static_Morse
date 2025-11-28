using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    [Header("Camera Movement Speed")]
    public float scrollSpeed = 2f;

    void FixedUpdate()
    {
        transform.position += Vector3.right * scrollSpeed * Time.fixedDeltaTime;
    }

    public void SetSpeed(float newSpeed)
    {
        scrollSpeed = newSpeed;
    }
}
