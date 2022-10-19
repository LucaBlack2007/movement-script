using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {

    // enemy ai cube
    GameObject cube;
    bool cubeMoving;

    // temporaries to track speed and reset
    Vector3 temp;
    Vector3 temp2;

    //bool moving = false;
    bool inSession;

    // cooldowns
    float lastRecorded = 0f;
    float boostTime = 0f;
    float gameModeCooldown = 0f;
    float speedDevCd = 0f;

    float speed;
    string GameMode;
    Vector3 vec;

    // list to track locations 
    ArrayList locations;

    void Start() {

        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(0, 0.5f, 0);

        // BOUNCE or TELEPORT or DIE
        GameMode = "TELEPORT";
        // GameMode is the way the object collides

        inSession = true;
        // starts the game

        speed = 0.01f;
        // speed variable at which user travels at

        vec = new Vector3(speed, 0f, 0f);
        // actual speed vector user travels at

        locations = new ArrayList();
        cubeMoving = false;
        // ai code initiation
    }


    void print(string a) {
        Debug.Log(a);
    }

    // Update is called once per frame
    void Update() {  
        
        // some logic to make sure you can edit speed before you change direction
        if (vec.x > 0) 
            vec = new Vector3(speed, 0f, 0f);
        else if (vec.x < 0)
            vec = new Vector3(-speed, 0f, 0f);
        else if (vec.y > 0)
            vec = new Vector3(0f, speed, 0f);
        else if (vec.y < 0)
            vec = new Vector3(0f, -speed, 0f);

        transform.Translate(vec);
        //locations.Add(transform.position);
        
        if (!(transform.position.x <= -9.5 || transform.position.x >= 9.5 || transform.position.y <= -9.5 || transform.position.y >= 9.5) && inSession) {
            if (Input.GetKey(KeyCode.A)) vec = new Vector3(-speed, 0f, 0f);    
            if (Input.GetKey(KeyCode.D)) vec = new Vector3(speed, 0f, 0f);  
            if (Input.GetKey(KeyCode.S)) vec = new Vector3(0.0f, -speed, 0.0f);  
            if (Input.GetKey(KeyCode.W)) vec = new Vector3(0.0f, speed, 0.0f);  
            
            // speed modification dev tool
            if ((Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow)) && Time.time - speedDevCd > 0.2) {
                if (Input.GetKey(KeyCode.UpArrow)) speed+=0.01f;
                if (Input.GetKey(KeyCode.DownArrow)) speed-=0.01f;

                print("Speed raised to: " + speed + " m/s");

                speedDevCd = Time.time;
            }
            if (Input.GetKey(KeyCode.B)) {
                cubeMoving = !cubeMoving;
                print(cubeMoving ? "AI following you now!" : "AI stopped following you!");
            }
            
        }
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
            GameMode = (GameMode == "TELEPORT") ? "BOUNCE" : ((GameMode == "BOUNCE") ? "DIE" : "TELEPORT");
            print("GameMode set to " + GameMode);
            gameModeCooldown = Time.time;
        }


        // BOOST
        if (Input.GetKey(KeyCode.Space) && (Time.time - lastRecorded) > 1) {
            if (GameMode == "DIE" && !inSession) {
                GameMode = "TELEPORT";
                speed = 0.01f;
                inSession = true;
                vec = new Vector3(speed, 0f, 0f);
                print("Game restarted, GameMode reverted to TELEPORT.");
            } else {
                temp2 = vec;
                vec = new Vector3(vec.x*3, vec.y*3, vec.z*3);
                //print("boost STARTED");
                boostTime = Time.time; // 1
                lastRecorded = Time.time;
            }
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
            } else if (GameMode == "DIE") { 
                transform.position = new Vector3(0f,0f,0f);
                speed = 0f;
                inSession = false;
                print("gg");
            }
        }
        
        if (cubeMoving) {
            /*
            check location of player
            check direction/velocity of player

            */ 
            Vector3 aiVec = vec;
            float xDiff = 0f;
            float yDiff = 0f;

            if (transform.position.x != 0) 
                xDiff = transform.position.x - cube.transform.position.x;
            if (transform.position.y != 0)
                yDiff = transform.position.y - cube.transform.position.y;

            if (xDiff < 0) xDiff = -speed;
            if (xDiff > 0) xDiff = speed;

            if (yDiff < 0) yDiff = -speed;
            if (yDiff > 0) yDiff = speed;

            aiVec = new Vector3(xDiff, yDiff, 0f);

            cube.transform.Translate(aiVec);


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
