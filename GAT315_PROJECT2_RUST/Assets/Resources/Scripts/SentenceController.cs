using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Interactable
{
    void MouseOver(bool active);
    void Use();
}


struct SentenceChosen
{
    public SentenceController sentController;
}

struct PassSentence
{
    public Sentence sent;
    public string name;
}

[RequireComponent(typeof(UnityEngine.UI.Image))]
public class SentenceController : MonoBehaviour, Interactable {

    Color startColorSave;
    public Color overColor;
    public Color chosenColor;

    Sentence curSentence;
    string curAccused;
    bool chosen = false;
    

    public void SetupSentence(Sentence sent, string accused)
    {
        var textDisplay = transform.Find("Text").GetComponent<UnityEngine.UI.Text>();

        curSentence = sent;
        curAccused = accused;
        textDisplay.text = sent.text;
    }

    void Start()
    {
        var image = GetComponent<UnityEngine.UI.Image>();

        FFMessage<SentenceChosen>.Connect(OnSentenceChosen);
        FFMessage<BeginCharacterHearing>.Connect(OnBeginCharacterHearing);
        FFMessage<EndCharacterHearing>.Connect(OnEndCharacterHearing);
        startColorSave = image.color;
    }
    void OnDestroy()
    {
        FFMessage<SentenceChosen>.Disconnect(OnSentenceChosen);
        FFMessage<BeginCharacterHearing>.Disconnect(OnBeginCharacterHearing);
        FFMessage<EndCharacterHearing>.Disconnect(OnEndCharacterHearing);
    }

    private void OnBeginCharacterHearing(BeginCharacterHearing e)
    {
        chosen = false;
        MouseOver(false); // reset sprite colors
    }

    private void OnEndCharacterHearing(EndCharacterHearing e)
    {
        if(chosen && curSentence.stage != -1)
        {
            PassSentence ps;
            ps.sent = curSentence;
            ps.name = curAccused;
            FFMessage<PassSentence>.SendToLocal(ps);
        }
    }

    private void OnSentenceChosen(SentenceChosen e)
    {
        if (e.sentController != this) // chosen was not us
        {
            var image = GetComponent<UnityEngine.UI.Image>();
            image.color = startColorSave;
            chosen = false;
        }
        else
        {
            var image = GetComponent<UnityEngine.UI.Image>();
            image.color = chosenColor;
            chosen = true;
        }
    }

    public void MouseOver(bool active)
    {
        if (chosen)
            return;

        var image = GetComponent<UnityEngine.UI.Image>();
        
        if (active)
        {
            image.color = overColor;
        }
        else // active == false
        {
            image.color = startColorSave;
        }
    }

    public void Use()
    {
        ChooseSentence();
    }

    void ChooseSentence()
    {
        SentenceChosen e;
        e.sentController = this;
        FFMessage<SentenceChosen>.SendToLocal(e);
    }
}
