using UnityEngine;
using System.Collections;

public class ParticleDestroy : MonoBehaviour {
	public float duration;
	float timer = 0;
	
	// Update is called once per frame
	void Update () {
		timer += Time.deltaTime;
		if(timer >= duration)
			GameObject.Destroy(gameObject);
	}
}
