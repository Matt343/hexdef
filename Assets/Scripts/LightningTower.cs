using UnityEngine;
using System.Collections;

public class LightningTower : TowerController {
	
	public LineRenderer lightning;
	public LineRenderer staticLine;
	public Transform staticTransform;
	public Light glowLight;
	public float glow = 1;
	public Vector3 lightningOffset = Vector3.up;
	public int lightningNodes = 5;
	public int staticNodes = 10;
	public float lightningJitter = 1;
	public float updateDelay = 1;
	public float staticRadius = 1;
	public Texture tex;
	public float arcRange = 2;
	
	float updateTimer = 0;
	Vector3[] currentPositions;
	Vector3[] nextPositions;
	PathFollow targetMob;
	PathFollow arcMob;
	public Animator anim;
	
	int totalNodes;
	
	void Start() 
	{
		applyUpgrade(0);
		totalNodes = lightningNodes * 2 + staticNodes;
		staticLine.SetVertexCount(staticNodes + 1);
		
		currentPositions = new Vector3[totalNodes];
		nextPositions = new Vector3[totalNodes];
		for(int i = 0; i < totalNodes; i++)
		{
			currentPositions[i] = Random.onUnitSphere * lightningJitter;
			nextPositions[i] = Random.onUnitSphere * lightningJitter;
		}
		
		lightning.material = new Material(Shader.Find("Particles/Additive"));
		lightning.material.mainTexture = tex;
		staticLine.material = new Material(Shader.Find("Particles/Additive"));
		staticLine.material.mainTexture = tex;
	}
	
	// Update is called once per frame
	void Update () {
		
		updateTimer += Time.deltaTime;
		
		if(updateTimer >= updateDelay)
		{
			currentPositions = nextPositions;
			nextPositions = new Vector3[totalNodes];
			updateTimer = 0;
			for(int i = 0; i < totalNodes; i++)
			{
				nextPositions[i] = Random.onUnitSphere * lightningJitter;
			}
		}
		
		
		
		Collider[] inRange = Physics.OverlapSphere(transform.position, range);
		
		int maxPath = 0;
		target = null;
		foreach(Collider c in inRange)
		{
			PathFollow mob = c.gameObject.GetComponent<PathFollow>();
			if(mob != null && mob.nextNode.pathId > maxPath)
			{
				target = c.transform;
				maxPath = mob.nextNode.pathId;	
				targetMob = mob;
			}
		}
		
		if(target != null)
		{
			if(upgradeLevel == 3)
			{
				Collider[] arcMobs = Physics.OverlapSphere(target.position, arcRange);
				maxPath = 0;
				arcMob = null;
				foreach(Collider c in arcMobs)
				{
					PathFollow mob = c.gameObject.GetComponent<PathFollow>();
					if(mob != null && mob.nextNode.pathId > maxPath && mob != targetMob)
					{
						maxPath = mob.nextNode.pathId;	
						arcMob = mob;
					}
				}
				
				
			}
			fire();
			glowLight.intensity = glow;
		}
		else
		{
			glowLight.intensity = 0;
			lightning.SetVertexCount(0);
		}
		
		staticAnimation();
		
	}
	
	public override void staticAnimation ()
	{
		for(int i = 0; i < staticNodes; i++)
		{
			Vector3 circlePosition = Quaternion.AngleAxis(360 / (staticNodes) * i, Vector3.up) * Vector3.forward * staticRadius;
			Vector3 jitteredPosition = circlePosition + Vector3.Lerp(currentPositions[i + lightningNodes * 2], nextPositions[i + lightningNodes * 2], updateTimer / updateDelay) / 10;
			staticLine.SetPosition(i, jitteredPosition);
			
			if(i == 0)
				staticLine.SetPosition(staticNodes, jitteredPosition);
		}
		
		staticTransform.RotateAround(Vector3.up, Time.deltaTime);
	}
	
	public override void fire()
	{
		int vertexCount = upgradeLevel != 3 || arcMob == null ? lightningNodes + 2 : lightningNodes * 2 + 3;
		
		
		targetMob.takeDamage(damage * Time.deltaTime);
		lightning.SetVertexCount(vertexCount);
		
		Vector3 start = transform.position + lightningOffset;
		lightning.SetPosition(0, start);
		drawLightning(start, targetMob.transform.position, 1, lightningNodes + 1);
		lightning.SetPosition(lightningNodes + 1, targetMob.transform.position);
		
		if(upgradeLevel == 3 && arcMob != null)
		{
			arcMob.takeDamage(damage * .5f * Time.deltaTime);
			drawLightning(targetMob.transform.position, arcMob.transform.position, lightningNodes + 2, 2 * lightningNodes + 2);
			lightning.SetPosition(2 * lightningNodes + 2, arcMob.transform.position);
		}
		
	}
	
	void drawLightning(Vector3 start, Vector3 end, int nodeStart, int nodeEnd)
	{
		
		
		Vector3 direction = end - start;
		float distance = direction.magnitude / (nodeEnd - nodeStart + 2);
		direction = direction.normalized;
		
		for(int i = nodeStart; i < nodeEnd; i++)
		{
			lightning.SetPosition(i, start + direction * distance * (i - nodeStart + 1)
				+ Vector3.Lerp(currentPositions[i - 1], nextPositions[i - 1], updateTimer / updateDelay) * distance / 4);
		}
		glowLight.transform.position = start + direction * distance * lightningNodes / 2;
	}
	
	
	
	public override void setColor(Color color)
	{
		lightning.SetColors(color, color);
		staticLine.SetColors(color, color);
		glowLight.color = color;
		render.materials[0].color = color;
		if(hex != null)
			hex.setColor (color);
	}
}
