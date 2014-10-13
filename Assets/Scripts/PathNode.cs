using UnityEngine;
using System.Collections.Generic;

public class PathNode : MonoBehaviour {
	
	public List<PathNode> nextNodes;
	
	public PathNode getNextNode()
	{
		return nextNodes[Random.Range(0, nextNodes.Count)];
	}
	
	void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(transform.position, .5f);
		foreach(var node in nextNodes)
		{
			if(node != null)
				Gizmos.DrawLine(transform.position, node.transform.position);
		}
	}
}
