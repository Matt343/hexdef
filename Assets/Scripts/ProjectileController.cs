using UnityEngine;
using System.Collections;

public class ProjectileController : MonoBehaviour {
	public Transform target;
	public float speed = 1;
	public float damage = 1;
	public float life = 3;
	public float drain = 0;
	public float drainLength = 0;
	Vector3 direction;
	// Update is called once per frame
	void Update () {
		life -= Time.deltaTime;
		if(life <=0)
			GameObject.Destroy(gameObject);
		
		if(target != null)
		{
			direction = target.position - transform.position;
			transform.LookAt(target.position);
		}
		
		transform.position += direction.normalized * speed * Time.deltaTime;
			
		
	}
	
	void OnTriggerEnter(Collider collision)
	{
		PathFollow mob = collision.gameObject.GetComponent<PathFollow>();
		if(mob != null)
		{
			mob.takeDamage(damage);
			if(drain != 0)
				mob.applyEffect(drain, drainLength);
			GameObject.Destroy(gameObject);
		}
			
	}
	public void setColor(Color color)
	{
		renderer.material.color = color;
	}


}
