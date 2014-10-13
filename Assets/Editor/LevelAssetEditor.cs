using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(LevelAsset))]
public class LevelAssetEditor : UnityEditor.Editor {
	GUISkin skin;
	bool showGrid = true, showHex = true, showPathPreset = true, showBasePreset = true;
	Vector2 gridSelection = new Vector2(-1, -1);
	
	float pathHeight = 1, baseHeight = 0;
	int pathColor = 1, baseColor = 0;
	
	Texture2D hexBackTex;
	Texture2D hexInnerTex;
	Texture2D[] hexSegmentTexs = new Texture2D[6];
	
	Rect gridShowRect;
	Rect gridRect;
	
	int hexSize = 50;
	
	int nextPathPoint = 0;
	
	
	
	[MenuItem("Assets/Create/LevelAsset")]
	public static void CreateAsset ()
	{
		ScriptableObjectUtility.CreateAsset<LevelAsset> ();
	}
	
	public override void OnInspectorGUI() {
		if(skin == null)
			skin = EditorGUIUtility.Load("LevelEditorSkin.guiskin") as GUISkin;
		if(hexBackTex == null)
			hexBackTex = EditorGUIUtility.Load("Textures/hexagon_back.png") as Texture2D;
		if(hexInnerTex == null)
			hexInnerTex = EditorGUIUtility.Load("Textures/hexagon_inner.png") as Texture2D;
		
		for(int i = 0; i < 6; i++)
		{
			if(hexSegmentTexs[i] == null)
				hexSegmentTexs[i] = EditorGUIUtility.Load("Textures/hexagon_" + i + ".png") as Texture2D;
		}
		
		//let the default inspector handle the easy stuff
        DrawDefaultInspector();
		
		LevelAsset level = target as LevelAsset;
		
		//make sure that the grid is the correct size, also initiallizes any nulls
		if(level.grid == null || level.grid.Count != level.gridHeight || (level.grid.Count != 0 && level.grid[0].Count != level.gridWidth))
		{
			level.grid = createGrid(level.gridWidth, level.gridHeight, level.grid);
		}
		
		
		//grid editor
		showGrid = EditorGUILayout.Foldout(showGrid, "Grid");
		if( Event.current.type == EventType.Repaint ) 
			gridShowRect = GUILayoutUtility.GetLastRect();
		
		if(showGrid)
		{
			if(level.grid != null && level.grid.Count == level.gridHeight && level.grid.Count > 0 && level.grid[0].Count == level.gridWidth)
			{
				gridRect = new Rect(gridShowRect.x, gridShowRect.yMax, hexSize * level.gridHeight, hexSize * level.gridWidth);
				gridSelection = hexGrid(gridSelection, level, hexSize, gridRect);
				//GUILayout.Space(level.gridHeight * hexSize);
				
			}
			else
			{
				GUILayout.Label("Grid size changed, recreate grid");
			}
		}
		
		//next path point 
		GUILayout.BeginHorizontal();
		GUILayout.Label("Next Path ID");
		nextPathPoint = EditorGUILayout.IntField(nextPathPoint);
		GUILayout.EndHorizontal();
		
		//grid selection editor, changes HexInfo values for selected cell
		if(gridSelection.x != -1 && level.grid.Count != 0 && level.grid[0].Count != 0)
		{
			showHex = EditorGUILayout.Foldout(showHex, "Hex " + gridSelection.ToString());
			if(showHex && gridSelection.y < level.grid.Count && gridSelection.x < level.grid[0].Count)
			{
				hexEditor(level.grid[(int)gridSelection.y][(int)gridSelection.x]);
			}
		}
		
		//preset menu for base cells
		showBasePreset = EditorGUILayout.Foldout(showBasePreset, "Base Preset");
		if(showBasePreset)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(15);
			GUILayout.BeginVertical();
			GUILayout.Label("Color");
			GUILayout.Label("Height");
			GUILayout.EndVertical();
			
			GUILayout.BeginVertical();
			baseColor = EditorGUILayout.IntField(baseColor);
			baseHeight = EditorGUILayout.FloatField(baseHeight);
			GUILayout.EndVertical();
			
			GUILayout.EndHorizontal();
		}
		
		//preset menu for path cells
		showPathPreset = EditorGUILayout.Foldout(showPathPreset, "Path Preset");
		if(showPathPreset)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(15);
			GUILayout.BeginVertical();
			GUILayout.Label("Color");
			GUILayout.Label("Height");
			GUILayout.EndVertical();
			
			GUILayout.BeginVertical();
			pathColor = EditorGUILayout.IntField(pathColor);
			pathHeight = EditorGUILayout.FloatField(pathHeight);
			GUILayout.EndVertical();
			
			GUILayout.EndHorizontal();
		}
		
		//tells the editor to save the changes to disk
		EditorUtility.SetDirty(level);
		
    }
	
	//custom element to display the hex grid as toggle buttons and return the selection
	Vector2 hexGrid (Vector2 selection, LevelAsset level, int hexSize, Rect gridRect)
	{
		GUILayout.BeginVertical();
		for(int y = 0; y < level.gridHeight; y++)
		{
			
			GUILayout.BeginHorizontal();
			if(y % 2 == 1)
				GUILayout.Space(hexSize / 2);
			for(int x = 0; x < level.gridWidth; x++)
			{
				Rect hexRect = new Rect(gridRect.x + x * hexSize + (y%2 == 1 ? hexSize / 2 : 0), gridRect.y + y * hexSize, hexSize, hexSize);
				if(level.grid[y][x] == null)
					level.grid[y][x] = new HexInfo(x,y);
				if(level.grid[y][x].pathId >= nextPathPoint)
					nextPathPoint = level.grid[y][x].pathId + 1;
				if(hexDisplay((x == selection.x && y == selection.y), level, level.grid[y][x], hexSize, hexRect))
					selection = new Vector2(x,y);
			}
			if(y % 2 == 0)
				GUILayout.Space(hexSize / 2);
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
		return selection;
	}
	
	//custom element to display the hex in the grid
	//provides more buttons for the selected cell
	bool hexDisplay(bool toggle, LevelAsset level, HexInfo hex, int hexSize, Rect hexRect)
	{
		//if not toggled, display empty hex with info
		if(!toggle)
		{
			Color tempColor = GUI.backgroundColor;
			Color color;
			if(level.colors != null && hex.color < level.colors.Length)
				color = level.colors[hex.color];
			else
				color = Color.white;
			color.a = 1;
			GUI.backgroundColor = color;

			string content = "" + hex.pathId;
			
			toggle = GUILayout.Toggle(toggle, content, skin.customStyles[0], GUILayout.Height(hexSize), GUILayout.Width(hexSize)); 
	
			GUI.backgroundColor = tempColor;
		}
		//if it is toggled, display the more complex button
		else
		{
			GUILayout.Space(hexSize);
			//path gui contains the toggle in the center and toggles for next positions on each edge
			if(hex.path)
			{
				Color tempColor = GUI.color;
				Color color;
				if(level.colors != null && hex.color < level.colors.Length)
				color = level.colors[hex.color];
				else
					color = Color.white;
				color.a = 1;
				GUI.color = color;
				
				GUI.DrawTexture(hexRect, hexBackTex);
				//bool result = GUI.Button(hexRect, hexBack, skin.customStyles[0]);
				if( alphaButton(hexRect, hexInnerTex))
				{
					setBase(hex);
				}
				
				Vector2[] pathPoints = (((int)hex.position.y) % 2) == 0 ? HexInfo.pathPointsEven : HexInfo.pathPointsOdd;

				//draw each segment button
				for(int i = 0; i < 6; i++)
				{
					bool active = ((hex.nextPositions & (1 << i)) != 0);
					bool result = alphaToggle(active, hexRect, hexSegmentTexs[i]);
					if(active && !result)
					{
						hex.nextPositions = (byte)(hex.nextPositions & (~ (1 <<i)));
					}
					else if(!active && result)
					{
						hex.nextPositions = (byte)(hex.nextPositions | (byte)(1 << i));
						Vector2 next = hex.position + pathPoints[i];
						if(next.x >= 0 && next.y >=0 && next.x < level.gridWidth && next.y < level.gridHeight && !level.grid[(int)next.y][(int)next.x].path)
							setPath(level.grid[(int)next.y][(int)next.x]);
					}
					
				
				}
				
				
				GUI.color = tempColor;
				GUI.Label(hexRect, "" + hex.pathId, skin.label);
			}
			//base gui only contains the toggle in the center
			else
			{
				Color tempColor = GUI.color;
				Color color;
				if(level.colors != null && hex.color < level.colors.Length)
				color = level.colors[hex.color];
				else
					color = Color.white;
				color.a = 1;
				GUI.color = color;
				
				GUI.DrawTexture(hexRect, hexBackTex);
				//bool result = GUI.Button(hexRect, hexBack, skin.customStyles[0]);
				if( alphaButton(hexRect, hexInnerTex))
				{
					setPath(hex);
				}
				
				GUI.color = tempColor;
				GUI.Label(hexRect, "" + hex.pathId, skin.label);
				
			}
		}
		return toggle;
	}
	
	public bool alphaButton(Rect rect, Texture2D image)
	{
	    Vector2 relativeClick;	    
		bool result = false;
	    if( Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition) )
	    {
			
	        //relativeClick = new Vector2( Event.current.mousePosition.x - backRect.x, Event.current.mousePosition.y - backRect.y );
			relativeClick = new Vector2(Event.current.mousePosition.x - rect.x, (rect.y + rect.height) - Event.current.mousePosition.y);
			relativeClick *= image.width / rect.width;
			result = (image.GetPixel((int)relativeClick.x, (int)relativeClick.y).a > .5f);
			
			//Debug.Log(relativeClick);
	        
	        
	    }
    
		GUI.DrawTexture(rect, image);
	    return result;
	}
	
	public bool alphaToggle(bool toggle, Rect rect, Texture2D image)
	{
		Color oldColor = GUI.color;
		if(toggle)
		{
			Color newColor = new Color(oldColor.r - .2f, oldColor.g - .2f, oldColor.b - .2f, oldColor.a);
			GUI.color = newColor;
		}
		if(alphaButton(rect, image))
			toggle = !toggle;
		GUI.color = oldColor;
		return toggle;
	}
		
	//custom editor for HexInfo values
	void hexEditor(HexInfo hex)
	{
		if(hex != null)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(15);
			GUILayout.BeginVertical();
			GUILayout.Label("Color");
			GUILayout.Label("Height");
			GUILayout.Label("Path");
			GUILayout.Label("Path ID");
			GUILayout.Label("Start");
			GUILayout.Label("End");
			GUILayout.EndVertical();
			
			GUILayout.BeginVertical();
			hex.color = EditorGUILayout.IntField(hex.color);
			hex.height = EditorGUILayout.FloatField(hex.height);
			hex.path = EditorGUILayout.Toggle(hex.path);
			hex.pathId = EditorGUILayout.IntField(hex.pathId);
			hex.start = EditorGUILayout.Toggle(hex.start);
			hex.end = EditorGUILayout.Toggle(hex.end);
			GUILayout.EndVertical();
			
			GUILayout.EndHorizontal();
		}
	}
	
	//updates the grid to reflect changes in size, fills in new spaces 
	List<GridRow> createGrid(int w, int h, List<GridRow> oldGrid)
	{
		List<GridRow> newGrid = new List<GridRow>();
		
		for(int y = 0; y < h; y++)
		{
			//newGrid.Add(CreateInstance<GridRow>());
			newGrid.Add(new GridRow());
			for(int x = 0; x < w; x++)
			{
				if(oldGrid != null && y < oldGrid.Count && x < oldGrid[y].Count)
					newGrid[y].Add(oldGrid[y][x]);
				else
					//newGrid[y].Add(CreateInstance<HexInfo>());
					newGrid[y].Add(new HexInfo(x,y));
			}
		}
	
		return newGrid;
		
	}
	
	void setBase(HexInfo hex)
	{
		hex.color = hex.path ? baseColor : pathColor;
		hex.height = hex.path ? baseHeight : pathHeight;
		hex.path = !hex.path;
		hex.pathId = 0;
		nextPathPoint--;
		hex.nextPositions = 0; //new List<Vector2>();
	}
	
	void setPath(HexInfo hex)
	{
		hex.color = hex.path ? baseColor : pathColor;
		hex.height = hex.path ? baseHeight : pathHeight;
		hex.path = !hex.path;
		hex.pathId = nextPathPoint;
		nextPathPoint++;
	}
}
