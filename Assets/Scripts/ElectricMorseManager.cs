using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 
using UnityEngine.UI;

// Classi
[System.Serializable]
public class ActiveWord
{
    public GameObject panelObject;  
    public WordPanelMovement movement;
    public string morseSequence;
    public List<MorseSymbolUI> symbols;
    public int currentIndex;
    public float exitX;
    public WordResetMode currentResetMode;
}

[System.Serializable]
public class WordEncounter
{
    public string word;
    [Tooltip("Qua metti modalità della parola, se è normale o nascosta")]
    public WordResetMode resetMode = WordResetMode.normal;
}

[System.Serializable]
public class LevelConfig
{
    public int totalWordsInLevel;
    public float spawnInterval;
    public float scrollSpeed;
}

public enum WordResetMode
{
    normal,
    
    Hide,

    superHide
}

public class ElectricMorseManager : MonoBehaviour
{
    [Header("Configuration")]
    public List<WordEncounter> encounters;
    public float dotDurationLimit = 0.2f; 
    public float dashMaxDuration = 1.0f;
    
    [Header("Panel Management")]
    public GameObject wordPanelPrefab; 
    public Transform canvasContainer; 
    public float startAnchorX = 600f; 
    
    private const float BASE_EXIT_ANCHOR_X = -1920f;
    private const float EXIT_GAP_PER_WORD = 46f;

    [Header("UI References")]
    public GameObject symbolPrefab; 
    public TextMeshProUGUI levelDisplay; 
    public TextMeshProUGUI scoreDisplay; 

    [Header("Scoring")]
    public int baseScorePerWord = 100;
    private int currentScore = 0;
    private int currentMultiplier = 1;
    
    [Header("Visual Feedback")]
    public float successFeedbackDuration = 0.5f;
    private readonly Color SuccessColor = new Color32(113, 192, 119, 255);

    [Header("Error Visuals")]
    public float errorShakeDuration = 0.2f;
    public float errorShakeMagnitude = 5f;
    public float errorFlashDuration = 0.1f; 
    public float resetDelayAfterError = 0.5f; 

    [Header("Level Configuration")]
    public List<LevelConfig> levelConfigs;

    private int currentLevelIndex = 0; 
    private int wordsSpawnedInCurrentLevel = 0; 
    private int totalWordsSpawnedGlobal = 0; 
    
    private float spawnTimer = 0f;
    private float buttonDownTime;
    private bool isProcessingInput = false;
    private bool dashDurationErrorTriggered = false;

    private List<ActiveWord> activeWords = new List<ActiveWord>();

    [Header("Spawning Safety")]
    public float minSpawnGap = 50f ; 

    [Header("Audio Settings")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    
    public AudioClip backgroundMusic;
    public AudioClip dotCorrectSound; 
    public AudioClip dashCorrectSound;
    public AudioClip wrongSound;
    public AudioClip wordCompleteSound;

    [Header("GameOver")]
    public GameObject gameOverScreen;

    void Awake()
    {
        
        if (gameOverScreen == null)
        {
            GameObject foundScreen = GameObject.FindWithTag("GameOverCanvas"); 
            if (foundScreen != null)
            {
                gameOverScreen = foundScreen;
            }
        }
        
        if (gameOverScreen != null) 
        {
            gameOverScreen.SetActive(false); 
        }
        else
        {
            Debug.LogError("FATAL ERROR: The 'GameOverCanvas' (Tagged) could not be found or assigned! Check scene or tag.");
        }

        currentLevelIndex = 0; 
        wordsSpawnedInCurrentLevel = 0; 
        totalWordsSpawnedGlobal = 0;
        
        currentScore = 0;
        currentMultiplier = 1;
        
        spawnTimer = 0f;

        activeWords.Clear();

        MusicManager manager = FindFirstObjectByType<MusicManager>();

        if (manager != null)
        {
            AudioSource[] sources = manager.GetComponents<AudioSource>(); 
            if (sources.Length >= 2) 
            {
                musicSource = sources[0]; 
                sfxSource = sources[1];  
            }
        }

    }

    void Start()
    {
        UpdateScoreUI();
        UpdateLevelUI();
        
        if (musicSource != null && backgroundMusic != null)
        {
            if (!musicSource.isPlaying)
            {
                musicSource.clip = backgroundMusic;
                musicSource.loop = true;
                musicSource.Play();
            }
        }

        SpawnWord();
    }

    void Update()
    {
        HandleSpawningLogic();

        CheckFailCondition();

        HandleInput();

        if (isProcessingInput && !dashDurationErrorTriggered)
        {
            float duration = Time.time - buttonDownTime;
            if (duration >= dashMaxDuration)
            {
                dashDurationErrorTriggered = true;
                ExecuteFailureSequence(); 
            }
        }
    }

    void HandleSpawningLogic()
    {
        if (currentLevelIndex >= levelConfigs.Count) return; 

        LevelConfig config = levelConfigs[currentLevelIndex];

        if (wordsSpawnedInCurrentLevel < config.totalWordsInLevel)
        {
            spawnTimer += Time.deltaTime;

            if (spawnTimer >= config.spawnInterval && CanSpawnNewWord())
            {
                SpawnWord();
                // spawnTimer = 0f; 
                spawnTimer -= config.spawnInterval;
            }
            
        }
        else if (activeWords.Count == 0)
        {
            StartNextLevel();
        }
    }

    void SpawnWord()
    {
        if (currentLevelIndex >= levelConfigs.Count) return;
        
        LevelConfig config = levelConfigs[currentLevelIndex];
        
        int wordIndex = totalWordsSpawnedGlobal % encounters.Count;
        WordEncounter encounter = encounters[wordIndex];

        ActiveWord newWord = new ActiveWord();
        newWord.currentIndex = 0;
        newWord.symbols = new List<MorseSymbolUI>();
        newWord.morseSequence = "";
        newWord.currentResetMode = encounter.resetMode;

        bool isHidden = (encounter.resetMode == WordResetMode.Hide ||
                 encounter.resetMode == WordResetMode.superHide);

        int len = encounter.word.Length;
        float offset = (len - 1) * EXIT_GAP_PER_WORD;
        newWord.exitX = BASE_EXIT_ANCHOR_X - offset;

        GameObject panelObj = Instantiate(wordPanelPrefab, canvasContainer);
        newWord.panelObject = panelObj;
        newWord.movement = panelObj.GetComponent<WordPanelMovement>();
        
        RectTransform rt = panelObj.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(startAnchorX, rt.anchoredPosition.y);

        TextMeshProUGUI textTMP = panelObj.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
        textTMP.text = encounter.word;

        Transform container = panelObj.transform.Find("SymbolContainer");
        
        foreach (char c in encounter.word)
        {
            newWord.morseSequence += MorseCodeHelper.GetMorse(c);
        }

        for(int i = 0; i < encounter.word.Length; i++)
        {
            string letterMorse = MorseCodeHelper.GetMorse(encounter.word[i]);
            for(int j = 0; j < letterMorse.Length; j++)
            {
                GameObject symObj = Instantiate(symbolPrefab, container);
                MorseSymbolUI ui = symObj.GetComponent<MorseSymbolUI>();
                ui.Setup(letterMorse[j], isHidden); 
                newWord.symbols.Add(ui);
            }
            
            if (i < encounter.word.Length - 1)
            {
                GameObject spacer = new GameObject("Spacer");
                spacer.transform.SetParent(container, false);
                spacer.AddComponent<LayoutElement>().preferredWidth = 60f;
            }
        }

        newWord.movement.StartMovement(config.scrollSpeed);
        activeWords.Add(newWord);

        wordsSpawnedInCurrentLevel++;
        totalWordsSpawnedGlobal++;

        LayoutRebuilder.ForceRebuildLayoutImmediate(panelObj.GetComponent<RectTransform>());
    }

    void HandleInput()
    {
        if (activeWords.Count == 0) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            buttonDownTime = Time.time;
            isProcessingInput = true;
            dashDurationErrorTriggered = false; 
        }

        if ((Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Space)) && isProcessingInput && !dashDurationErrorTriggered)
        {
            float duration = Time.time - buttonDownTime;
            char inputChar = (duration < dotDurationLimit) ? '.' : '-';

            ValidateInput(inputChar);
            isProcessingInput = false;
        }
    }

    void ValidateInput(char input)
    {
        ActiveWord targetWord = activeWords[0];

        char requiredChar = targetWord.morseSequence[targetWord.currentIndex];

        if (input == requiredChar)
        {
            MorseSymbolUI sym = targetWord.symbols[targetWord.currentIndex];
            sym.SetComplete();
            targetWord.currentIndex++;

            if (targetWord.currentIndex >= targetWord.morseSequence.Length)
            {
                targetWord.movement.StopMovement(); 
                StartCoroutine(CompleteWordCoroutine(targetWord));
            }
            else
            {
                AudioClip clipToPlay = (requiredChar == '.') ? dotCorrectSound : dashCorrectSound;
                PlaySFX(clipToPlay, true);
            }
        }
        else
        {
            Debug.Log("Wrong Input on word: " + targetWord.panelObject.name);
            
            PlaySFX(wrongSound);

            currentMultiplier = 1;
            UpdateScoreUI();

            StartCoroutine(ResetWordProgress(targetWord));
        }
    }

    private IEnumerator CompleteWordCoroutine(ActiveWord word)
    {

        PlaySFX(wordCompleteSound);

        currentScore += baseScorePerWord * currentMultiplier;
        currentMultiplier++;
        UpdateScoreUI();

        foreach (var symbolUI in word.symbols)
        {
            symbolUI.SetComplete(); 
            StartCoroutine(symbolUI.SuccessFlashAndShake(SuccessColor, successFeedbackDuration));
        }

        yield return new WaitForSeconds(successFeedbackDuration);

        if (word.panelObject != null)
        {
            Destroy(word.panelObject);
        }
        
        if (activeWords.Count > 0 && activeWords[0].panelObject == word.panelObject)
        {
            activeWords.RemoveAt(0);
        }
        else
        {
            activeWords.Remove(word);
        }
    }

    void CheckFailCondition()
    {
        for (int i = 0; i < activeWords.Count; i++)
        {
            ActiveWord w = activeWords[i];
            if (w.panelObject != null)
            {
                float xPos = w.panelObject.GetComponent<RectTransform>().anchoredPosition.x;
                if (xPos < w.exitX)
                {
                    Time.timeScale = 0;

                    gameOverScreen.SetActive(true);
                    return;
                }
            }
        }
    }

    private void ExecuteFailureSequence()
    {

        PlaySFX(wrongSound);

        isProcessingInput = false;
        dashDurationErrorTriggered = false; 
        currentMultiplier = 1;
        UpdateScoreUI();

        if (activeWords.Count > 0)
        {
            StartCoroutine(ResetWordProgress(activeWords[0]));
        }
    }

    private IEnumerator ResetWordProgress(ActiveWord word)
    {
        foreach (var sym in word.symbols)
        {
            StartCoroutine(sym.ShakeAndFlash(errorShakeDuration, errorShakeMagnitude, errorFlashDuration));
        }

        yield return new WaitForSeconds(resetDelayAfterError);

        switch (word.currentResetMode)
    {
        case WordResetMode.normal:
        case WordResetMode.Hide:

            word.currentIndex = 0;
            
            break;

            case WordResetMode.superHide:
                word.currentIndex = 0;
                
                for(int i = 0; i < word.symbols.Count; i++)
                {
                    word.symbols[i].ResetSymbol(true);
                }
            break;
        }

        if (word.currentResetMode == WordResetMode.normal || 
            word.currentResetMode == WordResetMode.Hide)
        {
            foreach (var sym in word.symbols)
            {
                sym.uiImage.color = Color.gray;
            }
        }
    }

    void StartNextLevel()
    {
        currentLevelIndex++;
        wordsSpawnedInCurrentLevel = 0;
        
        if (currentLevelIndex < levelConfigs.Count)
        {
            UpdateLevelUI();
            SpawnWord();
        }
        else
        {
            if(levelDisplay) levelDisplay.text = "VICTORY!";
            Debug.Log("ALL LEVELS DONE");
        }
    }

    void UpdateScoreUI()
    {
        if (scoreDisplay) scoreDisplay.text = $"Score: {currentScore} (x{currentMultiplier})";
    }

    void UpdateLevelUI()
    {
        if (levelDisplay) levelDisplay.text = $"LEVEL {currentLevelIndex + 1}";
    }

    bool CanSpawnNewWord()
    {
        if (activeWords.Count == 0) return true;

        ActiveWord lastWord = activeWords[activeWords.Count - 1];

        if (lastWord.panelObject == null) return true;

        RectTransform lastRect = lastWord.panelObject.GetComponent<RectTransform>();

        float distFromPivotToRightEdge = lastRect.rect.width * (1.0f - lastRect.pivot.x);
        float rightEdgePosition = lastRect.anchoredPosition.x + distFromPivotToRightEdge;

        bool hasSpace = (rightEdgePosition + minSpawnGap) < startAnchorX;

        if (!hasSpace) 
        {
            // aaa
        }

        return hasSpace;
    }
    
    void PlaySFX(AudioClip clip, bool varyPitch = false)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.pitch = varyPitch ? Random.Range(0.95f, 1.05f) : 1f;
            sfxSource.PlayOneShot(clip);
        }
    }

    public string getCurrentScore()
    {
        return currentScore.ToString();
    }

}