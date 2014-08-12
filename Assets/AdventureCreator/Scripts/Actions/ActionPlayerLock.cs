/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionPlayerLock.cs"
 * 
 *	This action constrains the player in various ways (movement, saving etc)
 *	In Direct control mode, the player can be assigned a path,
 *	and will only be able to move along that path during gameplay.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionPlayerLock : Action
{
	
	public LockType doUpLock = LockType.NoChange;
	public LockType doDownLock = LockType.NoChange;
	public LockType doLeftLock = LockType.NoChange;
	public LockType doRightLock = LockType.NoChange;
	
	public PlayerMoveLock doRunLock = PlayerMoveLock.NoChange;
	public LockType doInventoryLock = LockType.NoChange;
	public LockType doGravityLock = LockType.NoChange;
	public Paths movePath;

	
	public ActionPlayerLock ()
	{
		this.isDisplayed = true;
		title = "Player: Constrain";
	}
	
	
	override public float Run ()
	{
		PlayerInput playerInput = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>();
		RuntimeInventory runtimeInventory = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>();

		Player player = null;
		if (GameObject.FindWithTag (Tags.player) && GameObject.FindWithTag (Tags.player).GetComponent <Player>())
		{
			player = GameObject.FindWithTag (Tags.player).GetComponent <Player>();
		}
		
		if (playerInput)
		{
			if (AdvGame.GetReferences ().settingsManager && (AdvGame.GetReferences ().settingsManager.movementMethod == MovementMethod.PointAndClick || AdvGame.GetReferences ().settingsManager.movementMethod == MovementMethod.Drag))
			{
				doLeftLock = doUpLock;
				doRightLock = doUpLock;
				doDownLock = doUpLock;
			}

			if (doUpLock == LockType.Disabled)
			{
				playerInput.isUpLocked = true;
			}
			else if (doUpLock == LockType.Enabled)
			{
				playerInput.isUpLocked = false;
			}
	
			if (doDownLock == LockType.Disabled)
			{
				playerInput.isDownLocked = true;
			}
			else if (doDownLock == LockType.Enabled)
			{
				playerInput.isDownLocked = false;
			}
			
			if (doLeftLock == LockType.Disabled)
			{
				playerInput.isLeftLocked = true;
			}
			else if (doLeftLock == LockType.Enabled)
			{
				playerInput.isLeftLocked = false;
			}
	
			if (doRightLock == LockType.Disabled)
			{
				playerInput.isRightLocked = true;
			}
			else if (doRightLock == LockType.Enabled)
			{
				playerInput.isRightLocked = false;
			}
			
			if (doRunLock != PlayerMoveLock.NoChange)
			{
				playerInput.runLock = doRunLock;
			}
		}
		
		if (runtimeInventory)
		{
			if (doInventoryLock == LockType.Disabled)
			{
				runtimeInventory.isLocked = true;
			}
			else if (doInventoryLock == LockType.Enabled && runtimeInventory.localItems.Count > 0)
			{
				runtimeInventory.isLocked = false;		
			}
		}
		
		if (player)
		{
			if (movePath)
			{
				player.SetLockedPath (movePath);
				player.SetMoveDirectionAsForward ();
			}
			else if (player.activePath)
			{
				player.SetPath (null);
			}

			if (doGravityLock == LockType.Enabled)
			{
				player.ignoreGravity = false;
			}
			else if (doGravityLock == LockType.Disabled)
			{
				player.ignoreGravity = true;
			}
		}
		
		return 0f;
	}
	
	
	#if UNITY_EDITOR
	
	override public void ShowGUI ()
	{
		if (AdvGame.GetReferences ().settingsManager && (AdvGame.GetReferences ().settingsManager.movementMethod == MovementMethod.PointAndClick || AdvGame.GetReferences ().settingsManager.movementMethod == MovementMethod.Drag))
		{
			doUpLock = (LockType) EditorGUILayout.EnumPopup ("Movement:", doUpLock);
		}
		else
		{
			doUpLock = (LockType) EditorGUILayout.EnumPopup ("Up movement:", doUpLock);
			doDownLock = (LockType) EditorGUILayout.EnumPopup ("Down movement:", doDownLock);
			doLeftLock = (LockType) EditorGUILayout.EnumPopup ("Left movement:", doLeftLock);
			doRightLock = (LockType) EditorGUILayout.EnumPopup ("Right movement:", doRightLock);
		}

		doRunLock = (PlayerMoveLock) EditorGUILayout.EnumPopup ("Walk / run:", doRunLock);
		doInventoryLock = (LockType) EditorGUILayout.EnumPopup ("Inventory:", doInventoryLock);
		doGravityLock = (LockType) EditorGUILayout.EnumPopup ("Affected by gravity?", doGravityLock);

		movePath = (Paths) EditorGUILayout.ObjectField ("Move path:", movePath, typeof (Paths), true);
		
		AfterRunningOption ();
	}
	
	#endif

}