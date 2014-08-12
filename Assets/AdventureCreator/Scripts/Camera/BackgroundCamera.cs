/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"BackgroundCamera.cs"
 * 
 *	The BackgroundCamera is used to display background images underneath the scene geometry.
 * 
 */

using UnityEngine;
using System.Collections;

public class BackgroundCamera : MonoBehaviour
{
	
	private void Awake ()
	{
		TurnOn ();
	}
	
	public void TurnOn ()
	{
		if (LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.backgroundImageLayer) == -1)
		{
			Debug.LogWarning ("No '" + AdvGame.GetReferences ().settingsManager.backgroundImageLayer + "' layer exists - please define one in the Tags Manager.");
		}
		else
		{
			camera.cullingMask = (1 << LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.backgroundImageLayer));
		}
	}
	
}
