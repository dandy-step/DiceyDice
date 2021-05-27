using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//TODO: Fade to black at end, fade out music, text with the rules and current score limit

public class GameLogic : MonoBehaviour {
    //score and gamekeeping
    uint p1numPoints = 0;
    uint p2numPoints = 0;
    uint pointsToWin = 60;
    uint turnNumber = 0;
    uint numRounds = 0;
    bool p1turn = false;
    string gameType = "";

    //game state
    bool decidedPlayer = false;
    bool gameOver = false;
    string pickedCoinSide = null;

    //scene objects
    GameObject coin = null;
    Button headsButton = null;
    Button tailsButton = null;
    GameObject dicesEmpty = null;
    GameObject[] diceList = null;
    new Camera camera = null;
    Text victoryText = null;
    Text p1ScoreNum = null;
    Text p2ScoreNum = null;
    Text p1ScoreLabel = null;
    Text p2ScoreLabel = null;
    Text scoreLimitText = null;
    MeshCollider container = null;
    AudioSource backgroundMusic = null;
    AudioSource diceShakeSound = null;
    AudioSource coinClink = null;
    AudioSource coinResult = null;
    AudioSource scoreAddSound = null;
    AudioSource victorySound = null;
    GameObject[] resultAdds = null;
    Animator anim = null;
    Button passButton = null;
    Text passText = null;
    Text coinTossResultText = null;
    Image panel = null;

    //animation tests
    Animator p1LabelAnimator = null;
    Animator p2LabelAnimator = null;
    Animator p1NumAnimator = null;
    Animator p2NumAnimator = null;
    Animator panelAnimator = null;


    bool preCoinRoll = false;
    bool postCoinRoll = false;
    bool initCoinRoll = false;
    float randomCoinRoll = 0f;
    Quaternion afterCoinRoll = new Quaternion();
    bool cameraAnimInit = false;
    bool needToRoll = false;
    bool startedRoll = false;
    bool droppedDice = false;
    float lerpValue2 = 0f;
    bool mWasStopping = false;
    float mStoppedFramesTimer = 0f;
    float mRollTimerLimit = 0.10f;
    bool didDropInitialization = false;
    bool askedForRolls = false;
    uint[] rolls = {0, 0, 0};
    uint numRolls = 0;
    bool noRollYet = true;
    float timer = 0f;
    Vector3[] posList = new Vector3[3];
    Vector3 originalCamPos = new Vector3();
    Quaternion[] rotList = new Quaternion[3];
    Quaternion originalCamRot = new Quaternion();
    bool gatheredDice = false;
    uint currDice = 0;
    bool announcedResults = false;
    bool fadeComplete = false;
    bool timerInit = false;
    bool initCoinPick = false;
    bool fadingIn = false;
    bool fadingOut = false;

    enum State {
        SetUpGameType,
        CoinPick,
        CoinFlipAnimation,
        CoinResultScreen,
        InitPlayerRoll,
        PlayerRoll,
        InitDicesDropped,
        DicesDropped,
        GetRollResultsInit,
        GetRollResults,
        InitRollPointsAnimation,
        RollPointsAnimation,
        FadeIn,
        FadeOut,
        InitResultScreen,
        ResultScreen,
        PassPhoneScreen,
        VictoryScreen,
    }

    State gameState = new State();

    IEnumerator Fade(bool doFadeIn, bool doFadeOut) {
        Color color = panel.color;
        if (doFadeIn) {
            for (float f = 0f; f < 1.0f; f += Time.deltaTime) {
                color.a = f;
                panel.color = color;
                yield return null;
            }
        }
        fadeComplete = true;
        if (doFadeOut) {
            for (float ff = 1.0f; ff > 0.0f; ff -= Time.deltaTime) {
                color.a = ff;
                panel.color = color;
                yield return null;
            }
        }
    }

    IEnumerator FadeButton(Button button, float duration) {
        ColorBlock colorBlock = button.colors;
        colorBlock.normalColor = new Color(colorBlock.normalColor.r, colorBlock.normalColor.g, colorBlock.normalColor.b, 0f);
        for (float f = 0f; f < 1.0f; f += Time.smoothDeltaTime / duration) {
            colorBlock.normalColor = new Color(colorBlock.normalColor.r, colorBlock.normalColor.g, colorBlock.normalColor.b, f);
            button.colors = colorBlock;
            yield return null;
        }
    }

    IEnumerator FadeText(Text text, float duration) {
        Color color = text.color;
        for (float f = 0f; f < 1.0f; f += Time.smoothDeltaTime / duration) {
            color.a = f;
            text.color = color;
            yield return null;
        }
    }

    IEnumerator FadeCanvas(CanvasGroup group) {
        fadeComplete = false;
        for (float f = 1f; f > 0f; f -= Time.deltaTime) {
            group.alpha = f;
            yield return null;
        }
    }

    IEnumerator ScoreAddTextEffect(Text text) {
        Vector3 original = text.rectTransform.localScale;
        Vector3 curr;
        for (float f = original.y; f < original.y + 0.35f; f += Time.deltaTime) {
            curr = text.rectTransform.localScale;
            curr.y = f;
            text.rectTransform.localScale = curr;
            text.rectTransform.Rotate(new Vector3(0f, 0f, 1f), -f * 3);
            yield return null;
        }
        StartCoroutine(ScoreAddTextEffectReverse(text));
    }

    IEnumerator ScoreAddTextEffectReverse(Text text) {
        Vector3 original = text.rectTransform.localScale;
        Vector3 curr;

        for (float f = original.y; f > original.y - 0.35f; f -= Time.deltaTime) {
            curr = text.rectTransform.localScale;
            curr.y = f;
            text.rectTransform.localScale = curr;
            text.rectTransform.Rotate(new Vector3(0f, 0f, 1f), f * 3);
            yield return null;
        }
    }

    IEnumerator AnimTest(Animation anim) {
        anim.Play();
        yield return null;
    }

    IEnumerator PassButton() {
        gameState = State.InitPlayerRoll;
        passButton.gameObject.SetActive(false);
        passText.gameObject.SetActive(false);
        StartCoroutine(Fade(false, true));
        yield return null;
    }

    void GoToMainMenu() {
        SceneManager.LoadScene("MainMenu");
    }


    void Start() {
        camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        headsButton = GameObject.Find("HeadsButton").GetComponent<Button>();
        tailsButton = GameObject.Find("TailsButton").GetComponent<Button>();
        dicesEmpty = GameObject.Find("Dices");
        coin = GameObject.Find("Coin");
        victoryText = GameObject.Find("VictoryText").GetComponent<Text>();
        p1ScoreNum = GameObject.Find("P1ScoreNum").GetComponent<Text>();
        p2ScoreNum = GameObject.Find("P2ScoreNum").GetComponent<Text>();
        p1ScoreLabel = GameObject.Find("P1Score").GetComponent<Text>();
        p2ScoreLabel = GameObject.Find("P2Score").GetComponent<Text>();
        container = GameObject.Find("Container").GetComponent<MeshCollider>();
        diceList = GameObject.FindGameObjectsWithTag("dice");
        diceShakeSound = GameObject.Find("DiceShakeSound").GetComponent<AudioSource>();
        backgroundMusic = GameObject.Find("BackgroundMusic").GetComponent<AudioSource>();
        coinClink = GameObject.Find("Coindrop").GetComponent<AudioSource>();
        coinResult = GameObject.Find("CoinResult").GetComponent<AudioSource>();
        scoreAddSound = GameObject.Find("ScoreAddSound").GetComponent<AudioSource>();
        victorySound = GameObject.Find("VictorySound").GetComponent<AudioSource>();
        anim = GameObject.Find("P1ScoreNum").GetComponent<Animator>();
        passButton = GameObject.Find("PassButton").GetComponent<Button>();
        passText = GameObject.Find("PassText").GetComponent<Text>();
        coinTossResultText = GameObject.Find("CoinTossResultText").GetComponent<Text>();
        panel = GameObject.Find("FadePanel").GetComponent<Image>();
        scoreLimitText = GameObject.Find("ScoreLimit").GetComponent<Text>();

        p1LabelAnimator = p1ScoreLabel.GetComponent<Animator>();
        p2LabelAnimator = p2ScoreLabel.GetComponent<Animator>();
        p1NumAnimator = p1ScoreNum.GetComponent<Animator>();
        p2NumAnimator = p2ScoreNum.GetComponent<Animator>();
        panelAnimator = panel.GetComponent<Animator>();

        //hide relevant objects
        headsButton.gameObject.SetActive(false);
        tailsButton.gameObject.SetActive(false);
        victoryText.gameObject.SetActive(false);
        p1ScoreNum.gameObject.SetActive(false);
        p2ScoreNum.gameObject.SetActive(false);
        p1ScoreLabel.gameObject.SetActive(false);
        p2ScoreLabel.gameObject.SetActive(false);
        container.gameObject.SetActive(false);
        passButton.gameObject.SetActive(false);
        passText.gameObject.SetActive(false);
        coinTossResultText.gameObject.SetActive(false);
        scoreLimitText.gameObject.SetActive(false);
        for (uint i = 0; i < diceList.Length; i++) {
            diceList[i].SetActive(false);
        }

        //play background music
        backgroundMusic.Play();
    }

    void PickSide(Button button) {
        pickedCoinSide = button.GetComponentInChildren<Text>().text;
        gameState = State.CoinFlipAnimation;
        headsButton.gameObject.SetActive(false);
        tailsButton.gameObject.SetActive(false);
        coinClink.Play();
    }


    void Update() {
        switch (gameState) {
            case State.SetUpGameType:
                if (PlayerPrefs.GetInt("vsCPU") == 1) {
                    gameType = "vsCPU";
                } else if (PlayerPrefs.GetInt("vsP2") == 1) {
                    gameType = "vsP2";
                } else {
                    //unimplemented gamemode? using for debugging main menu on device
                    gameType = "vsCPU";
                }

                if (gameType.Equals("vsCPU")) {
                    p1numPoints = 0;
                    p2numPoints = 0;
                } else {
                    Debug.Log("Unimplemented game type!");
                }
                gameState = State.CoinPick;
                break;

            case State.CoinPick:
                if (!initCoinPick) {
                    initCoinPick = true;
                    dicesEmpty.SetActive(false);
                    coin.SetActive(true);
                    camera.transform.SetPositionAndRotation(new Vector3(coin.transform.position.x, coin.transform.position.y - 0.5f, coin.transform.position.z - 4f), camera.transform.rotation);
                    headsButton.gameObject.SetActive(true);
                    tailsButton.gameObject.SetActive(true);
                    headsButton.onClick.AddListener(delegate { PickSide(headsButton); });
                    tailsButton.onClick.AddListener(delegate { PickSide(tailsButton); });

                    //fade in buttons
                    StartCoroutine(FadeButton(headsButton, 1f));
                    StartCoroutine(FadeButton(tailsButton, 1f));
                }

                //rotate coin
                coin.transform.Rotate(new Vector3(1.0f, 0f, 0f), 2f);

                StartCoroutine(FadeText(headsButton.GetComponentInChildren<Text>(), 1f));
                StartCoroutine(FadeText(tailsButton.GetComponentInChildren<Text>(), 1f));
                break;

            case State.CoinFlipAnimation:
                if (!initCoinRoll) {
                    initCoinRoll = true;
                    randomCoinRoll = Random.Range(2000, 3500);
                }

                if (randomCoinRoll > 0) {
                    coin.transform.Rotate(new Vector3(1.0f, 0f, 0f), 1080 * Time.deltaTime);
                    randomCoinRoll -= 1080 * Time.deltaTime;
                } else {
                    randomCoinRoll = Random.value;
                    if (randomCoinRoll > 0.5f) {
                        randomCoinRoll = 90f;
                    } else {
                        randomCoinRoll = 270f;
                    }
                    //set rotation to 0 before presenting side so we have a consistent time and origin when lerping the rotation
                    coin.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    gameState = State.CoinResultScreen;
                    coinResult.Play();
                }
                break;

            case State.CoinResultScreen:
                coin.transform.eulerAngles = new Vector3(Mathf.LerpAngle(coin.transform.rotation.eulerAngles.x, randomCoinRoll, Time.deltaTime * 2), 0f, 0f);
                camera.transform.position = new Vector3(camera.transform.position.x, coin.transform.position.y, Mathf.Lerp(camera.transform.position.z, coin.transform.position.z - 2f, Time.deltaTime * 2));
                coinTossResultText.gameObject.SetActive(true);
                Debug.Log("Roll: " + randomCoinRoll + " " + gameType);
                if (randomCoinRoll == 90f) {
                    if (pickedCoinSide.Equals("Heads")) {
                        if (gameType.Equals("vsCPU")) {
                            coinTossResultText.text = "Player rolls first";
                        } else {
                            coinTossResultText.text = "Player 1 rolls first";
                        }
                    } else {
                        if (gameType.Equals("vsCPU")) {
                            coinTossResultText.text = "CPU rolls first";
                        } else {
                            coinTossResultText.text = "Player 2 rolls first";
                        }
                    }
                } else {
                    if (pickedCoinSide.Equals("Tails")) {
                        if (gameType.Equals("vsCPU")) {
                            coinTossResultText.text = "Player rolls first";
                        } else {
                            coinTossResultText.text = "Player 1 rolls first";
                        }
                    } else {
                        if (gameType.Equals("vsCPU")) {
                            coinTossResultText.text = "CPU rolls first";
                        } else {
                            coinTossResultText.text = "Player 2 rolls first";
                        }
                    }
                }

                if (Mathf.Round(coin.transform.eulerAngles.x) == randomCoinRoll) {
                    if (Mathf.Round(Vector3.Angle(camera.transform.forward, coin.transform.up)) == 0f) {
                        if (pickedCoinSide == "Heads") {
                            p1turn = true;
                        } else {
                            p1turn = false;
                        }
                        decidedPlayer = true;
                    } else {
                        if (pickedCoinSide == "Tails") {
                            p1turn = true;
                        } else {
                            p1turn = false;
                        }
                        decidedPlayer = true;
                    }
                }
                if (decidedPlayer) {
                    coinTossResultText.gameObject.SetActive(false); //disable text
                    needToRoll = true;
                    dicesEmpty.SetActive(true);
                    coin.SetActive(false);
                    camera.transform.position = new Vector3(0f, 8.3f, -23.9f);
                    p1ScoreLabel.gameObject.SetActive(true);
                    p2ScoreLabel.gameObject.SetActive(true);
                    p1ScoreNum.gameObject.SetActive(true);
                    p2ScoreNum.gameObject.SetActive(true);
                    gameState = State.InitPlayerRoll;
                }
                break;

            case State.InitPlayerRoll:
                //only switch player after the first round
                if (turnNumber == 0) {
                    turnNumber = 1;

                    //activate and fill out original dice and camera positions so we can reset later
                    for (uint i = 0; i < diceList.Length; i++) {
                        diceList[i].SetActive(true);
                        posList[i] = diceList[i].transform.position;
                        rotList[i] = diceList[i].transform.rotation;
                        originalCamPos = camera.transform.position;
                        originalCamRot = camera.transform.rotation;
                    }

                    //activate container collider
                    container.gameObject.SetActive(true);

                    //activate score GUI
                    p1ScoreLabel.gameObject.SetActive(true);
                    p2ScoreLabel.gameObject.SetActive(true);
                    p1ScoreNum.gameObject.SetActive(true);
                    p2ScoreNum.gameObject.SetActive(true);
                    scoreLimitText.gameObject.SetActive(true);

                } else {
                    //switch players and increment turn count
                    p1turn = !p1turn;
                    turnNumber++;

                    //restore dice and camera positions
                    for (uint i = 0; i < diceList.Length; i++) {
                        diceList[i].transform.position = posList[i];
                        diceList[i].transform.rotation = rotList[i];
                        camera.transform.position = originalCamPos;
                        camera.transform.rotation = originalCamRot;
                    }
                }

                //player label animation
                if (p1turn) {
                    p1LabelAnimator.SetBool("currentPlayer", true);
                    p2LabelAnimator.SetBool("currentPlayer", false);
                } else {
                    p1LabelAnimator.SetBool("currentPlayer", false);
                    p2LabelAnimator.SetBool("currentPlayer", true);
                }

                //disable gravity
                for (uint i = 0; i < diceList.Length; i++) {
                    diceList[i].GetComponent<Rigidbody>().useGravity = false;
                }

                //update score text
                p1ScoreNum.text = p1numPoints.ToString();
                p2ScoreNum.text = p2numPoints.ToString();

                //reset variables and enable objects for new roll
                container.enabled = true;
                timer = 0;
                askedForRolls = false;
                numRolls = 0;
                gatheredDice = false;
                announcedResults = false;
                rolls = new uint[3];
                resultAdds = new GameObject[3];
                currDice = 0;
                startedRoll = false; // 
                mStoppedFramesTimer = 0f; // 
                mWasStopping = false;   // 
                cameraAnimInit = false; // 

                gameState = State.PlayerRoll;
                break;

            case State.PlayerRoll:
                if (p1turn || (!p1turn && gameType.Equals("vsP2"))) {
                    //rotate animation until user starts shaking device
                    if (!startedRoll) {
                        for (int i = 0; i < diceList.Length; i++) {
                            diceList[i].transform.Rotate(new Vector3(0.5f, 1.0f, 0.5f), 180 * Time.deltaTime);
                        }
                        if (Input.acceleration.x > 1.0 || Input.acceleration.y > 1.0) {
                            startedRoll = true;
                            diceShakeSound.Play();
                            Handheld.Vibrate();
                        }
                    } else {
                        if ((Input.acceleration.x < 1f) && (Input.acceleration.x > -1f) && (Input.acceleration.y < 1f) && (Input.acceleration.y > -1f)) {
                            if (mWasStopping) {
                                mStoppedFramesTimer += Time.deltaTime;
                                mWasStopping = true;
                            } else {
                                mStoppedFramesTimer = 0.0f;
                                mWasStopping = true;
                            }
                        } else {
                            mWasStopping = false;
                        }

                        if (mStoppedFramesTimer >= mRollTimerLimit) {
                            gameState = State.InitDicesDropped;
                        }
                    }
                } else if (!p1turn && gameType.Equals("vsCPU")) {
                    if (timer == 0f) {
                        //roll a timer for the CPU shake and play sound
                        timer = Random.Range(3f, 5f);
                        diceShakeSound.Play();
                    }

                    if (timer <= 0f) {
                        //GetDiceRolls();
                        gameState = State.InitDicesDropped;
                    } else {
                        timer -= Time.deltaTime;
                    }
                }
                break;

            case State.InitDicesDropped:
                //enable gravity, container, and add impulse to falling dice - also, ask for rolled number in the script
                for (uint i = 0; i < diceList.Length; i++) {
                    diceList[i].GetComponent<Rigidbody>().useGravity = true;
                    diceList[i].GetComponent<Rigidbody>().AddExplosionForce(500f, Vector3.down, 250f);
                    diceList[i].GetComponent<shakeScript>().wantRolledNumber = true;
                }


                diceShakeSound.Stop();
                container.enabled = false;
                camera.transform.LookAt(diceList[0].transform);
                gameState = State.DicesDropped;
                break;

            case State.InitResultScreen:
                for (uint i = 0; i < diceList.Length; i++) {
                    //switch sides where the add text appears based on the player
                    if (p1turn) {
                        resultAdds[i] = (GameObject)Instantiate(Resources.Load("scoreAdd"), (p1ScoreNum.transform.position - new Vector3(0f, p1ScoreNum.rectTransform.rect.height + 15f, 0f)), p1ScoreNum.transform.rotation, GameObject.Find("Canvas").transform);
                    } else {
                        resultAdds[i] = (GameObject)Instantiate(Resources.Load("scoreAdd"), (p2ScoreNum.transform.position - new Vector3(0f, p2ScoreNum.rectTransform.rect.height + 15f, 0f)), p2ScoreNum.transform.rotation, GameObject.Find("Canvas").transform);
                    }
                    resultAdds[i].GetComponent<Text>().text = "+ " + rolls[i].ToString();
                    resultAdds[i].SetActive(false);
                }
                currDice = 0;   //reuse variable for the score adds
                resultAdds[0].SetActive(true);
                gameState = State.ResultScreen;

                break;

            case State.ResultScreen:
                GameObject.Find("ScoreText").GetComponent<Text>().enabled = false;

                //fade for the add numbers
                if (currDice < diceList.Length) {
                    if (resultAdds[currDice].GetComponent<Text>().color.a > 0f) {
                        if (resultAdds[currDice].GetComponent<Text>().color.a == 1f) {
                            scoreAddSound.Play();
                        }
                        Color color = resultAdds[currDice].GetComponent<Text>().color;
                        color.a -= Time.deltaTime * 0.85f;
                        resultAdds[currDice].GetComponent<Text>().color = color;
                    } else {
                        resultAdds[currDice].SetActive(false);
                        currDice++;
                        resultAdds[currDice].SetActive(true);
                    }
                } else {
                    timer = 0f;     //reset timer

                    //give points and update text
                    for (uint i = 0; i < diceList.Length; i++) {
                        if (p1turn) {
                            p1NumAnimator.Play("ScoreIncrease");
                            p1numPoints += rolls[i];
                        } else {
                            p2NumAnimator.Play("ScoreIncrease");
                            p2numPoints += rolls[i];
                        }
                    }
                    p1ScoreNum.text = p1numPoints.ToString();
                    p2ScoreNum.text = p2numPoints.ToString();
                    if (p1numPoints < pointsToWin && p2numPoints < pointsToWin) {
                        if (gameType.Equals("vsCPU")) {
                            gameState = State.InitPlayerRoll;
                        } else {
                            gameState = State.PassPhoneScreen;
                        }
                    } else {
                        gameState = State.VictoryScreen;
                    }
                }
                break;

            case State.PassPhoneScreen:
                StartCoroutine(Fade(true, false));
                passButton.gameObject.SetActive(true);
                passText.gameObject.SetActive(true);
                if (p1turn) {
                    passText.text = "Player 1, please pass the phone to Player 2.\n\nPlayer 2, press the button when ready!";
                } else {
                    passText.text = "Player 2, please pass the phone to Player 1.\n\nPlayer 1, press the button when ready!";
                }
                passButton.onClick.AddListener(delegate { StartCoroutine(PassButton()); });
                break;

            case State.VictoryScreen:
                if (!fadingIn) {
                    panelAnimator.Play("FadeIn");
                    victorySound.Play();
                    Invoke("GoToMainMenu", 6);
                    fadingIn = true;
                }
                if (timer == 0f) {
                    //victorySound.Play();
                    //StartCoroutine(Fade(true, false));
                    victoryText.gameObject.SetActive(true);
                    if (p1numPoints > p2numPoints) {
                        victoryText.text = "Player 1 wins!";
                    } else {
                        victoryText.text = "Player 2 wins!";
                    }
                    Invoke("GoToMainMenu", 6);
                }
                timer += Time.deltaTime;
                break;
        }
    }

    private void FixedUpdate() {
        switch (gameState) {
            case State.PlayerRoll:
                if (p1turn && startedRoll || (!p1turn && startedRoll && gameType.Equals("vsP2"))) {
                    for (int i = 0; i < diceList.Length; i++) {
                        diceList[i].GetComponent<Rigidbody>().AddForce(Input.acceleration.normalized.x * 1000f, Input.acceleration.normalized.y * 1000f, Input.acceleration.normalized.z);
                        if (lerpValue2 < 1.0f) {
                            camera.transform.SetPositionAndRotation(new Vector3(camera.transform.position.x, camera.transform.position.y, Mathf.Lerp(camera.transform.position.z, camera.transform.position.z + .05f, lerpValue2)), camera.transform.rotation);
                        }
                    }
                } else if (!p1turn && gameType.Equals("vsCPU")) {
                    for (uint i = 0; i < diceList.Length; i++) {
                        if ((timer % 0.2) <= 0.32) {
                            diceList[i].GetComponent<Rigidbody>().AddForce(Random.Range(-1000f, 1000f), Random.Range(-1000f, 1000f), 0f);
                        }
                    }
                }
                break;
        }
    }

    private void LateUpdate() {
        switch (gameState) {
            case State.DicesDropped:
                //check for dice results
                for (uint i = 0; i < diceList.Length; i++) {
                    if (rolls[i] == 0) {
                        if (diceList[i].GetComponent<shakeScript>().rolledNumber != 0) {
                            rolls[i] = diceList[i].GetComponent<shakeScript>().rolledNumber;
                            diceList[i].GetComponent<shakeScript>().rolledNumber = 0;
                            Debug.Log("Got " + rolls[i].ToString() + " for dice " + i.ToString());
                            diceList[i].GetComponent<shakeScript>().wantRolledNumber = false;
                            numRolls++;
                        }
                    }
                }

                if (numRolls < 3) {
                    //look at dice 0 while we don't have results
                    camera.transform.LookAt(diceList[0].transform);
                } else {
                    gameState = State.InitRollPointsAnimation;
                }
                break;

            case State.InitRollPointsAnimation:
                //place camera at start position for the animation and look at dice
                camera.transform.position = new Vector3(diceList[currDice].transform.position.x - 6f, diceList[currDice].transform.position.y + 10f, diceList[currDice].transform.position.z - 10f);
                camera.transform.LookAt(diceList[currDice].transform);
                cameraAnimInit = true;
                gameState = State.RollPointsAnimation;
                break;

            case State.RollPointsAnimation:
                if (Mathf.Round(camera.transform.position.z) != (Mathf.Round(diceList[currDice].transform.position.z - 5.0f))) {
                    //if camera is still not at the target, keep lerping towards it
                    camera.transform.LookAt(diceList[currDice].transform);
                    camera.transform.position = new Vector3(Mathf.Lerp(camera.transform.position.x, Mathf.Round(diceList[currDice].transform.position.x), Time.deltaTime), Mathf.Lerp(camera.transform.position.y, Mathf.Round(diceList[currDice].transform.position.y + 4.0f), Time.deltaTime), Mathf.Lerp(camera.transform.position.z, Mathf.Round(diceList[currDice].transform.position.z - 5.0f), Time.deltaTime));
                    GameObject.Find("ScoreText").GetComponent<Text>().text = "Rolled a " + rolls[currDice].ToString();
                    GameObject.Find("ScoreText").GetComponent<Text>().enabled = true;
                    lerpValue2 += Time.deltaTime / 2;
                } else {
                    //else, switch to next dice by incrementing currDice and going back to animation init state, or finish animations
                    //menu is now broken on the phone for some reason?
                    if (currDice == (diceList.Length - 1)) {
                        gameState = State.InitResultScreen;
                    } else {
                        currDice++;
                        gameState = State.InitRollPointsAnimation;
                    }
                }
                break;
        }
    }
}