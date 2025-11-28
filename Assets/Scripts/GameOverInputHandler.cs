using UnityEngine;
using UnityEngine.SceneManagement; 

public class GameOverInputHandler : MonoBehaviour
{
    
    public string sceneToLoad = "0"; 
    public float dotDurationLimit = 0.2f; 

    private float buttonDownTime;
    private bool isProcessingInput = false;

    void OnEnable()
    {
        isProcessingInput = false;
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            // Usato unscaledTime, se usa Time.timescale non prende dash perchè fermato tempo e non può più quittare gioco
            buttonDownTime = Time.unscaledTime; 
            isProcessingInput = true;
        }

        // Decidere se è dot o dash
        if ((Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Space)) && isProcessingInput)
        {
            
            float duration = Time.unscaledTime - buttonDownTime; 
            isProcessingInput = false;
            
            if (duration < dotDurationLimit)
            {
                RestartGame(); 
            }
            else
            {
                QuitGame();
            }
        }
    }

    void RestartGame()
    {
        Debug.Log("Restart restart restart ");
        Time.timeScale = 1; 
        SceneManager.LoadScene(sceneToLoad);
    }

    void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; 
        #else
            Application.Quit(); 
        #endif
    }
}