using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeLevelOnTrigger : FFComponent {

    public string LevelName = "";
    public float delayTillChange = 2.5f;

    FFAction.ActionSequence ChangeSequence;
    // Use this for initialization
    void Start()
    {
        ChangeSequence = action.Sequence();

        FFMessageBoard<TriggerObject>.Connect(OnTriggerObject, gameObject);

        if(LevelName == "")
        {
            Debug.LogError("ChangeLevelOnTrigger does not have a level name to goto");
        }
    }
    void OnDestroy()
    {
        FFMessageBoard<TriggerObject>.Disconnect(OnTriggerObject, gameObject);
    }

    private void OnTriggerObject(TriggerObject e)
    {
        ChangeSequence.Delay(delayTillChange);
        ChangeSequence.Sync();
        ChangeSequence.Call(ChangeLevel);
    }

    void ChangeLevel()
    {
        if (LevelName != "")
        {
            SceneManager.LoadScene(LevelName);
        }
    }
}
