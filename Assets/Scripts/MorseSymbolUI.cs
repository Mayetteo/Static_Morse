using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MorseSymbolUI : MonoBehaviour
{
    public Image uiImage;
    public Sprite dotSprite;
    public Sprite dashSprite;
    public Sprite questionMarkSprite;

    [Header("Feedback Successo")]
    public float successShakeDuration = 0.2f;
    public float successShakeMagnitude = 5f;
    
    private char requiredSymbol;

    public void Setup(char symbol, bool isHidden) //Mette Dot Dash o punti di domanda
    {
        requiredSymbol = symbol;
        RectTransform rect = GetComponent<RectTransform>();

        if (isHidden)
        {
            uiImage.sprite = questionMarkSprite;
            uiImage.color = Color.gray;
            rect.sizeDelta = new Vector2(100f, 100f);
        }

        else
        {
            uiImage.sprite = (symbol == '.') ? dotSprite : dashSprite;
            uiImage.color = Color.gray;
            
            if (symbol == '-')
            {
                rect.sizeDelta = new Vector2(100f, 40f); 
            }
            else
            {
                rect.sizeDelta = new Vector2(100f, 100f);
            }
        }

    }
    
    public void SetComplete()
    {
        RectTransform rect = GetComponent<RectTransform>(); 
        
        if (requiredSymbol == '-')
        {
            uiImage.sprite = dashSprite;
            rect.sizeDelta = new Vector2(100f, 40f); 
        }
        else
        {
            uiImage.sprite = dotSprite;
            rect.sizeDelta = new Vector2(100f, 100f);
        }

        //AAAA CAMBIA COLORE
        uiImage.color = Color.black; 
        
        ForceLayoutRebuild();
    }
    
    public IEnumerator SuccessFlashAndShake(Color flashColor, float delayBeforeReset)
    {
        Vector3 originalPosition = transform.localPosition;
        
        uiImage.color = flashColor;
        
        
        float elapsed = 0.0f;
        while (elapsed < successShakeDuration)
        {
            float x = originalPosition.x + Random.Range(-1f, 1f) * successShakeMagnitude;
            float y = originalPosition.y + Random.Range(-1f, 1f) * successShakeMagnitude;

            transform.localPosition = new Vector3(x, y, originalPosition.z);

            elapsed += Time.deltaTime;
            yield return null; 
        }

        transform.localPosition = originalPosition;

    }
    
    public IEnumerator ShakeAndFlash(float shakeDuration, float shakeMagnitude, float flashDuration)
    {

        Vector3 originalPosition = transform.localPosition;
        Color originalColor = uiImage.color; 
        
        uiImage.color = Color.red;
        yield return new WaitForSeconds(flashDuration);

        float elapsed = 0.0f;
        while (elapsed < shakeDuration)
        {
            float x = originalPosition.x + Random.Range(-1f, 1f) * shakeMagnitude;
            float y = originalPosition.y + Random.Range(-1f, 1f) * shakeMagnitude;

            transform.localPosition = new Vector3(x, y, originalPosition.z);

            elapsed += Time.deltaTime;
            yield return null; 
        }

        transform.localPosition = originalPosition;
        uiImage.color = originalColor; 
    }

    private void ForceLayoutRebuild()
    {
        if (transform.parent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
        }
    }
    public void ResetSymbol(bool hideCompletely)
    {
        RectTransform rect = GetComponent<RectTransform>();

        if (hideCompletely)
        {
            uiImage.sprite = questionMarkSprite;
            uiImage.color = Color.gray;
            rect.sizeDelta = new Vector2(100f, 100f);
        }
        else
        {
            uiImage.sprite = (requiredSymbol == '.') ? dotSprite : dashSprite;
            uiImage.color = Color.gray; 

            if (requiredSymbol == '-')
            {
                rect.sizeDelta = new Vector2(100f, 40f); 
            }
            else
            {
                rect.sizeDelta = new Vector2(100f, 100f);
            }
        }
    }
}