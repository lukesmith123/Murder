/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionCamera.cs"
 * 
 *	This action controls the MainCamera's "activeCamera",
 *	i.e., which GameCamera it is attached to.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionCamera : Action
{

	public int constantID = 0;
	public _Camera linkedCamera;
	public float transitionTime;
	public MoveMethod moveMethod;
	public bool returnToLast;

	
	public ActionCamera ()
	{
		this.isDisplayed = true;
		title = "Camera: Switch";
	}


	private void GetAssetFile ()
	{
		if (isAssetFile && constantID != 0)
		{
			// Attempt to find the correct scene object
			ConstantID idObject = Serializer.returnComponent <ConstantID> (constantID);
			if (idObject != null && idObject.GetComponent <_Camera>())
			{
				linkedCamera = idObject.GetComponent <_Camera>();
			}
			else
			{
				linkedCamera = null;
			}
		}
	}
	
	
	override public float Run ()
	{
		if (!isRunning)
		{
			isRunning = true;

			GetAssetFile ();			
			MainCamera mainCam = GameObject.FindWithTag (Tags.mainCamera).GetComponent <MainCamera>();
			
			if (mainCam)
			{
				_Camera cam = linkedCamera;
				
				if (returnToLast && mainCam.lastNavCamera)
				{
					cam = (_Camera) mainCam.lastNavCamera;
				}
				
				if (cam)
				{
					if (mainCam.attachedCamera != cam)
					{
						if (cam is GameCameraThirdPerson)
						{
							GameCameraThirdPerson tpCam = (GameCameraThirdPerson) cam;
							tpCam.ResetRotation ();
						}
						else if (cam is GameCameraAnimated)
						{
							GameCameraAnimated animCam = (GameCameraAnimated) cam;
							animCam.PlayClip ();
						}

						mainCam.SetGameCamera (cam);
						if (transitionTime > 0f)
						{
							if (linkedCamera is GameCamera25D)
							{
								mainCam.SnapToAttached ();
								Debug.LogWarning ("Switching to a 2.5D camera (" + linkedCamera.name + ") must be instantaneous.");
							}
							else
							{
								mainCam.SmoothChange (transitionTime, moveMethod);
								
								if (willWait)
								{
									return (transitionTime);
								}
							}
						}
						else
						{
							if (!returnToLast)
							{
								linkedCamera.MoveCameraInstant ();
							}
							mainCam.SnapToAttached ();

							if (linkedCamera is GameCameraAnimated && willWait)
							{
								return (defaultPauseTime);
							}
						}
					}
				}
			}
		}
		else
		{
			if (linkedCamera is GameCameraAnimated && willWait)
			{
				GameCameraAnimated animatedCamera = (GameCameraAnimated) linkedCamera;
				if (animatedCamera.isPlaying ())
				{
					return defaultPauseTime;
				}
				else
				{
					isRunning = false;
					return 0f;
				}
			}
			else
			{
				isRunning = false;
				return 0f;
			}
		}
		
		return 0f;
	}


	override public void Skip ()
	{
		GetAssetFile ();
		MainCamera mainCam = GameObject.FindWithTag (Tags.mainCamera).GetComponent <MainCamera>();
		
		if (mainCam)
		{
			_Camera cam = linkedCamera;
			
			if (returnToLast && mainCam.lastNavCamera)
			{
				cam = (_Camera) mainCam.lastNavCamera;
			}
			
			if (cam)
			{
				if (mainCam.attachedCamera != cam)
				{
					if (cam is GameCameraThirdPerson)
					{
						GameCameraThirdPerson tpCam = (GameCameraThirdPerson) cam;
						tpCam.ResetRotation ();
					}
					
					mainCam.SetGameCamera (cam);

					if (!returnToLast)
					{
						linkedCamera.MoveCameraInstant ();
					}
					mainCam.SnapToAttached ();
				}
			}
		}
	}

	
	#if UNITY_EDITOR

	override public void ShowGUI ()
	{
		bool showWaitOption = false;
		returnToLast = EditorGUILayout.Toggle ("Return to last gameplay?", returnToLast);
		
		if (!returnToLast)
		{
			if (isAssetFile)
			{
				constantID = EditorGUILayout.IntField ("New camera (ID):", constantID);
			}
			else
			{
				linkedCamera = (_Camera) EditorGUILayout.ObjectField ("New camera:", linkedCamera, typeof(_Camera), true);
				if (linkedCamera && linkedCamera is GameCameraAnimated)
				{
					GameCameraAnimated animatedCamera = (GameCameraAnimated) linkedCamera;
					if (animatedCamera.animatedCameraType == AnimatedCameraType.PlayWhenActive && transitionTime <= 0f)
					{
						showWaitOption = true;
					}
				}
			}
		}
		
		if (linkedCamera is GameCamera25D && !returnToLast)
		{
			transitionTime = 0f;
		}
		else
		{
			transitionTime = EditorGUILayout.FloatField ("Transition time (s):", transitionTime);
			
			if (transitionTime > 0f)
			{
				moveMethod = (MoveMethod) EditorGUILayout.EnumPopup ("Move method:", moveMethod);
				showWaitOption = true;
			}
		}

		if (showWaitOption)
		{
			willWait = EditorGUILayout.Toggle ("Pause until finish?", willWait);
		}
		
		AfterRunningOption ();
	}
	
	
	override public string SetLabel ()
	{
		string labelAdd = "";
		if (linkedCamera && !returnToLast)
		{
			labelAdd = " (" + linkedCamera.name + ")";
		}
		
		return labelAdd;
	}

	#endif
	
}