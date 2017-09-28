using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionParticles : FFComponent {
    
    public RectTransform UIRoot;


    FFAction.ActionSequence particlesSeq;
    Dictionary<MenuState, List<RectTransform>> RectsInState = new Dictionary<MenuState, List<RectTransform>>();

    public GameObject[] ParticlesPrefabs;
    public Vector2 ParticleEmitters;


    float maxButtonArea;
    float minButtonArea;

    void Start()
    {
        particlesSeq = action.Sequence();

        FFMessage<PopMenuState>.Connect(OnPopMenuState);
        FFMessage<PushMenuState>.Connect(OnPushMenuState);
    }
    private void OnDestroy()
    {
        FFMessage<PopMenuState>.Disconnect(OnPopMenuState);
        FFMessage<PushMenuState>.Disconnect(OnPushMenuState);
    }

    private void OnPushMenuState(PushMenuState e)
    {
    }

    private void OnPopMenuState(PopMenuState e)
    {
        GetComponent<ParticleSystem>().speed
    }


    void ParticlesExplode()
    {

    }
    void ParticlesContract()
    {

    }
}
