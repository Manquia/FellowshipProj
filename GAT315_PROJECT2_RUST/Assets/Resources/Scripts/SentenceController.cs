using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Interactable
{
    void MouseOver(bool active);
    void Use();
}


[RequireComponent(typeof(UnityEngine.UI.Image))]
public class SentenceController : MonoBehaviour, Interactable {

    Color startColorSave;
    public Color OverColor;

    void Start()
    {
        var image = GetComponent<UnityEngine.UI.Image>();

        startColorSave = image.color;
    }

    public void MouseOver(bool active)
    {
        var image = GetComponent<UnityEngine.UI.Image>();
        
        if (active)
        {
            image.color = OverColor;
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


    }
}
