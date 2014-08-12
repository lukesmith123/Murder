/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"SaveSystem.cs"
 * 
 *	This script processes saved game data to and from the scene objects.
 * 
 * 	It is partially based on Zumwalt's code here:
 * 	http://wiki.unity3d.com/index.php?title=Save_and_Load_from_XML
 *  and uses functions by Nitin Pande:
 *  http://www.eggheadcafe.com/articles/system.xml.xmlserialization.asp 
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AC;

public class SaveSystem : MonoBehaviour
{
	
	public bool isLoadingNewScene { get; set; }

	#if !UNITY_WEBPLAYER
	private string saveDirectory = Application.persistentDataPath;
	private string saveExtention = ".save";
	#endif
	
	private SaveData saveData;
	private LevelStorage levelStorage;

	
	private void Awake ()
	{
		levelStorage = this.GetComponent <LevelStorage>();
	}
	

	public static SaveMethod GetSaveMethod ()
	{
		#if UNITY_IPHONE || UNITY_WP8
		return SaveMethod.XML;
		#else
		return SaveMethod.Binary;
		#endif
	}


	public static void LoadAutoSave ()
	{
		if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>())
		{
			SaveSystem saveSystem = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>();
			
			if (File.Exists (saveSystem.GetSaveFileName (0)))
			{
				saveSystem.LoadSaveGame (0);
			}
			else
			{
				Debug.LogWarning ("Could not load game: file " + saveSystem.GetSaveFileName (0) + " does not exist.");
			}
		}
	}
	
	
	public static void LoadGame (int slot)
	{
		if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>())
		{
			GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>().LoadSaveGame (slot);
		}
	}
	
	
	public void LoadSaveGame (int slot)
	{
		if (!HasAutoSave ())
		{
			slot ++;
		}

		string allData = "";

		#if UNITY_WEBPLAYER

		allData = Serializer.LoadSaveFile (GetSaveFileName (slot));

		#else

		if (File.Exists (GetSaveFileName (slot)))
		{
			allData = Serializer.LoadSaveFile (GetSaveFileName (slot));
		}
		else
		{
			Debug.LogWarning ("Could not load game: file " + GetSaveFileName (slot) + " does not exist.");
		}

		#endif

		if (allData.ToString () != "")
		{
			string mainData;
			string roomData;
			
			int divider = allData.IndexOf ("||");
			mainData = allData.Substring (0, divider);
			roomData = allData.Substring (divider + 2);
			
			if (SaveSystem.GetSaveMethod () == SaveMethod.XML)
			{
				saveData = (SaveData) Serializer.DeserializeObjectXML <SaveData> (mainData);
				levelStorage.allLevelData = (List<SingleLevelData>) Serializer.DeserializeObjectXML <List<SingleLevelData>> (roomData);
			}
			else
			{
				saveData = Serializer.DeserializeObjectBinary <SaveData> (mainData);
				levelStorage.allLevelData = Serializer.DeserializeRoom (roomData);
			}

			saveData.mainData.isLoadingGame = true;
			
			// Stop any current-running ActionLists, dialogs and interactions
			KillActionLists ();

			// If player has changed, destroy the old one and load in the new one
			SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
			if (settingsManager.playerSwitching == PlayerSwitching.Allow)
			{
				Player player = GameObject.FindWithTag (Tags.player).GetComponent <Player>();
				if (player.ID != saveData.mainData.currentPlayerID)
				{
					KickStarter.ResetPlayer (GetPlayerByID (saveData.mainData.currentPlayerID), saveData.mainData.currentPlayerID, true);
				}
			}

			int newScene = GetPlayerScene (saveData.mainData.currentPlayerID, saveData.playerData);
			
			// Load correct scene
			if (newScene != Application.loadedLevel)
			{
				isLoadingNewScene = true;
				
				if (this.GetComponent <SceneChanger>())
				{
					SceneChanger sceneChanger = this.GetComponent <SceneChanger> ();
					sceneChanger.ChangeScene (newScene, false);
				}
			}
			else
			{
				// Already in the scene
				Sound[] sounds = FindObjectsOfType (typeof (Sound)) as Sound[];
				foreach (Sound sound in sounds)
				{
					if (sound.GetComponent <AudioSource>())
					{
						if (sound.soundType != SoundType.Music && !sound.GetComponent <AudioSource>().loop)
						{
							sound.Stop ();
						}
					}
				}

				OnLevelWasLoaded ();
			}
		}
	}


	private Player GetPlayerByID (int id)
	{
		SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;

		foreach (PlayerPrefab playerPrefab in settingsManager.players)
		{
			if (playerPrefab.ID == id)
			{
				if (playerPrefab.playerOb)
				{
					return playerPrefab.playerOb;
				}

				return null;
			}
		}

		return null;
	}


	private int GetPlayerScene (int playerID, List<PlayerData> _playerData)
	{
		SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
		if (settingsManager.playerSwitching == PlayerSwitching.DoNotAllow)
		{
			if (_playerData.Count > 0)
			{
				return _playerData[0].currentScene;
			}
		}
		else
		{
			foreach (PlayerData _data in _playerData)
			{
				if (_data.playerID == playerID)
				{
					return (_data.currentScene);
				}
			}
		}

		return Application.loadedLevel;
	}
	
	
	private void OnLevelWasLoaded ()
	{
		if (saveData != null && saveData.mainData.isLoadingGame)
		{
			if (GameObject.FindWithTag (Tags.gameEngine))
			{
				if (GameObject.FindWithTag (Tags.gameEngine).GetComponent <Dialog>())
				{
					Dialog dialog = GameObject.FindWithTag (Tags.gameEngine).GetComponent <Dialog>();
					dialog.KillDialog ();
				}
				
				if (GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInteraction>())
				{
					PlayerInteraction playerInteraction = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInteraction>();
					playerInteraction.StopInteraction ();
				}
			}
				
			ReturnMainData ();
			levelStorage.ReturnCurrentLevelData ();

			saveData.mainData.isLoadingGame = false;
		}

		if (GetComponent <RuntimeInventory>())
	    {
			GetComponent <RuntimeInventory>().RemoveRecipes ();
		}
	}
	
	
	public static void SaveNewGame ()
	{
		if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>())
		{
			GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>().SaveNewSaveGame ();
		}
	}
	
	
	public void SaveNewSaveGame ()
	{
		int slot = GetNumSlots ();
		SaveGame (slot);
	}
	
	
	public static void SaveGame (int slot)
	{
		if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>())
		{
			GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>().SaveSaveGame (slot);
		}
	}
	
	
	public void SaveSaveGame (int slot)
	{
		if (!HasAutoSave () || slot == -1)
		{
			slot ++;
		}
		
		levelStorage.StoreCurrentLevelData ();
		
		Player player = null;
		if (GameObject.FindWithTag (Tags.player) && GameObject.FindWithTag (Tags.player).GetComponent <Player>())
		{
			player = GameObject.FindWithTag (Tags.player).GetComponent <Player>();
		}

		PlayerInput playerInput = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>();
		MainCamera mainCamera = GameObject.FindWithTag (Tags.mainCamera).GetComponent <MainCamera>();
		RuntimeInventory runtimeInventory = this.GetComponent <RuntimeInventory>();
		SceneChanger sceneChanger = this.GetComponent <SceneChanger>();
		SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
		StateHandler stateHandler = this.GetComponent <StateHandler>();

		if (playerInput && mainCamera && runtimeInventory && sceneChanger && settingsManager && stateHandler)
		{
			if (saveData != null && saveData.playerData != null && saveData.playerData.Count > 0)
			{
				foreach (PlayerData _data in saveData.playerData)
				{
					if (_data.playerID == player.ID)
					{
						saveData.playerData.Remove (_data);
						break;
					}
				}
			}
			else
			{
				saveData = new SaveData ();
				saveData.mainData = new MainData ();
				saveData.playerData = new List<PlayerData>();
			}

			PlayerData playerData = SavePlayerData (player);
			saveData.playerData.Add (playerData);

			// Main data
			saveData.mainData.cursorIsOff = stateHandler.cursorIsOff;
			saveData.mainData.inputIsOff = stateHandler.inputIsOff;
			saveData.mainData.interactionIsOff = stateHandler.interactionIsOff;
			saveData.mainData.menuIsOff = stateHandler.menuIsOff;
			saveData.mainData.movementIsOff = stateHandler.movementIsOff;
			saveData.mainData.cameraIsOff = stateHandler.cameraIsOff;
			saveData.mainData.triggerIsOff = stateHandler.triggerIsOff;
			saveData.mainData.playerIsOff = stateHandler.playerIsOff;

			if (player != null)
			{
				saveData.mainData.currentPlayerID = player.ID;
			}
			else
			{
				saveData.mainData.currentPlayerID = 0;
			}

			saveData.mainData.timeScale = playerInput.timeScale;

			if (playerInput.activeArrows)
			{
				saveData.mainData.activeArrows = Serializer.GetConstantID (playerInput.activeArrows.gameObject);
			}
			
			if (playerInput.activeConversation)
			{
				saveData.mainData.activeConversation = Serializer.GetConstantID (playerInput.activeConversation.gameObject);
			}
			
			if (mainCamera.attachedCamera)
			{
				saveData.mainData.gameCamera = Serializer.GetConstantID (mainCamera.attachedCamera.gameObject);
			}
			
			mainCamera.StopShaking ();
			saveData.mainData.mainCameraLocX = mainCamera.transform.position.x;
			saveData.mainData.mainCameraLocY = mainCamera.transform.position.y;
			saveData.mainData.mainCameraLocZ = mainCamera.transform.position.z;
			
			saveData.mainData.mainCameraRotX = mainCamera.transform.eulerAngles.x;
			saveData.mainData.mainCameraRotY = mainCamera.transform.eulerAngles.y;
			saveData.mainData.mainCameraRotZ = mainCamera.transform.eulerAngles.z;

			saveData.mainData.isSplitScreen = mainCamera.isSplitScreen;
			if (mainCamera.isSplitScreen)
			{
				saveData.mainData.isTopLeftSplit = mainCamera.isTopLeftSplit;
				if (mainCamera.splitOrientation == MenuOrientation.Vertical)
				{
					saveData.mainData.splitIsVertical = true;
				}
				else
				{
					saveData.mainData.splitIsVertical = false;
				}
				if (mainCamera.splitCamera && mainCamera.splitCamera.GetComponent <ConstantID>())
				{
					saveData.mainData.splitCameraID = mainCamera.splitCamera.GetComponent <ConstantID>().constantID;
				}
				else
				{
					saveData.mainData.splitCameraID = 0;
				}
			}

			if (runtimeInventory.selectedItem != null)
			{
				saveData.mainData.selectedInventoryID = runtimeInventory.selectedItem.id;
			}
			else
			{
				saveData.mainData.selectedInventoryID = -1;
			}
			RuntimeVariables.DownloadAll ();
			saveData.mainData.runtimeVariablesData = SaveSystem.CreateVariablesData (RuntimeVariables.GetAllVars (), false, VariableLocation.Global);

			saveData.mainData.menuLockData = CreateMenuLockData (PlayerMenus.GetMenus ());
			saveData.mainData.menuElementVisibilityData = CreateMenuElementVisibilityData (PlayerMenus.GetMenus ());
			saveData.mainData.menuJournalData = CreateMenuJournalData (PlayerMenus.GetMenus ());
			
			string mainData = "";
			string levelData = "";
			
			if (SaveSystem.GetSaveMethod () == SaveMethod.XML)
			{
				mainData = Serializer.SerializeObjectXML <SaveData> (saveData);
				levelData = Serializer.SerializeObjectXML <List<SingleLevelData>> (levelStorage.allLevelData);
			}
			else
			{
				mainData = Serializer.SerializeObjectBinary (saveData);
				levelData = Serializer.SerializeObjectBinary (levelStorage.allLevelData);
			}
			string allData = mainData + "||" + levelData;
	
			Serializer.CreateSaveFile (GetSaveFileName (slot), allData);

		}
		else
		{
			if (playerInput == null)
			{
				Debug.LogWarning ("Save failed - no PlayerInput found.");
			}
			if (mainCamera == null)
			{
				Debug.LogWarning ("Save failed - no MainCamera found.");
			}
			if (runtimeInventory == null)
			{
				Debug.LogWarning ("Save failed - no RuntimeInventory found.");
			}
			if (sceneChanger == null)
			{
				Debug.LogWarning ("Save failed - no SceneChanger found.");
			}
			if (settingsManager == null)
			{
				Debug.LogWarning ("Save failed - no Settings Manager found.");
			}
		}
	}


	public void SaveCurrentPlayerData ()
	{
		Player player = GameObject.FindWithTag (Tags.player).GetComponent <Player>();

		if (saveData != null && saveData.playerData != null && saveData.playerData.Count > 0)
		{
			foreach (PlayerData _data in saveData.playerData)
			{
				if (_data.playerID == player.ID)
				{
					saveData.playerData.Remove (_data);
					break;
				}
			}
		}
		else
		{
			saveData = new SaveData ();
			saveData.mainData = new MainData ();
			saveData.playerData = new List<PlayerData>();
		}
		
		PlayerData playerData = SavePlayerData (player);
		saveData.playerData.Add (playerData);
	}


	private PlayerData SavePlayerData (Player player)
	{
		PlayerData playerData = new PlayerData ();

		playerData.currentScene = Application.loadedLevel;
		SceneChanger sceneChanger = this.GetComponent <SceneChanger>();
		playerData.previousScene = sceneChanger.previousScene;

		PlayerInput playerInput = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>();
		playerData.playerUpLock = playerInput.isUpLocked;
		playerData.playerDownLock = playerInput.isDownLocked;
		playerData.playerLeftlock = playerInput.isLeftLocked;
		playerData.playerRightLock = playerInput.isRightLocked;
		playerData.playerRunLock = (int) playerInput.runLock;
		RuntimeInventory runtimeInventory = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>();
		playerData.playerInventoryLock = runtimeInventory.isLocked;
		playerData.inventoryData = CreateInventoryData (runtimeInventory);

		if (player == null)
		{
			playerData.playerPortraitGraphic = "";
			return playerData;
		}

		playerData.playerID = player.ID;
		
		playerData.playerLocX = player.transform.position.x;
		playerData.playerLocY = player.transform.position.y;
		playerData.playerLocZ = player.transform.position.z;
		playerData.playerRotY = player.transform.eulerAngles.y;
		
		playerData.playerWalkSpeed = player.walkSpeedScale;
		playerData.playerRunSpeed = player.runSpeedScale;
		
		// Animation clips
		if (player.animationEngine == AnimationEngine.Sprites2DToolkit || player.animationEngine == AnimationEngine.SpritesUnity)
		{
			playerData.playerIdleAnim = player.idleAnimSprite;
			playerData.playerWalkAnim = player.walkAnimSprite;
			playerData.playerRunAnim = player.runAnimSprite;
			playerData.playerTalkAnim = player.talkAnimSprite;
		}
		else if (player.animationEngine == AnimationEngine.Legacy)
		{
			playerData.playerIdleAnim = player.GetStandardAnimClipName (AnimStandard.Idle);
			playerData.playerWalkAnim = player.GetStandardAnimClipName (AnimStandard.Walk);
			playerData.playerRunAnim = player.GetStandardAnimClipName (AnimStandard.Run);
			playerData.playerTalkAnim = player.GetStandardAnimClipName (AnimStandard.Talk);
		}
		else if (player.animationEngine == AnimationEngine.Mecanim)
		{
			playerData.playerWalkAnim = player.moveSpeedParameter;
			playerData.playerTalkAnim = player.talkParameter;
			playerData.playerRunAnim = player.turnParameter;
		}

		// Sound
		playerData.playerWalkSound = player.GetStandardSoundName (AnimStandard.Walk);
		playerData.playerRunSound = player.GetStandardSoundName (AnimStandard.Run);

		// Portrait graphic
		if (player.portraitGraphic)
		{
			playerData.playerPortraitGraphic = player.portraitGraphic.name;
		}
		else
		{
			playerData.playerPortraitGraphic = "";
		}
		
		// Rendering
		playerData.playerLockDirection = player.lockDirection;
		playerData.playerLockScale = player.lockScale;
		if (player.spriteChild && player.spriteChild.GetComponent <FollowSortingMap>())
		{
			playerData.playerLockSorting = player.spriteChild.GetComponent <FollowSortingMap>().lockSorting;
		}
		else if (player.GetComponent <FollowSortingMap>())
		{
			playerData.playerLockSorting = player.GetComponent <FollowSortingMap>().lockSorting;
		}
		else
		{
			playerData.playerLockSorting = false;
		}
		playerData.playerSpriteDirection = player.spriteDirection;
		playerData.playerSpriteScale = player.spriteScale;
		if (player.spriteChild && player.spriteChild.renderer)
		{
			playerData.playerSortingOrder = player.spriteChild.renderer.sortingOrder;
			playerData.playerSortingLayer = player.spriteChild.renderer.sortingLayerName;
		}
		else if (player.renderer)
		{
			playerData.playerSortingOrder = player.renderer.sortingOrder;
			playerData.playerSortingLayer = player.renderer.sortingLayerName;
		}
		
		if (player.GetPath ())
		{
			playerData.playerTargetNode = player.GetTargetNode ();
			playerData.playerPrevNode = player.GetPrevNode ();
			playerData.playerIsRunning = player.isRunning;
			
			if (player.GetComponent <Paths>() && player.GetPath () == player.GetComponent <Paths>())
			{
				playerData.playerPathData = Serializer.CreatePathData (player.GetComponent <Paths>());
				playerData.playerActivePath = 0;
				playerData.playerLockedPath = false;
			}
			else
			{
				playerData.playerPathData = "";
				playerData.playerActivePath = Serializer.GetConstantID (player.GetPath ().gameObject);
				playerData.playerLockedPath = player.lockedPath;
			}
		}
		
		playerData.playerIgnoreGravity = player.ignoreGravity;

		return playerData;
	}
	
	
	public static int GetNumSlots ()
	{
		if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>())
		{
			SaveSystem saveSystem = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>();

			#if UNITY_WEBPLAYER

			int count = 0;

			for (int i=0; i<50; i++)
			{
				if (PlayerPrefs.HasKey (saveSystem.GetProjectName () + "_" + i.ToString ()))
				{
					count ++;
				}
			}

			return count;

			#else
			
				DirectoryInfo dir = new DirectoryInfo (saveSystem.saveDirectory);
				FileInfo[] info = dir.GetFiles (saveSystem.GetProjectName() + "_*" + saveSystem.saveExtention);
			
				return info.Length;
		
			#endif
		}
		
		return 0;		
	}
	
	
	public bool HasAutoSave ()
	{
		#if UNITY_WEBPLAYER

		return (PlayerPrefs.HasKey (GetProjectName () + "_0"));

		#else

		if (File.Exists (saveDirectory + Path.DirectorySeparatorChar.ToString () + GetProjectName () + "_0" + saveExtention))
		{
			return true;
		}
		
		return false;
		
		#endif
	}
	
	
	private string GetProjectName ()
	{
		SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
		if (settingsManager)
		{
			if (settingsManager.saveFileName == "")
			{
				settingsManager.saveFileName = SetProjectName ();
			}
			
			if (settingsManager.saveFileName != "")
			{
				return settingsManager.saveFileName;
			}
		}
		
		return SetProjectName ();
	}
	
	
	
	public static string SetProjectName ()
	{
		string[] s = Application.dataPath.Split ('/');
		string projectName = s[s.Length - 2];
		return projectName;
	}
	
	
	private string GetSaveFileName (int slot)
	{
		string fileName = "";

		#if UNITY_WEBPLAYER

		fileName = GetProjectName () + "_" + slot.ToString ();

		#else

		fileName = saveDirectory + Path.DirectorySeparatorChar.ToString () + GetProjectName () + "_" + slot.ToString () + saveExtention;

		#endif

		return (fileName);
	}
	
	
	private void KillActionLists ()
	{
		ActionListManager actionListManager = GameObject.FindWithTag (Tags.gameEngine).GetComponent <ActionListManager>();
		actionListManager.KillAllLists ();

		Moveable[] moveables = FindObjectsOfType (typeof (Moveable)) as Moveable[];
		foreach (Moveable moveable in moveables)
		{
			moveable.Kill ();
		}
	}

	
	public static string GetSaveSlotName (int slot)
	{
		string fileName = "Save test (01/01/2001 12:00:00)";

		if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>())
		{
			SaveSystem saveSystem = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>();

			#if UNITY_WEBPLAYER

			if (!saveSystem.HasAutoSave ())
			{
				fileName = "Save " + (slot + 1).ToString ();
			}
			else
			{
				fileName = "Save " + slot.ToString ();
			}
			
			if (slot == 0 && saveSystem.HasAutoSave())
			{
				fileName = "Autosave";
			}

			#else
		
			DirectoryInfo dir = new DirectoryInfo (saveSystem.saveDirectory);
			FileInfo[] info = dir.GetFiles (saveSystem.GetProjectName() + "_*" + saveSystem.saveExtention);
			
			if (!saveSystem.HasAutoSave ())
			{
				fileName = "Save " + (slot + 1).ToString ();
			}
			else
			{
				fileName = "Save " + slot.ToString ();
			}
			
			if (slot == 0 && saveSystem.HasAutoSave())
			{
				fileName = "Autosave";
			}

			if (slot < info.Length && AdvGame.GetReferences ().settingsManager && AdvGame.GetReferences ().settingsManager.saveTimeDisplay != SaveTimeDisplay.None)
			{
				string creationTime = info[slot].LastWriteTime.ToString ();
				if (AdvGame.GetReferences ().settingsManager.saveTimeDisplay == SaveTimeDisplay.DateOnly)
				{
					creationTime = creationTime.Substring (0, creationTime.IndexOf (" "));
				}
				fileName += " (" + creationTime + ")";
			}

			#endif
		}

		return fileName;
	}
	
	
	private void ReturnMainData ()
	{
		Player player = null;
		if (GameObject.FindWithTag (Tags.player) && GameObject.FindWithTag (Tags.player).GetComponent <Player>())
		{
			player = GameObject.FindWithTag (Tags.player).GetComponent <Player>();
		}

		PlayerInput playerInput = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>();
		MainCamera mainCamera = GameObject.FindWithTag (Tags.mainCamera).GetComponent <MainCamera>();
		RuntimeInventory runtimeInventory = this.GetComponent <RuntimeInventory>();
		SceneChanger sceneChanger = this.GetComponent <SceneChanger>();
		SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
		StateHandler stateHandler = this.GetComponent <StateHandler>();
		
		if (playerInput && mainCamera && runtimeInventory && settingsManager && stateHandler)
		{
			PlayerData playerData = new PlayerData ();

			if (settingsManager.playerSwitching == PlayerSwitching.DoNotAllow)
			{
				if (saveData.playerData.Count > 0)
				{
					playerData = saveData.playerData[0];
				}
			}
			else
			{
				foreach (PlayerData _data in saveData.playerData)
				{
					if (_data.playerID == saveData.mainData.currentPlayerID)
					{
						playerData = _data;
					}
				}
			}

			ReturnPlayerData (playerData, player);

			stateHandler.cursorIsOff = saveData.mainData.cursorIsOff;
			stateHandler.inputIsOff = saveData.mainData.inputIsOff;
			stateHandler.interactionIsOff = saveData.mainData.interactionIsOff;
			stateHandler.menuIsOff = saveData.mainData.menuIsOff;
			stateHandler.movementIsOff = saveData.mainData.movementIsOff;
			stateHandler.cameraIsOff = saveData.mainData.cameraIsOff;
			stateHandler.triggerIsOff = saveData.mainData.triggerIsOff;
			stateHandler.playerIsOff = saveData.mainData.playerIsOff;

			sceneChanger.previousScene = playerData.previousScene;
			
			playerInput.isUpLocked = playerData.playerUpLock;
			playerInput.isDownLocked = playerData.playerDownLock;
			playerInput.isLeftLocked = playerData.playerLeftlock;
			playerInput.isRightLocked = playerData.playerRightLock;
			playerInput.runLock = (PlayerMoveLock) playerData.playerRunLock;
			runtimeInventory.isLocked = playerData.playerInventoryLock;

			// Inventory
			AssignInventory (runtimeInventory, playerData.inventoryData);
			if (saveData.mainData.selectedInventoryID > -1)
			{
				runtimeInventory.SelectItemByID (saveData.mainData.selectedInventoryID);
			}
			else
			{
				runtimeInventory.SetNull ();
			}
			runtimeInventory.RemoveRecipes ();

			// Active screen arrows
			playerInput.RemoveActiveArrows ();
			ArrowPrompt loadedArrows = Serializer.returnComponent <ArrowPrompt> (saveData.mainData.activeArrows);
			if (loadedArrows)
			{
				loadedArrows.TurnOn ();
			}
			
			// Active conversation
			playerInput.activeConversation = Serializer.returnComponent <Conversation> (saveData.mainData.activeConversation);

			playerInput.timeScale = saveData.mainData.timeScale;

			mainCamera.RemoveSplitScreen ();
			mainCamera.StopShaking ();
			mainCamera.SetGameCamera (Serializer.returnComponent <_Camera> (saveData.mainData.gameCamera));
			mainCamera.ResetMoving ();
			mainCamera.transform.position = new Vector3 (saveData.mainData.mainCameraLocX, saveData.mainData.mainCameraLocY, saveData.mainData.mainCameraLocZ);
			mainCamera.transform.eulerAngles = new Vector3 (saveData.mainData.mainCameraRotX, saveData.mainData.mainCameraRotY, saveData.mainData.mainCameraRotZ);
			mainCamera.SnapToAttached ();
			mainCamera.ResetProjection ();

			if (mainCamera.attachedCamera)
			{
				mainCamera.attachedCamera.MoveCameraInstant ();
			}
			else
			{
				Debug.LogWarning ("MainCamera has no attached GameCamera");
			}

			mainCamera.isSplitScreen = saveData.mainData.isSplitScreen;
			if (mainCamera.isSplitScreen)
			{
				mainCamera.isTopLeftSplit = saveData.mainData.isTopLeftSplit;
				if (saveData.mainData.splitIsVertical)
				{
					mainCamera.splitOrientation = MenuOrientation.Vertical;
				}
				else
				{
					mainCamera.splitOrientation = MenuOrientation.Horizontal;
				}
				if (saveData.mainData.splitCameraID != 0)
				{
					_Camera splitCamera = Serializer.returnComponent <_Camera> (saveData.mainData.splitCameraID);
					if (splitCamera)
					{
						mainCamera.splitCamera = splitCamera;
					}
				}
				mainCamera.StartSplitScreen ();
			}

			// Variables
			SaveSystem.AssignVariables (saveData.mainData.runtimeVariablesData);

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
			AssignMenuLocks (PlayerMenus.GetMenus (), saveData.mainData.menuLockData);
			AssignMenuElementVisibility (PlayerMenus.GetMenus (), saveData.mainData.menuElementVisibilityData);
			AssignMenuJournals (PlayerMenus.GetMenus (), saveData.mainData.menuJournalData);

			// StateHandler
			stateHandler.gameState = GameState.Cutscene;

			// Fade in camera
			mainCamera.FadeIn (0.5f);

			Invoke ("ReturnToGameplay", 0.01f);
		}
		else
		{
			if (playerInput == null)
			{
				Debug.LogWarning ("Load failed - no PlayerInput found.");
			}
			if (mainCamera == null)
			{
				Debug.LogWarning ("Load failed - no MainCamera found.");
			}
			if (runtimeInventory == null)
			{
				Debug.LogWarning ("Load failed - no RuntimeInventory found.");
			}
			if (sceneChanger == null)
			{
				Debug.LogWarning ("Load failed - no SceneChanger found.");
			}
			if (settingsManager == null)
			{
				Debug.LogWarning ("Load failed - no Settings Manager found.");
			}
		}
	}


	public bool DoesPlayerDataExist (int ID)
	{
		if (saveData.playerData.Count > 0)
		{
			foreach (PlayerData _data in saveData.playerData)
			{
				if (_data.playerID == ID)
				{
					return true;
				}
			}
		}

		return false;
	}


	public int AssignPlayerData (int ID, bool doInventory)
	{

		if (GameObject.FindWithTag (Tags.player) && GameObject.FindWithTag (Tags.player).GetComponent <Player>())
		{
			Player player = GameObject.FindWithTag (Tags.player).GetComponent <Player>();

			if (saveData.playerData.Count > 0)
			{
				foreach (PlayerData _data in saveData.playerData)
				{
					if (_data.playerID == ID)
					{
						ReturnPlayerData (_data, player);

						PlayerInput playerInput = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>();
						RuntimeInventory runtimeInventory = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>();

						playerInput.isUpLocked = _data.playerUpLock;
						playerInput.isDownLocked = _data.playerDownLock;
						playerInput.isLeftLocked = _data.playerLeftlock;
						playerInput.isRightLocked = _data.playerRightLock;
						playerInput.runLock = (PlayerMoveLock) _data.playerRunLock;
						runtimeInventory.isLocked = _data.playerInventoryLock;
						runtimeInventory.SetNull ();

						if (doInventory)
						{
							AssignInventory (runtimeInventory, _data.inventoryData);
						}

						return (_data.currentScene);
					}
				}
			}
		}

		return Application.loadedLevel;
	}


	private void ReturnPlayerData (PlayerData playerData, Player player)
	{
		if (player == null)
		{
			return;
		}

		player.transform.position = new Vector3 (playerData.playerLocX, playerData.playerLocY, playerData.playerLocZ);
		player.transform.eulerAngles = new Vector3 (0f, playerData.playerRotY, 0f);
		player.SetLookDirection (Vector3.zero, true);
		
		player.walkSpeedScale = playerData.playerWalkSpeed;
		player.runSpeedScale = playerData.playerRunSpeed;
		
		// Animation clips
		if (player.animationEngine == AnimationEngine.Sprites2DToolkit || player.animationEngine == AnimationEngine.SpritesUnity)
		{
			player.idleAnimSprite = playerData.playerIdleAnim;
			player.walkAnimSprite = playerData.playerWalkAnim;
			player.talkAnimSprite = playerData.playerTalkAnim;
			player.runAnimSprite = playerData.playerRunAnim;
		}
		else if (player.animationEngine == AnimationEngine.Legacy)
		{
			player.AssignStandardAnimClipFromResource (AnimStandard.Idle, playerData.playerIdleAnim);
			player.AssignStandardAnimClipFromResource (AnimStandard.Walk, playerData.playerWalkAnim);
			player.AssignStandardAnimClipFromResource (AnimStandard.Talk, playerData.playerTalkAnim);
			player.AssignStandardAnimClipFromResource (AnimStandard.Run, playerData.playerRunAnim);
		}
		else if (player.animationEngine == AnimationEngine.Mecanim)
		{
			player.moveSpeedParameter = playerData.playerWalkAnim;
			player.talkParameter = playerData.playerTalkAnim;
			player.turnParameter = playerData.playerRunAnim;
		}

		// Sound
		player.AssignStandardSoundFromResource (AnimStandard.Walk, playerData.playerWalkSound);
		player.AssignStandardSoundFromResource (AnimStandard.Run, playerData.playerRunSound);

		// Portrait graphic
		player.AssignPortraitGraphicFromResource (playerData.playerPortraitGraphic);
		
		// Rendering
		player.lockDirection = playerData.playerLockDirection;
		player.lockScale = playerData.playerLockScale;
		if (player.spriteChild && player.spriteChild.GetComponent <FollowSortingMap>())
		{
			player.spriteChild.GetComponent <FollowSortingMap>().lockSorting = playerData.playerLockSorting;
		}
		else if (player.GetComponent <FollowSortingMap>())
		{
			player.GetComponent <FollowSortingMap>().lockSorting = playerData.playerLockSorting;
		}
		else
		{
			player.ReleaseSorting ();
		}
		
		if (playerData.playerLockDirection)
		{
			player.spriteDirection = playerData.playerSpriteDirection;
		}
		if (playerData.playerLockScale)
		{
			player.spriteScale = playerData.playerSpriteScale;
		}
		if (playerData.playerLockSorting)
		{
			if (player.spriteChild && player.spriteChild.renderer)
			{
				player.spriteChild.renderer.sortingOrder = playerData.playerSortingOrder;
				player.spriteChild.renderer.sortingLayerName = playerData.playerSortingLayer;
			}
			else if (player.renderer)
			{
				player.renderer.sortingOrder = playerData.playerSortingOrder;
				player.renderer.sortingLayerName = playerData.playerSortingLayer;
			}
		}
		
		// Active path
		player.Halt ();
		if (playerData.playerPathData != null && playerData.playerPathData != "" && player.GetComponent <Paths>())
		{
			Paths savedPath = player.GetComponent <Paths>();
			savedPath = Serializer.RestorePathData (savedPath, playerData.playerPathData);
			player.SetPath (savedPath, playerData.playerTargetNode, playerData.playerPrevNode);
			player.isRunning = playerData.playerIsRunning;
			player.lockedPath = false;
		}
		else if (playerData.playerActivePath != 0)
		{
			Paths savedPath = Serializer.returnComponent <Paths> (playerData.playerActivePath);
			if (savedPath)
			{
				player.lockedPath = playerData.playerLockedPath;
				
				if (player.lockedPath)
				{
					player.SetLockedPath (savedPath);
				}
				else
				{
					player.SetPath (savedPath, playerData.playerTargetNode, playerData.playerPrevNode);
				}
			}
		}

		player.ignoreGravity = playerData.playerIgnoreGravity;
	}


	private void ReturnToGameplay ()
	{
		PlayerInput playerInput = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>();
		StateHandler stateHandler = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>();

		if (playerInput.activeConversation)
		{
			stateHandler.gameState = GameState.DialogOptions;
		}
		else
		{
			stateHandler.gameState = GameState.Normal;
		}

		playerInput.ResetClick ();

		if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>())
		{
			SceneSettings sceneSettings = GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>();
			sceneSettings.OnLoad ();
		}
	}
	
	
	public static void AssignVariables (string runtimeVariablesData)
	{
		if (runtimeVariablesData == null)
		{
			return;
		}
		
		if (runtimeVariablesData.Length > 0)
		{
			string[] varsArray = runtimeVariablesData.Split ("|"[0]);
			
			foreach (string chunk in varsArray)
			{
				string[] chunkData = chunk.Split (":"[0]);
				
				int _id = 0;
				int.TryParse (chunkData[0], out _id);

				GVar var = RuntimeVariables.GetVariable (_id);
				if (var.type == VariableType.String)
				{
					string _text = chunkData[1];
					var.SetValue (_text);
				}
				else if (var.type == VariableType.Float)
				{
					float _value = 0f;
					float.TryParse (chunkData[1], out _value);
					var.SetValue (_value, SetVarMethod.SetValue);
				}
				else
				{
					int _value = 0;
					int.TryParse (chunkData[1], out _value);
					var.SetValue (_value, SetVarMethod.SetValue);
				}
			}
		}
		
		RuntimeVariables.UploadAll ();
	}

	
	private void AssignInventory (RuntimeInventory runtimeInventory, string inventoryData)
	{
		if (runtimeInventory)
		{
			runtimeInventory.localItems.Clear ();
			
			if (inventoryData != null && inventoryData.Length > 0)
			{
				string[] countArray = inventoryData.Split ("|"[0]);
				
				foreach (string chunk in countArray)
				{
					string[] chunkData = chunk.Split (":"[0]);
					
					int _id = 0;
					int.TryParse (chunkData[0], out _id);
		
					int _count = 0;
					int.TryParse (chunkData[1], out _count);
					
					runtimeInventory.Add (_id, _count);
				}
			}

			runtimeInventory.RemoveRecipes ();
		}
	}


	private void AssignMenuLocks (List<Menu> menus, string menuLockData)
	{
		if (menuLockData.Length > 0)
		{
			string[] lockArray = menuLockData.Split ("|"[0]);

			foreach (string chunk in lockArray)
			{
				string[] chunkData = chunk.Split (":"[0]);
				
				int _id = 0;
				int.TryParse (chunkData[0], out _id);
				
				bool _lock = false;
				bool.TryParse (chunkData[1], out _lock);
				
				foreach (Menu _menu in menus)
				{
					if (_menu.id == _id)
					{
						_menu.isLocked = _lock;
						break;
					}
				}
			}
		}
	}


	private void AssignMenuElementVisibility (List<Menu> menus, string menuElementVisibilityData)
	{
		if (menuElementVisibilityData.Length > 0)
		{
			string[] visArray = menuElementVisibilityData.Split ("|"[0]);
			
			foreach (string chunk in visArray)
			{
				string[] chunkData = chunk.Split (":"[0]);
				
				int _menuID = 0;
				int.TryParse (chunkData[0], out _menuID);

				foreach (Menu _menu in menus)
				{
					if (_menu.id == _menuID)
					{
						// Found a match
						string[] perMenuData = chunkData[1].Split ("+"[0]);
						
						foreach (string perElementData in perMenuData)
						{
							string [] chunkData2 = perElementData.Split ("="[0]);
							
							int _elementID = 0;
							int.TryParse (chunkData2[0], out _elementID);
							
							bool _elementVisibility = false;
							bool.TryParse (chunkData2[1], out _elementVisibility);
							
							foreach (MenuElement _element in _menu.elements)
							{
								if (_element.ID == _elementID && _element.isVisible != _elementVisibility)
								{
									_element.isVisible = _elementVisibility;
									break;
								}
							}
						}

						_menu.ResetVisibleElements ();
						_menu.Recalculate ();
						break;
					}
				}
			}
		}
	}


	private void AssignMenuJournals (List<Menu> menus, string menuJournalData)
	{
		if (menuJournalData.Length > 0)
		{
			string[] journalArray = menuJournalData.Split ("|"[0]);
			
			foreach (string chunk in journalArray)
			{
				string[] chunkData = chunk.Split (":"[0]);
				
				int menuID = 0;
				int.TryParse (chunkData[0], out menuID);
				
				int elementID = 0;
				int.TryParse (chunkData[1], out elementID);

				foreach (Menu _menu in menus)
				{
					if (_menu.id == menuID)
					{
						foreach (MenuElement _element in _menu.elements)
						{
							if (_element.ID == elementID && _element is MenuJournal)
							{
								MenuJournal journal = (MenuJournal) _element;
								journal.pages = new List<JournalPage>();
								journal.showPage = 1;

								string[] pageArray = chunkData[2].Split ("~"[0]);

								foreach (string chunkData2 in pageArray)
								{
									string[] chunkData3 = chunkData2.Split ("*"[0]);

									int lineID = -1;
									int.TryParse (chunkData3[0], out lineID);

									journal.pages.Add (new JournalPage (lineID, chunkData3[1]));
								}

								break;
							}
						}
					}
				}
			}
		}
	}


	private string CreateInventoryData (RuntimeInventory runtimeInventory)
	{
		System.Text.StringBuilder inventoryString = new System.Text.StringBuilder ();
		
		foreach (InvItem item in runtimeInventory.localItems)
		{
			inventoryString.Append (item.id.ToString ());
			inventoryString.Append (":");
			inventoryString.Append (item.count.ToString ());
			inventoryString.Append ("|");
		}
		
		if (runtimeInventory && runtimeInventory.localItems.Count > 0)
		{
			inventoryString.Remove (inventoryString.Length-1, 1);
		}
		
		return inventoryString.ToString ();		
	}
	
		
	public static string CreateVariablesData (List<GVar> vars, bool isOptionsData, VariableLocation location)
	{
		System.Text.StringBuilder variablesString = new System.Text.StringBuilder ();

		foreach (GVar _var in vars)
		{
			if ((isOptionsData && _var.link == VarLink.OptionsData) || (!isOptionsData && _var.link != VarLink.OptionsData) || location == VariableLocation.Local)
			{
				variablesString.Append (_var.id.ToString ());
				variablesString.Append (":");
				if (_var.type == VariableType.String)
				{
					string textVal = _var.textVal;
					if (textVal.Contains ("|"))
					{
						textVal = textVal.Replace ("|", "");
						Debug.LogWarning ("Removed pipe delimeter from variable " + _var.label);
					}
					variablesString.Append (textVal);
				}
				else if (_var.type == VariableType.Float)
				{
					variablesString.Append (_var.floatVal.ToString ());
				}
				else
				{
					variablesString.Append (_var.val.ToString ());
				}
				variablesString.Append ("|");
			}
		}
		
		if (variablesString.Length > 0)
		{
			variablesString.Remove (variablesString.Length-1, 1);
		}

		return variablesString.ToString ();		
	}


	private string CreateMenuLockData (List<Menu> menus)
	{
		System.Text.StringBuilder menuString = new System.Text.StringBuilder ();

		foreach (Menu _menu in menus)
		{
			menuString.Append (_menu.id.ToString ());
			menuString.Append (":");
			menuString.Append (_menu.isLocked.ToString ());
			menuString.Append ("|");
		}

		if (menus.Count > 0)
		{
			menuString.Remove (menuString.Length-1, 1);
		}

		return menuString.ToString ();
	}


	private string CreateMenuElementVisibilityData (List<Menu> menus)
	{
		System.Text.StringBuilder visibilityString = new System.Text.StringBuilder ();
		
		foreach (Menu _menu in menus)
		{
			visibilityString.Append (_menu.id.ToString ());
			visibilityString.Append (":");

			foreach (MenuElement _element in _menu.elements)
			{
				visibilityString.Append (_element.ID.ToString ());
				visibilityString.Append ("=");
				visibilityString.Append (_element.isVisible.ToString ());
				visibilityString.Append ("+");
			}

			if (_menu.elements.Count > 0)
			{
				visibilityString.Remove (visibilityString.Length-1, 1);
			}

			visibilityString.Append ("|");
		}
		
		if (menus.Count > 0)
		{
			visibilityString.Remove (visibilityString.Length-1, 1);
		}

		return visibilityString.ToString ();
	}


	private string CreateMenuJournalData (List<Menu> menus)
	{
		System.Text.StringBuilder journalString = new System.Text.StringBuilder ();

		foreach (Menu _menu in menus)
		{
			foreach (MenuElement _element in _menu.elements)
			{
				if (_element is MenuJournal)
				{
					MenuJournal journal = (MenuJournal) _element;
					journalString.Append (_menu.id.ToString ());
					journalString.Append (":");
					journalString.Append (journal.ID);
					journalString.Append (":");

					foreach (JournalPage page in journal.pages)
					{
						journalString.Append (page.lineID);
						journalString.Append ("*");
						journalString.Append (page.text);
						journalString.Append ("~");
					}

					if (journal.pages.Count > 0)
					{
						journalString.Remove (journalString.Length-1, 1);
					}

					journalString.Append ("|");
				}
			}
		}

		if (journalString.ToString () != "")
		{
			journalString.Remove (journalString.Length-1, 1);
		}

		return journalString.ToString ();
	}

}