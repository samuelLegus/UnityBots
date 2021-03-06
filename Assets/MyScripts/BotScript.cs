﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
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
	public GameObject BulletPrefab;
	public List<string> HostileTags; //For this simulation I'm going to use "RedTeam", "BlueTeam", "Neutral"
	private List<GameObject> _hostiles = new List<GameObject>();
	
	private List<GameObject> _waypointObjects = new List<GameObject>();
	private List<Waypoint> _waypoints = new List<Waypoint>();
	
	public float SightDistance = 10.0f;
	public float TurnRate = 10.0f;
	public float FieldOfView = 110.0f;
	public int ShotDelay = 100;
	bool _nextWaypointDetermined = false;
	bool _targetLocked = false;
	int _exploredNodes = 0;
	private DateTime _lastShot = DateTime.Now;

	#endregion

	#region Decision Tree Components
	
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
	
	bool TargetInLoS()
	{
		//TODO: Add Raycast so bots don't shoot fucking walls.
		if(Target)
		{
			//Done
			Vector3 targetDir = (Target.transform.position - transform.position).normalized;
			
			//Ray ray = new Ray(Gun.transform.position, targetDir);
			//RaycastHit hit;
			//Debug.DrawRay (ray.origin, ray.direction * SightDistance , Color.green );
			
			//Vector3.Angle returns in degrees 
			if(		Vector3.Angle ( transform.forward, targetDir) < FieldOfView * .5 
			   && 	Vector3.Distance (transform.position, Target.transform.position) < SightDistance)
			{
				return true;
			}
		}
		return false;
	}
	
	bool CanShoot()
	{
		return DateTime.Now >= _lastShot + new TimeSpan(0,0,0,0, ShotDelay);
	}
	
	void Wander()
	{
		List<Waypoint> sortedWaypoints = _waypoints.OrderBy (Waypoint => Waypoint.distanceFromTarget).ToList ();
		sortedWaypoints = _waypoints.OrderBy (Waypoint => Waypoint.explored).ToList ();
		
		if(!_nextWaypointDetermined)	
		{
			_nextWaypointDetermined = true;
			FindPath ( transform.position, sortedWaypoints[0].position);
		}
		
		if(_nextWaypointDetermined && Vector3.Distance (transform.position, sortedWaypoints[0].position) < 5.0f)
		{
			_nextWaypointDetermined = false;
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
		
		Move ();
	}
	
	bool FoundTarget()
	{
		RaycastHit[] hits;
		hits = Physics.SphereCastAll(transform.position, 1.0f, transform.forward);
		
		foreach(RaycastHit hit in hits)
		{
			foreach(string t in HostileTags)
			{
				if(hit.collider.tag == t)
				{
					Debug.Log (tag + " hit " + t);
					Target = hit.collider.gameObject;
					return true;
				}
			}
		}
		
		return false;
	}
	
	void LookAtTarget()
	{
		//find the vector pointing from us to our target
		Vector3 playerDir = (Target.transform.position - transform.position).normalized;
		//create the rotation we need to be in to look at the target
		Quaternion lookRotation = Quaternion.LookRotation (playerDir);
		//rotate us over time according to our speed until we are in the required direction
		transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * TurnRate );
	}

	void Shoot()
	{
		if(Target)
		{
			//rotate us towards the target
			Vector3 targetDir = (Target.transform.position - transform.position).normalized;
			Quaternion lookRotation = Quaternion.LookRotation (targetDir);
			transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * TurnRate );
			
			//shoot
			_lastShot = DateTime.Now;
			GameObject newBullet = Instantiate (BulletPrefab, Gun.transform.position, Gun.transform.rotation) as GameObject;
			Rigidbody rb = newBullet.GetComponent<Rigidbody>();
			Physics.IgnoreCollision(newBullet.collider, collider); 
			rb.velocity = (Target.transform.position - Gun.transform.position).normalized * 50.0f;
		}
	}
	
	#endregion

	#region Unity Events
	
	void Start () 
	{
		//Finds all the waypoints in the scene and constructs our waypoint object out of them.
		foreach(GameObject waypoint in GameObject.FindGameObjectsWithTag("Waypoint"))
		{
			_waypoints.Add(new Waypoint(waypoint.transform.position, false));
		}
		
		//Finds all hostile objects in the scene, adds them to our list of enemies. 
		foreach(string t in HostileTags)
		{
			foreach(GameObject obj in GameObject.FindGameObjectsWithTag (t))
			{
				_hostiles.Add (obj);
				//Debug.Log (tag + " object added " + t + " tag to its hostile list.");
			}

		}
		
		//AI = Selector(FoundTarget, Selector(TargetInLoS, Sequence(LookAtTarget, Shoot), LookAtTarget), Wander);
		AI = Selector(FoundTarget, Shoot , Wander);
		//AI = Wander;
	}
	
	void Update () 
	{
		for(int i = 0; i < _waypoints.Count; i++)
		{
			_waypoints[i].distanceFromTarget = Vector3.Distance (transform.position, _waypoints[i].position);
		}
		
		AI();
	}

	#endregion
}
