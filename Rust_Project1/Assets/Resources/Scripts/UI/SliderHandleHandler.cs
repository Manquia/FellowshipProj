using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SliderHandleHandler : EventTrigger
{
    public override void OnPointerEnter(PointerEventData data)
    {
        ButtonHover bh;
        bh.button = GetComponent<RectTransform>();
        bh.over = true;
        FFMessage<ButtonHover>.SendToLocal(bh);
    }

    public override void OnPointerExit(PointerEventData data)
    {
        ButtonHover bh;
        bh.button = GetComponent<RectTransform>(); ;
        bh.over = false;
        FFMessage<ButtonHover>.SendToLocal(bh);
    }
}
