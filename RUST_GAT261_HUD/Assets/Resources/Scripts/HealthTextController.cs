using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HealthTextController : MonoBehaviour {

    public Health health;

    TextMesh textMesh;
    // Use this for initialization
    void Start ()
    {
        textMesh = transform.GetChild(0).GetComponent<TextMesh>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(health.current <= 0.5f)
        {
            textMesh.gameObject.SetActive(false);
        }
        else
        {
            textMesh.gameObject.SetActive(true);
            string text = "";
            text += Mathf.Floor(health.current + 0.5f);

            textMesh.text = text;

        }
	}
}
