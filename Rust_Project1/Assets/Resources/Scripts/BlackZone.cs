using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackZone : FFComponent {
    
    public enum Type
    {
        Enter,
        Trigger,
    }
    FFAction.ActionSequence fadeSeq;
    bool triggered = false;

    public Type type = Type.Enter;
    public string TriggerName = "";

	// Use this for initialization
	void Start ()
    {
        fadeSeq = action.Sequence();

        if(type == Type.Trigger)
        {
            Debug.Assert(TriggerName != "");

            var box = FFMessageBoard<CustomEventOn>.Box(TriggerName);
            box.Connect(OnTriggeredByEvent);
        }
    }
    void OnDestroy()
    {
        if (type == Type.Trigger)
        {
            Debug.Assert(TriggerName != "");

            var box = FFMessageBoard<CustomEventOn>.Box(TriggerName);
            box.Disconnect(OnTriggeredByEvent);
        }
    }
	
    void OnTriggerEnter(Collider other)
    {
        if (type == Type.Trigger) return;

        var character = other.GetComponent<Character>();
        if(triggered == false &&
            character != null &&
            character.details.person == DialogManager.OratorNames.Sierra)
        {
            triggered = true;
            FadeOut();
        }

    }

    void OnTriggeredByEvent(CustomEventOn ceo)
    {
        if(triggered == false)
        {
            triggered = true;
            FadeOut();
        }
    }
    
    void FadeOut()
    {
        var colorRefs = GetColorReferences();

        // FadeSprites To transparent
        foreach(var colorRef in colorRefs)
        {
            fadeSeq.Property(colorRef, colorRef.Val.MakeClear(), FFEase.E_SmoothEnd, 0.78f);
        }

        // Turn off particles
        var particles = transform.Find("BlackZoneParticles").GetComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        fadeSeq.Delay(1.5f);
        fadeSeq.Sync();
        fadeSeq.Call(DeactivateObject);
    }

    List<FFRef<Color>> GetColorReferences()
    {
        List<FFRef<Color>> colorRefs = new List<FFRef<Color>>();

        foreach (Transform child in transform)
        {
            var sprite = child.GetComponent<SpriteRenderer>();
            if (sprite != null)
            {
                colorRefs.Add(new FFRef<Color>(
                    () => sprite.color,
                    (v) => { sprite.color = v; }
                    ));
            }
        }
        return colorRefs;
    }

    void DeactivateObject()
    {
        Destroy(gameObject);
    }
}