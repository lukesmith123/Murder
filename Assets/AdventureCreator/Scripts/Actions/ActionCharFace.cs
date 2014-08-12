/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionCharFace.cs"
 * 
 *	This action is used to make characters turn to face GameObjects.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionCharFace : Action
{

	public int charToMoveID = 0;
	public int faceObjectID = 0;

	public bool isInstant;
	public Char charToMove;
	public GameObject faceObject;
	public bool copyRotation;
	public bool facePlayer;
	
	public bool isPlayer;
	public bool lookUpDown;

	public ActionCharFace ()
	{
		this.isDisplayed = true;
		title = "Character: Face object";
	}


	private void GetAssetFile ()
	{
		if (isPlayer)
		{
			if (GameObject.FindWithTag (Tags.player) && GameObject.FindWithTag (Tags.player).GetComponent <Player>())
			{
				charToMove = GameObject.FindWithTag (Tags.player).GetComponent <Player>();
			}
		}
		else if (facePlayer)
		{
			if (GameObject.FindWithTag (Tags.player))
			{
				faceObject = GameObject.FindWithTag (Tags.player);
			}
		}
		
		if (isAssetFile)
		{
			if (!isPlayer && charToMoveID != 0)
			{
				// Attempt to find the correct scene object
				ConstantID idObject = Serializer.returnComponent <ConstantID> (charToMoveID);
				if (idObject != null && idObject.GetComponent <Char>())
				{
					charToMove = idObject.GetComponent <Char>();
				}
			}
			if (!facePlayer && faceObjectID != 0)
			{
				// Attempt to find the correct scene object
				ConstantID idObject = Serializer.returnComponent <ConstantID> (faceObjectID);
				if (idObject != null)
				{
					faceObject = idObject.gameObject;
				}
			}
		}
	}

	
	override public float Run ()
	{
		if (!isRunning)
		{
			isRunning = true;
			GetAssetFile ();

			if (charToMove && faceObject)
			{
				FirstPersonCamera firstPersonCamera = null;
				if (GameObject.FindWithTag (Tags.firstPersonCamera) && GameObject.FindWithTag (Tags.firstPersonCamera).GetComponent <FirstPersonCamera>())
				{
					firstPersonCamera = GameObject.FindWithTag (Tags.firstPersonCamera).GetComponent <FirstPersonCamera>();
				}
				
				SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;

				Vector3 lookVector = faceObject.transform.position - charToMove.transform.position;
				if (copyRotation)
				{
					lookVector = faceObject.transform.forward;
				}
				else if (settingsManager.ActInScreenSpace ())
				{
					lookVector = AdvGame.GetScreenDirection (charToMove.transform.position, faceObject.transform.position);
				}
				lookVector.y = 0;
				
				if (settingsManager && settingsManager.movementMethod == MovementMethod.FirstPerson && lookUpDown)
				{
					if (firstPersonCamera)
					{
						firstPersonCamera.SetTilt (faceObject.transform.position, isInstant);
					}
					else
					{
						Debug.LogWarning ("Cannot tilt player, since no FirstPersonCamera script was found");
					}
				}
				
				if (isInstant)
				{
					charToMove.SetLookDirection (lookVector, true);
					return 0f;
				}
				else
				{
					charToMove.Halt ();
					charToMove.SetLookDirection (lookVector, false);
					
					if (willWait)
					{
						return (defaultPauseTime);
					}
					else
					{
						return 0f;
					}
				}
			}

			return 0f;
		}
		else
		{
			if (!charToMove.IsTurning ())
			{
				isRunning = false;
				return 0f;
			}
			else
			{
				return (defaultPauseTime);
			}
		}
	}


	override public void Skip ()
	{
		GetAssetFile ();
		
		if (charToMove && faceObject)
		{
			FirstPersonCamera firstPersonCamera = null;
			if (GameObject.FindWithTag (Tags.firstPersonCamera) && GameObject.FindWithTag (Tags.firstPersonCamera).GetComponent <FirstPersonCamera>())
			{
				firstPersonCamera = GameObject.FindWithTag (Tags.firstPersonCamera).GetComponent <FirstPersonCamera>();
			}
			
			SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
			
			Vector3 lookVector = faceObject.transform.position - charToMove.transform.position;
			if (copyRotation)
			{
				lookVector = faceObject.transform.forward;
			}
			else if (settingsManager.ActInScreenSpace ())
			{
				lookVector = AdvGame.GetScreenDirection (charToMove.transform.position, faceObject.transform.position);
			}
			lookVector.y = 0;
			
			if (settingsManager && settingsManager.movementMethod == MovementMethod.FirstPerson && lookUpDown)
			{
				if (firstPersonCamera)
				{
					firstPersonCamera.SetTilt (faceObject.transform.position, true);
				}
			}
			
			charToMove.SetLookDirection (lookVector, true);
		}
	}

	
	#if UNITY_EDITOR

	override public void ShowGUI ()
	{
		isPlayer = EditorGUILayout.Toggle ("Affect Player?", isPlayer);
		if (!isPlayer)
		{
			charToMove = (Char) EditorGUILayout.ObjectField ("Character to turn:", charToMove, typeof(Char), true);
			facePlayer = EditorGUILayout.Toggle ("Face player?", facePlayer);

			if (charToMove && charToMove.GetComponent <ConstantID>())
			{
				charToMoveID = charToMove.GetComponent <ConstantID>().constantID;
			}
		}
		else
		{
			facePlayer = false;
			
			SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
			if (settingsManager && settingsManager.movementMethod == MovementMethod.FirstPerson)
			{
				lookUpDown = EditorGUILayout.Toggle ("1st-person head tilt?", lookUpDown);
			}
		}
			
		if (!facePlayer)
		{
			if (isAssetFile)
			{
				faceObjectID = EditorGUILayout.IntField ("Object to face (ID):", faceObjectID);
			}
			else
			{
				faceObject = (GameObject) EditorGUILayout.ObjectField ("Object to face:", faceObject, typeof(GameObject), true);
			}
		}
		copyRotation = EditorGUILayout.Toggle ("Use object's rotation?", copyRotation);

		isInstant = EditorGUILayout.Toggle ("Is instant?", isInstant);
		if (!isInstant)
		{
			willWait = EditorGUILayout.Toggle ("Pause until finish?", willWait);
		}

		AfterRunningOption ();
	}

	
	override public string SetLabel ()
	{
		string labelAdd = "";
		
		if (charToMove && faceObject)
		{
			labelAdd = " (" + charToMove.name + " to " + faceObject.name + ")";
		}
		else if (isPlayer && faceObject)
		{
			labelAdd = " (Player to " + faceObject.name + ")";
		}
		
		return labelAdd;
	}

	#endif
	
}