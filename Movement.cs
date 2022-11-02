using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Movement : MonoBehaviour {

    // enemy ai cube & fruits
    //GameObject[] gos;

    int counter;
    int _counter;

    bool attract;
    bool magnet;
    bool canBoost;
    bool powerUp;
    bool pickedUp;

    List<GameObject> obstacles = new List<GameObject>();

    GameObject cube;
    GameObject buff;
    Renderer mainColor;
    Renderer buffColor;
    //GameObject healthText;

    bool cubeMoving;

    // temporaries to track speed and reset
    Vector3 temp;
    float temp2;
    float temp3;

    // different objects being on the board / moaving
    bool inSession;
    bool fruitOnBoard;

    // cooldowns
    float lastRecorded = 0f;
    // WORKS JUST DEPRECATED ---> float fruitLastRecorded = 0f;
    float collideLastRecorded = 0f;

    float magnetCd = 0f;
    float boostTime = 0f;
    float fruitBoostTime = 0f;
    float gameModeCooldown = 0f;
    float speedDevCd = 0f;
    float cubeMovingCooldown = 0f;

    int health;

    float speed;
    string GameMode;
    Vector3 vec;

    // list to track locations 
    List<Vector3> locations = new List<Vector3>();

    void print(string a) {
        Debug.Log(a);
    }

    List<GameObject> initiateObstacles(int amount) {

        foreach (GameObject g in obstacles) Destroy(g);
        // g.transform.position = new Vector3(1000,1000,1000);

        for (int i = 0; i < amount; i++) {

            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            obj.transform.position = new Vector3((float)Random.Range(-8.5f, 8.5f), (float)Random.Range(-8.5f, 8.5f), 0);
            
            while (Mathf.Abs(obj.transform.position.x) < 2 || Mathf.Abs(obj.transform.position.y) < 2) 
                obj.transform.position = new Vector3((float)Random.Range(-8.5f, 8.5f), (float)Random.Range(-8.5f, 8.5f), 0);

            foreach (GameObject g in obstacles) {
                if (g != null) {
                    float xDiff = 0f;
                    float yDiff = 0f;

                    if (g.transform.position.x != 0) 
                        xDiff = g.transform.position.x - obj.transform.position.x;
                    if (g.transform.position.y != 0)
                        yDiff = g.transform.position.y - obj.transform.position.y;

                    if (Mathf.Abs(xDiff) < 1 && Mathf.Abs(yDiff) < 1) {
                        obj.transform.position = new Vector3((float)Random.Range(-8.5f, 8.5f), (float)Random.Range(-8.5f, 8.5f), 0);
                    }
                }
            }

            obj.name = "Obstacle " + i;
            obj.GetComponent<Renderer>().material.SetColor("_Color", Color.blue);

            //while (locations.Contains(obj.transform.position)) obj.transform.position = new Vector3((float)Random.Range(-9.5f, 9.5f), (float)Random.Range(-9.5f, 9.5f), 0);
            //locations.Add(obj.transform.position);

            BoxCollider bc = obj.AddComponent<BoxCollider>();
            bc.isTrigger = true;

            Rigidbody rb = obj.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;

            obj.tag = "Obstacle";

            obstacles.Add(obj);
        }

        return obstacles;
    }

    void Start() {

        obstacles = initiateObstacles(3);

        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        void spawnCube() => cube.transform.position = new Vector3((float)Random.Range(-9.5f, 9.5f), (float)Random.Range(-9.5f, 9.5f), 0);

        pickedUp = false;
        attract = false;
        magnet = false;
        canBoost = false;
        powerUp = false;
        counter = 0;
        health = 3;

        spawnCube();
        cube.tag = "Enemy";

        BoxCollider bc = cube.AddComponent<BoxCollider>();
        bc.isTrigger = true;

        Rigidbody rb = cube.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
        //rb.angularDrag = 0f;
        

        buff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        buff.transform.position = new Vector3(1000, 1000, 0);
        buff.tag = "Buff";

        BoxCollider bc2 = buff.AddComponent<BoxCollider>();
        bc2.isTrigger = true;

        Rigidbody rb2 = buff.AddComponent<Rigidbody>();
        rb2.useGravity = false;
        rb2.isKinematic = true;

        mainColor = this.GetComponent<Renderer>();
        buffColor = buff.GetComponent<Renderer>();

        buffColor.material.SetColor("_Color", Color.yellow);

        // defaults to no fruit not board
        fruitOnBoard = false;

        // BOUNCE or TELEPORT or DIE
        GameMode = "TELEPORT";
        // GameMode is the way the object collides

        inSession = true;
        // starts the game

        speed = 0.01f;
        // speed variable at which user travels at

        vec = new Vector3(speed, 0f, 0f);
        // actual speed vector user travels at

        cubeMoving = true;
        // ai code initiation
        //gos = GameObject.FindGameObjectsWithTag("Text");
        //healthText = gos[0];
    }


    void death() {
        transform.position = new Vector3(0f,0f,0f);
        speed = 0f;
        health = 0;
        inSession = false;
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
            if (Input.GetKey(KeyCode.B) && (Time.time - cubeMovingCooldown) > 1) {
                cubeMoving = !cubeMoving;
                print(cubeMoving ? "AI following you now!" : "AI stopped following you!");
                cubeMovingCooldown = Time.time;
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

        if (Input.GetKey(KeyCode.M) && (Time.time - magnetCd) > 0.5f) {
            magnet = !magnet;
            print("Magnet toggled: " + magnet);
            magnetCd = Time.time;
        }

        // BOOST
        if (Input.GetKey(KeyCode.Space) && (Time.time - lastRecorded) > 1) {
            if (!inSession) {
                GameMode = "TELEPORT";
                speed = 0.01f;
                inSession = true;
                vec = new Vector3(speed, 0f, 0f);
                print("Game restarted, GameMode reverted to TELEPORT.");
            } else {
                if (canBoost) {
                    temp2 = speed;
                    speed*=3;
                    //print("boost STARTED");
                    boostTime = Time.time; // 1
                    lastRecorded = Time.time;
                    canBoost = false;
                    mainColor.material.SetColor("_Color", new Color(0,248,255));
                }
            }
        }


        if (Time.time - boostTime > 0.2 && boostTime > 0) { 
            speed = temp3 == 0 ? temp2 : temp3;
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
                death();
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
            float aiSpeed = speed/(pickedUp ? 4 : 2);

            if (Time.time - boostTime < 0.2 && boostTime > 0) aiSpeed/=3;
            if (Time.time - fruitBoostTime < 0.2 && fruitBoostTime > 0) aiSpeed/=4;

            if (transform.position.x != 0) 
                xDiff = transform.position.x - cube.transform.position.x;
            if (transform.position.y != 0)
                yDiff = transform.position.y - cube.transform.position.y;

            if (xDiff < 0) xDiff = -aiSpeed;
            if (xDiff > 0) xDiff = aiSpeed;

            if (yDiff < 0) yDiff = -aiSpeed;
            if (yDiff > 0) yDiff = aiSpeed;

            aiVec = new Vector3(xDiff, yDiff, 0f);

            cube.transform.Translate(aiVec);
            

        }

        float timeRounded = (Mathf.Round(Time.time * 100.0f) * 0.01f);

        //print(timeRounded % 5);

        void spawnFruit() { 
            if (!inSession) return;
             
            double numX = Random.Range(-9.5f, 9.5f);
            double numY = Random.Range(-9.5f, 9.5f);

            powerUp = Mathf.Round((float)Random.Range(0, 5)) == 3;
            if (powerUp) buffColor.material.SetColor("_Color", Color.magenta); else buffColor.material.SetColor("_Color", Color.yellow);

            buff.transform.position = new Vector3((float)numX, (float)numY, 0);

           // print("fruit spawned at: (" + numX + ", " + numY + ", 0)");

            fruitOnBoard = true;

            //return powerUp;
        }

        if (timeRounded % 5 < 0.01 && Time.time > 0 && !fruitOnBoard) spawnFruit();

        /*
        ALL FUNTIONAL COLLISION DETECTION CODE JUST BAD :D vvvvv
        
        if (fruitOnBoard) {
            //print("(" + (Mathf.Round(buff.transform.position.x * 1f) * 0.01f) + ", " + (Mathf.Round(transform.position.y * 1f) * 0.01f) + ") (" + (Mathf.Round(transform.position.x * 1f) * 0.01f) + ", " + (Mathf.Round(buff.transform.position.y * 1f) * 0.01f) + ")");
            if (((Mathf.Round(buff.transform.position.x * 1f) * 0.01f) == (Mathf.Round(transform.position.x * 1f) * 0.01f)) && ((Mathf.Round(buff.transform.position.y * 1f) * 0.01f) == (Mathf.Round(transform.position.y * 1f) * 0.01f))) {
                fruitOnBoard = false;
                buff.transform.position = new Vector3(1000f, 1000f, 0);
                print("PICKED UP FRUIT! Speed Boost");
                
                temp3 = speed;
                speed*=4;
                //print("boost STARTED");
                fruitBoostTime = Time.time; // 1
                fruitLastRecorded = Time.time;
            }
        }
        

        if (Time.time - fruitBoostTime > 0.2 && fruitBoostTime > 0) { 
            speed = temp2 == 0 ? temp3 : temp2;
            fruitBoostTime = 0;
            //print("boost OVER");
        }

        */

        /*

            - 

            - left bound, x: -9.5
            - right bound, x: 9.5
            - top bound, y: 9.5
            - bottom bound, y: -9.5

        */

        //Debug.Log(Time.time);

        if (fruitOnBoard && magnet) {
            //  print("SFLKSDJF")
            Vector3 aiVec = vec;
            float xDiff = 0f;
            float yDiff = 0f;
            float aiSpeed = speed*1.5f;

            float x = 0f;
            float y = 0f;

            int distance = 2;

            if (transform.position.x != 0) 
                xDiff = transform.position.x - buff.transform.position.x;
            if (transform.position.y != 0)
                yDiff = transform.position.y - buff.transform.position.y;

            if (xDiff < 0) x = -aiSpeed;
            if (xDiff > 0) x = aiSpeed;

            if (yDiff < 0) y = -aiSpeed;
            if (yDiff > 0) y = aiSpeed;

            aiVec = new Vector3(x, y, 0f);

            if (Mathf.Abs(xDiff) < distance && Mathf.Abs(yDiff) < distance) attract = true;

            if (attract) buff.transform.Translate(aiVec);
            
        }
        
        if (!fruitOnBoard) spawnFruit();
        this.name = health > 0 ? ("Health: " + health) : "You died!";
    }  

    void OnTriggerEnter(Collider other) {
        
        //print(other.gameObject.tag);

        string tag = other.gameObject.tag;
        if (tag == "Buff") {
            fruitOnBoard = false;
            buff.transform.position = new Vector3(1000f, 1000f, 0);
            // speed boost works but is kinda dumb "PICKED UP FRUIT! Speed Boost &"
            print("Health Increased by 1: " + health + " -> " + (health+1));
            //print(counter % 5 == 0);
            health++;
            if (!canBoost) counter++;
            _counter++;
            attract = false;
            //print("Collected another, #" + counter + "!");

            if (counter % 3 == 0 && !canBoost) {
                mainColor.material.SetColor("_Color", Color.red);
                print("boost charged!");
                canBoost = true;
            }
            if (_counter % 5 == 0) obstacles = initiateObstacles(obstacles.Count + 1);

            if (powerUp) {
                powerUp = false;
                pickedUp = true;
            } else pickedUp = false;
            

            /*
            ALL FUNCTIONAL CODE vvvv JUST SPEED BOOST KINDA SUCKS

            temp3 = speed;
            speed*=4;
            
            fruitBoostTime = Time.time;
            fruitLastRecorded = Time.time;
            */
        } else if (tag == "Enemy") {
            if (Time.time - collideLastRecorded > 0.5f) {
                int reduced = 2;
                if (health < 2) reduced = 1;
                if (health <= reduced) {
                    death();
                    print("\ngg");
                } else {
                    health -= reduced;
                    print("COLLIDED! Health reduced by " + reduced + ": " + (health+reduced) + " -> " + health);
                    collideLastRecorded = 0f;
                }
            }
        } else if (tag == "Obstacle") {
            death();
            print("collided with obstacle!");
        }

        //print("collided");
    }

     
}
