using UnityEngine;
using System.Collections.Generic;

public class LaserTower : TowerController {
	
	Vector3 laserPosition;
	Vector3 laserVelocity;
	public float laserSpeed = 1;
	public float laserLength = 2;
	public float laserSplash = 1;
	float laserTime = 0;
	bool firing = false;
	
	public LineRenderer laser;
	public Vector3 laserOffset;
	public Light laserPointLight;
	public Light glowLight;

	public LaserTrail trail;
	public float trailSpacing;
	Vector3 trailLast;
	
	List<PathFollow> damaged;
	
	// Use this for initialization
	void Start () {
		fireTimer = fireDelay;
		applyUpgrade(0);
	}
	
	// Update is called once per frame
	void Update () {
		//if(target != null)
		//	aimTowards(target.transform.position);
		
		//staticAnimation();
		
		Vector3 targetVelocity = Vector3.zero;
		//try to get a target if we are on cooldown
		if(!firing)
		{
			Collider[] inRange = Physics.OverlapSphere(transform.position, range);
			
			int maxPath = 0;
			//target = null;
			foreach(Collider c in inRange)
			{
				PathFollow mob = c.gameObject.GetComponent<PathFollow>();
				if(mob != null && mob.nextNode.pathId > maxPath)
				{
					target = c.transform;
					maxPath = mob.nextNode.pathId;	
					targetVelocity = mob.velocity;
				}
			}
			
			fireTimer += Time.deltaTime;
			if(fireTimer >= fireDelay && target != null)
			{
				fireTimer = 0;
				firing = true;
				laserTime = 0;
				laserPosition = target.position;
				laserVelocity = -targetVelocity.normalized;
				damaged = new List<PathFollow>();
				laserPointLight.enabled = true;
				audio.Play();
			}
			laser.SetVertexCount(0);
			
		}
		
		if(firing)
		{
			laserTime += Time.deltaTime;
			if(laserTime > laserLength)
			{
				firing = false;
				laserPointLight.enabled = false;
			}
			laserPosition += laserVelocity * laserSpeed * Time.deltaTime;
			fire ();
			laser.SetVertexCount(2);
			Vector3 dir = transform.position - laserPosition;
			dir.y = 0;
			laser.SetPosition(0, transform.position + laserOffset);
			laser.SetPosition(1, laserPosition + new Vector3(0, .5f, 0));
			
			laserPointLight.transform.position = laserPosition;
			laserPointLight.range = laserSplash * 1.5f;
			Collider[] hit = Physics.OverlapSphere(laserPosition, laserSplash);
			foreach(Collider c in hit)
			{
				PathFollow mob = c.gameObject.GetComponent<PathFollow>();
				if(mob != null && !damaged.Contains(mob))
				{
					mob.takeDamage(damage);
					damaged.Add(mob);
				
				}
			}
			
			if(upgradeLevel == 3 && (laserPosition - trailLast).sqrMagnitude >= trailSpacing)
			{
				Instantiate(trail, laserPosition, Quaternion.identity);
				trailLast = laserPosition;
			}
		}
	}
	
	void LateUpdate()
	{
		//if(target != null && firing)
			aimTowards(laserPosition);
	}
	
	public override void aimTowards(Vector3 target)
	{
		
		target.y = aimBone.transform.position.y;
		
		Vector3 direction = target - aimBone.transform.position;
		direction.y = 0;
		if(direction.Equals(Vector3.zero))
			direction = aimBone.transform.forward;
		
		float angle = Vector3.Angle(-Vector3.right, direction);
		Quaternion newRotation = Quaternion.AngleAxis(angle * AngleDir(-Vector3.right, direction, aimBone.transform.up), Vector3.up);
		aimBone.rotation = newRotation;

		
	}
	
	public override void setColor(Color color)
	{
		render.materials[1].color = color;
		laser.SetColors(color, color);
		laserPointLight.color = color;
		glowLight.color = color;
		if(hex != null)
			hex.setColor(color);
	}
	
	public override void fire()
	{
		
	}
}
