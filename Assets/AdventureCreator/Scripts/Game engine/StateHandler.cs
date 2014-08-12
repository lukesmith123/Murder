/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"SceneHandler.cs"
 * 
 *	This script stores the gameState variable, which is used by
 *	other scripts to determine if the game is running normal gameplay,
 *	in a cutscene, paused, or displaying conversation options.
 * 
 */

using UnityEngine;
using AC;

public class StateHandler : MonoBehaviour
{
	
	public GameState gameState = GameState.Normal;
	
	private GameState lastGameplayState = GameState.Normal;
	private GameState lastGameplayState_Backup = GameState.Normal;

	public bool cursorIsOff;
	public bool inputIsOff;
	public bool interactionIsOff;
	public bool menuIsOff;
	public bool movementIsOff;
	public bool cameraIsOff;
	public bool triggerIsOff;
	public bool playerIsOff;

	
	private void Awake ()
	{
		DontDestroyOnLoad(this);
	}
	
	
	private void Update ()
	{
		if (gameState != GameState.Paused)
		{
			lastGameplayState = gameState;
		}
	}
	
	
	public void RestoreLastGameplayState ()
	{
		gameState = lastGameplayState;
	}
	
	
	public GameState GetLastGameplayState ()
	{
		return lastGameplayState;
	}
	
	
	public void BackupLastGameplayState ()
	{
		lastGameplayState_Backup = lastGameplayState;
	}
	
	
	public void UpdateLastGameplayState ()
	{
		lastGameplayState = lastGameplayState_Backup;
	}


	public void TurnOnAC ()
	{
		gameState = GameState.Normal;
	}
	
	
	public void TurnOffAC ()
	{
		if (GameObject.FindWithTag (Tags.gameEngine))
		{
			if (GameObject.FindWithTag (Tags.gameEngine).GetComponent <ActionListManager>())
			{
				GameObject.FindWithTag (Tags.gameEngine).GetComponent <ActionListManager>().KillAllLists ();
			}

			if (GameObject.FindWithTag (Tags.gameEngine).GetComponent <Dialog>())
			{
				GameObject.FindWithTag (Tags.gameEngine).GetComponent <Dialog>().KillDialog ();
			}
		}
		
		Moveable[] moveables = FindObjectsOfType (typeof (Moveable)) as Moveable[];
		foreach (Moveable moveable in moveables)
		{
			moveable.Kill ();
		}

		Char[] chars = FindObjectsOfType (typeof (Char)) as Char[];
		foreach (Char _char in chars)
		{
			_char.EndPath ();
		}
		
		gameState = GameState.Cutscene;
	}

}