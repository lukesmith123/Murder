/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"SceneSettings.cs"
 * 
 *	This script defines which cutscenes play when the scene is loaded,
 *	and where the player should begin from.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class SceneSettings : MonoBehaviour
	{

		public Cutscene cutsceneOnStart;
		public Cutscene cutsceneOnLoad;
		public Cutscene cutsceneOnVarChange;
		public PlayerStart defaultPlayerStart;
		public AC_NavigationMethod navigationMethod = AC_NavigationMethod.meshCollider;
		public NavigationMesh navMesh;
		public SortingMap sortingMap;
		public Sound defaultSound;

		private bool playCutsceneOnVarChange = false;
		private StateHandler stateHandler;
		
		
		private void Awake ()
		{
			// Turn off all NavMesh objects
			NavigationMesh[] navMeshes = FindObjectsOfType (typeof (NavigationMesh)) as NavigationMesh[];
			foreach (NavigationMesh _navMesh in navMeshes)
			{
				if (navMesh != _navMesh)
				{
					_navMesh.TurnOff ();
				}
			}
			
			// Turn on default NavMesh if using MeshCollider method
			if (navMesh && (navMesh.GetComponent <Collider>() || navMesh.GetComponent <Collider2D>()))
			{
				navMesh.TurnOn ();
			}	
		}

		
		private void Start ()
		{
			if (GameObject.FindWithTag (Tags.persistentEngine))
			{
				if (GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>())
				{
					SaveSystem saveSystem = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SaveSystem>();
			
					LevelStorage levelStorage = saveSystem.GetComponent <LevelStorage>();
					if (levelStorage)
					{
						levelStorage.ReturnCurrentLevelData ();	
					}
					
					if (!saveSystem.isLoadingNewScene)
					{
						FindPlayerStart ();
					}
					else
					{
						saveSystem.isLoadingNewScene = false;
					}
				}

				if (GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>())
				{
					stateHandler = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>();
				}
			}
		}


		private void Update ()
		{
			if (playCutsceneOnVarChange && stateHandler && (stateHandler.gameState == GameState.Normal || stateHandler.gameState == GameState.DialogOptions))
			{
				playCutsceneOnVarChange = false;
				cutsceneOnVarChange.Interact ();
			}
		}
		

		private void FindPlayerStart ()
		{
			if (GetPlayerStart () != null)
			{
				GetPlayerStart ().SetPlayerStart ();
			}

			if (cutsceneOnStart != null)
			{
				cutsceneOnStart.Interact ();
			}
		}


		public PlayerStart GetPlayerStart ()
		{
			SceneChanger sceneChanger = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <SceneChanger>();

			PlayerStart[] starters = FindObjectsOfType (typeof (PlayerStart)) as PlayerStart[];
			foreach (PlayerStart starter in starters)
			{
				if (starter.previousScene > -1 && starter.previousScene == sceneChanger.previousScene)
				{
					return starter;
				}
			}
			
			if (defaultPlayerStart)
			{
				return defaultPlayerStart;
			}
			
			return null;
		}
		
		
		public void OnLoad ()
		{
			if (cutsceneOnLoad != null)
			{
				cutsceneOnLoad.Interact ();
			}
		}


		public void VarChanged ()
		{
			if (cutsceneOnVarChange != null)
			{
				playCutsceneOnVarChange = true;
			}
		}


		public void PlayDefaultSound (AudioClip audioClip, bool doLoop)
		{
			if (defaultSound == null)
			{
				Debug.Log ("Cannot play sound since no Default Sound Prefab is defined - please set one in the Scene Manager.");
				return;
			}

			if (audioClip && defaultSound.GetComponent <AudioSource>())
			{
				defaultSound.GetComponent <AudioSource>().clip = audioClip;
				defaultSound.Play (doLoop);
			}
		}

	}

}