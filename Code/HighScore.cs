using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class HighScore : MonoBehaviour {
    
    public TMP_Text highScoreText;
    
    void Update() => highScoreText.text = $"High Score: {PlayerPrefs.GetInt("highScore", 0)}";
    
}
