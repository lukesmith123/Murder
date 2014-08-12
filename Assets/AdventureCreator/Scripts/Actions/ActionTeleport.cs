/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionTeleport.cs"
 * 
 *	This action moves an object to a specified GameObject's position.
 *	Markers are helpful in this regard.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionTeleport : Action
{

	public int obToMoveID = 0;
	public int markerID = 0;

	public bool isPlayer;
	public GameObject obToMove;
	public Marker teleporter;
	public bool copyRotation;
	
	
	public ActionTeleport ()
	{
		this.isDisplayed = true;
		title = "Object: Teleport";
	}
	
	
	override public float Run ()
	{
		Transform teleporterTransform = null;

		if (teleporter)
		{
			teleporterTransform = teleporter.transform;
		}

		if (isAssetFile)
		{
			// Attempt to find the correct scene objects
			if (obToMoveID != 0)
			{
				ConstantID idObject = Serializer.returnComponent <ConstantID> (obToMoveID);
				if (idObject != null)
				{
					obToMove = idObject.gameObject;
				}
				else
				{
					obToMove = null;
				}
			}

			if (markerID != 0)
			{
				ConstantID idObject = Serializer.returnComponent <ConstantID> (markerID);
				if (idObject != null)
				{
					teleporterTransform = idObject.transform;
				}
				else
				{
					teleporterTransform = null;
				}
			}
		}

		if (isPlayer)
		{
			if (GameObject.FindWithTag (Tags.player))
			{
				obToMove = GameObject.FindWithTag ("Player");
			}
		}
		
		if (teleporterTransform && obToMove)
		{
			obToMove.transform.position = teleporterTransform.position;
			
			if (copyRotation)
			{
				if (obToMove.GetComponent <Char>())
				{
					// Is a character, so set the lookDirection, otherwise will revert back to old rotation
					obToMove.GetComponent <Char>().SetLookDirection (teleporterTransform.forward, true);
					obToMove.GetComponent <Char>().Halt ();
				}
			
				obToMove.transform.rotation = teleporterTransform.rotation;
			}
		}
		
		return 0f;
	}
	
	
	#if UNITY_EDITOR
	
	override public void ShowGUI ()
	{
		isPlayer = EditorGUILayout.Toggle ("Is Player?", isPlayer);
		if (!isPlayer)
		{
			if (isAssetFile)
			{
				obToMoveID = EditorGUILayout.IntField ("Object to move (ID):", obToMoveID);
			}
			else
			{
				obToMove = (GameObject) EditorGUILayout.ObjectField ("Object to move:", obToMove, typeof(GameObject), true);
			}
		}
			
		if (isAssetFile)
		{
			markerID = EditorGUILayout.IntField ("Teleport to (ID):", markerID);
		}
		else
		{
			teleporter = (Marker) EditorGUILayout.ObjectField ("Teleport to:", teleporter, typeof (Marker), true);
		}

		copyRotation = EditorGUILayout.Toggle ("Copy rotation?", copyRotation);
		
		AfterRunningOption ();
	}

	
	override public string SetLabel ()
	{
		string labelAdd = "";
		
		if (teleporter)
		{
			if (obToMove)
			{
				labelAdd = " (" + obToMove.name + " to " + teleporter.name + ")";
			}
			else if (isPlayer)
			{
				labelAdd = " (Player to " + teleporter.name + ")";
			}
		}
		
		return labelAdd;
	}

	#endif
}