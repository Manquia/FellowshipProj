using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeechController : FFComponent
{
    #region FFRef
    
    public FFRef<Color> BubbleColor()
    {
        return new FFRef<Color>(() => BubbleSprite().color, (v) => { transform.Find("Bubble").GetComponent<SpriteRenderer>().color = v; });
    }
    public FFRef<Color> TextColor()
    {
        return new FFRef<Color>(() => GetDialogText().color, (v) => { GetDialogText().color = v; });
    }

    #endregion

    Transform mainCamera;

    void Start()
    {
        mainCamera = GameObject.Find("Camera").transform;

        // setup sub elements
        transform.Find("Text").localPosition = new Vector3(0, 0.6f, -0.1f);
        transform.Find("Text").GetComponent<TextMesh>().fontSize = 300;
        transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

        BubbleSprite().color = BubbleSprite().color.MakeClear();
        GetDialogText().color = GetDialogText().color.MakeClear();
    }

    void Update()
    {
        var vecToCamera = mainCamera.position - transform.position;
        var oppositePosCamera = transform.position + (-vecToCamera);

        transform.LookAt(oppositePosCamera, Vector3.up);
    }

    public SpriteRenderer BubbleSprite()
    {
        return transform.Find("Bubble").GetComponent<SpriteRenderer>();
    }
    public TextMesh GetDialogText()
    {
        return transform.Find("Text").GetComponent<TextMesh>();
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
