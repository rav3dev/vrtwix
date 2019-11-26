using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityFix : MonoBehaviour
{
//	public Rigidbody r;
//	public int posFrames=10, rotFrames=10;
//	public float positionMultiply=1.1f,rotationMultiply=1.1f;
//	public Vector3[] posSave, rotSave;

//	int currentframe;
    // Start is called before the first frame update
    void Start()
    {
//		r=GetComponent<Rigidbody> ();

    }

    // Update is called once per frame

	void GrabStart(CustomHand hand)
	{
//		posSave = new Vector3[posFrames];
//		rotSave = new Vector3[rotFrames];
//		currentframe = 0;
		BeginEstimatingVelocity();
	}

	void GrabUpdate(CustomHand hand){
//		currentframe++;
//		posSave [currentframe % posFrames] = r.velocity;
//		rotSave [currentframe % rotFrames] = r.angularVelocity;
	}

	void GrabEnd(CustomHand hand)
    {

		GetComponent<Rigidbody> ().velocity = GetVelocityEstimate ();
		GetComponent<Rigidbody> ().angularVelocity = GetAngularVelocityEstimate ();
//		Vector3 tempPos = Vector3.zero;
//		for (int i = 0; i < posSave.Length; i++) {
//			tempPos += posSave [i];
//		}
//
//		r.velocity = tempPos/posFrames*positionMultiply;
//
//		Vector3 tempRot = Vector3.zero;
//		for (int i = 0; i < rotSave.Length; i++) {
//			tempRot += rotSave [i];
//		}
//
//		r.angularVelocity = tempRot/rotFrames*rotationMultiply;
    }

	[Tooltip( "How many frames to average over for computing velocity" )]
	public int velocityAverageFrames = 5;
	[Tooltip( "How many frames to average over for computing angular velocity" )]
	public int angularVelocityAverageFrames = 11;

	public bool estimateOnAwake = false;

	private Coroutine routine;
	private int sampleCount;
	private Vector3[] velocitySamples;
	private Vector3[] angularVelocitySamples;


	//-------------------------------------------------
	public void BeginEstimatingVelocity()
	{
		FinishEstimatingVelocity();

		routine = StartCoroutine( EstimateVelocityCoroutine() );
	}


	//-------------------------------------------------
	public void FinishEstimatingVelocity()
	{
		if ( routine != null )
		{
			StopCoroutine( routine );
			routine = null;
		}
	}


	//-------------------------------------------------
	public Vector3 GetVelocityEstimate()
	{
		// Compute average velocity
		Vector3 velocity = Vector3.zero;
		int velocitySampleCount = Mathf.Min( sampleCount, velocitySamples.Length );
		if ( velocitySampleCount != 0 )
		{
			for ( int i = 0; i < velocitySampleCount; i++ )
			{
				velocity += velocitySamples[i];
			}
			velocity *= ( 1.0f / velocitySampleCount );
		}

		return velocity;
	}


	//-------------------------------------------------
	public Vector3 GetAngularVelocityEstimate()
	{
		// Compute average angular velocity
		Vector3 angularVelocity = Vector3.zero;
		int angularVelocitySampleCount = Mathf.Min( sampleCount, angularVelocitySamples.Length );
		if ( angularVelocitySampleCount != 0 )
		{
			for ( int i = 0; i < angularVelocitySampleCount; i++ )
			{
				angularVelocity += angularVelocitySamples[i];
			}
			angularVelocity *= ( 1.0f / angularVelocitySampleCount );
		}

		return angularVelocity;
	}


	//-------------------------------------------------
	public Vector3 GetAccelerationEstimate()
	{
		Vector3 average = Vector3.zero;
		for ( int i = 2 + sampleCount - velocitySamples.Length; i < sampleCount; i++ )
		{
			if ( i < 2 )
				continue;

			int first = i - 2;
			int second = i - 1;

			Vector3 v1 = velocitySamples[first % velocitySamples.Length];
			Vector3 v2 = velocitySamples[second % velocitySamples.Length];
			average += v2 - v1;
		}
		average *= ( 1.0f / Time.deltaTime );
		return average;
	}


	//-------------------------------------------------
	void Awake()
	{
		velocitySamples = new Vector3[velocityAverageFrames];
		angularVelocitySamples = new Vector3[angularVelocityAverageFrames];

		if ( estimateOnAwake )
		{
			BeginEstimatingVelocity();
		}
	}


	//-------------------------------------------------
	private IEnumerator EstimateVelocityCoroutine()
	{
		sampleCount = 0;

		Vector3 previousPosition = transform.position;
		Quaternion previousRotation = transform.rotation;
		while ( true )
		{
			yield return new WaitForFixedUpdate();//WaitForEndOfFrame

			float velocityFactor = 1.0f / Time.deltaTime;

			int v = sampleCount % velocitySamples.Length;
			int w = sampleCount % angularVelocitySamples.Length;
			sampleCount++;

			// Estimate linear velocity
			velocitySamples[v] = velocityFactor * ( transform.position - previousPosition );

			// Estimate angular velocity
			Quaternion deltaRotation = transform.rotation * Quaternion.Inverse( previousRotation );

			float theta = 2.0f * Mathf.Acos( Mathf.Clamp( deltaRotation.w, -1.0f, 1.0f ) );
			if ( theta > Mathf.PI )
			{
				theta -= 2.0f * Mathf.PI;
			}

			Vector3 angularVelocity = new Vector3( deltaRotation.x, deltaRotation.y, deltaRotation.z );
			if ( angularVelocity.sqrMagnitude > 0.0f )
			{
				angularVelocity = theta * velocityFactor * angularVelocity.normalized;
			}

			angularVelocitySamples[w] = angularVelocity;

			previousPosition = transform.position;
			previousRotation = transform.rotation;
		}
	}
}
