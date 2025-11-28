using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class getPoints : MonoBehaviour
{
    public TextMeshProUGUI finalScore;
    public ElectricMorseManager gm;

    void OnEnable()
    {
        finalScore.text = "Points: "+gm.getCurrentScore();
    }
}
