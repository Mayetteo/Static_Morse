using UnityEngine;

public class WordPanelMovement : MonoBehaviour
{
    private float scrollSpeed;
    public bool isMoving = false;

    public void StartMovement(float speed)
    {
        scrollSpeed = speed;
        isMoving = true;
    }

    public void StopMovement()
    {
        isMoving = false;
    }

    void Update()
    {
        if (!isMoving) return;
        
        float scrollAmount = scrollSpeed * Time.deltaTime;
        
        RectTransform rt = (RectTransform)transform;
        rt.anchoredPosition += Vector2.left * scrollAmount;
    }
}