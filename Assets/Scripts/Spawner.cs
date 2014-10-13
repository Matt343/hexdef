using UnityEngine;
using System.Collections.Generic;

public class Spawner : MonoBehaviour {
	
	public GameManager manager;
	public HexController startNode;
	public List<GameObject> mobs;
	public float spawnRange = 1;
	public string[] spawnOrder;
	public char[] orderArray;
	private int spawnCount = 0;
	public float[] spawnDelay;
	private float spawnTimer = 0;
	int wave = 0;
	private List<List<GameObject>> mobCache = new List<List<GameObject>>();
	private List<int> mobCounters = new List<int>();
	bool spawning = false; 
	
	bool finished = false;
	
	// Use this for initialization
	void Awake () {
		manager = FindObjectOfType(typeof(GameManager)) as GameManager;
		manager.spawners.Add(this);
		setWave(0);
		finished = false;
	}
	
	public void setWave(int w)
	{
		if(w < spawnOrder.Length)
		{
			wave = w;
			spawnCount = 0;
			//set up mob cache for level
			orderArray = spawnOrder[wave].ToCharArray();
			mobCache = new List<List<GameObject>>();
			mobCounters = new List<int>();
			for(int i = 0; i < mobs.Count; i++)
			{
				mobCache.Add(new List<GameObject>());
				mobCounters.Add(0);
			}
			
			//add a mob for each point in the order
			foreach(var c in orderArray)
			{
				int i = c - 'a';
				if(i >= 0)
				{
					GameObject newMob = (GameObject)Instantiate(mobs[i]);
					(newMob.GetComponent(typeof(PathFollow)) as PathFollow).nextNode = startNode;
					newMob.SetActive(false);
					mobCache[i].Add(newMob);
				}
			}
			finished = false;
		}
		else
		{
			finished = true;
		}
	}
	
	public bool startWave()
	{
		if(wave < spawnOrder.Length)
		{
			spawning = true;
			
		}
		return !finished;
	}
	
	// Update is called once per frame
	void Update () {
		if(manager == null)
		manager = FindObjectOfType(typeof(GameManager)) as GameManager;
		if(spawning)
		{
			spawnTimer += Time.deltaTime;
			
			//spawn next unit in order
			if(spawnTimer > spawnDelay[wave] && spawnCount < orderArray.Length)
			{
				spawnTimer = 0;
				int nextUnit = (orderArray[spawnCount] - 'a');
				if(nextUnit >=0)
				{
					spawnUnit(nextUnit);
				}
				spawnCount++;
			}
			else if(spawnCount >= orderArray.Length)
			{
				spawning = false;
				setWave(wave + 1);
				manager.waveFinished = true;
			}
		}
	}
	
	
	//activate the next mob in the cache for this type
	void spawnUnit(int type)
	{
		//get the next mob
		GameObject newMob = mobCache[type][mobCounters[type]];
		newMob.SetActive(true);
		newMob.transform.position = transform.position;
		
		
		mobCounters[type]++;
	}
}
