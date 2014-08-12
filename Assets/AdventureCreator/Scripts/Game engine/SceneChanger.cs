/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"SceneChanger.cs"
 * 
 *	This script handles the changing of the scene, and stores
 *	which scene was previously loaded, for use by PlayerStart.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class SceneChanger : MonoBehaviour
	{

		public int previousScene = -1;
		
		
		public void ChangeScene (int sceneNumber, bool saveRoomData)
		{
			MainCamera mainCamera = GameObject.FindWithTag (Tags.mainCamera).GetComponent <MainCamera>();
			mainCamera.FadeOut (0f);

			if (GameObject.FindWithTag (Tags.player) && GameObject.FindWithTag (Tags.player).GetComponent <Player>())
			{
				GameObject.FindWithTag (Tags.player).GetComponent <Player>().Halt ();
			}

			Sound[] sounds = FindObjectsOfType (typeof (Sound)) as Sound[];
			foreach (Sound sound in sounds)
			{
				if (sound.canDestroy)
				{
					Destroy (sound);
				}
			}

			LevelStorage levelStorage = this.GetComponent <LevelStorage>();
			if (saveRoomData)
			{
				levelStorage.StoreCurrentLevelData ();
				previousScene = Application.loadedLevel;
			}
			
			StateHandler stateHandler = this.GetComponent <StateHandler>();
			stateHandler.gameState = GameState.Normal;
			
			Application.LoadLevel (sceneNumber);
		}

	}

}