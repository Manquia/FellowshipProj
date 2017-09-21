using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MenuState
{
    None,
    MainMenu,
    QuitDialog,
    Controls,

    Game,
    GameMenu,
    GameControls,
    GameQuit,
}


public class PushMenuState
{
    public MenuState newState;
    public PushMenuState(MenuState state_)
    { newState = state_; }
}

public class PopMenuState
{
    public MenuState popedState;
    public MenuState newState;
    PopMenuState()
    {
        popedState = MenuController.GetState();
        newState = MenuController.GetPrevState();
    }

}

public class MenuController : FFComponent
{
    FFAction.ActionSequence StartSeq;
    private void Start()
    {
        StartSeq = action.Sequence();
        // Call on first FixedUpdate
        StartSeq.Call(BeginMenu);
    }

    void BeginMenu()
    {
        // Send out message for everything to start happenings
        PushMenuState pms = new PushMenuState(MenuState.MainMenu);
        FFMessage<PushMenuState>.SendToLocal(pms);
    }





    static bool Ready = false;
    static public void GetReady()
    {
        if(Ready == false)
        {
            PushState(MenuState.None);

            Ready = true;
        }
    }

    static List<MenuState> states = new List<MenuState>();
    public static MenuState GetState(){
        if (states.Count > 0)
            return states[states.Count - 1];
        else
            return MenuState.None;
    }
    public static MenuState GetPrevState() {
        if (states.Count > 1)
            return states[states.Count - 2];
        else
            return MenuState.None;
    }
    static void PushState(MenuState state){
        states.Add(state);
    }
    static void PopState() { if(states.Count > 1) states.RemoveAt(states.Count - 1); }
}
