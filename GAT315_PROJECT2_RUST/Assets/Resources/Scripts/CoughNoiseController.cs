using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoughNoiseController : FFComponent
{
    AudioSource audioSrc;
    FFAction.ActionSequence seq;

    public AudioClip[] coughNoises;

	// Use this for initialization
	void Start ()
    {
        seq = action.Sequence();
        audioSrc = GetComponent<AudioSource>();
        RandomCoughLoop();
    }

    void RandomCoughLoop()
    {
        seq.Delay(Random.Range(10.0f, 45.0f));
        seq.Sync();
        seq.Call(PlayCoughSound);
        seq.Call(RandomCoughLoop);
    }

    void PlayCoughSound()
    {
        Debug.Assert(coughNoises.Length > 0, "Need cough noises");
        audioSrc.PlayOneShot(coughNoises[Random.Range(0, coughNoises.Length - 1)]);
    }

	
}
