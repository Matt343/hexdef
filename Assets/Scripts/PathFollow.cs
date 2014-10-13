using UnityEngine;
using System.Collections.Generic;

public class PathFollow : MonoBehaviour {
	
	public GameManager manager;
	public HexController nextNode;
	public float nodeRange = 5;
	public float speed = 3;
	public float maxHealth = 2;
	public ParticleSystem deathParticles;
	public Vector3 velocity;
	public float energy = 1;
	public int reward = 1;
	float health;
	List<Vector3> effects = new List<Vector3>();
	
	//public Vector3 pathOffset = Vector3.zero;
	// Use this for initialization
	void Awake () {
		manager = FindObjectOfType(typeof(GameManager)) as GameManager;
		health = maxHealth;
		renderer.material.color = new Color(.9f, .05f, 0, 1);
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 target = (nextNode.transform.position + nextNode.spawnOffset) - transform.position;
		if(!target.Equals(Vector3.zero) && energy > 0)
			transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(target, Vector3.up), 5 * energy);
		//move to the next node when we reach our target
		if(target.sqrMagnitude < nodeRange)
		{
			//reached the end of the path
			if(nextNode.nextNodes.Count <= 0)
			{
				manager.health -= (int)maxHealth;
				GameObject.Destroy(gameObject);	
			}
			else
			{
				nextNode = nextNode.getNextNode();
				target = (nextNode.transform.position + nextNode.spawnOffset) - transform.position;
			}
		}
		
		velocity = target.normalized * speed  * Time.deltaTime;
		transform.position += velocity * energy;
		
		//update status effects
		energy = 1;
		for(int i = 0; i < effects.Count; i++)
		{
			effects[i] += Vector3.forward * Time.deltaTime;
			if(effects[i].z >= effects[i].y)
			{
				effects.RemoveAt(i);
				i--;
			}
			else
				energy *= effects[i].x;
		}
				
		
		float ratio = health / maxHealth;
		renderer.material.color = new Color(.9f, Mathf.Lerp(.25f, .05f, ratio), 0, energy);
	}
	
	public bool takeDamage(float damage)
	{
		health -= damage;
		if(health <= 0)
		{
			manager.money += reward;
			manager.score += reward;
			Instantiate(deathParticles, transform.position, Quaternion.identity);
			gameObject.SetActive(false);
			GameObject.Destroy(gameObject);
			return true;
		}
		return false;
		
	}
	
	public void applyEffect(float amount, float duration)
	{
		effects.Add(new Vector3(amount, duration, 0));
	}
}
