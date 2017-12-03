using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitGameHandler : MonoBehaviour {
    
	// Update is called once per frame
	void Update ()
    {

        var quitGame = Input.GetKeyUp(KeyCode.Escape);

        if(quitGame)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
