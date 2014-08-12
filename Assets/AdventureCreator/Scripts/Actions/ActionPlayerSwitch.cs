/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionPlayerSwitch.cs"
 * 
 *	This action causes a different Player prefab
 *	to be controlled.  Note that only one Player prefab
 *  can exist in a scene at any one time - for two player
 *  "characters" to be present, one must be a swapped-out
 * 	NPC instead.
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
public class ActionPlayerSwitch : Action
{
	
	public int playerID;
	public int playerNumber;

	public NewPlayerPosition newPlayerPosition = NewPlayerPosition.ReplaceNPC;
	public OldPlayer oldPlayer = OldPlayer.RemoveFromScene;

	public int newPlayerScene;

	public int oldPlayerNPC_ID;
	public NPC oldPlayerNPC;

	public int newPlayerNPC_ID;
	public NPC newPlayerNPC;

	public int newPlayerMarker_ID;
	public Marker newPlayerMarker;

	private SettingsManager settingsManager;


	public ActionPlayerSwitch ()
	{
		this.isDisplayed = true;
		title = "Player: Switch";
	}
	
	
	override public float Run ()
	{
		if (isAssetFile)
		{
			if (oldPlayerNPC_ID != 0)
			{
				// Attempt to find the correct scene object
				ConstantID idObject = Serializer.returnComponent <ConstantID> (oldPlayerNPC_ID);
				if (idObject != null && idObject.GetComponent <NPC>())
				{
					oldPlayerNPC = idObject.GetComponent <NPC>();
				}
				else
				{
					oldPlayerNPC = null;
				}
			}

			if (newPlayerNPC_ID != 0)
			{
				// Attempt to find the correct scene object
				ConstantID idObject = Serializer.returnComponent <ConstantID> (newPlayerNPC_ID);
				if (idObject != null && idObject.GetComponent <NPC>())
				{
					newPlayerNPC = idObject.GetComponent <NPC>();
				}
				else
				{
					newPlayerNPC = null;
				}
			}

			if (newPlayerMarker_ID != 0)
			{
				// Attempt to find the correct scene object
				ConstantID idObject = Serializer.returnComponent <ConstantID> (newPlayerMarker_ID);
				if (idObject != null && idObject.GetComponent <Marker>())
				{
					newPlayerMarker = idObject.GetComponent <Marker>();
				}
				else
				{
					newPlayerMarker = null;
				}
			}
		}

		if (!settingsManager)
		{
			settingsManager = AdvGame.GetReferences ().settingsManager;
		}

		if (settingsManager && settingsManager.playerSwitching == PlayerSwitching.Allow)
		{
			if (settingsManager.players.Count > 0 && settingsManager.players.Count > playerNumber && playerNumber > -1)
			{
				if (GameObject.FindWithTag (Tags.player).GetComponent <Player>().ID == playerID)
				{
					Debug.Log ("Cannot switch player - already controlling the desired prefab.");
					return 0f;
				}

				if (settingsManager.players[playerNumber].playerOb != null)
				{
					SaveSystem saveSystem = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>();
					saveSystem.SaveCurrentPlayerData ();

					Vector3 oldPlayerPosition = GameObject.FindWithTag (Tags.player).GetComponent <Player>().transform.position;
					Quaternion oldPlayerRotation = GameObject.FindWithTag (Tags.player).GetComponent <Player>().transform.rotation;
					Vector3 oldPlayerScale = GameObject.FindWithTag (Tags.player).GetComponent <Player>().transform.localScale;

					if (oldPlayer == OldPlayer.ReplaceWithNPC && oldPlayerNPC != null &&
					    (newPlayerPosition == NewPlayerPosition.RestorePrevious || newPlayerPosition == NewPlayerPosition.ReplaceNPC || newPlayerPosition == NewPlayerPosition.AppearAtMarker))
					{
						oldPlayerNPC.transform.position = oldPlayerPosition;
						oldPlayerNPC.transform.rotation = oldPlayerRotation;
						oldPlayerNPC.transform.localScale = oldPlayerScale;
					}

					KickStarter.ResetPlayer (settingsManager.players[playerNumber].playerOb, playerID, true);

					// Menus
					foreach (Menu menu in PlayerMenus.GetMenus ())
					{
						foreach (MenuElement element in menu.elements)
						{
							if (element is MenuInventoryBox)
							{
								MenuInventoryBox invBox = (MenuInventoryBox) element;
								invBox.ResetOffset ();
							}
						}
					}

					Player newPlayer = GameObject.FindWithTag (Tags.player).GetComponent <Player>();

					// Does data already exist?
					int sceneToLoad = Application.loadedLevel;

					if (saveSystem.DoesPlayerDataExist (playerID))
					{
						// Load player data
						sceneToLoad = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>().AssignPlayerData (playerID, !settingsManager.shareInventory);
					}
					else 
					{
						if (!settingsManager.shareInventory)
						{
							// Clear inventory
							GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>().selectedItem = null;
							GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>().localItems.Clear ();
						}
					}

					if (newPlayerPosition == NewPlayerPosition.RestorePrevious)
					{
						if (sceneToLoad != Application.loadedLevel)
						{
							saveSystem.isLoadingNewScene = true;
							GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SceneChanger>().ChangeScene (sceneToLoad, true);
						}
						else if (!saveSystem.DoesPlayerDataExist (playerID))
						{
							Debug.LogWarning ("No previous position data found for " + newPlayer.name);
						}
					}
					else if (newPlayerPosition == NewPlayerPosition.ReplaceCurrentPlayer)
					{
						newPlayer.transform.position = oldPlayerPosition;
						newPlayer.transform.rotation = oldPlayerRotation;
						newPlayer.transform.localScale = oldPlayerScale;
					}
					else if (newPlayerPosition == NewPlayerPosition.ReplaceNPC)
					{
						if (newPlayerNPC)
						{
							newPlayer.transform.position = newPlayerNPC.transform.position;
							newPlayer.transform.rotation = newPlayerNPC.transform.rotation;
							newPlayer.transform.localScale = newPlayerNPC.transform.localScale;

							newPlayerNPC.transform.position += new Vector3 (100f, -100f, 100f);
						}
					}
					else if (newPlayerPosition == NewPlayerPosition.AppearAtMarker)
					{
						if (newPlayerMarker)
						{
							newPlayer.transform.position = newPlayerMarker.transform.position;
							newPlayer.transform.rotation = newPlayerMarker.transform.rotation;
							newPlayer.transform.localScale = newPlayerMarker.transform.localScale;
						}
					}
					else if (newPlayerPosition == NewPlayerPosition.AppearInOtherScene)
					{
						if (newPlayerScene != Application.loadedLevel)
						{
							GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SceneChanger>().ChangeScene (newPlayerScene, true);
						}
					}

				}
				else
				{
					Debug.LogWarning ("Cannot switch player - no player prefabs is defined.");
				}
			}
		}

		return 0f;
	}
	
	
	#if UNITY_EDITOR
	
	override public void ShowGUI ()
	{
		if (!settingsManager)
		{
			settingsManager = AdvGame.GetReferences ().settingsManager;
		}
		
		if (!settingsManager)
		{
			return;
		}
		
		if (settingsManager.playerSwitching == PlayerSwitching.DoNotAllow)
		{
			EditorGUILayout.HelpBox ("This Action requires Player Switching to be allowed, as set in the Settings Manager.", MessageType.Info);
			return;
		}
		
		// Create a string List of the field's names (for the PopUp box)
		List<string> labelList = new List<string>();
		
		int i = 0;
		playerNumber = -1;
		
		if (settingsManager.players.Count > 0)
		{
			
			foreach (PlayerPrefab playerPrefab in settingsManager.players)
			{
				if (playerPrefab.playerOb != null)
				{
					labelList.Add (playerPrefab.playerOb.name);
				}
				else
				{
					labelList.Add ("(Undefined prefab)");
				}
				
				// If a player has been removed, make sure selected player is still valid
				if (playerPrefab.ID == playerID)
				{
					playerNumber = i;
				}
				
				i++;
			}
			
			if (playerNumber == -1)
			{
				// Wasn't found (item was possibly deleted), so revert to zero
				Debug.LogWarning ("Previously chosen Player no longer exists!");
				
				playerNumber = 0;
				playerID = 0;
			}
			
			playerNumber = EditorGUILayout.Popup ("New Player:", playerNumber, labelList.ToArray());
			playerID = settingsManager.players[playerNumber].ID;

			newPlayerPosition = (NewPlayerPosition) EditorGUILayout.EnumPopup ("New Player position:", newPlayerPosition);

			if (newPlayerPosition == NewPlayerPosition.ReplaceNPC)
			{
				newPlayerNPC = (NPC) EditorGUILayout.ObjectField ("NPC to be replaced:", newPlayerNPC, typeof (NPC), true);
				
				if (newPlayerNPC && newPlayerNPC.GetComponent <ConstantID>())
				{
					newPlayerNPC_ID = newPlayerNPC.GetComponent <ConstantID>().constantID;
				}
			}
			else if (newPlayerPosition == NewPlayerPosition.AppearAtMarker)
			{
				if (isAssetFile)
				{
					newPlayerMarker_ID = EditorGUILayout.IntField ("Marker to appear at (ID):", newPlayerMarker_ID);
				}
				else
				{
					newPlayerMarker = (Marker) EditorGUILayout.ObjectField ("Marker to appear at:", newPlayerMarker, typeof (Marker), true);
				}
			}
			else if (newPlayerPosition == NewPlayerPosition.AppearInOtherScene)
			{
				newPlayerScene = EditorGUILayout.IntField ("Scene to appear in:", newPlayerScene);
			}

			if (newPlayerPosition == NewPlayerPosition.RestorePrevious || newPlayerPosition == NewPlayerPosition.ReplaceNPC || newPlayerPosition == NewPlayerPosition.AppearAtMarker)
			{
				EditorGUILayout.Space ();
				oldPlayer = (OldPlayer) EditorGUILayout.EnumPopup ("Old Player", oldPlayer);

				if (oldPlayer == OldPlayer.ReplaceWithNPC)
				{
					oldPlayerNPC = (NPC) EditorGUILayout.ObjectField ("NPC to replace old Player:", oldPlayerNPC, typeof (NPC), true);

					if (oldPlayerNPC && oldPlayerNPC.GetComponent <ConstantID>())
					{
						oldPlayerNPC_ID = oldPlayerNPC.GetComponent <ConstantID>().constantID;
					}
				}
			}
		}
		
		else
		{
			EditorGUILayout.LabelField ("No players exist!");
			playerID = -1;
			playerNumber = -1;
		}

		EditorGUILayout.Space ();
		
		AfterRunningOption ();
	}
	
	
	public override string SetLabel ()
	{
		if (!settingsManager)
		{
			settingsManager = AdvGame.GetReferences ().settingsManager;
		}

		if (settingsManager && settingsManager.playerSwitching == PlayerSwitching.Allow)
		{
			if (settingsManager.players.Count > 0 && settingsManager.players.Count > playerNumber && playerNumber > -1)
			{
				if (settingsManager.players[playerNumber].playerOb != null)
				{
					return " (" + settingsManager.players[playerNumber].playerOb.name + ")";
				}
				else
				{
					return (" (Undefined prefab");
				}
			}
		}
		
		return "";
	}
	
	#endif
	
}