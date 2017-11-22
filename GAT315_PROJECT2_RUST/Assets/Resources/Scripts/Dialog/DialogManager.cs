using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class QueuedDialog
{
    public enum Type
    {
        Say,
        Wisper,
        Scream,
        Spooky,
        Yell,
    }
    public FFAction.ActionSequence dialogSeq;
    public SpeechController controller;
    public string text;
    public float time;
    public Type type;
}

public class DialogManager : FFComponent
{
    private static DialogManager singleton;
    public static DialogManager sing()
    {
        return singleton;
    }

    List<CharacterDialog.Dialog> gameDialog = new List<CharacterDialog.Dialog>();
    
    public SentenceController sentence1;
    public SentenceController sentence2;
    public SentenceController sentence3;

    public enum OratorNames
    {
        None,
        
        Player,
        Witness,
        Accused,
        
        PublicSeating,
    }
    [Serializable]
    public struct NameToTrans
    {
        public OratorNames name;
        public Transform trans;
    }

    Dictionary<OratorNames, Transform> Orators = new Dictionary<OratorNames, Transform>();
    

    FFAction.ActionSequence updateDialogSeq;
    FFAction.ActionSequence dialogSequence;


    // Use this for initialization
    void Start ()
    {
        // Set Singleton
        Debug.Assert(singleton == null, "Dialog Manager Singleton already setup. Duplicate Dialog Managers?");
        Debug.Log("Start Dialog Manager");
        singleton = this;

        updateDialogSeq = action.Sequence();
        dialogSequence = action.Sequence();
        
        // Add Dialogs
        {
            foreach(Transform child in transform)
            {
                var cd = child.GetComponent<CharacterDialog>();
                if (cd != null && child.gameObject.activeSelf)
                {
                    AddCharacterDialog(cd);
                }
            }
        }
        
        // Start update of dialogs
        UpdateDialog();
    }
    void OnDestroy()
    {
        Debug.Assert(singleton != null, "OnDestroy of DialogManager Singleton value was already null");

        singleton = null;
    }
    
    Dictionary<string, bool> CustomStatus = new Dictionary<string, bool>();
    
    
    private void OnCustomEvent(CustomEvent e)
    {
        Debug.Log("DialogOn " + e.tag);
        if (CustomStatus.ContainsKey(e.tag))
        {
            CustomStatus[e.tag] = true;
        }
        else
        {
            CustomStatus.Add(e.tag, true);
        }
    }

    public void SetOrator(OratorNames name, Transform trans)
    {
        Debug.Assert(trans != null);

        if(Orators.ContainsKey(name))
        {
            Orators[name] = trans;
        }
        else
        {
            Orators.Add(name, trans);
        }
    }

    public void AddCharacterDialog(CharacterDialog characterDialog)
    {
        if (characterDialog == null)
            return;

        foreach (var dialog in characterDialog.dialogSets)
        {
            AddDialog(dialog);
        }
    }
    
    void AddDialog(CharacterDialog.Dialog dialog)
    {
        gameDialog.Add(dialog);

        foreach(var cond in dialog.condition)
        {
            FFMessageBoard<CustomEvent> .Box(cond.identifier).Connect(OnCustomEvent);
        }
    }
    
    void UpdateDialog()
    {
        for(int i = 0; i < gameDialog.Count; ++i)
        {
            if(DialogConditionsTrue(gameDialog[i]))
            {
                // Debug.Log("Dialog " + i);
                QueueDialog(gameDialog[i]);
                QueueSideEffects(gameDialog[i]);

                if (gameDialog[i].repeats)
                {
                    SetDialogConditions(gameDialog[i], false);
                }
                else
                {
                    gameDialog.RemoveAt(i);
                }
            }
        }

        updateDialogSeq.Delay(0.25f);
        updateDialogSeq.Sync();
        updateDialogSeq.Call(UpdateDialog);
    }
    
    // @ TODO. make Enter/Leave events tracked

    bool DialogConditionsTrue(CharacterDialog.Dialog dialog)
    {
        for(int i = 0; i < dialog.condition.Length; ++i)
        {
            bool conditionMet;
            conditionMet = GetFlag(CustomStatus, dialog.condition[i].identifier);
            
            if (dialog.condition[i].invertCondition)
                conditionMet = !conditionMet;

            if(!conditionMet)
                return false;
        }
        return true;
    }
    void SetDialogConditions(CharacterDialog.Dialog dialog, bool flag)
    {
        for (int i = 0; i < dialog.condition.Length; ++i)
        {
            SetFlag(CustomStatus, dialog.condition[i].identifier, flag);
        }

    }
    void SetFlag(Dictionary<string,bool> container, string identifier, bool flag)
    {
        if(container.ContainsKey(identifier) == false)
        {
            container.Add(identifier, flag);
            return;
        }
        else
        {
            container[identifier] = flag;
        }
    }
    bool GetFlag(Dictionary<string, bool> container, string identifier)
    {
        bool ret = false;
        if(container.TryGetValue(identifier, out ret))
        {
            return ret;
        }
        return false;
    }

    public float averageLengthPerWord = 5.0f;
    public float displayTimePerWord = 95.0f / 60.0f;
    public float minDisplayTime = 1.45f;
    public int charactersPerLine = 24;

    void QueueDialog(CharacterDialog.Dialog dialog)
    {
        for(int i = 0; i < dialog.conversation.Length; ++i)
        {
            var text = dialog.conversation[i].text;
            var orator = dialog.conversation[i].orator;
            var type = dialog.conversation[i].type;
            CharacterOrate(orator, text, type);
        }
    }

    public Sprite DefaultTextBubble;
    public Sprite ScreamTextBubble;
    public Sprite SpookyTextBubble;

    public float FadeInTime = 0.35f;
    public float FadeOutTime = 0.15f;

    public float CharacterOrate(OratorNames orator, string text, QueuedDialog.Type type)
    {
        var echoDisplayTime = Mathf.Max(minDisplayTime, ((float)text.Length / averageLengthPerWord) * displayTimePerWord);

        var speechRoot = Orators[orator].GetComponent<Character>().GetSpeachRoot();
        var speechController = speechRoot.GetComponent<SpeechController>();

        var textWithNewLines = AddNewlines(text, charactersPerLine);


        QueuedDialog qd = new QueuedDialog
        {
            controller = speechController,
            text = textWithNewLines,
            time = echoDisplayTime,
            dialogSeq = dialogSequence,
            type = type
        };

        { // Queue the Dialog
            dialogSequence.Sync();
            dialogSequence.Call(Speak, qd);
            dialogSequence.Sync();
        }

        return echoDisplayTime;
    }


    void Speak(object queuedDialog)
    {
        QueuedDialog qd = (QueuedDialog)queuedDialog;

        var textColor = qd.controller.TextColor();
        var bubbleColor = qd.controller.BubbleColor();

        var fadeInTime = FadeInTime;
        Color newTextColor;
        Color newBubbleColor;

        { // Setup text and bubble
            if (qd.type == QueuedDialog.Type.Scream)
            {
                fadeInTime *= 0.2f;
                newBubbleColor = Color.red;
                newTextColor = Color.yellow;
            }
            else if (qd.type == QueuedDialog.Type.Spooky)
            {
                fadeInTime *= 1.2f;
                newBubbleColor = Color.magenta;
                newTextColor = Color.cyan;
            }
            else
            {
                newBubbleColor = Color.white;
                newTextColor = Color.black;
            }
        }
        
        newTextColor.a = 1.0f;
        newBubbleColor.a = 1.0f;

        // Setup sprite,colors
        qd.dialogSeq.Call(SetupSpeak, qd);

        // Fade in
        qd.dialogSeq.Property(bubbleColor, newBubbleColor, FFEase.E_SmoothEnd, fadeInTime);
        qd.dialogSeq.Property(textColor, newTextColor, FFEase.E_SmoothEnd, fadeInTime);

        // Hold for reading
        qd.dialogSeq.Sync();
        qd.dialogSeq.Delay(qd.time);
        qd.dialogSeq.Sync();


        // Fade out
        newTextColor.a = 0.0f;
        newBubbleColor.a = 0.0f;
        qd.dialogSeq.Property(bubbleColor, bubbleColor, FFEase.E_SmoothStart, FadeOutTime);
        qd.dialogSeq.Property(textColor, newTextColor, FFEase.E_SmoothStart, FadeOutTime);

        // Clean up
        qd.dialogSeq.Sync();
        qd.dialogSeq.Call(EndSpeak, qd);
        qd.dialogSeq.Sync();
    }
    void SetupSpeak(object queuedDialog)
    {
        QueuedDialog qd = (QueuedDialog)queuedDialog;

        { // Setup text and bubble
            var text = qd.controller.GetDialogText();
            var bubble = qd.controller.BubbleSprite();

            if (qd.type == QueuedDialog.Type.Scream)
            {
                bubble.sprite = ScreamTextBubble;
                bubble.color = Color.red;
                text.color = Color.yellow;
            }
            else if (qd.type == QueuedDialog.Type.Spooky)
            {
                bubble.sprite = SpookyTextBubble;
                bubble.color = Color.magenta;
                text.color = Color.cyan;
            }
            else
            {
                bubble.sprite = DefaultTextBubble;
                bubble.color = Color.white;
                text.color = Color.black;
            }
            
            bubble.color = new Color(
                bubble.color.r,
                bubble.color.g,
                bubble.color.b,
                0);
            text.color = new Color(
                text.color.r,
                text.color.g,
                text.color.b,
                0);
        }

        // Get text Item to set text
        qd.controller.DisableTooltip();
        qd.controller.GetDialogText().text = qd.text;
        
    }
    void EndSpeak(object queuedDialog)
    {
        QueuedDialog qd = (QueuedDialog)queuedDialog;

        qd.controller.EnableTooltip();
        qd.controller.GetDialogText().color = Color.clear;
        qd.controller.BubbleSprite().color = Color.clear;
    }

    string AddNewlines(string text, int charactersPerLine)
    {
        string textWithNewLines = "";
        int charactersRemaining = text.Length;

        int start = 0;
        while (charactersRemaining > 0)
        {
            int lineLength = Mathf.Min(charactersPerLine, charactersRemaining);

            // if string contains any new lines
            for(int i = 0; i < lineLength; ++i)
            {
                if(text[start + i] == '\n')
                {
                    textWithNewLines += text.Substring(start, i + 1);

                    lineLength = Mathf.Min(charactersPerLine, charactersRemaining);
                    charactersRemaining -= i + 1;
                    start += i + 1;
                    i = 0; // reset counter
                }
            }

            // All character cannot fit on the line
            if (charactersRemaining > charactersPerLine)
            {
                while (lineLength > 0 &&
                       text[start + lineLength] != ' ')
                {
                    --lineLength;
                }
            }

            textWithNewLines += text.Substring(start, lineLength);
            textWithNewLines += "\n";

            //end = start + CharactersPerLine;
            charactersRemaining -= lineLength;
            start += lineLength; // set start to end
        }

        return textWithNewLines;
    }


    void QueueSideEffects(CharacterDialog.Dialog dialog)
    {
        for (int i = 0; i < dialog.sideEffects.Length; ++i)
        {
            QueuedDialog qd = new QueuedDialog();

            qd.controller = null;
            qd.text = dialog.sideEffects[i];
            qd.time = 0.0f;
            qd.dialogSeq = dialogSequence;
            qd.type = dialog.conversation[i].type;
            
            { // Queue the Dialog
                dialogSequence.Sync();
                dialogSequence.Call(QueueSentDialogEvent, qd);
                dialogSequence.Sync();
            }
        }
    }
    void QueueSentDialogEvent(object queuedDialog)
    {
        QueuedDialog qd = (QueuedDialog)queuedDialog;

        qd.dialogSeq.Sync();
        qd.dialogSeq.Call(SendCustomDialogEvent, qd.text);
    }

    void SendCustomDialogEvent(object text)
    {
        var box = FFMessageBoard<CustomEvent>.Box((string)text);
        CustomEvent cdo;
        cdo.tag = (string)text;
        box.SendToLocal(cdo);
    }
}
