using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EndingLevelController : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        string text = "" +
            "You have judged 18 criminals and collectively\n" +
            "sentenced 58 years of jail time, $67,000 of\n" +
            "fines, and have taken $11,500 for bail.\n" +
            "Approval Rating: " + CountRoomController.JudgeApprovalRatting.ToString("0.#") + "%\n\n" +
            "Thank you for your service,\n" +
            "The City\n\n" +
            "Press any key to continue";
        
        GetComponent<TextMesh>().text = text;
    }
	
	// Update is called once per frame
	void Update ()
    {



    }
}
