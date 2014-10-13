using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PathController : MonoBehaviour {
	
	public List<Transform> path;
	public int pathCount;
	
	
	// Use this for initialization
	void Start () {
		path = transform.Cast<Transform>().OrderBy(t=>t.name).ToList();
		pathCount = path.Count;
		
	}
}
