using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct UISpeakerEvent
{
    public enum Voice
    {
        ButtonClicked,
        ButtonHoverOn,
        ButtonHoverOff,
        ButtonHoverStart,
        ButtonBack,
        PlayGame,
    }
    public Voice voice;
}

public class UISpeaker : FFComponent
{
    public static void Play(UISpeakerEvent.Voice voice)
    {
        UISpeakerEvent uise;
        uise.voice = voice;
        FFMessage<UISpeakerEvent>.SendToLocal(uise);
    }

    public AudioClip ButtonClickedSound;
    public AudioClip PlayGameSound;
    public AudioClip ButtonBackSound;
    public AudioClip ButtonHoverStart;

    AudioSource hoverAudioSource;

    // Use this for initialization
    void Start ()
    {
        hoverVolSeq = action.Sequence();

        hoverAudioSource = transform.Find("HoverSource").GetComponent<AudioSource>();

        FFMessage<UISpeakerEvent>.Connect(OnUISpeakerEvent);
	}
    
    void OnDestroy()
    {
        FFMessage<UISpeakerEvent>.Disconnect(OnUISpeakerEvent);
    }

    private void OnUISpeakerEvent(UISpeakerEvent e)
    {
        switch (e.voice)
        {
            case UISpeakerEvent.Voice.ButtonClicked:
                PlaySound(ButtonClickedSound);
                break;
            case UISpeakerEvent.Voice.ButtonHoverOn:
                PlaySounce(hoverAudioSource, true);
                break;
            case UISpeakerEvent.Voice.ButtonHoverOff:
                PlaySounce(hoverAudioSource, false);
                break;
            case UISpeakerEvent.Voice.PlayGame:
                PlaySound(PlayGameSound);
                break;
            case UISpeakerEvent.Voice.ButtonBack:
                PlaySound(ButtonBackSound);
                break;
            case UISpeakerEvent.Voice.ButtonHoverStart:
                PlaySound(ButtonHoverStart);
                break;
        }
    }
    void PlaySound(AudioClip clip)
    {
        var audioSrc = GetComponent<AudioSource>();
        audioSrc.PlayOneShot(clip);
    }

    public FFRef<float> RefVolHoverSrc()
    {
        return new FFRef<float>(() => hoverAudioSource.volume, (v) => { hoverAudioSource.volume = v; });
    }

    FFAction.ActionSequence hoverVolSeq;
    void PlaySounce(AudioSource src, bool active)
    {
        if (active)
        {
            if (hoverAudioSource.isPlaying == false)
                hoverAudioSource.Play();

            hoverVolSeq.ClearSequence();
            hoverVolSeq.Property(RefVolHoverSrc(), 1.0f, FFEase.E_SmoothEnd, 0.1f);
        }
        else
        {
            hoverVolSeq.Property(RefVolHoverSrc(), 0.0f, FFEase.E_SmoothEnd, 2.9f);
        }
    }



    // Update is called once per frame
    void Update () {
		
	}
}
