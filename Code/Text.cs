using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class Text : MonoBehaviour {
    
    public TMP_Text healthText;
    GameObject player;

    void Start() => player = GameObject.FindWithTag("Player");
    void Update() => healthText.text = player.name;
    
}
