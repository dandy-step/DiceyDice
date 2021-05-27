using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class rollScript : MonoBehaviour {
    public Button button;
    public bool doRoll = false;


    // Use this for initialization
    void Start () {
        button = (Button) GameObject.Find("button").GetComponent<Button>();
        button.onClick.AddListener(HandleClick);
	}
	
	// Update is called once per frame
	public void Update () {
        //see if button was pressed

        GameObject dice;
        dice = GameObject.Find("dice");
        //Debug.Log("doroll " + doRoll.ToString());
        if (doRoll)
        {
			Rigidbody rb = dice.GetComponent<Rigidbody> ();
			rb.useGravity = true;
        }
	}

    void HandleClick()
    {
        doRoll = true;
        Debug.Log("Clicked");
    }
}
