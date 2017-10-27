using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioHandler : MonoBehaviour {

    public static AudioHandler sing;

    public static void Play(AudioClip clip)
    {
        sing.GetComponent<AudioSource>().PlayOneShot(clip);
    }
	// Use this for initialization
	void Start ()
    {
        Debug.Assert(sing == null);
        sing = this;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
