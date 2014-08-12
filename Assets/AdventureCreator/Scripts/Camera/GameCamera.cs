/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"GameCamera.cs"
 * 
 *	This is attached to cameras that act as "guides" for the Main Camera.
 *	They are never active: only the Main Camera is ever active.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

public class GameCamera : _Camera
{
	
	public bool followCursor = false;
	public Vector2 cursorInfluence = new Vector2 (0.3f, 0.1f);
	
	public bool targetIsPlayer = true;
	public Transform target;
	public bool actFromDefaultPlayerStart = true;
	
	public bool lockXLocAxis = true;
	public bool lockYLocAxis = true;
	public bool lockZLocAxis = true;
	public bool lockYRotAxis = true;
	public bool lockFOV = true;
	
	public CameraLocConstrainType xLocConstrainType;
	public CameraRotConstrainType yRotConstrainType;
	public CameraLocConstrainType zLocConstrainType;
	
	public float xGradient = 1f;
	public float yGradientLoc = 1f;
	public float zGradient = 1f;
	public float yGradient = 2f;
	public float FOVGradient = 2f;
	
	public float xOffset = 0f;
	public float yOffsetLoc = 0f;
	public float zOffset = 0f;
	public float yOffset = 0f;
	public float FOVOffset = 0f;
	
	public float xFreedom = 2f;
	public float yFreedom = 2f;
	public float zFreedom = 2f;
	
	public bool limitX;
	public bool limitYLoc;
	public bool limitZ;
	public bool limitY;
	public bool limitFOV;
	
	public float targetHeight;
	public float targetXOffset;
	public float targetZOffset;
	
	public Vector2 constrainX;
	public Vector2 constrainYLoc;
	public Vector2 constrainZ;
	public Vector2 constrainY;
	public Vector2 constrainFOV;
	
	public float dampSpeed = 0.9f;
	
	private Vector3 desiredPosition;
	private float desiredRotation;
	private float desiredFOV;
	
	private Vector3 originalTargetPosition;
	private Vector3 originalPosition;
	private float originalRotation;
	private float originalFOV;
	
	
	protected override void Awake ()
	{
		base.Awake ();
		
		originalPosition = transform.position;
		originalRotation = transform.eulerAngles.y;
		originalFOV = this.camera.fieldOfView;
		
		desiredPosition = originalPosition;
		desiredRotation = originalRotation;
		desiredFOV = originalFOV;
		
		if (!lockXLocAxis && limitX)
		{
			desiredPosition.x = ConstrainAxis (desiredPosition.x, constrainX);
		}
		
		if (!lockYLocAxis && limitY)
		{
			desiredPosition.y = ConstrainAxis (desiredPosition.y, constrainYLoc);
		}
		
		if (!lockZLocAxis && limitZ)
		{
			desiredPosition.z = ConstrainAxis (desiredPosition.z, constrainZ);
		}
		
		if (!lockYRotAxis && limitY && yRotConstrainType != CameraRotConstrainType.LookAtTarget)
		{
			desiredRotation = ConstrainAxis (desiredRotation, constrainY);
		}
		
		if (!lockFOV && limitFOV)
		{
			desiredFOV = ConstrainAxis (desiredFOV, constrainFOV);
		}
	}
	
	
	private void Start ()
	{
		ResetTarget ();
		
		if (target)
		{
			SetTargetOriginalPosition ();
			MoveCameraInstant ();
		}
	}
	
	
	public override void ResetTarget ()
	{
		if (targetIsPlayer && GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>() && GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>().GetPlayerStart () != null)
		{
			target = GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>().GetPlayerStart ().transform;
		}
	}
	
	
	public void SwitchTarget (Transform _target)
	{
		target = _target;
		originalTargetPosition = Vector3.zero;
	}
	
	
	private void Update ()
	{
		if (target)
		{
			SetTargetOriginalPosition ();
			MoveCamera ();
		}
	}
	
	
	private void SetTargetOriginalPosition ()
	{
		if (originalTargetPosition == Vector3.zero)
		{
			if (actFromDefaultPlayerStart)
			{
				if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>() && GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>().defaultPlayerStart != null)
				{
					originalTargetPosition = GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>().defaultPlayerStart.transform.position;
				}
				else
				{
					originalTargetPosition = target.transform.position;
				}
			}
			else
			{
				originalTargetPosition = target.transform.position;
			}
		}
	}
	
	
	private void TrackTarget2D_X ()
	{
		if (target.transform.position.x < (transform.position.x - xFreedom))
		{
			desiredPosition.x = target.transform.position.x + xFreedom;
		}
		else if (target.transform.position.x > (transform.position.x + xFreedom))
		{
			desiredPosition.x = target.transform.position.x - xFreedom;
		}
	}
	
	
	private void TrackTarget2D_Z ()
	{
		if (target.transform.position.z < (transform.position.z - zFreedom))
		{
			desiredPosition.z = target.transform.position.z + zFreedom;
		}
		else if (target.transform.position.z > (transform.position.z + zFreedom))
		{
			desiredPosition.z = target.transform.position.z -zFreedom;
		}
	}
	
	
	private float GetDesiredPosition (float originalValue, float gradient, float offset, CameraLocConstrainType constrainType )
	{
		float desiredPosition = originalValue + offset;
		
		if (constrainType == CameraLocConstrainType.TargetX)
		{
			desiredPosition += (target.transform.position.x - originalTargetPosition.x) * gradient;
		}
		
		else if (constrainType == CameraLocConstrainType.TargetZ)
		{
			desiredPosition += (target.transform.position.z - originalTargetPosition.z) * gradient;
		}
		
		else if (constrainType == CameraLocConstrainType.TargetIntoScreen)
		{
			desiredPosition += (PositionRelativeToCamera (originalTargetPosition).x - PositionRelativeToCamera (target.position).x) * gradient;
		}
		
		else if (constrainType == CameraLocConstrainType.TargetAcrossScreen)
		{
			desiredPosition += (PositionRelativeToCamera (originalTargetPosition).z - PositionRelativeToCamera (target.position).z) * gradient;
		}
		
		return desiredPosition;
	}
	
	
	private void MoveCamera ()
	{
		SetDesired ();
		
		if (!lockXLocAxis || !lockYLocAxis || !lockZLocAxis)
		{
			transform.position = Vector3.Lerp (transform.position, desiredPosition, Time.deltaTime * dampSpeed);
		}
		
		if (!lockFOV)
		{
			this.camera.fieldOfView = Mathf.Lerp (this.camera.fieldOfView, desiredFOV, Time.deltaTime * dampSpeed);
		}
		
		if (!lockYRotAxis)
		{
			if (yRotConstrainType == CameraRotConstrainType.LookAtTarget)
			{
				if (target)
				{
					Vector3 lookAtPos = target.position;
					lookAtPos.y += targetHeight;
					lookAtPos.x += targetXOffset;
					lookAtPos.z += targetZOffset;
					
					// Look at and dampen the rotation
					Quaternion rotation = Quaternion.LookRotation (lookAtPos - transform.position);
					
					transform.rotation = Quaternion.Slerp (transform.rotation, rotation, Time.deltaTime * dampSpeed);
					
				}
				else if (!targetIsPlayer)
				{
					Debug.LogWarning (this.name + " has no target");
				}
			}
			else
			{
				float newRotation = Mathf.Lerp (transform.eulerAngles.y, desiredRotation, Time.deltaTime * dampSpeed);
				transform.eulerAngles = new Vector3 (transform.eulerAngles.x, newRotation, transform.eulerAngles.z);
			}
		}
	}
	
	
	public override void MoveCameraInstant ()
	{
		if (targetIsPlayer && GameObject.FindWithTag (Tags.player))
		{
			target = GameObject.FindWithTag (Tags.player).transform;
		}
		
		SetDesired ();
		
		if (!lockXLocAxis || !lockYLocAxis || !lockZLocAxis)
		{
			transform.position = desiredPosition;
		}
		
		if (!lockFOV)
		{
			this.camera.fieldOfView = desiredFOV;
		}
		
		if (!lockYRotAxis)
		{
			if (yRotConstrainType == CameraRotConstrainType.LookAtTarget)
			{
				if (target)
				{
					Vector3 lookAtPos = target.position;
					lookAtPos.y += targetHeight;
					lookAtPos.x += targetXOffset;
					lookAtPos.z += targetZOffset;
					
					Quaternion rotation = Quaternion.LookRotation (lookAtPos - transform.position);
					transform.rotation = rotation;
				}
			}
			else
			{
				transform.eulerAngles = new Vector3 (transform.eulerAngles.x, desiredRotation, transform.eulerAngles.z);
			}
		}
	}
	
	
	private void SetDesired ()
	{
		if (lockXLocAxis)
		{
			desiredPosition.x = transform.position.x;
		}
		else
		{
			if (target)
			{
				if (xLocConstrainType == CameraLocConstrainType.SideScrolling)
				{
					TrackTarget2D_X ();
				}
				else
				{
					desiredPosition.x = GetDesiredPosition (originalPosition.x, xGradient, xOffset, xLocConstrainType);
				}
			}
			
			if (limitX)
			{
				desiredPosition.x = ConstrainAxis (desiredPosition.x, constrainX);
			}
		}
		
		if (lockYLocAxis)
		{
			desiredPosition.y = transform.position.y;
		}
		else
		{
			if (target)
			{
				desiredPosition.y = originalPosition.y + yOffsetLoc + ((target.transform.position.y - originalTargetPosition.y) * yGradientLoc);
			}
			
			if (limitYLoc)
			{
				desiredPosition.y = ConstrainAxis (desiredPosition.y, constrainYLoc);
			}
		}
		
		if (lockYRotAxis)
		{
			desiredRotation = 0f;
		}
		else
		{
			if (target)
			{
				desiredRotation = GetDesiredPosition (originalRotation, yGradient, yOffset, (CameraLocConstrainType) yRotConstrainType);
			}
			
			if (limitY)
			{
				desiredRotation = ConstrainAxis (desiredRotation, constrainY);
			}
			
		}
		
		if (lockZLocAxis)
		{
			desiredPosition.z = transform.position.z;
		}
		else
		{
			if (target)
			{
				if (zLocConstrainType == CameraLocConstrainType.SideScrolling)
				{
					TrackTarget2D_Z ();
				}
				else
				{
					desiredPosition.z = GetDesiredPosition (originalPosition.z, zGradient, zOffset, zLocConstrainType);
				}
			}
			
			if (limitZ)
			{
				desiredPosition.z = ConstrainAxis (desiredPosition.z, constrainZ);
			}
		}
		
		if (lockFOV)
		{
			desiredFOV = this.camera.fieldOfView;
		}
		else
		{
			if (target)
			{
				desiredFOV = GetDesiredPosition (originalFOV, FOVGradient, FOVOffset, CameraLocConstrainType.TargetIntoScreen);
			}
			
			if (limitFOV)
			{
				desiredFOV = ConstrainAxis (desiredFOV, constrainFOV);
			}
		}
		
	}
	
}