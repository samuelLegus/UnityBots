using UnityEngine;
using System.Collections;

public class LookAt : MonoBehaviour 
{
	public GameObject Target;
	public float RotationSpeed;

	private Quaternion _lookRotation;
	private Vector3 _direction;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		//find the vector pointing from us to our target
		_direction = (Target.transform.position - transform.position).normalized;
		//create the rotation we need to be in to look at the target
		_lookRotation = Quaternion.LookRotation (_direction);
		//rotate us over time according to our speed until we are in the required direction
		transform.rotation = Quaternion.Slerp(transform.rotation, _lookRotation, Time.deltaTime * RotationSpeed );
	}
}
