using UnityEngine;
 
public class ShieldUvAnimation : MonoBehaviour 
{
	public float iSpeed;
 
	private Material mMaterial; 
	private float mTime;
 
	// Use this for initialization
	void Start () 
	{
		mMaterial = renderer.material;
 
		mTime = 0.0f;
	}
  
	// Update is called once per frame
	void Update () 
	{
		mTime += Time.deltaTime * iSpeed;
 
		mMaterial.SetFloat ("_Offset", Mathf.Repeat (mTime, 1.0f));
	}
}