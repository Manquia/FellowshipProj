using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MenuState
{
    None,
    MainMenu,
    QuitDialog,
    RestartDialog,
    Controls,

    Game,     // return from Game menu
    PlayGame, // first time
    GameMenu,
    GameControls,
    GameQuit,
    Options,
    Back,
}


public class PushMenuState
{
    public MenuState newState;
    public MenuState prevState;

    public PushMenuState(MenuState state_)
    {
        newState = state_;
        prevState = MenuController.GetState();
    }
}

public class PopMenuState
{
    public MenuState popedState;
    public MenuState newState;

    public PopMenuState()
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
        MenuController.GetReady();
        

        FFMessage<PushMenuState>.Connect(OnPushMenuState);
        FFMessage<PopMenuState>.Connect(OnPopMenuState);

        StartSeq = action.Sequence();
        // Call on first FixedUpdate
        // Start on MainMenu
        StartSeq.Call(BeginMenu);
    }

    void OnDestroy()
    {
        FFMessage<PushMenuState>.Disconnect(OnPushMenuState);
        FFMessage<PopMenuState>.Disconnect(OnPopMenuState);
        Update();
    }

    void Update()
    {
        Debug.Log("Current State " + GetState());
        Debug.Log("Previous State " + GetPrevState());

        if(Input.GetKeyDown(KeyCode.Escape) && GetPrevState() != MenuState.None)
        {
            PopMenuState pms = new PopMenuState();
            FFMessage<PopMenuState>.SendToLocal(pms);
            UISpeaker.Play(UISpeakerEvent.Voice.ButtonBack);
        }
    }
    

    void BeginMenu()
    {
        // Send out message for everything to start happenings
        PushMenuState pms = new PushMenuState(MenuState.MainMenu);
        FFMessage<PushMenuState>.SendToLocal(pms);
    }

    private void OnPopMenuState(PopMenuState e)
    {
        PopState();
    }

    private void OnPushMenuState(PushMenuState e)
    {
        PushState(e.newState);
    }

    public static void PushStateQuiet(MenuState state)
    {
        PushState(state);
    }
    public static void PopStateQuiet()
    {
        PopState();
    }



    static public void ClearMenuStates()
    {
        ClearStates();
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


    static void PushState(MenuState state)
    {
        Debug.Log("Pushed" + state); // DEBUG
        states.Add(state);
    }
    static void PopState()
    {
        if (states.Count > 1)
        {
            Debug.Log("Poped "  + states[states.Count - 1]); // DEBUG
            states.RemoveAt(states.Count - 1);
        }
    }
    static void ClearStates()
    {
        PushMenuState pms = new PushMenuState(MenuState.None);
        FFMessage<PushMenuState>.SendToLocal(pms);
        states.Clear();
    }
}
