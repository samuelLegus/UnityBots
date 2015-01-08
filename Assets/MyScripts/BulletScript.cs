using UnityEngine;
using System.Collections;

public class BulletScript : MonoBehaviour
{
	//public float Speed = 50.0f;
	private int _elapsedTicks = 0;
	
	// Use this for initialization
	void Awake ()
	{

	}
	
	// Update is called once per frame
	void Update () 
	{
		//transform.Translate ( transform.forward * Time.deltaTime * Speed );	
		_elapsedTicks++;
		if(_elapsedTicks > 1000)
		{
			Destroy (gameObject);
		}

	}

	void OnCollisionEnter(Collision c)
	{
		if(c.collider.tag == "Bullet")
		{
			Physics.IgnoreCollision (c.collider, collider);
		}
		Destroy (gameObject);
	}
}
