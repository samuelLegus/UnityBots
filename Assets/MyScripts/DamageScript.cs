using UnityEngine;
using System.Collections;

public class DamageScript : MonoBehaviour 
{
	#region Variables
	int _health = 10;
	#endregion
	
	#region Unity Events
	
	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(_health <= 0)
		{
			Destroy (gameObject);
		}
	}
	
	void OnCollisionEnter(Collision c)
	{
		if(c.collider.tag == "Bullet")
		{
			_health--;
		}
	}
	
	#endregion
}
