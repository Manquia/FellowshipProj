using UnityEngine;
using System.Collections;
using System.Collections.Generic;


struct ChangePlayerCursorEvent
{
    public Sprite sprite;
    public Color color;
}

#region Notification Events
public struct ShowPlayerControlsEvent
{
}

public struct ShowCustomTextMessage
{
    public string text;
    public Color color;
    public float timeToDisplay;
}

public struct ShowCustomIconMessage
{
    public Sprite sprite;
    public Color color;
    public float timeToDisplay;
}
#endregion Notification Events

public class PlayerHUDController : FFComponent
{
    // Struct to contain HUD objects for text, icons, fullscreen
    public class DisplayElement
    {
        public FFAction.ActionSequence seq;
        public Transform trans;
    }
    
    // Object setup
    public Transform textObjectsRoot;
    List<DisplayElement> textHandlers = new List<DisplayElement>();
    int currentTextHandler = 0;
    public Transform iconObjectsRoot;
    List<DisplayElement> iconHandlers = new List<DisplayElement>();
    int currentIconHandler = 0;
    public Transform fullScreenObjectsRoot;
    List<DisplayElement> fullScreenHandlers = new List<DisplayElement>();
    int currentFullScreenHandler = 0;

    // Object content
    public Sprite WarningSprite;
    public Sprite DeathSprite;
    public Sprite ActivatePlayerSprite;
    public Sprite DeactivatePlayerSprite;
    public Sprite InstructionImage;

    public AudioClip DeathAudio;
    public AudioClip ActivatePlayerAudio;
    public AudioClip DeactivatePlayerAudio;
    public AudioClip CartCollectedAudio;

    #region AudioSampleHelpers

    #endregion

    // Use this for initialization
    void Start ()
    {
        // Setup handloers
        {
            foreach (Transform textobj in textObjectsRoot)
                AddToHandler(textHandlers, textobj);

            foreach (Transform iconobj in iconObjectsRoot)
                AddToHandler(iconHandlers, iconobj);

            foreach (Transform fullscreenobj in fullScreenObjectsRoot)
                AddToHandler(fullScreenHandlers, fullscreenobj);

        }

        { // listen to UI events
            FFMessage<ActivatePlayer>.Connect(OnActivatePlayer);
            FFMessage<DeactivatePlayer>.Connect(OnDeactivatePlayer);
            FFMessage<ShowPlayerControlsEvent>.Connect(OnShowPlayerControlsEvent);
            FFMessage<ShowCustomTextMessage>.Connect(OnShowCustomTextMessage);
            FFMessage<ShowCustomIconMessage>.Connect(OnShowCustomIconMessage);
        }
	}
    void OnDestroy()
    {

        { // disconnect
            FFMessage<ActivatePlayer>.Disconnect(OnActivatePlayer);
            FFMessage<DeactivatePlayer>.Disconnect(OnDeactivatePlayer);
            FFMessage<ShowPlayerControlsEvent>.Disconnect(OnShowPlayerControlsEvent);
            FFMessage<ShowCustomTextMessage>.Disconnect(OnShowCustomTextMessage);
            FFMessage<ShowCustomIconMessage>.Disconnect(OnShowCustomIconMessage);
        }
    }

    #region Game Events

    private void OnShowCustomTextMessage(ShowCustomTextMessage e)
    {
        var endColor = new Color(e.color.r, e.color.g, e.color.b, 0.0f);

        ShowTextNotification(e.text,
            e.timeToDisplay,
            e.color,
            endColor);
    }
    private void OnShowCustomIconMessage(ShowCustomIconMessage e)
    {
        var endColor = new Color(e.color.r, e.color.g, e.color.b, 0.0f);

        ShowIconNotification(e.sprite,
            e.timeToDisplay,
            e.color,
            endColor);
    }


    private void OnShowPlayerControlsEvent(ShowPlayerControlsEvent e)
    {
        ShowFullScreenNotification(
            InstructionImage,
            1.2f,
            new Color(1.0f, 1.0f, 1.0f, 1.0f),
            new Color(1.0f, 1.0f, 1.0f, 0.0f));
    }
    private void OnActivatePlayer(ActivatePlayer e)
    {
        ShowTextNotification("!!! PLAYER ACTIVATED !!!",
            4.8f,
            new Color(0.8f, 0.8f, 0.5f, 1.0f),
            new Color(0.8f, 0.8f, 0.3f, 0.0f));
        ShowFullScreenNotification(ActivatePlayerSprite,
            8.0f,
            new Color(1.0f, 1.0f, 1.0f, 1.0f),
            new Color(1.0f, 1.0f, 0.5f, 0.0f));


        PlayAudioClip(ActivatePlayerAudio);
    }
    private void OnDeactivatePlayer(DeactivatePlayer e)
    {
    }

    #endregion

    #region Helper Functions
    void AddToHandler(List<DisplayElement> handler, Transform trans)
    {
        var pair = new DisplayElement();
        pair.trans = trans;
        pair.seq = action.Sequence();

        handler.Add(pair);
    }
    void PlayAudioClip(AudioClip clip)
    {
        GetComponent<AudioSource>().PlayOneShot(clip);
    }
    void ShowTextNotification(string text, float timeToComplete, Color startColor, Color endColor)
    {
        var disp = GetTextDisplay();

        var textMesh = disp.trans.GetComponent<TextMesh>();

        textMesh.text = text;
        textMesh.color = startColor;

        disp.seq.Property(disp.trans.ffTextMeshColor(), endColor, FFEase.E_SmoothEnd, timeToComplete);
        //disp.seq.Property(disp.trans.fflocalposition(), new Vector3(0,-0.2f, 0.0f), FFEase.E_SmoothEnd, timeToComplete);

        disp.seq.Sync();
        disp.seq.Call(DeactivateDisplayItem, disp);
    }
    void ShowIconNotification(Sprite icon, float timeToComplete, Color startColor, Color endColor)
    {
        var disp = GetIconDisplay();

        var spriteRend = disp.trans.GetComponent<SpriteRenderer>();

        spriteRend.color = startColor;
        spriteRend.sprite = icon;
        
        disp.seq.Property(disp.trans.ffSpriteColor(), endColor, FFEase.E_SmoothStart, timeToComplete);
    }
    void ShowFullScreenNotification(Sprite image, float timeToComplete, Color startColor, Color endColor)
    {
        var disp = GetFullScreenDisplay();

        var spriteRend = disp.trans.GetComponent<SpriteRenderer>();

        spriteRend.color = startColor;
        spriteRend.sprite = image;

        disp.seq.Property(disp.trans.ffSpriteColor(), endColor, FFEase.E_Continuous, timeToComplete);
    }
    // wrapper function
    void DeactivateDisplayItem(object displayElement)
    {
        DeactivateDisplayItem((DisplayElement)displayElement);
    }
    void DeactivateDisplayItem(DisplayElement displayElement)
    {
        displayElement.trans.gameObject.SetActive(false);
        displayElement.seq.ClearSequence();
    }

    void ActivateDisplayItem(DisplayElement displayElement)
    {
        displayElement.trans.gameObject.SetActive(true);
        displayElement.seq.ClearSequence();
        displayElement.trans.localPosition = Vector3.zero;
    }

    DisplayElement GetTextDisplay()
    {
        currentTextHandler = currentTextHandler % textHandlers.Count;
        var displayElement = textHandlers[currentTextHandler];
        ++currentTextHandler;
        ActivateDisplayItem(displayElement);
        return displayElement;
    }
    DisplayElement GetIconDisplay()
    {
        currentIconHandler = currentIconHandler % iconHandlers.Count;
        var displayElement = iconHandlers[currentIconHandler];
        ++currentIconHandler;
        ActivateDisplayItem(displayElement);
        return displayElement;
    }
    DisplayElement GetFullScreenDisplay()
    {
        currentFullScreenHandler = currentFullScreenHandler % fullScreenHandlers.Count;
        var displayElement = fullScreenHandlers[currentFullScreenHandler];
        ++currentFullScreenHandler;
        ActivateDisplayItem(displayElement);
        return displayElement;
    }
#endregion
}
