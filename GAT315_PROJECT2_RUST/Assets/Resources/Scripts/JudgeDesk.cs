using System.Collections;
using System.Collections.Generic;
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
	
	void SetupDesk(Crime crime)
	{
		var charges = ChargesRoot();
		var accused = AccusedRoot();
		var sentence = SentenceRoot();
		var gavel = GavelRoot();
		
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
			string str = "";
			foreach(var notes in crime.chargesNotes)
			{
				str += notes;
				str += "\n";
			}
			additionalNotes.text = str;
			
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
