using UnityEngine;
using System.Collections;

public class LaserTrail : MonoBehaviour {
	
	public float duration;
	float timer = 0;
	public float range;
	public float damage;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		timer += Time.deltaTime;
		if(timer >= duration)
			GameObject.Destroy(gameObject);
		
		Collider[] hit = Physics.OverlapSphere(transform.position, range);
		foreach(Collider c in hit)
		{
			PathFollow mob = c.gameObject.GetComponent<PathFollow>();
			if(mob != null)
			{
				mob.takeDamage(damage * Time.deltaTime);
			
			}
		}
	}
}
