using UnityEngine;
using System.Collections;

public class MobPulse : MonoBehaviour {
	public float pulseLength = 1;
	float pulseTime = 0;
	public float range;
	public float damage;
	public float delay;
	float delayTime = 0;
	bool pulsing = false;
	public Color color = Color.yellow;
	public int depth = 0;
	public int maxDepth = 5; 
	public float secondaryChance = .5f;
	
	// Use this for initialization
	void Start () {
		transform.localScale = Vector3.zero;
		renderer.material.color = color;
	}
	
	// Update is called once per frame
	void Update () {
		if(!pulsing)
		{
			delayTime += Time.deltaTime;
			if(delayTime > delay)
			{
				pulsing = true;
				audio.Play();
				
				Collider[] colliders = Physics.OverlapSphere(transform.position, range);
				foreach(Collider c in colliders)
				{
					PathFollow mob = c.gameObject.GetComponent<PathFollow>();
					if(mob != null)
					{
						if(mob.takeDamage(damage) && depth <= maxDepth && Random.value <= secondaryChance)
						{
							GameObject newPulse = Instantiate(gameObject, c.transform.position, Quaternion.identity)as GameObject;
							MobPulse pulse = newPulse.GetComponent<MobPulse>();
							pulse.damage = damage * .5f;
							pulse.range = range * .5f;
							pulse.depth = depth + 1;
						}
					}
				}
				
			}
		}
		if(pulsing)
		{
			pulseTime += Time.deltaTime;
			
			if(pulseTime >= pulseLength)
				GameObject.Destroy(gameObject);
			
			float amount = pulseTime / pulseLength;
			float amountScaled = amount * .7854f + .7854f;
			transform.localScale = Vector3.one * 1.5f + Vector3.one * ((Mathf.Sin(amountScaled) - .7071f) * range) / 0.29289f;
			renderer.material.SetFloat("_Strength", 10f - amount * 9);
			
		}
	}
}
