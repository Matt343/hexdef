using UnityEngine;
using System.Collections;

public class TowerController : MonoBehaviour {
	public int type;
	public HexController hex;
	public Transform aimBone;
	public Transform barrelBone;
	public float staticSpeed = 10;
	public float range = 10;
	public float fireDelay = 1;
	public float fireTimer = 0;
	public float damage = 1;
	public ProjectileController projectile;
	public Vector3 projectileOffset = new Vector3(0, 1.25f, 0);
	public Transform target;
	public float maxAngle = 5;
	public Renderer render;
	Color color;
	
	
	
	public UpgradeInfo[] upgrades;
	public int upgradeLevel = 0;
	
	
	// Use this for initialization
	void Start () {
		//render = GetComponentInChildren<SkinnedMeshRenderer>();
		fireTimer = fireDelay;
		applyUpgrade(0);
	}
	
	// Update is called once per frame
	void Update () {
		
		if(target != null)
			aimTowards(target.transform.position);
		
		staticAnimation();
		
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
			}
		}
		
		fireTimer += Time.deltaTime;
		if(fireTimer >= fireDelay && target != null)
		{
			aimTowards(target.transform.position);
			fire();
		}
		
	}
	
	public virtual void staticAnimation()
	{
		barrelBone.RotateAround(barrelBone.transform.right, Time.deltaTime * staticSpeed);//localRotation = aimBone.localRotation * Quaternion.Euler(new Vector3(Time.deltaTime * staticSpeed,0,0));
		//barrelBone.localRotation = Quaternion.AngleAxis(Time.time * staticSpeed, Vector3.right);
	}
	
	public virtual void fire()
	{
		fireTimer = 0;
		GameObject projObject = Instantiate(projectile.gameObject, transform.position + projectileOffset, Quaternion.identity) as GameObject;
		ProjectileController proj = projObject.GetComponent<ProjectileController>();
		proj.target = target;
		proj.damage = damage;
		proj.setColor(color);
		if(upgradeLevel == 3)
		{
			proj.drain = .75f;
			proj.drainLength = 1;
		}
		audio.Play();
	}
	
	public virtual void aimTowards(Vector3 target)
	{
		
		target.y = aimBone.transform.position.y;
		
		Vector3 direction = target - aimBone.transform.position;
		direction.y = 0;
		if(direction.Equals(Vector3.zero))
			direction = aimBone.transform.forward;
		
		float angle = Vector3.Angle(-Vector3.forward, direction);
		Quaternion newRotation = Quaternion.AngleAxis(angle * AngleDir(-Vector3.forward, direction, aimBone.transform.up), Vector3.right);
		aimBone.localRotation = Quaternion.RotateTowards(aimBone.localRotation, newRotation, maxAngle);
			

		
	}
	
	public void OnTriggerEnter(Collider collision)
	{
		PathFollow mob = collision.gameObject.GetComponent<PathFollow>();
		if(mob != null)
			target = mob.transform;
		//Debug.Log(collision.ToString());	
	}
	
	public float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up) {
		Vector3 perp = Vector3.Cross(fwd, targetDir);
		float dir = Vector3.Dot(perp, up);
		
		if (dir > 0f) {
			return 1f;
		} else if (dir < 0f) {
			return -1f;
		} else {
			return 0f;
		}
	}
	
	public virtual void setColor(Color color)
	{
		render.materials[1].color = color;
		this.color = color;
		if(hex != null)
			hex.setColor(color);
	}
	
	public virtual void applyUpgrade(int level)
	{
		setColor(upgrades[level].color);
		damage = upgrades[level].damage;
		range = upgrades[level].range;
		fireDelay = upgrades[level].fireDelay;
		upgradeLevel = upgrades[level].level;
	}
	
	
}
[System.Serializable]
public class UpgradeInfo
{
	public int level;
	public Color color;
	public float damage;
	public float range;
	public float fireDelay;
	public string name;
	public string description;
	public int cost;
	
	public UpgradeInfo()
	{	
	}
	
	public UpgradeInfo(int level, Color color, float damage, float range, float fireDelay, string name, string description, int cost)
	{
		this.level = level;
		this.color = color;
		this.damage = damage;
		this.range = range;
		this.fireDelay = fireDelay;
		this.name = name;
		this.description = description;
		this.cost = cost;
	}
}