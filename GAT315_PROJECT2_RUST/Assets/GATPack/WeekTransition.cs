using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

public struct StartWeek
{
    public int week;
}

// This is the controller of the splace screen object
public class WeekTransition : FFComponent {
    
    public float FadeOutTime;
    public float FadeInTime;

    Color FadeScreenColorSave;
    Color titleColorSave;
    Color pressKeyToContinueSave;

    private FFAction.ActionSequence FadeSequence;

    // Use this for initialization
    void Start ()
    {
        titleColorSave = transform.Find("Title").GetComponent<TextMesh>().color;
        pressKeyToContinueSave = transform.Find("PressToContinue").GetComponent<TextMesh>().color;
        FadeScreenColorSave = GetComponent<SpriteRenderer>().color;

        transform.Find("Title").GetComponent<TextMesh>().color = Color.clear;
        transform.Find("PressToContinue").GetComponent<TextMesh>().color = Color.clear;
        GetComponent<SpriteRenderer>().color = Color.clear;


        // Initialize
        {
            FadeSequence = action.Sequence();
        }

        FFMessage<FadeToNextWeek>.Connect(OnFadeToNextWeek);
	}

    void OnDestroy()
    {
        FFMessage<FadeToNextWeek>.Disconnect(OnFadeToNextWeek);

    }

    private void OnFadeToNextWeek(FadeToNextWeek e)
    {
        FadeOut(e.week);
    }
    
    void FadeOut(int weekIndex)
    {
        // Init Details
        LockPlayerController();
        var approvalRating = CountRoomController.JudgeApprovalRatting;
        string approvalLetter = "_";
        string letterMessage = "";

        if (approvalRating < 60.0f)
        {
            approvalLetter = "F";
            letterMessage = "\nBe sure to talk with the accused and witness\nto make sure your decision was informed.";
        }
        else if (approvalRating < 70.0f)
        {
            approvalLetter = "D";
            letterMessage = "\nBe sure to talk with the accused and witness\nto make sure your decision was informed.";
        }
        else if (approvalRating < 80.0f)
        {
            approvalLetter = "C";
            letterMessage = "\nBe sure to talk with the accused and witness\nto make sure your decision was informed.";
        }
        else if (approvalRating < 90.0f)
        {
            approvalLetter = "B";
        }
        else if (approvalRating <= 93.0f)
        {
            approvalLetter = "A";
        }
        else if (approvalRating <= 100.0f)
        {
            approvalLetter = "A+";
        }


        var title = transform.Find("Title");
        var pressToContinue = transform.Find("PressToContinue");
        title.GetComponent<TextMesh>().text = "Week " + weekIndex + "\n";

        pressToContinue.GetComponent<TextMesh>().text = "Approval Rating\n" + approvalRating.ToString("0.#") + "% Grade:" + approvalLetter + letterMessage + "\n\n" + "Press any key\n to continue";

        // Fade out
        FadeSequence.Sync();

        FadeSequence.Property(
            ffSpriteColor,
            FadeScreenColorSave,
            FFEase.E_SmoothStartEnd,
            FadeOutTime);
        
        FadeSequence.Property(
            title.ffTextMeshColor(),
            titleColorSave,
            FFEase.E_SmoothStartEnd,
            FadeOutTime);

        FadeSequence.Property(
            pressToContinue.ffTextMeshColor(),
            pressKeyToContinueSave,
            FFEase.E_SmoothStartEnd,
            FadeOutTime);

        FadeSequence.Sync();
        FadeSequence.Call(WaitForInput);
        FadeSequence.Call(StartNextWeek, weekIndex);
    }

    void StartNextWeek(object week)
    {
        StartWeek sw;
        sw.week = (int)week;
        FFMessage<StartWeek>.SendToLocal(sw);
    }

    // self queuing message
    void WaitForInput()
    {
        var fadeToNextWeek =
            Input.anyKey;

        if (fadeToNextWeek)
        {
            FadeIn();
        }
        else // continue to check for update
        {
            FadeSequence.Sync();
            FadeSequence.Call(WaitForInput);
        }
    }


    void FadeIn()
    {
        var title = transform.Find("Title");
        var pressToContinue = transform.Find("PressToContinue");

        FadeSequence.Property(
            ffSpriteColor,
            FadeScreenColorSave.MakeClear(),
            FFEase.E_SmoothStartEnd,
            FadeInTime);

        FadeSequence.Property(
            title.ffTextMeshColor(),
            titleColorSave.MakeClear(),
            FFEase.E_SmoothStartEnd,
            FadeInTime);

        FadeSequence.Property(
            pressToContinue.ffTextMeshColor(),
            pressKeyToContinueSave.MakeClear(),
            FFEase.E_SmoothStartEnd,
            FadeInTime);

        FadeSequence.Sync();
        FadeSequence.Call(UnlockPlayerController);
    }

    void LockPlayerController()
    {
        GameObject.Find("Player").GetComponent<PlayerController>().active = false;
        GameObject.Find("Camera").GetComponent<PlayerInteractor>().active = false;
    }

    void UnlockPlayerController()
    {
        GameObject.Find("Player").GetComponent<PlayerController>().active = true;
        GameObject.Find("Camera").GetComponent<PlayerInteractor>().active = true;
    }
	
}
