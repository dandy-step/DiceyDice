using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroScreens : MonoBehaviour {
    Animator dandyStepLogoAnimator = null;
    Image dandyStepLogo = null;
    float fadeTimer = 0f;
    bool fadedIn = false;
    bool fadedOut = false;

    void LaunchMainMenu() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

	void Start () {
        dandyStepLogo = GameObject.Find("DandyStepLogo").GetComponent<Image>();
        dandyStepLogoAnimator = GameObject.Find("DandyStepLogo").GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void Update () {
        if (fadeTimer > 1f && !fadedIn) {
            dandyStepLogoAnimator.Play("FadeIn");
            fadedIn = true;
        } else if (fadeTimer > 3f && !fadedOut) {
            dandyStepLogoAnimator.Play("FadeOut");
            fadedOut = true;
            Invoke("LaunchMainMenu", 2);
        }
        fadeTimer += Time.deltaTime;
	}
}
