using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class JudgeDesk : MonoBehaviour {
	
	
	Transform ChargesRoot()
	{
		return transform.Find("Charges");
	}
	
	Transform AccusedRoot()
	{
		return transform.Find("Accused");
	}
	Transform SentenceRoot()
	{
		return transform.Find("Sentence");
	}
	Transform GavelRoot()
	{
		return transform.Find("Gavel");
	}
	
	public void SetupDesk(Character character)
	{
        Crime crime = character.crime;

        var charges = ChargesRoot(); Debug.Assert(charges != null);
		var accused = AccusedRoot(); Debug.Assert(accused != null);
        var sentence = SentenceRoot(); Debug.Assert(sentence != null);
        var gavel = GavelRoot(); Debug.Assert(gavel != null);

        // Add changes
        {
			var description = charges.Find("Description").GetComponent<UnityEngine.UI.Text>();
			string str = "";
			foreach(var charge in crime.charges)
			{
				str += charge;
				str += "\n";
			}
			description.text = str; 
		}
		// Add changes Notes
		{
			var additionalNotes = charges.Find("Additional Notes").GetComponent<UnityEngine.UI.Text>(); 
			additionalNotes.text = crime.chargesNotes;
		}

        // Add Accused mugshot
        {
            var mugshot = accused.Find("Mugshot");
            var mugshotImage = mugshot.GetComponent<UnityEngine.UI.Image>();
            mugshotImage.sprite = character.mugshot;
        }
        // Add Accussed name
        {
            var name = accused.Find("Detail_Name_Age");
            var nameText  = name.GetComponent<UnityEngine.UI.Text>();
            var givenNames = crime.characterNameAge.Split(' ').ToList();

            var str = "";
            foreach(var part in givenNames)
            {
                str += part;
                str += "\n";
            }
            nameText.text = str;
        }
        // Accused Notes
        {
            var additionalNotes = accused.Find("Additional Notes");
            var notesText = additionalNotes.GetComponent<UnityEngine.UI.Text>();
            notesText.text = crime.Investigation;
        }

        // Sentences
        {
            var sent1 = sentence.Find("SentenceOption1");
            var sent2 = sentence.Find("SentenceOption2");
            var sent3 = sentence.Find("SentenceOption3");

            sent1.GetComponent<SentenceController>().SetupSentence(crime.sent1, character.details.name);
            sent2.GetComponent<SentenceController>().SetupSentence(crime.sent2, character.details.name);
            sent3.GetComponent<SentenceController>().SetupSentence(crime.sent3, character.details.name);
        }
		
		
	}
	
	

	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}
}
