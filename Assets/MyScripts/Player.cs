using UnityEngine;
using System.Collections;

public class Player : Pathfinding 
{
	public GameObject target;
	Vector3 _endPosition;

	// Use this for initialization
	void Start () 
	{
		_endPosition = target.transform.position;
	}
	
	// Update is called once per frame
	void Update () 
	{

		FindPath (transform.position, _endPosition);

		if(Path.Count > 0)
		{
			Move ();
		}
	}
}
