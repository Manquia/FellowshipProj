using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeechController : FFComponent
{
    #region FFRef
    
    public FFRef<Color> BubbleColor()
    {
        return new FFRef<Color>(() => BubbleImage().color, (v) => { BubbleImage().color = v; });
    }
    public FFRef<Color> TextColor()
    {
        return new FFRef<Color>(() => GetDialogText().color, (v) => { GetDialogText().color = v; });
    }

    #endregion

    Transform mainCamera;
    Transform DialogueBubble;

    void Start()
    {
        mainCamera = GameObject.Find("Camera").transform;

        // Destroy some old crap if its there... too laze to do this in editor, Unity could really use an upgraded editor...
        if(transform.Find("Text") != null)  Destroy(transform.Find("Text").gameObject);
        if (transform.Find("Bubble") != null) Destroy(transform.Find("Bubble").gameObject);

        DialogueBubble = Instantiate(FFResource.Load_Prefab("DialogueBubble")).transform;
        DialogueBubble.SetParent(transform);
        DialogueBubble.localPosition = Vector3.zero;

        DialogueBubble.GetComponent<Canvas>().worldCamera = mainCamera.GetComponent<Camera>();

        // Witness has differently sized bubble
        if(transform.parent.GetComponent<Character>().details.oratorMapping == DialogManager.OratorNames.Witness)
        {
            // CHange Size
            transform.localScale *= 0.35f;
            transform.localPosition *= 0.6f;
        }
        
        //transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

        BubbleImage().color = BubbleImage().color.MakeClear();
        GetDialogText().color = GetDialogText().color.MakeClear();
    }

    void Update()
    {
        var vecToCamera = mainCamera.position - transform.position;
        var oppositePosCamera = transform.position + (-vecToCamera);

        transform.LookAt(oppositePosCamera, Vector3.up);
    }

    public UnityEngine.UI.Image BubbleImage()
    {
        return DialogueBubble.GetComponent<UnityEngine.UI.Image>();
    }
    public UnityEngine.UI.Text GetDialogText()
    {
        return DialogueBubble.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>();
    }

    public void DisableTooltip()
    {
        transform.parent.Find("TalkToolTip").gameObject.SetActive(false);
    }

    public void EnableTooltip()
    {
        transform.parent.Find("TalkToolTip").gameObject.SetActive(true);
    }

}
