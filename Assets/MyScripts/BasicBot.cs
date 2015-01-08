using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class BasicBot : Pathfinding
{
	#region Variables

	public GameObject Target;
	public GameObject BulletPrefab;
	public GameObject Gun;

	public float Fov = 110.0f;
	public float SightDistance = 10.0f;
	public float TurnRate = 50.0f;
	public int FireDelayMS = 75;

	private DateTime _lastShot = DateTime.Now;
	private Quaternion _lookRotation;
	private bool _targetDetected = false;

	public GameObject[] _waypoints;
	private GameObject _currentWaypoint;
	public Dictionary<GameObject, bool> _flags = new Dictionary<GameObject, bool>();

	Action AI;

	#endregion

	# region Behaviour Tree Components
	//	-- Behaviour tree composition is expained here --

	//public delegate void Action(); -- Defined in System --

	//This function will return a single action which is actually a lambda that calls both.
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

	/* -- Psuedo coded behaviour tree example --

		Action AI;
		...
		void Start()
		{
			AI = Selector( PlayerVisible ( Sequence ( Aim, Fire ) ,			
					Search );						
		}

		void Update()
		{
			AI();
		}
		...
	*/

	#endregion

	#region Methods

	bool TargetInSight()
	{
		Vector3 playerDir = (Target.transform.position - transform.position).normalized;

		//Ray ray = new Ray(Gun.transform.position, playerDir);
		//RaycastHit hit;
		//Debug.DrawRay (ray.origin, ray.direction * SightDistance , Color.green );

		//Vector3.Angle returns in degrees 
		if(	Vector3.Angle ( transform.forward, playerDir) < Fov * .5 
			&& Vector3.Distance (transform.position, Target.transform.position) < SightDistance)
		{
			return true;
		}
		return false;
	}

	bool CanShoot()
	{
		//Ray ray = new Ray(Gun.transform.position, transform.forward);
		//RaycastHit hit;
		//Debug.DrawRay (ray.origin, ray.direction * SightDistance , Color.green );
		
		//if(Physics.Raycast (Gun.transform.position, transform.forward, out hit, SightDistance))
		//{
		if( DateTime.Now >= _lastShot + new TimeSpan(0,0,0,0, FireDelayMS))
		{
			return true;
		}
		//}
		return false;
	}

	bool DetectTarget()
	{
		//Simulates crappy "hearing" by projecting a sphere in a radius around us.
		//If the sphere collides with the player we've heard it.
		RaycastHit hit;
		if(Physics.SphereCast(transform.position, 50, transform.forward, out hit, Mathf.Infinity))
		{
			if(hit.collider.tag == Target.tag)
			{
				Debug.Log ("Target detected.");
				return true;
			}
		}
		return false;
	}
	
	#endregion

	#region Actions
	// -- To compose the behaviour tree --

	Action Okay = () => Debug.Log ("Okay!");
	
	void Yes()
	{
		Debug.Log("Yes!");
	}

	void No()
	{
		Debug.Log ("No.");
	}
	
	void Shoot()
	{
		_lastShot = DateTime.Now;
		GameObject newBullet = Instantiate (BulletPrefab, Gun.transform.position, Gun.transform.rotation) as GameObject;
		Rigidbody rb = newBullet.GetComponent<Rigidbody>();
		Physics.IgnoreCollision(newBullet.collider, collider); 
		rb.velocity = (Target.transform.position - Gun.transform.position).normalized * 50.0f;
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
	
	void NavigateThroughWaypoints()
	{
		//Always going to navigate towards the current waypoint w/ A * Pathfinding
		FindPath ( transform.position, _currentWaypoint.transform.position);

		//If our current node hasn't been visited we can navigate to it.
		if( _flags[_currentWaypoint] == false )
		{
			Move ();
			//Once we reach it, flag it as visited.
			if( Vector3.Distance( transform.position, _currentWaypoint.transform.position) < 5.0f)
			{
				Debug.Log ("Arrived at waypoint, flagging as visited.");
				_flags[_currentWaypoint] = true; 
			}
		}
		//This will be triggered once the waypoint has been reached and we need to decide on a new destination.
		else
		{
			List<GameObject> remainingWaypoints = new List<GameObject>();

			//Add all the waypoints we haven't visited yet to a list to sort through.
			for(int i = 0; i < _waypoints.Length; i++)
			{
				if(_flags[_waypoints[i]] == false)
				{
					remainingWaypoints.Add (_waypoints[i]);
				}
			}

			//If there are no more remaining waypoints, reset it to it's original state.
			if(remainingWaypoints.Count == 0)
			{
				for(int i = 0; i < _waypoints.Length; i++)
				{
					remainingWaypoints.Add (_waypoints[i]);
					foreach(KeyValuePair<GameObject , bool > waypoint in _flags)
					{
						_flags[waypoint.Key] = false;
					}
				}
			}

			//Find the closest waypoint and set our new destination to its location.
			for(int i = 0;i < remainingWaypoints.Count; i++)
			{
				if( i == 0)
				{
					_currentWaypoint = remainingWaypoints[i];
				}
				else
				{
					if(Vector3.Distance (transform.position, remainingWaypoints[i].transform.position) < Vector3.Distance (transform.position, _currentWaypoint.transform.position))
					{
						_currentWaypoint = remainingWaypoints[i];
					}
				}
			}
		}
	}

	void NavigateToTarget()
	{
		//_flags.Add(Target, false);
		//_currentWaypoint = Target;
		//FindPath(transform.position, _currentWaypoint.transform.position);
		//Move ();
	}
			

	
	#endregion
	                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      
	#region Unity Events

	void Awake()
	{
		for(int i = 0 ; i < _waypoints.Length; i++)
		{
			_flags.Add ( _waypoints[i], false);
		}
	}
 
	void Start () 
	{           
		_currentWaypoint = _waypoints[0];
		AI = Selector( TargetInSight, Sequence(LookAtTarget, Shoot), 
			NavigateThroughWaypoints);
	}

	void Update ()
	{
		AI();
	}

	#endregion
}

