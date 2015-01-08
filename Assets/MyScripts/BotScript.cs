using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;

public class Waypoint
{
	public Waypoint(Vector3 pos, bool exp)
	{
		position = pos;
		explored = exp;
		distanceFromTarget = 0.0f;
	}
	
	public Vector3 position;
	public bool explored;
	public float distanceFromTarget;
};

public class BotScript : Pathfinding
{
	#region Variables
	
	public GameObject Target;
	public GameObject Gun;
	public List<GameObject> WaypointObjects = new List<GameObject>();
	private List<Waypoint> _waypoints = new List<Waypoint>();
	private Waypoint _destination = new Waypoint(new Vector3(0,0,0), false);
	public float SightDistance = 25.0f;
	public float TurnRate = 10.0f;
	public float FieldOfView = 110.0f;

	#endregion

	#region Behaviour Tree
	Action Sequence(Action a, Action b) 
	{
		return () => { a(); b(); };
	}
	
	//Testing to see if function overloading works here
	Action Sequence(Action a, Action b, Action c)
	{
		return () => { a(); b(); c(); };
	}
	
	//This function takes in boolean function, followed by the actions to perform if the condition was true or false.
	Action Selector(Func<bool> cond, Action ifTrue, Action ifFalse) 
	{
		return () => { if (cond()) ifTrue(); else ifFalse(); };
	}
	
	Action AI; //This is our behaviour tree, we compose it out of the action functions above by chaining them together.
	
	#endregion
	
	#region Methods
	
	bool HaveTarget()
	{
		return true;
	}
	
	bool HaveLineOfSight()
	{
		return true;
	}
	
	bool WithinRange()
	{
		return true;
	}
	
	bool SearchForTarget()
	{
		//Done
		Vector3 playerDir = (Target.transform.position - transform.position).normalized;
		
		Ray ray = new Ray(Gun.transform.position, playerDir);
		RaycastHit hit;
		Debug.DrawRay (ray.origin, ray.direction * SightDistance , Color.green );
		
		//Vector3.Angle returns in degrees 
		if(		Vector3.Angle ( transform.forward, playerDir) < FieldOfView * .5 
		   && 	Vector3.Distance (transform.position, Target.transform.position) < SightDistance)
		{
			return true;
		}
		return false;
	}
	
	bool _targetLocked = false;
	int _exploredNodes = 0;
	
	void Wander()
	{
		List<Waypoint> sortedWaypoints = _waypoints.OrderBy (Waypoint => Waypoint.distanceFromTarget).ToList ();
		sortedWaypoints = _waypoints.OrderBy (Waypoint => Waypoint.explored).ToList ();
		
		if(!_targetLocked)	
		{
			_targetLocked = true;
			FindPath ( transform.position, sortedWaypoints[0].position);
		}
		
		if(_targetLocked && Vector3.Distance (transform.position, sortedWaypoints[0].position) < 5.0f)
		{
			_targetLocked = false;
			sortedWaypoints[0].explored = true;
			_exploredNodes++;
		}
		
		if(_exploredNodes == _waypoints.Count)
		{
			foreach(Waypoint w in _waypoints)
			{
				w.explored = false;
			}
			_exploredNodes = 0;
		}
	
		
	}
	
	void CloseDistanceToTarget()
	{
	
	}
	
	void Shoot()
	{
	
	}
	
	#endregion

	#region Unity Events
	
	void Start () 
	{
		for(int i = 0; i < WaypointObjects.Count; i++)
		{
			_waypoints.Add (new Waypoint(WaypointObjects[i].transform.position ,false));
		}
		_destination = _waypoints[0];
		AI = Wander;
	}
	
	void Update () 
	{
		for(int i = 0; i < _waypoints.Count; i++)
		{
			_waypoints[i].distanceFromTarget = Vector3.Distance (transform.position, _waypoints[i].position);
		}
		
		if(Path.Count > 0)
		{
			Move ();
		}
		
		AI();
	}
	
	#endregion
}
