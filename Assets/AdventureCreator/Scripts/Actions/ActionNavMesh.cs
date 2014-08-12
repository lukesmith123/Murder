/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionNavMesh.cs"
 * 
 *	This action changes the active NavMesh.
 *	All NavMeshes must be on the same unique layer.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionNavMesh : Action
{

	public int constantID = 0;
	public NavigationMesh newNavMesh;
	public SortingMap sortingMap;
	public PlayerStart playerStart;
	public Cutscene cutscene;
	public SceneSetting sceneSetting = SceneSetting.DefaultNavMesh;
	
	public ActionNavMesh ()
	{
		this.isDisplayed = true;
		title = "Engine: Change scene setting";
	}
	
	
	override public float Run ()
	{
		if (isAssetFile && constantID != 0)
		{
			newNavMesh = null;
			sortingMap = null;
			playerStart = null;
			cutscene = null;

			// Attempt to find the correct scene object
			ConstantID idObject = Serializer.returnComponent <ConstantID> (constantID);
			if (idObject != null)
			{
				if (sceneSetting == SceneSetting.DefaultNavMesh)
				{
					if (idObject.GetComponent <NavigationMesh>())
					{
						newNavMesh = idObject.GetComponent <NavigationMesh>();
					}
					else 
					{
						Debug.LogWarning ("Cannot change NavMesh because referenced object has no NavigationMesh component!");
					}
				}
				else if (sceneSetting == SceneSetting.DefaultPlayerStart)
				{
					if (idObject.GetComponent <PlayerStart>())
					{
						playerStart = idObject.GetComponent <PlayerStart>();
					}
					else 
					{
						Debug.LogWarning ("Cannot change PlayerStart because referenced object has no PlayerStart component!");
					}
				}
				else if (sceneSetting == SceneSetting.SortingMap)
				{
					if (idObject.GetComponent <SortingMap>())
					{
						sortingMap = idObject.GetComponent <SortingMap>();
					}
					else 
					{
						Debug.LogWarning ("Cannot change SortingMap because referenced object has no SortingMap component!");
					}
				}
				else if (sceneSetting == SceneSetting.OnLoadCutscene || sceneSetting == SceneSetting.OnStartCutscene)
				{
					if (idObject.GetComponent <Cutscene>())
					{
						cutscene = idObject.GetComponent <Cutscene>();
					}
					else 
					{
						Debug.LogWarning ("Cannot change Cutscene because referenced object has no Cutscene component!");
					}
				}
			}
		}

		SceneSettings sceneSettings = GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>();

		if (sceneSetting == SceneSetting.DefaultNavMesh && newNavMesh)
		{
			NavigationMesh oldNavMesh = sceneSettings.navMesh;
			oldNavMesh.TurnOff ();
			newNavMesh.TurnOn ();
			sceneSettings.navMesh = newNavMesh;

			if (newNavMesh.GetComponent <ConstantID>() == null)
			{
				Debug.LogWarning ("Warning: Changing to new NavMesh with no ConstantID - change will not be recognised by saved games.");
			}
		}
		else if (sceneSetting == SceneSetting.DefaultPlayerStart && playerStart)
		{
			sceneSettings.defaultPlayerStart = playerStart;

			if (playerStart.GetComponent <ConstantID>() == null)
			{
				Debug.LogWarning ("Warning: Changing to new default PlayerStart with no ConstantID - change will not be recognised by saved games.");
			}
		}
		else if (sceneSetting == SceneSetting.SortingMap && sortingMap)
		{
			sceneSettings.sortingMap = sortingMap;

			// Reset all FollowSortingMap components
			FollowSortingMap[] followSortingMaps = FindObjectsOfType (typeof (FollowSortingMap)) as FollowSortingMap[];
			foreach (FollowSortingMap followSortingMap in followSortingMaps)
			{
				followSortingMap.UpdateSortingMap ();
			}

			if (sortingMap.GetComponent <ConstantID>() == null)
			{
				Debug.LogWarning ("Warning: Changing to new SortingMap with no ConstantID - change will not be recognised by saved games.");
			}
		}
		else if (sceneSetting == SceneSetting.OnLoadCutscene)
		{
			sceneSettings.cutsceneOnLoad = cutscene;

			if (cutscene.GetComponent <ConstantID>() == null)
			{
				Debug.LogWarning ("Warning: Changing to Cutscene On Load with no ConstantID - change will not be recognised by saved games.");
			}
		}
		else if (sceneSetting == SceneSetting.OnStartCutscene)
		{
			sceneSettings.cutsceneOnStart = cutscene;

			if (cutscene.GetComponent <ConstantID>() == null)
			{
				Debug.LogWarning ("Warning: Changing to Cutscene On Start with no ConstantID - change will not be recognised by saved games.");
			}
		}
		
		return 0f;
	}
	

	#if UNITY_EDITOR

	override public void ShowGUI ()
	{
		SceneSettings sceneSettings = null;
		if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>())
		{
			sceneSettings = GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>();
		}

		sceneSetting = (SceneSetting) EditorGUILayout.EnumPopup ("Scene setting to change:", sceneSetting);

		if (sceneSettings == null)
		{
			return;
		}

		if (sceneSetting == SceneSetting.DefaultNavMesh)
		{
			if (sceneSettings.navigationMethod == AC_NavigationMethod.meshCollider || sceneSettings.navigationMethod == AC_NavigationMethod.PolygonCollider)
			{
				if (isAssetFile)
				{
					constantID = EditorGUILayout.IntField ("New NavMesh (ID):", constantID);
				}
				else
				{
					newNavMesh = (NavigationMesh) EditorGUILayout.ObjectField ("New NavMesh:", newNavMesh, typeof (NavigationMesh), true);
				}
			}
			else
			{
				EditorGUILayout.HelpBox ("This action is not compatible with the Unity Navigation pathfinding method, as set in the Scene Manager.", MessageType.Warning);
			}
		}
		else if (sceneSetting == SceneSetting.DefaultPlayerStart)
		{
			if (isAssetFile)
			{
				constantID = EditorGUILayout.IntField ("New default PlayerStart (ID):", constantID);
			}
			else
			{
				playerStart = (PlayerStart) EditorGUILayout.ObjectField ("New default PlayerStart:", playerStart, typeof (PlayerStart), true);
			}
		}
		else if (sceneSetting == SceneSetting.SortingMap)
		{
			if (isAssetFile)
			{
				constantID = EditorGUILayout.IntField ("New SortingMap (ID):", constantID);
			}
			else
			{
				sortingMap = (SortingMap) EditorGUILayout.ObjectField ("New SortingMap:", sortingMap, typeof (SortingMap), true);
			}
		}
		else if (sceneSetting == SceneSetting.OnLoadCutscene)
		{
			if (isAssetFile)
			{
				constantID = EditorGUILayout.IntField ("New OnLoad cutscene (ID):", constantID);
			}
			else
			{
				cutscene = (Cutscene) EditorGUILayout.ObjectField ("New OnLoad custscne:", cutscene, typeof (Cutscene), true);
			}
		}
		else if (sceneSetting == SceneSetting.OnStartCutscene)
		{
			if (isAssetFile)
			{
				constantID = EditorGUILayout.IntField ("New OnStart cutscene (ID):", constantID);
			}
			else
			{
				cutscene = (Cutscene) EditorGUILayout.ObjectField ("New OnStart cutscene:", cutscene, typeof (Cutscene), true);
			}
		}
		
		AfterRunningOption ();
	}
	
	
	override public string SetLabel ()
	{
		string labelAdd = "";
		
		labelAdd = " (" + sceneSetting.ToString () + ")";

		return labelAdd;
	}

	#endif
	
}