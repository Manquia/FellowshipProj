using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


struct ButtonHover
{
    public RectTransform button;
    public bool over;
}
public class TransitionParticles : FFComponent {
    
    public RectTransform UIRoot;
    public Transform UICamera;

    public GameObject ParticlesPrefab;
    public int TotalNumberOfHolders = 10;
    public float particleOffsetTowardCamera = 0.2f;


    FFAction.ActionSequence particlesSeq;
    Dictionary<MenuState, List<RectTransform>> RectsInState = new Dictionary<MenuState, List<RectTransform>>();


    List<RectTransform> particleHolders = new List<RectTransform>();
    RectTransform particleSelection;
    Transform rectRuler;

    float maxButtonArea;
    float minButtonArea;

    void Start()
    {
        particlesSeq = action.Sequence();

        FFMessage<PopMenuState>.Connect(OnPopMenuState);
        FFMessage<PushMenuState>.Connect(OnPushMenuState);

        FFMessage<ButtonHover>.Connect(OnButtonHover);

        AddRectsWithUIState(UIRoot);
        
        rectRuler = new GameObject("Ruler").transform;
        rectRuler.parent = transform;

        particleSelection = CreateParticlesHolder();
        for (int i = 0; i < TotalNumberOfHolders; ++i)
            particleHolders.Add(CreateParticlesHolder());
    }
    

    private void OnDestroy()
    {
        FFMessage<PopMenuState>.Disconnect(OnPopMenuState);
        FFMessage<PushMenuState>.Disconnect(OnPushMenuState);

        FFMessage<ButtonHover>.Disconnect(OnButtonHover);
    }

    #region Setup
    RectTransform CreateParticlesHolder()
    {
        var particleHolder = Instantiate(ParticlesPrefab);
        var particleHolderRect = particleHolder.GetComponent<RectTransform>();

        var transitionRect = GetComponent<RectTransform>();

        particleHolderRect.SetParent(transitionRect, false);
        particleHolderRect.position = new Vector3(0, 0, 0);

        SetParticleHolder(particleHolderRect, false);

        return particleHolderRect;
    }

    void AddRectsWithUIState(RectTransform rectTrans)
    {
        if (rectTrans.GetComponent<UIBase>() != null)
        {
            AddRect(rectTrans.GetComponent<UIBase>());
        }

        foreach (RectTransform child in rectTrans)
        {
            AddRectsWithUIState(child);
        }
    }
    void AddRect(UIBase ui)
    {
        foreach (var state in ui.ActiveStates)
        {
            List<RectTransform> rects;
            if (RectsInState.TryGetValue(state, out rects))
            {
                rects.Add(ui.GetComponent<RectTransform>());
            }
            else // State doesn't exist in mapping
            {
                RectsInState.Add(state, new List<RectTransform>());
                RectsInState[state].Add(ui.GetComponent<RectTransform>());
            }
        }
    }
    #endregion



    void SetParticleHolder(RectTransform rect, bool active)
    {
        foreach (RectTransform childParticle in rect)
        {
            childParticle.gameObject.SetActive(active);
            var particleSys = childParticle.GetComponent<ParticleSystem>();
        }
    }
    void MatchParticleHolderToRect(RectTransform rect, RectTransform particleHolder)
    {
        Vector3 pos, scale;
        Quaternion rot;
        Vector3[] corners;
        Measure(rect, out pos, out rot, out scale, out corners);

        particleHolder.position = pos;
        //particleHolder.rotation = rot;
        particleHolder.localScale = Vector3.one;


        foreach (RectTransform childParticle in particleHolder)
        {
            //childParticle.rotation = rot;
            childParticle.localScale = scale;
        }
    }
    


    private void OnButtonHover(ButtonHover e)
    {
        Debug.Log("ButtonHover");

        if(e.over)
        {

            SetParticleHolder(particleSelection, true);
            MatchParticleHolderToRect(e.button, particleSelection);
        }
        else
        {
            // TODO Make this better...
            SetParticleHolder(particleSelection, false);
        }
    }
    
    void Measure(RectTransform rect, out Vector3 pos, out Quaternion rot, out Vector3 scale, out Vector3[] corners)
    {
        corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        pos = (corners[1] + corners[3]) * 0.5f;
        
        var normVecToCamera = Vector3.Normalize(UICamera.position - pos);

        pos += normVecToCamera * particleOffsetTowardCamera;
        rot = rect.rotation;
        scale = new Vector3(
            corners[2].x - corners[1].x,
            0,
            corners[1].y - corners[0].y);

    }

    private void OnPushMenuState(PushMenuState e)
    {
        Vector3 posStart, posEnd,
                scaleStart, scaleEnd;
    }

    private void OnPopMenuState(PopMenuState e)
    {
    }


    void SelectionParticlesHighlight(RectTransform Area)
    {


    }
}
