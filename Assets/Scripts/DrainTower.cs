using UnityEngine;
using System.Collections.Generic;

public class DrainTower : TowerController {
	public float drainLength = 1;
	public float drainStrength = .5f;
	public Renderer distortion;
	Material distortionMat;
	float distortionTime = 0;
	public float distortionStrength = 50;
	bool distort = false;
	List<PathFollow> affectedMobs;
	
	public float[] drainStrengths;
	public float[] drainLengths;
	// Use this for initialization
	void Start () {
		distortionMat = distortion.material;
		applyUpgrade(0);
	}
	
	// Update is called once per frame
	void Update () 
	{	
		//try to get a target if we are on cooldown
		fireTimer += Time.deltaTime;
		if(fireTimer >= fireDelay)
		{
			
			Collider[] inRange = Physics.OverlapSphere(transform.position, range);
		
		
			foreach(Collider c in inRange)
			{
				PathFollow mob = c.gameObject.GetComponent<PathFollow>();
				if(mob != null)
				{
					mob.applyEffect(drainStrength, drainLength);
					mob.takeDamage(damage);
					
					fireTimer = 0;
					distort = true;
					audio.Play();
					
				}
			}
				
		}
		
		if(distort)
		{
			distortion.transform.localScale = new Vector3(range / 4, range / 4, range / 4);
			float dist = 0;
			distortionTime += Time.deltaTime;
			if(distortionTime >= drainLength)
			{
				distortionTime = 0;
				distort = false;
			}
			else
			{
				 dist = distortionTime < drainLength / 2 ? Mathf.Lerp(0, distortionStrength * (1 - drainStrength), distortionTime / (drainLength / 2)) : 
					Mathf.Lerp(distortionStrength* (1 - drainStrength), 0, (distortionTime - drainLength / 2) / (drainLength / 2));
				
			}
			
			distortionMat.SetFloat("_BumpAmt", dist);
		}
		
	}
	
	public override void setColor(Color color)
	{
		render.materials[1].color = color;
		if(hex != null)
			hex.setColor (color);
	}
	
	public override void applyUpgrade (int level)
	{
		base.applyUpgrade (level);
		drainStrength = drainStrengths[level];
		drainLength = drainLengths[level];
	}
}
