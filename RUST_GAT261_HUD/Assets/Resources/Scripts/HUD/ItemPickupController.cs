using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickupController : FFComponent {

    FFAction.ActionSequence seq;

    public FFPath stagePathPoints;
	
	
    public float scaleMultiplier = 2.0f;
    float scaleMultiplierSave;
    Vector3 scaleSave;
	
    Transform itemText;
	Transform itemSprite;
	Transform itemBackground;
	
    //SpriteRenderer barBackground;
    public string[] demoItemNames;
	public Sprite[] demoItemSprites;
	
	bool running = false;

    
    // Use this for initialization
    void Start()
    {
        scaleSave = transform.localScale;
        scaleMultiplierSave = scaleMultiplier;

        seq = action.Sequence();


        itemText = transform.Find("Text");
        itemSprite = transform.Find("Sprite");
        itemBackground = transform.Find("Background");
		
        FFMessage<ItemPickupNotification>.Connect(OnItemPickupNotification);
        reset();
    }

    void OnDestroy()
    {
        FFMessage<ItemPickupNotification>.Disconnect(OnItemPickupNotification);
    }


    private void OnItemPickupNotification(ItemPickupNotification e)
    {        
		if(e.handled == false && running == false)
		{
			running = true;
			e.handled = true;
			
			
			// Set sprite
			{
				var randsprite = Random.Range(0, demoItemSprites.Length - 1);
				itemSprite.GetComponent<SpriteRenderer>().sprite = demoItemSprites[randsprite];
			}
			
			// Set name
			{
				var randName = Random.Range(0, demoItemNames.Length - 1);
				itemText.GetComponent<TextMesh>().text = demoItemNames[randName];
			}
			
			StartPhase1();
		}
    }
    

    public float StartPhase1Time = 0.4f;
    public AnimationCurve StartPhase1MoveCurve;
    public AnimationCurve StartPhase1ScaleCurve;
    public AnimationCurve StartPhase1ColorCurve;
    void StartPhase1()
    {
        seq.ClearSequence();

        seq.Property(ffposition, stagePathPoints.PositionAtPoint(1), StartPhase1MoveCurve, StartPhase1Time);
        seq.Property(ffscale, scaleSave * scaleMultiplier, StartPhase1ScaleCurve, StartPhase1Time);

        seq.Sync();
        seq.Call(StartPhase2);
    }
	
    public float StartPhase2Time = 0.4f;
    public AnimationCurve StartPhase2MoveCurve;
    public AnimationCurve StartPhase2ScaleCurve;
    public AnimationCurve StartPhase2ColorCurve;
    void StartPhase2()
    {
        seq.ClearSequence();

        seq.Property(ffposition, stagePathPoints.PositionAtPoint(2), StartPhase2MoveCurve, StartPhase2Time);
        seq.Property(ffscale, scaleSave * scaleMultiplier, StartPhase2ScaleCurve, StartPhase2Time);

        seq.Sync();
        seq.Call(StartPhase3);
    }
	
    public float StartPhase3Time = 0.4f;
    public AnimationCurve StartPhase3MoveCurve;
    public AnimationCurve StartPhase3ScaleCurve;
    public AnimationCurve StartPhase3ColorCurve;
    void StartPhase3()
    {
        seq.ClearSequence();

        seq.Property(ffposition, stagePathPoints.PositionAtPoint(3), StartPhase3MoveCurve, StartPhase3Time);
        seq.Property(ffscale, scaleSave * scaleMultiplier, StartPhase3ScaleCurve, StartPhase3Time);

        seq.Sync();
        seq.Call(reset);
    }


    // to avoid any strange timing problems
    void reset()
    {
		running = false;
        transform.localScale = scaleSave;
        scaleMultiplier = scaleMultiplierSave;
		transform.position = stagePathPoints.PositionAtPoint(0);
    }
}
