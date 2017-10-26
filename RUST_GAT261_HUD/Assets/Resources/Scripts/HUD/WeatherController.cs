using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherController : FFComponent {

    FFAction.ActionSequence seq;

    public FFPath StartEndPoints;

    Vector3 scaleSave;
    Vector3 arrowsLocalPosSave;
    
    SpriteRenderer weatherBackground;
    Transform[] arrows;
    Transform arrowsRoot;
    Transform pause;

    public float DemoStartTime = 10.0f;
    float timer = 0;
    bool running = false;

    // Use this for initialization
    void Start()
    {
        scaleSave = transform.localScale;
        scaleMultiplierSav = scaleMultiplier;

        seq = action.Sequence();

        FFMessage<ShowWeatherBar>.Connect(OnShowWeatherBar);
        FFMessage<HideWeatherBar>.Connect(OnHideWeatherBar);

        
        weatherBackground = transform.Find("Background").GetComponent<SpriteRenderer>();
        arrowsRoot = transform.Find("Arrows");
        pause = transform.Find("Pause");

        int arrowGroupingCount = arrowsRoot.childCount;
        arrowsLocalPosSave = arrowsRoot.localPosition;
        arrows = new Transform[arrowGroupingCount];
        for (int i = 0; i < arrowGroupingCount; ++i)
        {
            arrows[i] = arrowsRoot.GetChild(i);
        }
    }

    void OnDestroy()
    {
        FFMessage<ShowWeatherBar>.Disconnect(OnShowWeatherBar);
        FFMessage<HideWeatherBar>.Disconnect(OnHideWeatherBar);
    }
    
    private void OnShowWeatherBar(ShowWeatherBar e)
    {
        running = true;
        Activate();
    }
    private void OnHideWeatherBar(HideWeatherBar e)
    {
        running = false;
        Deactivate();
    }

    public float windShellIncrements = 1.0f;
    public float maxWindSpeed = 4.0f;
    public float windvector = 2.5f;
    
    void Update()
    {
        if (running == false)
            return;

        float prevVector = windvector;

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.E))
        {
            windvector = Mathf.Max(windvector - Time.deltaTime * 0.8f, -maxWindSpeed);
        }
        else if (Input.GetKey(KeyCode.E)) //show timer
        {
            windvector = Mathf.Min(windvector + Time.deltaTime * 0.8f, maxWindSpeed);
        }

        if(prevVector * windvector < 0.0f) // changed from pos -> neg, OR, neg -> pos
        {
            arrowsRoot.localScale = new Vector3(
                -arrowsRoot.localScale.x,
                 arrowsRoot.localScale.y,
                 arrowsRoot.localScale.z);

            reset();
            ActiveLoop();
        }


        // Set all arrows to false
        foreach(var arrow in arrows)
        {
            arrow.gameObject.SetActive(false);
        }

        // Activate and setup Arrow to display
        while(true)
        {
            var windSpeed = Mathf.Abs(windvector);

            if (windSpeed < (0.1f * windShellIncrements)) // no wind!
            {
                pause.gameObject.SetActive(true);
                break;
            }
            else
            {
                pause.gameObject.SetActive(false);
            }

            int index = (int)Mathf.Clamp(Mathf.Floor(windSpeed / windShellIncrements), 0 , arrows.Length - 1);
            arrows[index].gameObject.SetActive(true);

            break; // always breaks; While loop is for quick exit
        }


    }



    public float scaleMultiplier = 2.0f;
    float scaleMultiplierSav;

    public float deactivationTime = 0.4f;
    public AnimationCurve deactivationMoveCurve;
    public AnimationCurve deactivationScaleCurve;
    void Deactivate()
    {
        seq.ClearSequence();

        seq.Property(ffposition, StartEndPoints.PositionAtPoint(0), deactivationMoveCurve, deactivationTime);
        seq.Property(ffscale, scaleSave * scaleMultiplier, deactivationScaleCurve, deactivationTime);

        seq.Sync();
        seq.Call(reset);
    }

    public float activationTime = 0.9f;
    public AnimationCurve activateMoveCurve;
    public AnimationCurve activateScaleCurve;
    void Activate()
    {
        seq.ClearSequence();

        seq.Property(ffposition, StartEndPoints.PositionAtPoint(1), activateMoveCurve, activationTime);
        seq.Property(ffscale, scaleSave * scaleMultiplier, activateScaleCurve, activationTime);

        seq.Sync();
        seq.Call(ActiveLoop);
    }

    
    public float LoopTime = 1.0f;
    public float loopPositionScale = 10.0f;
    public float loopScaleX = 10.0f;
    public AnimationCurve loopArrowPositionX;
    public AnimationCurve loopArrowScaleX;
    void ActiveLoop()
    {
        seq.ClearSequence();
        float scaleNeg = arrowsRoot.localScale.x < 0.0f ? -1.0f : 1.0f;
        float windSpeed = Mathf.Abs(windvector);

        var variableLoopTime = (LoopTime) - (LoopTime * 0.75f) * (windSpeed / maxWindSpeed);

        seq.Property(arrowsRoot.ffposition(), arrowsRoot.position   + (Vector3.right * loopPositionScale ), loopArrowPositionX, variableLoopTime);
        seq.Property(arrowsRoot.ffscale(),    arrowsRoot.localScale + (Vector3.right * loopScaleX * scaleNeg * windSpeed), loopArrowPositionX, variableLoopTime);

        seq.Sync();
        seq.Call(ActiveLoop);
    }

    // to avoid any strange timing problems
    void reset()
    {
        transform.localScale = scaleSave;
        scaleMultiplier = scaleMultiplierSav;
        arrowsRoot.localPosition = arrowsLocalPosSave;
    }
}
