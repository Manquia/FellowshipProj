using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {
    
    public int TeamId = 0;
    public string characterName = "Ant";

    public static Dictionary<int, List<Character>> TeamRosters = new Dictionary<int, List<Character>>();

	// Use this for initialization
	void Start ()
    {
        if(TeamRosters.ContainsKey(TeamId) == false) // No lookup for our id
        {
            TeamRosters.Add(TeamId, new List<Character>());
        }
        TeamRosters[TeamId].Add(this);
    }
    void OnDestroy()
    {
        if (TeamRosters.ContainsKey(TeamId))
        {
            TeamRosters[TeamId].Remove(this);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
