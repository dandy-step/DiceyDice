using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour, IPointerUpHandler {
    Button vsCPU = null;
    Button vsP2 = null;

    public void OnPointerUp(PointerEventData eventData) {
        if (name.ToString().Equals("VsCPU")) {
            PlayerPrefs.SetInt("vsCPU", 1);
            PlayerPrefs.SetInt("vsP2", 0);
            SceneManager.LoadScene("Level");
        } else if (name.ToString().Equals("VsP2")) {
            PlayerPrefs.SetInt("vsCPU", 0);
            PlayerPrefs.SetInt("vsP2", 1);
            SceneManager.LoadScene("Level");
        }
    }


    void VsCPUGame() {
        vsCPU.transform.position.Set(0f, 20f, 0f);
        SceneManager.LoadScene("Level");
        //GetComponent<Light>().enabled = false;
    }

    void VsP2Game() {
        SceneManager.LoadScene("Level");
        GetComponent<Light>().enabled = false;
    }

    void Start() {
        vsCPU = GameObject.Find("VsCPU").GetComponent<Button>();
        vsP2 = GameObject.Find("VsP2").GetComponent<Button>();
    }

    void Update() {
        Debug.Log("");
    }
}
