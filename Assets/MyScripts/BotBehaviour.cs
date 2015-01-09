using UnityEngine;
using System.Collections;

public class BotBehaviour : MonoBehaviour 
{
	#region Behaviour Tree components
	
	delegate bool Action();
	
	bool Sequence(params Action[] children)
	{
		foreach(Action child in children)
		{
			if(!child())
			{
				return false;
			}
		}
		return true;
	}
	
	bool Selector(params Action[] children)
	{
		foreach(Action child in children)
		{
			if(child())
			{
				return true;
			}
		}
		return false;
	}
	
	#endregion
	#region Variables
	public GameObject Target;
	public float TurnRate= 20.0f;
	#endregion
	#region Methods
	
	#endregion
	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
}
