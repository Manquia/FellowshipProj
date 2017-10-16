using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PathFollowOnTrigger : FFComponent {
    FFAction.ActionSequence seq;

    public float distAlongPath = 0;
    [Range(0.1f, 100.0f)]
    public float movementSpeed;
    public int currentPointNumber = 0;
    public FFPath PathToFollow;
    public bool isCircuit = true;

    public AudioClip MoveSound;

    FFAction.ActionSequence audioFadeSeq;
    float audioSrcVolumeSave = 0.0f;
    AudioSource audioSrc;
    // Use this for initialization
    void Start()
    {
        audioFadeSeq = action.Sequence();
        audioSrc = GetComponent<AudioSource>();
        audioSrcVolumeSave = audioSrc.volume;

        // Valid path
        if (PathToFollow != null && PathToFollow.points.Length > 1)
        {
            seq = action.Sequence();
            transform.position = PathToFollow.PointAlongPath(PathToFollow.LengthAlongPathToPoint(currentPointNumber));
            WaitForInput();
        }
        else
        {
            if (PathToFollow != null)
                Debug.Log("PathFollowOnTrigger has a path which has " + PathToFollow.points.Length + " points which is invalid.");
            else
                Debug.Log("PathFollowOnTrigger has no path to follow");
        }
    }

    // Wait For Input State
    void WaitForInput()
    {
        FFMessageBoard<TriggerObject>.Connect(OnTriggerObject, gameObject);
    }
    private void OnTriggerObject(TriggerObject e)
    {
        // Not doing circuit, reached end, don't do anything
        if (!isCircuit && currentPointNumber == PathToFollow.points.Length - 1)
        {
            return;
        }
        
        ++currentPointNumber;
        MoveForward();
        PlayAudioForMovePlatform();
    }

    // Move forward state
    public void MoveForward()
    {
        FFMessageBoard<TriggerObject>.Disconnect(OnTriggerObject, gameObject);

        float lengthToNextPoint = PathToFollow.LengthAlongPathToPoint(currentPointNumber);
        if (distAlongPath >= lengthToNextPoint) // reached next point
        {
            transform.position = PathToFollow.PointAlongPath(lengthToNextPoint); // goto point
            distAlongPath = lengthToNextPoint;

            //Debug.Log("LengthToNextPoint (finished): " + lengthToNextPoint);

            WaitForInput(); // waitForInput
            return;
        }
        else // keep moving along path
        {
            transform.position = PathToFollow.PointAlongPath(distAlongPath);
        }

        distAlongPath += Time.deltaTime * movementSpeed;

        seq.Sync();
        seq.Call(MoveForward);
    }

    void PlayAudioForMovePlatform()
    {
        audioSrc.volume = audioSrcVolumeSave;
        audioFadeSeq.ClearSequence();

        audioSrc.Play();
        audioSrc.PlayOneShot(MoveSound);


        float lengthToMove = 
            PathToFollow.LengthAlongPathToPoint(currentPointNumber) -
            PathToFollow.LengthAlongPathToPoint(currentPointNumber - 1);

        float timeToCompleteMove = lengthToMove / movementSpeed;

        audioFadeSeq.Delay(timeToCompleteMove * 0.7f);
        audioFadeSeq.Sync();
        audioFadeSeq.Property(AudioSrcRef(), 0.0f, FFEase.E_SmoothStart, timeToCompleteMove * 0.3f);
    }
    
    FFRef<float> AudioSrcRef()
    {
        return new FFRef<float>(
            () => audioSrc.volume,
            (v) => { audioSrc.volume = v; } );
    }
    
    
}
