/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionCameraCrossfade.cs"
 * 
 *	This action crossfades the MainCamera from one
 *	GameCamera to another.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionCameraCrossfade : Action
{
	
	public int constantID = 0;
	public _Camera linkedCamera;
	public float transitionTime;

	
	public ActionCameraCrossfade ()
	{
		this.isDisplayed = true;
		title = "Camera: Crossfade";
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
			
			if (mainCam != null && linkedCamera != null && mainCam.attachedCamera != linkedCamera)
			{
				if (linkedCamera is GameCameraThirdPerson)
				{
					GameCameraThirdPerson tpCam = (GameCameraThirdPerson) linkedCamera;
					tpCam.ResetRotation ();
				}
				else if (linkedCamera is GameCameraAnimated)
				{
					GameCameraAnimated animCam = (GameCameraAnimated) linkedCamera;
					animCam.PlayClip ();
				}
				
				linkedCamera.MoveCameraInstant ();
				mainCam.Crossfade (transitionTime, linkedCamera);
					
				if (transitionTime > 0f && willWait)
				{
					return (transitionTime);
				}
			}
		}
		else
		{
			isRunning = false;
		}
		
		return 0f;
	}
	
	
	override public void Skip ()
	{
		GetAssetFile ();
		
		MainCamera mainCam = GameObject.FindWithTag (Tags.mainCamera).GetComponent <MainCamera>();
		
		if (mainCam != null && linkedCamera != null && mainCam.attachedCamera != linkedCamera)
		{
			if (linkedCamera is GameCameraThirdPerson)
			{
				GameCameraThirdPerson tpCam = (GameCameraThirdPerson) linkedCamera;
				tpCam.ResetRotation ();
			}
			else if (linkedCamera is GameCameraAnimated)
			{
				GameCameraAnimated animCam = (GameCameraAnimated) linkedCamera;
				animCam.PlayClip ();
			}
			
			mainCam.SetGameCamera (linkedCamera);
			linkedCamera.MoveCameraInstant ();
			mainCam.SnapToAttached ();
		}
	}


	#if UNITY_EDITOR
	
	override public void ShowGUI ()
	{
		if (isAssetFile)
		{
			constantID = EditorGUILayout.IntField ("New camera (ID):", constantID);
		}
		else
		{
			linkedCamera = (_Camera) EditorGUILayout.ObjectField ("New camera:", linkedCamera, typeof(_Camera), true);
		}

		transitionTime = EditorGUILayout.FloatField ("Transition time (s):", transitionTime);
		willWait = EditorGUILayout.Toggle ("Pause until finish?", willWait);

		AfterRunningOption ();
	}
	
	
	override public string SetLabel ()
	{
		if (linkedCamera != null)
		{
			return (" (" + linkedCamera.name + ")");
		}
		return "";
	}
	
	#endif
	
}