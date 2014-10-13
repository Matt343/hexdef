using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class HexInfo{
	public int color;
	public bool path;
	public bool start;
	public bool end;
	public float height;
	public int pathId;
	public Vector2 position;
	//[SerializeField]
	//public List<Vector2> nextPositions = new List<Vector2>();
	public byte nextPositions = 0;
	
	public HexInfo(Vector2 pos)
	{
		position = pos;
	}
	
	public HexInfo(int x, int y)
	{
		position = new Vector2(x,y);
	}
	
	public static Vector2[] pathPointsEven = new Vector2[] {
		new Vector2(0, -1),
		new Vector2(1, 0),
		new Vector2(0, 1),
		new Vector2(-1, 1),
		new Vector2(-1, 0),
		new Vector2(-1, -1),
	};
	
	public static Vector2[] pathPointsOdd = new Vector2[] {
		new Vector2(1, -1),
		new Vector2(1, 0),
		new Vector2(1, 1),
		new Vector2(0, 1),
		new Vector2(-1, 0),
		new Vector2(0, -1),
	};
	
}
