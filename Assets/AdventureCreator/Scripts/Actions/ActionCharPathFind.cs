/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionCharPathFind.cs"
 * 
 *	This action moves characters by generating a path to a specified point.
 *	If a player is moved, the game will automatically pause.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionCharPathFind : Action
{

	public int charToMoveID = 0;
	public int markerID = 0;
	
	public Marker marker;
	public bool isPlayer;
	public Char charToMove;
	public PathSpeed speed;
	public bool pathFind = true;
	
	
	public ActionCharPathFind ()
	{
		this.isDisplayed = true;
		title = "Character: Move to point";
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
		else if (isAssetFile && charToMoveID != 0)
		{
			// Attempt to find the correct scene object
			ConstantID idObject = Serializer.returnComponent <ConstantID> (charToMoveID);
			if (idObject != null && idObject.GetComponent <Char>())
			{
				charToMove = idObject.GetComponent <Char>();
			}
		}
		
		if (isAssetFile && markerID != 0)
		{
			// Attempt to find the correct scene object
			ConstantID idObject = Serializer.returnComponent <ConstantID> (markerID);
			if (idObject != null && idObject.GetComponent <Marker>())
			{
				marker = idObject.GetComponent <Marker>();
			}
			else
			{
				marker = null;
			}
		}
	}
	
	
	override public float Run ()
	{
		if (!isRunning)
		{
			isRunning = true;
			GetAssetFile ();
			
			if (charToMove && marker)
			{
				Paths path = charToMove.GetComponent <Paths>();
				if (path == null)
				{
					Debug.LogWarning ("Cannot move a character with no Paths component");
				}
				else
				{
					if (charToMove is NPC)
					{
						NPC npcToMove = (NPC) charToMove;
						npcToMove.FollowReset ();
					}

					path.pathType = AC_PathType.ForwardOnly;
					path.pathSpeed = speed;
					path.affectY = true;

					Vector3[] pointArray;
					Vector3 targetPosition = marker.transform.position;

					SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
					if (settingsManager && settingsManager.ActInScreenSpace ())
					{
						targetPosition = AdvGame.GetScreenNavMesh (targetPosition);
					}

					if (pathFind && GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <NavigationManager>())
					{
						pointArray = GameObject.FindWithTag (Tags.gameEngine).GetComponent <NavigationManager>().navigationEngine.GetPointsArray (charToMove.transform.position, targetPosition);
					}
					else
					{
						List<Vector3> pointList = new List<Vector3>();
						pointList.Add (targetPosition);
						pointArray = pointList.ToArray ();
					}

					if (speed == PathSpeed.Walk)
					{
						charToMove.MoveAlongPoints (pointArray, false);
					}
					else
					{
						charToMove.MoveAlongPoints (pointArray, true);
					}
					
					if (willWait)
					{
						return defaultPauseTime;
					}
				}
			}

			return 0f;
		}
		else
		{
			if (charToMove.GetPath () == null)
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
		
		if (charToMove && marker)
		{
			charToMove.EndPath ();

			if (charToMove is NPC)
			{
				NPC npcToMove = (NPC) charToMove;
				npcToMove.FollowReset ();
			}
			
			Vector3[] pointArray;
			Vector3 targetPosition = marker.transform.position;
			
			SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
			if (settingsManager && settingsManager.ActInScreenSpace ())
			{
				targetPosition = AdvGame.GetScreenNavMesh (targetPosition);
			}
			
			if (pathFind && GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <NavigationManager>())
			{
				pointArray = GameObject.FindWithTag (Tags.gameEngine).GetComponent <NavigationManager>().navigationEngine.GetPointsArray (charToMove.transform.position, targetPosition);
			}
			else
			{
				List<Vector3> pointList = new List<Vector3>();
				pointList.Add (targetPosition);
				pointArray = pointList.ToArray ();
			}
			
			int i = pointArray.Length-1;
			charToMove.transform.position = pointArray [i];

			if (i>0)
			{
				charToMove.SetLookDirection (pointArray[i] - pointArray[i-1], true);
			}
		}
	}

	
	#if UNITY_EDITOR

	override public void ShowGUI ()
	{
		isPlayer = EditorGUILayout.Toggle ("Is Player?", isPlayer);

		if (!isPlayer)
		{
			charToMove = (Char) EditorGUILayout.ObjectField ("Character to move:", charToMove, typeof (Char), true);

			if (charToMove && charToMove.GetComponent <ConstantID>())
			{
				charToMoveID = charToMove.GetComponent <ConstantID>().constantID;
			}
		}

		if (isAssetFile)
		{
			markerID = EditorGUILayout.IntField ("Marker to reach (ID):", markerID);
		}
		else
		{
			marker = (Marker) EditorGUILayout.ObjectField ("Marker to reach:", marker, typeof (Marker), true);
		}
		speed = (PathSpeed) EditorGUILayout.EnumPopup ("Move speed:" , speed);
		pathFind = EditorGUILayout.Toggle ("Pathfind?", pathFind);
		willWait = EditorGUILayout.Toggle ("Pause until finish?", willWait);

		AfterRunningOption ();
	}
	
	
	override public string SetLabel ()
	{
		string labelAdd = "";
		
		if (marker)
		{
			if (charToMove)
			{
				labelAdd = " (" + charToMove.name + " to " + marker.name + ")";
			}
			else if (isPlayer)
			{
				labelAdd = " (Player to " + marker.name + ")";
			}
		}
		
		return labelAdd;
	}

	#endif
	
}