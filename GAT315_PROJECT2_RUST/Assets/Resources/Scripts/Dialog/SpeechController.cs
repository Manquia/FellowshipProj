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
        mainCamera = GameObject.Find("Main Camera").transform;

        BubbleSprite().color = BubbleSprite().color.MakeClear();
        GetDialogText().color = GetDialogText().color.MakeClear();
    }

    void Update()
    {
        transform.LookAt(mainCamera.transform.position + new Vector3(0,0,0), Vector3.Normalize(-Vector3.right + Vector3.up));
    }

    public SpriteRenderer BubbleSprite()
    {
        return transform.Find("Bubble").GetComponent<SpriteRenderer>();
    }
    public TextMesh GetDialogText()
    {
        return transform.Find("Text").GetComponent<TextMesh>();
    }

}
