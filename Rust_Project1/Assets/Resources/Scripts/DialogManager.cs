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
    List<CharacterDialog.Dialog> gameDialog = new List<CharacterDialog.Dialog>();

    public enum OratorNames
    {
        None,

        Sierra,
        Player,

        Monster,

        Prop1,
        Prop2,
        Prop3,
    }
    [Serializable]
    public struct NameToTrans
    {
        public OratorNames name;
        public Transform trans;
    }
    public NameToTrans[] OratorMapping;

    Dictionary<OratorNames, Transform> Orators = new Dictionary<OratorNames, Transform>();
    

    FFAction.ActionSequence updateDialogSeq;
    FFAction.ActionSequence dialogSequence;


    // Use this for initialization
    void Start ()
    {
        updateDialogSeq = action.Sequence();
        dialogSequence = action.Sequence();

        // Add mapping to dictionary
        foreach (var mapping in OratorMapping)
        {
            if(mapping.name != OratorNames.None && mapping.trans != null)
            Orators.Add(mapping.name, mapping.trans);
        }

        // Add Dialogs
        {
            foreach(Transform child in transform)
            {
                var cd = child.GetComponent<CharacterDialog>();
                if (cd != null)
                {
                    AddCharacterDialog(cd);
                }
            }
        }

        // Listen to events
        FFMessage<EnterParty>.Connect(OnEnterParty);
        FFMessage<LeaveParty>.Connect(OnLeaveParty);
        FFMessage<EnterArea>.Connect(OnEnterArea);
        FFMessage<LeaveArea>.Connect(OnLeaveArea);

        // Start update of dialogs
        UpdateDialog();
    }

    Dictionary<string, bool> PartyStatus = new Dictionary<string, bool>();
    Dictionary<string, bool> AreaStatus = new Dictionary<string, bool>();
    Dictionary<string, bool> CustomStatus = new Dictionary<string, bool>();

    private void OnLeaveArea(LeaveArea e)
    {
        if(AreaStatus.ContainsKey(e.area.name))
        {
            AreaStatus[e.area.name] = false;
        }
    }
    private void OnEnterArea(EnterArea e)
    {
        if (AreaStatus.ContainsKey(e.area.name))
        {
            AreaStatus[e.area.name] = true;
        }
        else
        {
            AreaStatus.Add(e.area.name, true);
        }
    }

    private void OnLeaveParty(LeaveParty e)
    {
        if (PartyStatus.ContainsKey(e.character.name))
        {
            PartyStatus[e.character.name] = false;
        }
    }
    private void OnEnterParty(EnterParty e)
    {
        if (PartyStatus.ContainsKey(e.character.name))
        {
            PartyStatus[e.character.name] = true;
        }
        else
        {
            PartyStatus.Add(e.character.name, true);
        }
    }
    
    private void OnCustomDialogOff(CustomEventOff e)
    {
        if (CustomStatus.ContainsKey(e.tag))
        {
            CustomStatus[e.tag] = false;
        }
    }
    private void OnCustomDialogOn(CustomEventOn e)
    {
        if (CustomStatus.ContainsKey(e.tag))
        {
            CustomStatus[e.tag] = true;
        }
        else
        {
            CustomStatus.Add(e.tag, true);
        }
    }

    



    private void OnDestroy()
    {
        FFMessage<EnterParty>.Disconnect(OnEnterParty);
        FFMessage<LeaveParty>.Disconnect(OnLeaveParty);
        FFMessage<EnterArea>.Disconnect(OnEnterArea);
        FFMessage<LeaveArea>.Disconnect(OnLeaveArea);
    }

    void AddCharacterDialog(CharacterDialog characterDialog)
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
            if(cond.type == CharacterDialog.Dialog.Condition.Type.Custom)
            {
                FFMessageBoard<CustomEventOn> .Box(cond.identifier).Connect(OnCustomDialogOn);
                FFMessageBoard<CustomEventOff>.Box(cond.identifier).Connect(OnCustomDialogOff);
            }
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

                gameDialog.RemoveAt(i);
            }
        }

        updateDialogSeq.Delay(0.25f);
        updateDialogSeq.Sync();
        updateDialogSeq.Call(UpdateDialog);
    }

    // @ TODO. Make conditions work!
    // @ TODO. make Enter/Leave events tracked

    bool DialogConditionsTrue(CharacterDialog.Dialog dialog)
    {
        for(int i = 0; i < dialog.condition.Length; ++i)
        {
            bool conditionMet = false;
            switch (dialog.condition[i].type)
            {
                case CharacterDialog.Dialog.Condition.Type.InParty:
                    conditionMet = PartyStatus.ContainsKey(dialog.condition[i].identifier);
                    break;
                case CharacterDialog.Dialog.Condition.Type.Area:
                    conditionMet = AreaStatus.ContainsKey(dialog.condition[i].identifier);
                    break;
                case CharacterDialog.Dialog.Condition.Type.Custom:
                    conditionMet = CustomStatus.ContainsKey(dialog.condition[i].identifier);
                    break;
            }

            if (dialog.condition[i].invertCondition)
                conditionMet = !conditionMet;

            if(!conditionMet)
                return false;
        }

        return true;
    }

    public float averageLengthPerWord = 5.0f;
    public float displayTimePerWord = 95.0f / 60.0f;
    public float minDisplayTime = 1.45f;
    public int charactersPerLine = 24;

    void QueueDialog(CharacterDialog.Dialog dialog)
    {
        for(int i = 0; i < dialog.conversation.Length; ++i)
        {
            var echoDisplayTime = Mathf.Max(minDisplayTime, ((float)dialog.conversation[i].text.Length / averageLengthPerWord) * displayTimePerWord);
            var orator = dialog.conversation[i].orator;
            var text = dialog.conversation[i].text;
            
            var speechRoot = Orators[orator].GetComponent<Character>().GetSpeachRoot();
            var speechController = speechRoot.GetComponent<SpeechController>();

            var textWithNewLines = AddNewlines(text, charactersPerLine);


            QueuedDialog qd = new QueuedDialog();

            qd.controller = speechController;
            qd.text = textWithNewLines;
            qd.time = echoDisplayTime;
            qd.dialogSeq = dialogSequence;
            qd.type = dialog.conversation[i].type;



            { // Queue the Dialog
                dialogSequence.Sync();
                dialogSequence.Call(Speak, qd);
                dialogSequence.Sync();
            }
        }
    }

    public Sprite DefaultTextBubble;
    public Sprite ScreamTextBubble;
    public Sprite SpookyTextBubble;

    public float FadeInTime = 0.35f;
    public float FadeOutTime = 0.15f;

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
        qd.controller.GetDialogText().text = qd.text;
        
    }
    void EndSpeak(object queuedDialog)
    {
        QueuedDialog qd = (QueuedDialog)queuedDialog;

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

            // All character cannot fit on the line
            if (charactersRemaining > charactersPerLine)
            {
                while (lineLength > 0 && text[start + lineLength] != ' ')
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
            dialogSequence.Sync();
            dialogSequence.Call(SendCustomDialogEvent, dialog.sideEffects[i]);
        }
    }
    void SendCustomDialogEvent(object text)
    {
        var box = FFMessageBoard<CustomEventOn>.Box((string)text);
        CustomEventOn cdo;
        cdo.tag = (string)text;
        box.SendToLocal(cdo);
    }
}
