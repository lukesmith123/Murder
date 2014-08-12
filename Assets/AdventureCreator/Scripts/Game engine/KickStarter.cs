/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"KickStarter.cs"
 * 
 *	This script will make sure that PersistentEngine and the Player gameObjects are always created,
 *	regardless of which scene the game is begun from.  It will also check the key gameObjects for
 *	essential scripts and references.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

public class KickStarter : MonoBehaviour
{

	public static void ResetPlayer (Player ref_player, int ID, bool resetReferences)
	{
		// Delete current player
		if (GameObject.FindWithTag (Tags.player))
		{
			Player oldPlayer = GameObject.FindWithTag (Tags.player).GetComponent <Player>();
			DestroyImmediate (oldPlayer.gameObject);
		}

		// Load new player
		if (ref_player)
		{
			Player newPlayer = (Player) Instantiate (ref_player);
			newPlayer.ID = ID;
			newPlayer.name = ref_player.name;
		}

		// Reset player references
		if (resetReferences)
		{
			GameObject.FindWithTag (Tags.gameEngine).SendMessage ("ResetPlayerReference");
			_Camera[] cameras = FindObjectsOfType (typeof (_Camera)) as _Camera[];
			foreach (_Camera camera in cameras)
			{
				camera.ResetTarget ();
			}
		}
	}


	private void Awake ()
	{
		// Test for key imports
		References references = (References) Resources.Load (Resource.references);

		if (references)
		{
			SceneManager sceneManager = AdvGame.GetReferences ().sceneManager;
			SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
			ActionsManager actionsManager = AdvGame.GetReferences ().actionsManager;
			InventoryManager inventoryManager = AdvGame.GetReferences ().inventoryManager;
			VariablesManager variablesManager = AdvGame.GetReferences ().variablesManager;
			SpeechManager speechManager = AdvGame.GetReferences ().speechManager;
			CursorManager cursorManager = AdvGame.GetReferences ().cursorManager;
			MenuManager menuManager = AdvGame.GetReferences ().menuManager;
			
			if (sceneManager == null)
			{
				Debug.LogError ("No Scene Manager found - please set one using the Adventure Creator Kit wizard");
			}
			
			if (settingsManager == null)
			{
				Debug.LogError ("No Settings Manager found - please set one using the Adventure Creator Kit wizard");
			}
			else
			{
				if (!GameObject.FindGameObjectWithTag (Tags.player))
				{
					KickStarter.ResetPlayer (AdvGame.GetReferences ().settingsManager.GetDefaultPlayer (), AdvGame.GetReferences ().settingsManager.GetDefaultPlayerID (), false);
				}
			}
			
			if (actionsManager == null)
			{
				Debug.LogError ("No Actions Manager found - please set one using the main Adventure Creator window");
			}
			
			if (inventoryManager == null)
			{
				Debug.LogError ("No Inventory Manager found - please set one using the main Adventure Creator window");
			}
			
			if (variablesManager == null)
			{
				Debug.LogError ("No Variables Manager found - please set one using the main Adventure Creator window");
			}
			
			if (speechManager == null)
			{
				Debug.LogError ("No Speech Manager found - please set one using the main Adventure Creator window");
			}

			if (cursorManager == null)
			{
				Debug.LogError ("No Cursor Manager found - please set one using the main Adventure Creator window");
			}

			if (menuManager == null)
			{
				Debug.LogError ("No Menu Manager found - please set one using the main Adventure Creator window");
			}
			
			if (GameObject.FindWithTag (Tags.player) == null)
			{
				Debug.LogWarning ("No Player found - please set one using the Settings Manager, tagging it as Player and placing it in a Resources folder");
			}
			
		}
		else
		{
			Debug.LogError ("No References object found. Please set one using the main Adventure Creator window");
		}
		
		if (!GameObject.FindGameObjectWithTag (Tags.persistentEngine))
		{
			try
			{
				GameObject persistentEngine = (GameObject) Instantiate (Resources.Load (Resource.persistentEngine));
				persistentEngine.name = AdvGame.GetName (Resource.persistentEngine);
			}
			catch {}
		}
		
		if (GameObject.FindWithTag (Tags.persistentEngine) == null)
		{
			Debug.LogError ("No PersistentEngine prefab found - please place one in the Resources directory, and tag it as PersistentEngine");
		}
		else
		{
			GameObject persistentEngine = GameObject.FindWithTag (Tags.persistentEngine);
			
			if (persistentEngine.GetComponent <Options>() == null)
			{
				Debug.LogError (persistentEngine.name + " has no Options component attached.");
			}
			if (persistentEngine.GetComponent <RuntimeInventory>() == null)
			{
				Debug.LogError (persistentEngine.name + " has no RuntimeInventory component attached.");
			}
			if (persistentEngine.GetComponent <RuntimeVariables>() == null)
			{
				Debug.LogError (persistentEngine.name + " has no RuntimeVariables component attached.");
			}
			if (persistentEngine.GetComponent <PlayerMenus>() == null)
			{
				Debug.LogError (persistentEngine.name + " has no PlayerMenus component attached.");
			}
			if (persistentEngine.GetComponent <StateHandler>() == null)
			{
				Debug.LogError (persistentEngine.name + " has no StateHandler component attached.");
			}
			if (persistentEngine.GetComponent <SceneChanger>() == null)
			{
				Debug.LogError (persistentEngine.name + " has no SceneChanger component attached.");
			}
			if (persistentEngine.GetComponent <SaveSystem>() == null)
			{
				Debug.LogError (persistentEngine.name + " has no SaveSystem component attached.");
			}
			if (persistentEngine.GetComponent <LevelStorage>() == null)
			{
				Debug.LogError (persistentEngine.name + " has no LevelStorage component attached.");
			}
		}
		
		if (GameObject.FindWithTag (Tags.mainCamera) == null)
		{
			Debug.LogError ("No MainCamera found - please click 'Organise room objects' in the Scene Manager to create one.");
		}
		else
		{
			if (GameObject.FindWithTag (Tags.mainCamera).GetComponent <MainCamera>() == null)
			{
				Debug.LogError ("MainCamera has no MainCamera component.");
			}
		}
		
		if (this.tag == Tags.gameEngine)
		{
			if (this.GetComponent <MenuSystem>() == null)
			{
				Debug.LogError (this.name + " has no MenuSystem component attached.");
			}
			if (this.GetComponent <Dialog>() == null)
			{
				Debug.LogError (this.name + " has no Dialog component attached.");
			}
			if (this.GetComponent <PlayerInput>() == null)
			{
				Debug.LogError (this.name + " has no PlayerInput component attached.");
			}
			if (this.GetComponent <PlayerInteraction>() == null)
			{
				Debug.LogError (this.name + " has no PlayerInteraction component attached.");
			}
			if (this.GetComponent <PlayerMovement>() == null)
			{
				Debug.LogError (this.name + " has no PlayerMovement component attached.");
			}
			if
				(this.GetComponent <PlayerCursor>() == null)
			{
				Debug.LogError (this.name + " has no PlayerCursor component attached.");
			}
			if (this.GetComponent <SceneSettings>() == null)
			{
				Debug.LogError (this.name + " has no SceneSettings component attached.");
			}
			else
			{
				if (this.GetComponent <SceneSettings>().navigationMethod == AC_NavigationMethod.meshCollider && this.GetComponent <SceneSettings>().navMesh == null)
				{
					Debug.LogWarning ("No NavMesh set.  Characters will not be able to PathFind until one is defined - please choose one using the Scene Manager.");
				}
				
				if (this.GetComponent <SceneSettings>().defaultPlayerStart == null)
				{
					Debug.LogWarning ("No default PlayerStart set.  The game may not be able to begin if one is not defined - please choose one using the Scene Manager.");
				}
			}
			if (this.GetComponent <RuntimeActionList>() == null)
			{
				Debug.LogError (this.name + " has no RuntimeActionList component attached.");
			}
			if (this.GetComponent <NavigationManager>() == null)
			{
				Debug.LogError (this.name + " has no NavigationManager component attached.");
			}
			if (this.GetComponent <ActionListManager>() == null)
			{
				Debug.LogError (this.name + " has no ActionListManager component attached.");
			}
		}
	}


	public void TurnOnAC ()
	{
		if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>())
		{
			GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>().gameState = GameState.Normal;
		}
	}
	
	
	public void TurnOffAC ()
	{
		this.GetComponent <ActionListManager>().KillAllLists ();
		this.GetComponent <Dialog>().KillDialog ();

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

		if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>())
		{
			GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>().gameState = GameState.Cutscene;
		}
	}
	
}
