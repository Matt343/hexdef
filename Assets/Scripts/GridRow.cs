using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GridRow  {
	[SerializeField]
	public List<HexInfo> row = new List<HexInfo>();
	
	public HexInfo this [int i]
	{
		get
		{
			return row[i];
		}
		set
		{
			row[i] = value;
		}
			
	}
	
	public int Count
	{
		get
		{
			return row.Count;
		}
	}
	
	public void Add(HexInfo h)
	{
		row.Add(h);
	}
	
//	public void OnEnable()
//	{
//		hideFlags = HideFlags.HideAndDontSave;
//	}
	
	public IEnumerator<HexInfo> GetEnumerator()
	{
		return row.GetEnumerator();
	}
}
