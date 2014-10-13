using UnityEngine;
using System.Collections.Generic;

public class PulseTower : TowerController {
	public GameObject pulse;
	public Light pulseLight;
	public Light pulseHalo;
	public float pulseLength = 1;
	float pulseTime = 0;
	bool pulsing = false;
	Material pulseMat;
	public MobPulse mobPulse;
	public float secondaryChance = .5f;
	
	// Use this for initialization
	void Start () {
		pulseMat = pulse.renderer.material;
		fireTimer = fireDelay;
		applyUpgrade(0);
	}
	
	// Update is called once per frame
	void Update () {
		if(pulsing)
		{
			pulseTime += Time.deltaTime;
			if(pulseTime >= pulseLength)
			{
				pulseTime = 0;
				pulsing = false;
				pulse.transform.localScale = Vector3.zero;
				pulseMat.SetFloat("_Strength", 9);
			}
			else
			{
				float amount = pulseTime / pulseLength;
				float amountScaled = amount * .7854f + .7854f;
				pulse.transform.localScale = Vector3.one * 1.5f + Vector3.one * ((Mathf.Sin(amountScaled) - .7071f) * range) / 0.29289f;
				pulseMat.SetFloat("_Strength", 10f - amount * 9);
				pulseLight.intensity = 5 - amount * 5;
				//float amount = (pulseTime / pulseLength) * 10 + .25f;
				//pulse.transform.localScale = Vector3.one * ((Mathf.Log(amount) + 1.39f) / 3.7f * range);
			}
			
		}
		else
		{
			
			fireTimer += Time.deltaTime;
			pulse.transform.localScale = Vector3.one * 1.5f * (fireTimer / fireDelay);
			if(fireTimer >= fireDelay)
			{
				fireTimer = 0;
				Collider[] colliders = Physics.OverlapSphere(transform.position, range);
				bool inRange = false;
			
				foreach(Collider c in colliders)
				{
					PathFollow mob = c.gameObject.GetComponent<PathFollow>();
					if(mob != null)
					{
						inRange = true;
						if(mob.takeDamage(damage) && upgradeLevel == 3 && Random.value <= secondaryChance)
						{
							Instantiate(mobPulse.gameObject, c.transform.position, Quaternion.identity);
						}
						
					}
				}
				if(inRange)
					fire();
			}
		}
	}
	
	public override void fire()
	{
		pulsing = true;
		pulseTime = 0;
		audio.Play();
		
	}
	
	public override void setColor(Color color)
	{
		renderer.materials[1].color = color;
		pulse.renderer.material.color = color;
		pulseLight.color = color;
		pulseHalo.color = color;
		if(hex != null)
			hex.setColor (color);
	}
}
