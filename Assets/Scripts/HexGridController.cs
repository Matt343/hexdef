using UnityEngine;
using System.Collections.Generic;

public class HexGridController : MonoBehaviour {
	
	public int width = 10, height = 10;
	public HexController hex;
	
	private HexController[,] grid;
	private Vector3 hexSize;
	List<Spawner> spawners = new List<Spawner>();
	
	private Vector2 animationPos = Vector2.zero;
	private Vector2 redPos = Vector2.zero;
	private Vector2 bluePos = Vector2.zero;
	private Vector2 greenPos = Vector2.zero;
	
	public float animationScale = .1f;
	public float colorScale = .1f;
	
	private float perlinAmount = 0;
	
	public bool animating = true;
	
	public float backOffset = .75f;
	
	// Use this for initialization
	void Start () {
		
	}
	
	public static Vector3 getPositionFromGrid(int x, int y, Vector3 hexSize)
	{
		return new Vector3(y * hexSize.z * .75f, 0, x * hexSize.x * .875f + ((y % 2) * hexSize.x * .45f));
	}
	// Update is called once per frame
	void Update () {
		if(perlinAmount < 1)
			perlinAmount = Mathf.Min(1, perlinAmount + Time.deltaTime * .5f);
		if(animating)
			animatePerlin(true, false, new Vector2(.5f, .5f), new Vector2(-.25f, -.25f), new Vector2(-.25f, -.25f), new Vector2(-.25f, -.25f));
	}
	
	public void setUpGrid(int w, int h)
	{
		width = w;
		height = h;
		
		grid = new HexController[height, width];
		hexSize = hex.size;
		
		Vector3 gridSize = getPositionFromGrid(width - 1, height - 1, hexSize);
		//initiallize hex grid
		for(int y = 0; y < height; y++)
		{
			for(int x = 0; x < width; x++)
			{
				//calculate position on grid
				Vector3 position = getPositionFromGrid(x, y, hexSize);
				position += transform.position - gridSize / 2;
				GameObject newHex = Instantiate(hex.gameObject, position, Quaternion.Euler(new Vector3(270, 30, 0))) as GameObject;
				newHex.transform.parent = transform;
				
				grid[y,x] = newHex.GetComponent(typeof(HexController)) as HexController;
				grid[y,x].x = x;
				grid[y,x].y = y;
				grid[y,x].grid = this;
				grid[y,x].locked = false;
				grid[y,x].defaultHeight = newHex.transform.position.y;
				
			}
		}
	}
	
	public void clearGrid()
	{
		if(grid != null)
		{
			for(int y = 0; y < height; y++)
				for(int x = 0; x < width; x++)
					if(grid[y,x] != null)
						GameObject.Destroy(grid[y,x].gameObject);
			width = 0;
			height = 0;
			grid = new HexController[0,0];
			while(spawners.Count > 0)
			{
				GameObject.Destroy(spawners[spawners.Count - 1].gameObject);
				spawners.RemoveAt(spawners.Count - 1);
			}
		}
	}
	
	public void setGridColors(int[,] colorIndices, Color[] colors)
	{
		for(int y = 0; y < height; y++)
		{
			for(int x = 0; x < width; x++)
			{
				grid[y,x].renderer.materials[1].color = colors[colorIndices[y,x]];
			}
		}
	}
	
	public void startPerlin()
	{
		
	}
	
	void animatePerlin(bool animation, bool color, Vector2 animationDir, Vector2 redDir, Vector2 greenDir, Vector2 blueDir)
	{
		animationPos += animationDir * Time.deltaTime;
		redPos += redDir * Time.deltaTime;
		greenPos += greenDir * Time.deltaTime;
		bluePos += blueDir * Time.deltaTime;
		
		if(animation || color)
		{
			for(int y = 0; y < height; y++)
			{
				for(int x = 0; x < width; x++)
				{
					if(animation)
					{
						float perlin = (Mathf.PerlinNoise((x * animationScale) + animationPos.x, (y * animationScale) + animationPos.y) - .5f);
						perlin += Mathf.Lerp(backOffset, 0, y / height);
						grid[y,x].startMoving(Mathf.Lerp(0, perlin, perlinAmount), 2, false, true);
					}
					if(color)
						grid[y,x].renderer.materials[1].color = perlinColor(x, y, grid[y,x].renderer.materials[1].color);
						
				}
			}
		}
	}
	
	Color perlinColor(int x, int y, Color current)
	{
		float red = Mathf.PerlinNoise((x * colorScale) + redPos.x, ((y + height) * colorScale) + redPos.y);
		float green =  Mathf.PerlinNoise((x * colorScale) + greenPos.x, ((y + height * 2) * colorScale) + greenPos.y);
		float blue = Mathf.PerlinNoise((x * colorScale) + bluePos.x, ((y + height * 3) * colorScale) + bluePos.y);
		return new Color(Mathf.Lerp(current.r, red, perlinAmount), Mathf.Lerp(current.g, green, perlinAmount), Mathf.Lerp(current.b, blue, perlinAmount), 1);
	}
	
	public HexController getHex(Vector2 position)
	{
		return grid[(int)position.y,(int)position.x];
	}
	
	public void loadLevel(LevelAsset level, Spawner start)
	{
		clearGrid();
		setUpGrid(level.gridWidth, level.gridHeight);
		
		for(int y = 0; y < height; y++)
		{
			for(int x = 0; x < width; x++)
			{
				grid[y,x].setHeight(level.grid[y][x].height);
				grid[y,x].locked = level.grid[y][x].path;
				grid[y,x].path = level.grid[y][x].path;
				grid[y,x].pathId = level.grid[y][x].pathId;
				grid[y,x].setColor(level.colors[level.grid[y][x].color]);
				if(level.grid[y][x].start)
				{
					GameObject newSpawn = Instantiate(start.gameObject, grid[y,x].transform.position + grid[y,x].spawnOffset, Quaternion.identity) as GameObject;
					Spawner spawn = newSpawn.GetComponent<Spawner>();
					spawn.startNode = grid[y,x];
					spawn.setWave(0);
					
					spawners.Add(spawn);
					
				}
				grid[y,x].nextNodes = new List<HexController>();
				
				for(int i = 0; i < 6; i++)
				{
					bool active = ((level.grid[y][x].nextPositions & (1 << i)) != 0);
					if(active)
					{
						Vector2 pathPoint = (y % 2 == 0) ? HexInfo.pathPointsEven[i] : HexInfo.pathPointsOdd[i];
						
						Vector2 next = level.grid[y][x].position + pathPoint;
						if(next.x >=0 && next.y >=0 && next.x < width && next.y < height)
						{	
							grid[y,x].nextNodes.Add(grid[(int)next.y, (int)next.x]);
						}
					}
				}
			}
		}
	}
	
}
