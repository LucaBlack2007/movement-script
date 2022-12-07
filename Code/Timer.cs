using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour {
    
    public TMP_Text time;

    void Update() => time.text = $"Time: {Movement.timerPretty}";
    
}
