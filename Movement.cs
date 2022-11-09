using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*

    -------------------------------------------------
    ------------ F E A T U R E   L I S T ------------
    -------------------------------------------------

        - WASD Movement
        - Arrow UP or DOWN for speed +/- 1 (dev command)
        - 3 Gamemodes: Bounce, Teleport, Die. Toggle with G (default is teleport)
            * Bounce: Bounce off the wall
            * Telerport: Teleport to the other side (like pacman)
            * Die: Die if you hit the wall
        - HP displays on your cube
        - AI constantly following you, toggle with B (dev command)
        - Obstacles Generate every 5th fruit picked up
        - Fruit randomly spawning after you pick it up
        - -1 HP if you hit the AI cube, instant death if you hit an obstacle
        - Magnet to drag fruit towards you if you're close, bind is M by default
        - Every 5 picked up fruits you get a boost you can activate with SPACE
        - Occasoinally a purple fruit will spawn which upon picked up will slow the enemy

        !!! all binds can be changed on LINE 40 - 54 !!!

    -------------------------------------------------
    -------------------------------------------------
    -------------------------------------------------

*/

public class Movement : MonoBehaviour {

    class Binds {

        // MOVEMENT
        public KeyCode UP = KeyCode.W;
        public KeyCode DOWN = KeyCode.S;
        public KeyCode LEFT = KeyCode.A;
        public KeyCode RIGHT = KeyCode.D;

        // SPEED
        public KeyCode SPEED_UP = KeyCode.UpArrow;
        public KeyCode SPEED_DOWN = KeyCode.DownArrow;

        // DEV BINDS / OTHER FEATURES
        public KeyCode TOGGLE_AI = KeyCode.B;
        public KeyCode GAMEMODE = KeyCode.G;
        public KeyCode MAGNET = KeyCode.M;
        public KeyCode DEV_DEBUG = KeyCode.O;
        public KeyCode BOOST = KeyCode.Space;

        // RESTART
        public KeyCode RESTART = KeyCode.Space;

    } 

    enum GameModes { 
        BOUNCE,
        TELEPORT, 
        DIE 
    }

    bool isDEV = true;

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

    Binds Bind;

    Renderer mainColor;
    Renderer buffColor;
    Renderer cubeColor;
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

    float debugCd = 0f;
    // DEBUG COOLDOWN

    float magnetCd = 0f;
    float boostTime = 0f;
    float fruitBoostTime = 0f;
    float gameModeCooldown = 0f;
    float speedDevCd = 0f;
    float cubeMovingCooldown = 0f;

    int health;

    float speed;
    GameModes GameMode;
    Vector3 vec;

    // list to track locations 
    List<Vector3> locations = new List<Vector3>();

    void print(string a) {
        Debug.Log(a);
    }

    bool isClose(GameObject obj, GameObject _obj, int distance) {
        float xDiff = obj.transform.position.x - _obj.transform.position.x;
        float yDiff = obj.transform.position.y - _obj.transform.position.y;

        return (Mathf.Abs(xDiff) < distance && Mathf.Abs(yDiff) < distance);
    }

    List<GameObject> initiateObstacles(int amount) {

        foreach (GameObject g in obstacles) Destroy(g);
        // g.transform.position = new Vector3(1000,1000,1000);

        for (int i = 0; i < amount; i++) {

            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            obj.transform.position = new Vector3((float)Random.Range(-8.5f, 8.5f), (float)Random.Range(-8.5f, 8.5f), 0);
            
            while (Mathf.Abs(obj.transform.position.x) < 2 || Mathf.Abs(obj.transform.position.y) < 2) 
                obj.transform.position = new Vector3((float)Random.Range(-8.5f, 8.5f), (float)Random.Range(-8.5f, 8.5f), 0);

            foreach (GameObject g in obstacles) 
                if (g != null) 
                    while (isClose(obj, g, 2)) obj.transform.position = new Vector3((float)Random.Range(-8.5f, 8.5f), (float)Random.Range(-8.5f, 8.5f), 0);
            

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

        Bind = new Binds();

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
        cubeColor = cube.GetComponent<Renderer>();

        buffColor.material.SetColor("_Color", Color.yellow);

        // defaults to no fruit not board
        fruitOnBoard = false;

        // BOUNCE or TELEPORT or DIE
        GameMode = GameModes.TELEPORT;
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
            if (Input.GetKey(Bind.LEFT)) vec = new Vector3(-speed, 0f, 0f);    
            if (Input.GetKey(Bind.RIGHT)) vec = new Vector3(speed, 0f, 0f);  
            if (Input.GetKey(Bind.DOWN)) vec = new Vector3(0.0f, -speed, 0.0f);  
            if (Input.GetKey(Bind.UP)) vec = new Vector3(0.0f, speed, 0.0f);  
            
            // speed modification dev tool
            if (isDEV && (Input.GetKey(Bind.SPEED_UP) || Input.GetKey(Bind.SPEED_DOWN)) && Time.time - speedDevCd > 0.2) {
                if (Input.GetKey(Bind.SPEED_UP)) speed+=0.01f;
                if (Input.GetKey(Bind.SPEED_DOWN)) speed-=0.01f;

                print("Speed raised to: " + speed + " m/s");

                speedDevCd = Time.time;
            }
            if (isDEV && Input.GetKey(Bind.TOGGLE_AI) && (Time.time - cubeMovingCooldown) > 1) {
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
        if (Input.GetKey(Bind.GAMEMODE) && (Time.time - gameModeCooldown) > 1) {
            GameMode = (GameMode == GameModes.TELEPORT) ? GameModes.BOUNCE : ((GameMode == GameModes.BOUNCE) ? GameModes.DIE : GameModes.TELEPORT);
            print("GameMode set to " + GameMode);
            gameModeCooldown = Time.time;
        }

        if (Input.GetKey(Bind.MAGNET) && (Time.time - magnetCd) > 0.5f) {
            magnet = !magnet;
            print("Magnet toggled: " + magnet);
            magnetCd = Time.time;
        }
        if (isDEV && Input.GetKey(Bind.DEV_DEBUG) && (Time.time - debugCd) > 0.5f) {
            foreach (GameObject o in obstacles) {
                if (o != null) {
                    print(o.name + ": " + o.transform.position);
                    if (isClose(o,buff,2)) print("IS CLOSE!!!");
                }
            } 
            print("Fruit: " + buff.transform.position);
            print("------------------------");
            debugCd = Time.time;
        }

        // BOOST
        if (Input.GetKey(Bind.RESTART)) {
            if (!inSession) {
                GameMode = GameModes.TELEPORT;
                speed = 0.01f;
                inSession = true;
                vec = new Vector3(speed, 0f, 0f);
                obstacles = initiateObstacles(3);
                _counter = 0;
                counter = 0;
                // DEBUG_PRINT print("Game restarted, GameMode reverted to TELEPORT.");
            } 
        }
        if (Input.GetKey(Bind.BOOST) && canBoost && (Time.time - lastRecorded) > 1) {
            temp2 = speed;
            speed*=3;
            //print("boost STARTED");
            boostTime = Time.time; // 1
            lastRecorded = Time.time;
            canBoost = false;
            mainColor.material.SetColor("_Color", new Color(0,248,255));
        }


        if (Time.time - boostTime > 0.2 && boostTime > 0) { 
            speed = temp3 == 0 ? temp2 : temp3;
            boostTime = 0;
            //print("boost OVER");
        }

        //print(transform.position);

        if (transform.position.x <= -9.5 || transform.position.x >= 9.5 || transform.position.y <= -9.5 || transform.position.y >= 9.5) {
            if (GameMode == GameModes.BOUNCE)
                vec = new Vector3(vec.x*-1,vec.y*-1,vec.z*-1);
            else if (GameMode == GameModes.TELEPORT) {
                if (transform.position.x <= -9.5)
                    transform.position = new Vector3((transform.position.x*-1)-0.05f,transform.position.y,0);
                if (transform.position.x >= 9.5)
                    transform.position = new Vector3((transform.position.x*-1)+0.05f,transform.position.y,0);
                if (transform.position.y <= -9.5)
                    transform.position = new Vector3(transform.position.x,(transform.position.y*-1)-0.05f,0);
                if (transform.position.y >= 9.5)
                    transform.position = new Vector3(transform.position.x,(transform.position.y*-1)+0.05f,0);   
            } else if (GameMode == GameModes.DIE) { 
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
            buff.transform.position = new Vector3((float)numX, (float)numY, 0);

            foreach (GameObject g in obstacles) {
                if (g != null) {
                    while (isClose(buff,g,1)) {
                        numX = Random.Range(-9.5f, 9.5f);
                        numY = Random.Range(-9.5f, 9.5f);

                        buff.transform.position = new Vector3((float)numX, (float)numY, 0);
                        //print("changed fruit locaiton");
                    }
                }
            }

            powerUp = Mathf.Round((float)Random.Range(0, 5)) == 3;
            if (powerUp) 
                buffColor.material.SetColor("_Color", Color.magenta);
            else 
                buffColor.material.SetColor("_Color", Color.yellow);

            buff.transform.position = new Vector3((float)numX, (float)numY, 0);

           // print("fruit spawned at: (" + numX + ", " + numY + ", 0)");

            fruitOnBoard = true;

            //return powerUp;
        }

        if (timeRounded % 5 < 0.01 && Time.time > 0 && !fruitOnBoard) spawnFruit();

        if (fruitOnBoard) {
            foreach (GameObject o in obstacles) {
                if (o != null) {
                    if (isClose(o,buff,2)) spawnFruit();
                }
            } 
        }

        foreach (GameObject o in obstacles) 
            if (o != null) 
                foreach (GameObject obj in obstacles) 
                    if (obj != null) 
                        if (o != obj && isClose(o,obj,2))
                            o.transform.position = new Vector3((float)Random.Range(-8.5f, 8.5f), (float)Random.Range(-8.5f, 8.5f), 0);

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
            // DEBUG_PRINT print("Health Increased by 1: " + health + " -> " + (health+1));
            //print(counter % 5 == 0);
            health++;
            if (!canBoost) counter++;
            _counter++;
            attract = false;
            //print("Collected another, #" + counter + "!");

            if (counter % 3 == 0 && !canBoost) {
                mainColor.material.SetColor("_Color", Color.red);
                // DEBUG_PRINT print("boost charged!");
                canBoost = true;
            }
            if (_counter % 5 == 0) {
                obstacles = initiateObstacles(_counter/5+3);   
            }
            

            if (powerUp) {
                powerUp = false;
                pickedUp = true;
                cubeColor.material.SetColor("_Color", Color.magenta);
            } else {
                pickedUp = false;
                cubeColor.material.SetColor("_Color", Color.white);
            }

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
                    // DEBUG_PRINT print("COLLIDED! Health reduced by " + reduced + ": " + (health+reduced) + " -> " + health);
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
