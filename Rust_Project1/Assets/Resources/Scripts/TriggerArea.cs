using System;
using UnityEngine;

struct TriggerAreaOverride
{
    public string tag;
    public TriggerArea.State newState;
}

public class TriggerArea : MonoBehaviour
{
    public DialogManager.OratorNames SpecifyPerson = DialogManager.OratorNames.None;
    public string areaTag;


    public AudioClip TriggerOnSound;
    public AudioClip TriggerOffSound;

    [Flags]
    public enum State
    {
        OFF = 0,
        ON = 1,
        Trigger_OFF = 2, // if less than this we changed based on trigger Area
        OVERRIDE_OFF = 2,
        OVERRIDE_ON = 3,
    }

    public bool single = false;
    [HideInInspector]
    public State ActiveState = State.OFF;
    int TriggerCounter = 0;

    public Transform[] ObjectsOnActivated;
    public Transform[] ObjectsOnDeactivated;

	// Use this for initialization
	void Start ()
    {
        UpdateActiveObjects();
        FFMessageBoard<TriggerAreaOverride>.Box(areaTag).Connect(OnTriggerAreaOverrride);
	}
    void OnDestroy()
    {
        FFMessageBoard<TriggerAreaOverride>.Box(areaTag).Disconnect(OnTriggerAreaOverrride);
    }

    private void OnTriggerAreaOverrride(TriggerAreaOverride e)
    {
        Debug.Assert(e.tag == areaTag); // should only recieve our own events!
        ActiveState = e.newState;

        UpdateActiveObjects();
    }

    // Update is called once per frame
    void UpdateActiveObjects ()
    {
        bool active;
        if (ActiveState == State.ON || ActiveState == State.OVERRIDE_ON)
            active = true;
        else // State.FALSE || State.OVERRIDE_FALSE
            active = false;

        foreach (var obj in ObjectsOnActivated)
        {
            obj.gameObject.SetActive(active);
        }
        
        foreach (var obj in ObjectsOnDeactivated)
        {
            obj.gameObject.SetActive(!active);
        }
    }

    void OnTriggerEnter(Collider col)
    {
        var character = col.GetComponent<Character>();
        if (character != null &&
           (SpecifyPerson == DialogManager.OratorNames.None)  ||
           (character.details.person == SpecifyPerson))
        {
            ++TriggerCounter;
            if(TriggerCounter > 0 && ActiveState < State.Trigger_OFF)
            {
                // if single time, we set to override
                if (single) {
                    ActiveState = State.OVERRIDE_ON;
                }
                else {
                    ActiveState = State.ON;
                }
                
                PlayClip(TriggerOnSound);
                UpdateActiveObjects();

                CustomEventOn cdo;
                cdo.tag = areaTag;
                var box = FFMessageBoard<CustomEventOn>.Box(areaTag);
                box.SendToLocal(cdo);

            }
        }
    }

    void OnTriggerExit(Collider col)
    {
        var character = col.GetComponent<Character>();
        if (character != null &&
           (SpecifyPerson == DialogManager.OratorNames.None) /* || @TODO Add non specific character requirement*/)
        {
            --TriggerCounter;
            if (TriggerCounter <= 0 && ActiveState < State.Trigger_OFF)
            {
                ActiveState = State.OFF;
                PlayClip(TriggerOffSound);
                UpdateActiveObjects();

                CustomEventOff cdo;
                cdo.tag = areaTag;
                var box = FFMessageBoard<CustomEventOff>.Box(areaTag);
                box.SendToLocal(cdo);
            }
        }
    }

    void PlayClip(AudioClip clip)
    {
        GetComponent<AudioSource>().PlayOneShot(clip);
    }
}
