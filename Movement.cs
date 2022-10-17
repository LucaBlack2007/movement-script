using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {

    Vector3 vec = new Vector3(0.01f, 0f, 0f);
    Vector3 temp;
    Vector3 temp2;

    bool moving = false;

    float lastRecorded = 0f;
    float boostTime = 0f;
    float gameModeCooldown = 0f;
    
    string GameMode;

    void Start() {

        // BOUNCE or TELEPORT
        GameMode = "TELEPORT";
        // GameMode is the way the object collides

    }


    void print(string a) {
        Debug.Log(a);
    }

    // Update is called once per frame
    void Update() {  
        
        transform.Translate(vec);

        if (Input.GetKey(KeyCode.A)) vec = new Vector3(-0.01f, 0f, 0f);    
        if (Input.GetKey(KeyCode.D)) vec = new Vector3(0.01f, 0f, 0f);  
        if (Input.GetKey(KeyCode.S)) vec = new Vector3(0.0f, -0.01f, 0.0f);  
        if (Input.GetKey(KeyCode.W)) vec = new Vector3(0.0f, 0.01f, 0.0f);  

        /* STOP 
        if (Input.GetKey(KeyCode.Space) && (Time.time - lastRecorded) > 0.5) {
            if (moving) {// moving / not mvoing change with space bar TOGGLE
                temp = vec;
                vec = new Vector3(0, 0, 0);
                moving = false;
            } else {
                vec = temp; 
                moving = true;
            }
            lastRecorded = Time.time;
            Debug.Log((!moving ? "started" : "stopped") + " moving");
            //Debug.Log("moving: " + moving);
        }
        */
        
        // TOGGLE GAMEMODE
        if (Input.GetKey(KeyCode.G) && (Time.time - gameModeCooldown) > 1) {
            GameMode = (GameMode == "TELEPORT") ? "BOUNCE" : "TELEPORT";
            print("GameMode set to " + GameMode);
            gameModeCooldown = Time.time;
        }


        // BOOST
        if (Input.GetKey(KeyCode.Space) && (Time.time - lastRecorded) > 1) {
            
            temp2 = vec;
            vec = new Vector3(vec.x*3, vec.y*3, vec.z*3);
            //print("boost STARTED");
            boostTime = Time.time; // 1
            lastRecorded = Time.time;

        }

        if (Time.time - boostTime > 0.2 && boostTime > 0) { 
            vec = temp2;
            boostTime = 0;
            //print("boost OVER");
        }

        //print(transform.position);

        if (transform.position.x <= -9.5 || transform.position.x >= 9.5 || transform.position.y <= -9.5 || transform.position.y >= 9.5) {
            if (GameMode == "BOUNCE")
                vec = new Vector3(vec.x*-1,vec.y*-1,vec.z*-1);
            else if (GameMode == "TELEPORT") {
                if (transform.position.x <= -9.5)
                    transform.position = new Vector3((transform.position.x*-1)-0.05f,transform.position.y,0);
                if (transform.position.x >= 9.5)
                    transform.position = new Vector3((transform.position.x*-1)+0.05f,transform.position.y,0);
                if (transform.position.y <= -9.5)
                    transform.position = new Vector3(transform.position.x,(transform.position.y*-1)-0.05f,0);
                if (transform.position.y >= 9.5)
                    transform.position = new Vector3(transform.position.x,(transform.position.y*-1)+0.05f,0);
            }


        }
        

        /*

            - 

            - left bound, x: -9.5
            - right bound, x: 9.5
            - top bound, y: 9.5
            - bottom bound, y: -9.5

        */

        //Debug.Log(Time.time);
    }  
}
