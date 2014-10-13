using UnityEngine;
using System.Collections.Generic;

public class HexController : MonoBehaviour {
	
	public int x, y;
	public HexGridController grid;
	public bool locked = false;
	public bool path = false;
	public int pathId = 0;
	private bool moving = false;
	public float defaultHeight = 0;
	private float targetHeight = 0;
	private float maxOffset = 1.5f;
	private float speed = 1;
	private bool ret = false;
	public Vector3 size = new Vector3(2,12,2);
	
	public Vector3 spawnOffset = new Vector3(0,3,0);
	public List<HexController> nextNodes;
	
	public TowerController tower;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		//if we are out of position move towards our default height
		if(!locked && !moving && Mathf.Abs(transform.position.y - defaultHeight) > .05f)
		{
			moveTowards(defaultHeight);			
		}
		else if(!locked && moving && Mathf.Abs(transform.position.y - targetHeight) > .05f)
		{
			moveTowards(targetHeight);
		}
		else if(!locked && moving && ret)
		{
			returnToDefault();
		}
	}
	
	void moveTowards(float height)
	{
		int dir = 1;
		if(transform.position.y > height)
			dir = -1;
		Vector3 newPos = transform.position + Vector3.up * dir * speed * Time.deltaTime;
		if(Mathf.Abs(newPos.y - defaultHeight) < maxOffset)
			transform.position = newPos;
		else if(ret)
			returnToDefault();
		
		
	}
	
	public void startMoving(float height, float newSpeed, bool returnWhenDone, bool relative)
	{
		if(!locked)
		{
			speed = newSpeed;
			if(relative)
				targetHeight = defaultHeight + height;
			else
				targetHeight = height;
			moving = true;
			ret = returnWhenDone;
		}
	}
	
	public void returnToDefault()
	{
		moving = false;
		ret = false;
	}
	
	public void setColor(Color color)
	{
		renderer.materials[1].color = color;
	}
	
	public void setHeight(float height)
	{
		defaultHeight = height;
		transform.position = new Vector3(transform.position.x, height, transform.position.z);
	}
	
	public HexController getNextNode()
	{
		return nextNodes[Random.Range(0, nextNodes.Count)];
	}
	
}
