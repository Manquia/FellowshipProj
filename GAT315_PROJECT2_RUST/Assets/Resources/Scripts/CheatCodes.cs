using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheatCodes : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1)) SceneManager.LoadScene("SplashScreen");
        if(Input.GetKeyDown(KeyCode.Alpha2)) SceneManager.LoadScene("CourtRoom");
        if(Input.GetKeyDown(KeyCode.Alpha3)) SceneManager.LoadScene("FiredLevel");
        if(Input.GetKeyDown(KeyCode.Alpha4)) SceneManager.LoadScene("EndingLevel");
        if(Input.GetKeyDown(KeyCode.Alpha5)) SceneManager.LoadScene("CreditsLevel");
	}
}
