using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {
	
	public enum GameStates {Play, Pause, LevelSelect, GameOver };
	public GameStates gameState = GameStates.LevelSelect;
	
	public HexGridController hexGrid;
	public Spawner spawner;
	public int gridWidth = 10, gridHeight = 10;
	public LevelAsset[] levels = new LevelAsset[0];
	public string[] levelNames;
	public string[] levelDifficulty;
	public int nextLevel = 0;
	public TowerController[] towers;
	public TowerController selection;
	public float towerHeight = 1.5f;
	public GUISkin skin;
	public int health = 100;
	public int money = 100;
	public int[] startingMoney;
	public int score = 0;
	public RenderTexture[] towerCams;
	public Texture previewBack;
	
	int buildSelection = -1;
	Vector2 buildScroll = Vector2.zero;
	int levelSelection = -1;
	Vector2 levelScroll = Vector2.zero;
	int upgradeSelection = -1;
	Vector2 upgradeScroll = Vector2.zero;
	int towerSelection = -1;
	
	float widthRatio = 1;
	float heightRatio = 1;
	
	int nextWave = 0;
	public List<Spawner> spawners = new List<Spawner>();
	public List<TowerController> towerObjects = new List<TowerController>();
	public bool waveFinished = true;
	
	bool won = false;
	bool credits = false;
	bool mute = false;
	
	
	
	// Use this for initialization
	void Start () {
		//hexGrid.setUpGrid(gridWidth, gridHeight);
		
		if(levels.Length > nextLevel)
			hexGrid.loadLevel(levels[nextLevel], spawner);
		
	}
	
	// Update is called once per frame
	void Update () {
		
		AudioListener.volume = mute ? 0 : 1;
		
		switch(gameState)
		{
		case GameStates.Play:
		
			//get the grid the mouse is over
			Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if(Physics.Raycast(mouseRay, out hit, 1000, 1 << 8))
			{
				HexController hex = hit.transform.gameObject.GetComponent(typeof(HexController)) as HexController;
				hex.startMoving(.4f, 2f, true, true);
				
				if(Input.GetMouseButtonDown(0))
				{
					if(hex.tower == null && !hex.path)
					{
						if(buildSelection >= 0)
						{
							selection = spawnTower(hex, towers[buildSelection]);
							money -= towers[buildSelection].upgrades[0].cost;
							towerSelection = towers[buildSelection].type;
							buildSelection = -1;
							
						}
						else
						{
							selection = null;
							towerSelection = -1;
						}
					}
					else if(hex.tower != null)
					{
						selection = hex.tower;
						towerSelection = hex.tower.type;
					}
					else
					{
						selection = null;
						towerSelection = -1;
					}
				}
				
				
				
			}
			if(Input.GetMouseButtonDown(1))
			{
				buildSelection = -1;
				towerSelection = -1;
			}
			
			if(Input.GetKeyDown(KeyCode.Alpha1))
				buildSelection = 0;
			if(Input.GetKeyDown(KeyCode.Alpha2))
				buildSelection = 1;
			if(Input.GetKeyDown(KeyCode.Alpha3))
				buildSelection = 2;
			if(Input.GetKeyDown(KeyCode.Alpha4))
				buildSelection = 3;
			if(Input.GetKeyDown(KeyCode.Alpha5))
				buildSelection = 4;
			if(Input.GetKeyDown(KeyCode.E) && selection != null && selection.upgradeLevel + 1 < selection.upgrades.Length)
				upgradeTower(selection, 1);
			if(Input.GetKeyDown(KeyCode.Space))
				startWave();
			
			if(Input.GetKeyDown(KeyCode.F))
				Time.timeScale = Time.timeScale ==1 ? 2 : 1;
			
			if(Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
			{
				Time.timeScale = 0;
				gameState = GameStates.Pause;
			}
			
			if(health <= 0)
			{
				gameState = GameStates.GameOver;
				Time.timeScale = 0;
			}
			
			break;
			
		case GameStates.Pause:
			if(Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
			{
				gameState = GameStates.Play;
				Time.timeScale = 1;
			}
			break;
		}
	}
	
	public TowerController spawnTower(HexController hex, TowerController tower)
	{
		hex.locked = true;
		hex.defaultHeight = 1.5f;
		hex.transform.position = new Vector3(hex.transform.position.x, 1.5f, hex.transform.position.z);
		
		GameObject towerObject = Instantiate(tower.gameObject, hex.transform.position + hex.spawnOffset, Quaternion.identity) as GameObject;
		TowerController newTower = towerObject.GetComponent<TowerController>();
		hex.tower = newTower;
		newTower.hex = hex;
		newTower.applyUpgrade(0);
		hex.setColor(newTower.upgrades[0].color);
		towerObjects.Add(newTower);
		return newTower;
	}
	
	public void upgradeTower(TowerController tower, int level)
	{
		int startLevel = tower.upgradeLevel;
		Debug.Log(""+ startLevel + " " + level);
		for(int i = startLevel + 1; i < tower.upgrades.Length && i < startLevel + level + 1; i++)
		{
			if(tower.upgrades[i].cost <= money)
			{
				money -= tower.upgrades[i].cost;
				tower.applyUpgrade(i);
			}
				
		}
	}
	
	public void startWave()
	{
		if(waveFinished)
		{
			foreach(Spawner s in spawners)
			{
				if(!s.startWave())
				{
					gameState = GameStates.GameOver;
					
					won = true;
				}
			}
			if(won)
				cleanup();
			nextWave++;
			waveFinished = false;
		}
		
	}
	
	void OnGUI()
	{
		widthRatio = Screen.width / 1920f;
		heightRatio = Screen.height / 1080f;
		GUI.skin = skin;
		
		
		
		switch(gameState)
		{
		case GameStates.LevelSelect:
			skin.label.fontSize = (int)(30.0f * heightRatio);
			skin.button.fontSize = (int)(30.0f * heightRatio);
			
			skin.label.alignment = TextAnchor.MiddleCenter;
			//left panel		
			GUI.Box(new Rect(0, 0, 415 * widthRatio, Screen.height), "", skin.GetStyle("panelLeft"));
			//health display
			GUI.Label(new Rect(40 * widthRatio, 40 * heightRatio,310 * widthRatio, 50 * heightRatio), 
				"Select a Level");
			//start button
			if(GUI.Button(new Rect(40 * widthRatio, 90 * heightRatio,310 * widthRatio, 50* heightRatio),
				"Start Game"))
			{
				gameState = GameStates.Play;
				Time.timeScale = 1;
				money = startingMoney[nextLevel];
			}
			
			
			//level list
			Rect scrollRect = new Rect(25 * widthRatio, 205 * heightRatio, 315 * widthRatio, 100 * towers.Length * heightRatio);
			Rect listlRect = new Rect(25 * widthRatio, 205 * heightRatio, 355 * widthRatio, 830 * heightRatio);
			levelScroll = GUI.BeginScrollView(listlRect, levelScroll, scrollRect);
			levelSelection = levelList(levelSelection, new Vector2(25 * widthRatio, 205 * heightRatio));
			GUI.EndScrollView();
			
			if(levelSelection != nextLevel && levelSelection != -1 && levels.Length > levelSelection)
			{
				nextLevel = levelSelection;
				
				hexGrid.clearGrid();
				hexGrid.loadLevel(levels[nextLevel], spawner);
				health = 100;
				score = 0;
				
			}
			skin.label.alignment = TextAnchor.MiddleLeft;
			
			if(GUI.Button(new Rect(Screen.width - 110 * widthRatio, Screen.height - 50 * heightRatio, 100 * widthRatio, 40 * heightRatio), "Credits"))
				credits = !credits;
			if(credits)
			{
				skin.label.wordWrap = true;
				GUI.Label(new Rect(Screen.width / 2 - 300 * widthRatio, Screen.height / 2 - 250 * heightRatio, 600 * widthRatio, 500 * heightRatio), 
					"Credits: \n Game: Matt Allen \n\n Music: Poss - Cypherfunk" +
					"\n\n Sound Effects: \n www.kenney.nl \n AstroMenace Artwork ver 1.2 Assets Copyright (c) 2006-2007 Michael Kurinnoy, Viewizard");
				skin.label.wordWrap = false; 
			}
			break;
		
		case GameStates.Play:
			
			skin.label.fontSize = (int)(22.0f * heightRatio);
			skin.button.fontSize = (int)(22.0f * heightRatio);
			skin.customStyles[2].fontSize = (int)(18.0f * heightRatio);
			//left panel		
			GUI.Box(new Rect(0, 0, 415 * widthRatio, Screen.height), "", skin.GetStyle("panelLeft"));
			
			//health display
			GUI.Label(new Rect(40 * widthRatio, 40 * heightRatio,80 * widthRatio, 30 * heightRatio), 
				"Health");
			GUI.Label(new Rect(125 * widthRatio, 40 * heightRatio,115 * widthRatio, 30 * heightRatio), 
				"" + health);
			
			//score display
			GUI.Label(new Rect(40 * widthRatio, 75 * heightRatio,80 * widthRatio, 30 * heightRatio), 
				"Score");
			GUI.Label(new Rect(125 * widthRatio, 75 * heightRatio,115 * widthRatio, 30 * heightRatio), 
				"" + score);
			
			//money display
			GUI.Label(new Rect(40 * widthRatio, 110 * heightRatio,80 * widthRatio, 30 * heightRatio), 
				"Money");
			GUI.Label(new Rect(125 * widthRatio, 110 * heightRatio,115 * widthRatio, 35 * heightRatio), 
				"" + money);
			
			//level display
			GUI.Label(new Rect(245 * widthRatio, 50 * heightRatio,120 * widthRatio, 45 * heightRatio), 
				"Wave " + (nextWave));
	
			//start button
			if(GUI.Button(new Rect(230 * widthRatio, 90 * heightRatio,130 * widthRatio, 55 * heightRatio),
				"Start Wave"))
				startWave();
			
			//build list
			Rect leftScrollRect = new Rect(25 * widthRatio, 205 * heightRatio, 315 * widthRatio, 100 * towers.Length * heightRatio);
			Rect leftListlRect = new Rect(25 * widthRatio, 205 * heightRatio, 355 * widthRatio, 830 * heightRatio);
			buildScroll = GUI.BeginScrollView(leftListlRect, buildScroll, leftScrollRect);
			buildSelection = towerList(buildSelection, new Vector2(25 * widthRatio, 205 * heightRatio));
			GUI.EndScrollView();
			
			if(towerSelection != -1 && selection != null)
			{
				//right panel-----------------------------------------------------------------------------
				float rightEdge = Screen.width - 405 * widthRatio;
				GUI.Box(new Rect(Screen.width - 415 * widthRatio, 0, 415 * widthRatio, Screen.height), "", skin.GetStyle("panelRight"));
				
				//selection name
				skin.label.fontStyle = FontStyle.Bold;
				GUI.Label(new Rect(rightEdge + 40 * widthRatio, 40 * heightRatio,400 * widthRatio, 30 * heightRatio), 
					selection.upgrades[selection.upgradeLevel].name);
				skin.label.fontStyle = FontStyle.Normal;
				
				//damage display
				GUI.Label(new Rect(rightEdge + 40 * widthRatio, 75 * heightRatio,100 * widthRatio, 30 * heightRatio), 
					"Damage");
				GUI.Label(new Rect(rightEdge + 155 * widthRatio, 75 * heightRatio,100 * widthRatio, 30 * heightRatio), 
					"" + selection.damage);
				
				//range display
				GUI.Label(new Rect(rightEdge + 40 * widthRatio, 110 * heightRatio,100 * widthRatio, 30 * heightRatio), 
					"Range");
				GUI.Label(new Rect(rightEdge + 155 * widthRatio, 110 * heightRatio,100 * widthRatio, 30 * heightRatio), 
					"" + selection.range);
				
				//rate of fire display
				GUI.Label(new Rect(rightEdge + 40 * widthRatio, 145 * heightRatio,100 * widthRatio, 30 * heightRatio), 
					"Rate of Fire");
				GUI.Label(new Rect(rightEdge + 155 * widthRatio, 145 * heightRatio,100 * widthRatio, 30 * heightRatio), 
					"" + selection.fireDelay);
				
				//upgrade list
				Rect rightScrollRect = new Rect(rightEdge + 25 * widthRatio, 255 * heightRatio, 315 * widthRatio, 100 * towers.Length * heightRatio);
				Rect rightListlRect = new Rect(rightEdge + 25 * widthRatio, 255 * heightRatio, 355 * widthRatio, 830 * heightRatio);
				upgradeScroll = GUI.BeginScrollView(rightListlRect, upgradeScroll, rightScrollRect);
				upgradeSelection = upgradeList(upgradeSelection, new Vector2(rightEdge + 25 * widthRatio, 255 * heightRatio));
				GUI.EndScrollView();
				
				if(upgradeSelection != -1)
				{
					Debug.Log(""+upgradeSelection);
					upgradeTower (selection, upgradeSelection + 1);
					upgradeSelection = -1;
				}
			}
			break;
			
		case GameStates.Pause:
			skin.label.fontSize = (int)(30.0f * heightRatio);
			skin.button.fontSize = (int)(30.0f * heightRatio);
			skin.label.fontStyle = FontStyle.Bold;
			skin.label.alignment = TextAnchor.MiddleCenter;
			
			GUI.Box(new Rect(Screen.width / 2 - 163 * widthRatio, Screen.height / 2 - 93 * heightRatio, 325 * widthRatio, 185 * heightRatio), "", skin.GetStyle("panelPause"));
			
			GUI.Label(new Rect(Screen.width / 2 - 153 * widthRatio, Screen.height / 2 - 93 * heightRatio, 295 * widthRatio, 45 * heightRatio), "Paused");
			
			if(GUI.Button(new Rect(Screen.width / 2 - 153 * widthRatio, Screen.height / 2 - 43 * heightRatio, 295 * widthRatio, 50 * heightRatio), "Quit"))
			{
				gameState = GameStates.LevelSelect;
				Time.timeScale = 1;
				cleanup();
				won = false;
				nextWave = 0;
				
			}
			
			if(GUI.Button(new Rect(Screen.width / 2 - 153 * widthRatio, Screen.height / 2 + 12 * heightRatio, 295 * widthRatio, 50 * heightRatio), "Continue"))
			{
				gameState = GameStates.Play;
				Time.timeScale = 1;
			}
			
			
			break;
			
		case GameStates.GameOver:
			
			skin.label.fontSize = (int)(30.0f * heightRatio);
			skin.button.fontSize = (int)(30.0f * heightRatio);
			skin.label.fontStyle = FontStyle.Bold;
			skin.label.alignment = TextAnchor.MiddleCenter;
			
			GUI.Box(new Rect(Screen.width / 2 - 163 * widthRatio, Screen.height / 2 - 93 * heightRatio, 325 * widthRatio, 185 * heightRatio), "", skin.GetStyle("panelPause"));
			
			GUI.Label(new Rect(Screen.width / 2 - 153 * widthRatio, Screen.height / 2 - 93 * heightRatio, 295 * widthRatio, 45 * heightRatio), 
				won ? "Level Complete!" : "Game Over");
			
			
			if(GUI.Button(new Rect(Screen.width / 2 - 163 * widthRatio, Screen.height / 2 + 12 * heightRatio, 325 * widthRatio, 50 * heightRatio), "Level Select"))
			{
				gameState = GameStates.LevelSelect;
				Time.timeScale = 1;
				cleanup();
				won = false;
				nextWave = 0;
			}
			break;
				
		}
		
		if(GUI.Button(new Rect(Screen.width - 100 * widthRatio, 5, 90 * widthRatio, 40 * heightRatio), mute ? "Unmute" : "Mute"))
		{
			mute = !mute;
		}
		
	}
	
	int towerList(int sel, Vector2 origin)
	{
		for(int i = 0; i < towers.Length; i++)
		{
			float x = origin.x + 5 * widthRatio;
			float y = origin.y + 5 * heightRatio;
			float width = 325 * widthRatio;
			float height = 125 * heightRatio;
			if(GUI.Toggle(new Rect(x, y + (height) * i, width, height), i == sel, 
			"", skin.GetStyle("panelStoreRow")))
			{
				if(towers[i].upgrades[0].cost <= money)
					sel = i;
				else
					sel = -1;
			}
			
			
			//name label
			skin.label.fontStyle = FontStyle.Bold;
			GUI.Label(new Rect(x + 90 * widthRatio, y + 5 * heightRatio + 125 * i * heightRatio, 185 * widthRatio, 35 * heightRatio), towers[i].upgrades[0].name);
			skin.label.fontStyle = FontStyle.Normal;
			
			
			//description label
			GUI.Label(new Rect(x + 90 * widthRatio, y + 35 * heightRatio + 125 * i * heightRatio, 215 * widthRatio, 85 * heightRatio), towers[i].upgrades[0].description, 
				skin.GetStyle("descriptionLabel"));
			
			
			//cost label
			if(towers[i].upgrades[0].cost > money)
				GUI.contentColor = Color.red;
			GUI.Label(new Rect(x + 280 * widthRatio, y + 5 * heightRatio + 125 * i * heightRatio, 30 * widthRatio, 35 * heightRatio), "" + towers[i].upgrades[0].cost);
			GUI.contentColor = Color.white;
			
			
			//preview window
			if(i < towerCams.Length && towerCams[i] != null)
			{
				//GUI.DrawTexture(new Rect(x + 5 * widthRatio, y + 5 * heightRatio + 125 * i * heightRatio, 100 * widthRatio, 100 * heightRatio), previewBack);
				GUI.DrawTexture(new Rect(x + 10 * widthRatio, y + 10 * heightRatio + 125 * i * heightRatio, 75 * widthRatio, 75 * heightRatio), towerCams[i], ScaleMode.ScaleToFit, false);
			}
		
		}
		
		return sel;
	}
	
	
	int levelList(int sel, Vector2 origin)
	{
		for(int i = 0; i < levels.Length; i++)
		{
			float x = origin.x + 5 * widthRatio;
			float y = origin.y + 5 * heightRatio;
			float width = 325 * widthRatio;
			float height = 125 * heightRatio;
			if(GUI.Toggle(new Rect(x, y + (height) * i, width, height), i == sel, 
			"", skin.GetStyle("panelUpgradeRow")))
			{
				sel = i;
			}
			
			//name label
			skin.label.fontStyle = FontStyle.Bold;
			
			GUI.Label(new Rect(x + 25 * widthRatio, y + 5 * heightRatio + 125 * i * heightRatio, 280 * widthRatio, 50 * heightRatio), 
				levelNames[i]);
			skin.label.fontStyle = FontStyle.Normal;
			
			
			//description label
			GUI.Label(new Rect(x + 25 * widthRatio, y + 35 * heightRatio + 125 * i * heightRatio, 280 * widthRatio, 85 * heightRatio), 
				levelDifficulty[i]);
		}
		
		return sel;
	}
	
	
	int upgradeList(int sel, Vector2 origin)
	{
		int cost = 0;
		if(selection.upgradeLevel >= selection.upgrades.Length - 1)
		{
			sel = -1;
			GUI.Label(new Rect(origin.x + 25 * widthRatio, origin.y + 5 * heightRatio, 300 * widthRatio, 45 * heightRatio), "No Upgrades Left");
		}
		else{
			for(int i = 0; i < selection.upgrades.Length - selection.upgradeLevel - 1; i++)
			{
				float x = origin.x + 5 * widthRatio;
				float y = origin.y + 5 * heightRatio;
				float width = 325 * widthRatio;
				float height = 125 * heightRatio;
				if(GUI.Toggle(new Rect(x, y + (height) * i, width, height), i == sel, 
				"", skin.GetStyle("panelUpgradeRow")))
				{
					sel = i;
				}
				
				//name label
				skin.label.fontStyle = FontStyle.Bold;
				GUI.Label(new Rect(x + 90 * widthRatio, y + 5 * heightRatio + 125 * i * heightRatio, 185 * widthRatio, 35 * heightRatio), 
					selection.upgrades[i + selection.upgradeLevel + 1].name);
				skin.label.fontStyle = FontStyle.Normal;
				
				
				//description label
				GUI.Label(new Rect(x + 25 * widthRatio, y + 35 * heightRatio + 125 * i * heightRatio, 280 * widthRatio, 85 * heightRatio), 
					selection.upgrades[i + selection.upgradeLevel + 1].description, 
					skin.GetStyle("descriptionLabel"));
				
				
				//cost label
				cost += selection.upgrades[i + selection.upgradeLevel + 1].cost;
				if(cost > money)
					GUI.contentColor = Color.red;
				GUI.Label(new Rect(x + 25 * widthRatio, y + 5 * heightRatio + 125 * i * heightRatio, 45 * widthRatio, 35 * heightRatio), 
					"" + cost);
				GUI.contentColor = Color.white;
			}
		}
		return sel;
	}
	
	void cleanup()
	{
		PathFollow[] mobs = FindObjectsOfType(typeof(PathFollow)) as PathFollow[];
		
		foreach(PathFollow mob in mobs)
			GameObject.Destroy(mob.gameObject);
		foreach(TowerController tower in towerObjects)
			GameObject.Destroy(tower.gameObject);
		towerObjects = new List<TowerController>();
		
		hexGrid.clearGrid();
		nextLevel = 0;
		spawner.setWave(0);
		hexGrid.loadLevel(levels[nextLevel], spawner);
		levelSelection = 0;
		nextLevel = -1;
		nextWave = 0;
		health = 100;
		score = 0;
		waveFinished = true;
		Application.LoadLevel("TowerDefenseLevel");
		
	}
}