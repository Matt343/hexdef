using UnityEditor;
using UnityEngine;
using System.IO;
public class GridEditor : EditorWindow
{
	
	int width, height;
	string newLevelName = "";
	
	Vector3 hexSize;

	// Add menu item named "My Window" to the Window menu
	[MenuItem("Window/Grid Editor")]
	public static void ShowWindow()
	{
		//Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow(typeof(GridEditor));
	}

	void OnGUI()
	{
		
		//string[] filePaths = Directory.GetFiles(@"\Assets\Levels", "*.asset");
		LevelAsset[] levels = AssetDatabase.LoadAllAssetsAtPath("Assets/Levels/*") as LevelAsset[];	
		if(levels != null)
			foreach(LevelAsset level in levels)
				GUILayout.Label(level.name);
		
		//if(filePaths != null)
		//	foreach(string file in filePaths)
		//		GUILayout.Label(file);
		
		GUILayout.BeginHorizontal();
		
		GUILayout.BeginVertical();
		GUILayout.Label("Width:");
		GUILayout.Label("Height:");
		GUILayout.EndVertical();
		
		GUILayout.BeginVertical();
		width = EditorGUILayout.IntField(width);		
		height = EditorGUILayout.IntField(height);
		GUILayout.EndVertical();
		
		GUILayout.EndHorizontal();
		
		newLevelName = GUILayout.TextField(newLevelName);
		if(GUILayout.Button("New Level"))
		{
			LevelAsset newLevel = ScriptableObject.CreateInstance(typeof(LevelAsset)) as LevelAsset;
			AssetDatabase.CreateAsset(newLevel, "Assets/Levels/" + newLevelName + ".asset");
			
			//PrefabUtility.InstantiatePrefab(hexObject);
			//setUpGrid(width, height);
			//hexGrid.setUpGrid(width, height);
		}
	}
}