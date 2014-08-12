/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionEndGame.cs"
 * 
 *	This Action will force the game to either
 *	restart an autosave, or quit.
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
public class ActionEndGame : Action
{
	
	public enum AC_EndGameType { QuitGame, LoadAutosave, ResetScene, RestartGame };
	public AC_EndGameType endGameType;
	public int sceneNumber;
	
	
	public ActionEndGame ()
	{
		this.isDisplayed = true;
		title = "Engine: End game";
		numSockets = 0;
	}
	
	
	override public float Run ()
	{
		if (endGameType == AC_EndGameType.QuitGame)
		{
			#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
			#else
				Application.Quit ();
			#endif
		}
		else if (endGameType == AC_EndGameType.LoadAutosave)
		{
			SaveSystem.LoadAutoSave ();
		}
		else
		{
			LevelStorage levelStorage = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <LevelStorage>();
			DestroyImmediate (GameObject.FindWithTag (Tags.player));

			if (endGameType == AC_EndGameType.RestartGame)
			{
				levelStorage.ClearAllLevelData ();
				levelStorage.GetComponent <RuntimeInventory>().Awake ();
				levelStorage.GetComponent <RuntimeVariables>().Awake ();

				Application.LoadLevel (sceneNumber);
			}
			else if (endGameType == AC_EndGameType.ResetScene)
			{
				levelStorage.ClearCurrentLevelData ();

				Application.LoadLevel (Application.loadedLevel);
			}
		}

		return 0f;
	}
	
	
	override public int End (List<Action> actions)
	{
		return -1;
	}
	
	
	#if UNITY_EDITOR

	override public void ShowGUI ()
	{
		endGameType = (AC_EndGameType) EditorGUILayout.EnumPopup ("Command:", endGameType);

		if (endGameType == AC_EndGameType.RestartGame)
		{
			sceneNumber = EditorGUILayout.IntField ("Scene to restart to:", sceneNumber);
		}
	}
	

	public override string SetLabel ()
	{
		return (" (" + endGameType.ToString () + ")");
	}

	#endif
	
}