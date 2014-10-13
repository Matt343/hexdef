using UnityEngine;
using System.Collections.Generic;



	
[System.Serializable]
public class LevelAsset : ScriptableObject {
	public int gridWidth;
	public int gridHeight;
	public Color[] colors;
	
	public List<GridRow> grid = new List<GridRow>();
	
}





