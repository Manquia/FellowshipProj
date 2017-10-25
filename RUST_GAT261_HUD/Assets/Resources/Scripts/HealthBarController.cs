using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarController : MonoBehaviour
{
    public Health health;

    public int shellLevel = 1;
    public float shellIncrementValue = 25.0f;
    Vector3 maxScale; 
    SpriteRenderer[] sprites;
    
    public Color shellColor             = Color.white;
    public Color barColor               = Color.white;
    public Color shellInvulnerableColor = Color.white;
    public Color barInvulnerableColor   = Color.white;

    // Use this for initialization
    void Start ()
    {
        sprites = new SpriteRenderer[2];

        sprites[0] = transform.Find("Bar").GetComponent<SpriteRenderer>();
        sprites[1] = transform.Find("Shell").GetComponent<SpriteRenderer>();

        maxScale = transform.localScale;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(shellLevel * shellIncrementValue + 0.5f < (health.current))
        {
            float appearsAt = (shellLevel * shellIncrementValue);
            float expansion = Mathf.Max((shellLevel * shellIncrementValue), shellIncrementValue);
            
            foreach (var sprite in sprites) sprite.gameObject.SetActive(true);

            float hpOnBar = health.current - appearsAt;
            float shellFraction = Mathf.Clamp(0.165f * shellLevel, 0.25f, 0.55f);
            float scaleX = (hpOnBar / (expansion + hpOnBar)) + shellFraction;
            
            transform.localScale = new Vector3(scaleX * maxScale.x, maxScale.y, maxScale.z);

            if (health.invulnerable)
            {
                sprites[0].color = barInvulnerableColor;
                sprites[1].color = shellInvulnerableColor;
            }
            else
            {
                sprites[0].color = barColor;
                sprites[1].color = shellColor;
            }
        }
        else
        {
            foreach (var sprite in sprites) sprite.gameObject.SetActive(false);
        }
		
	}
}
