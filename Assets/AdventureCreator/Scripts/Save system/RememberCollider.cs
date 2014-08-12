/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RememberCollider.cs"
 * 
 *	This script is attached to Colliders in the scene
 *	whose on/off state we wish to save. 
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

public class RememberCollider : ConstantID
{
	
	public AC_OnOff startState = AC_OnOff.On;
	
	
	public void Awake ()
	{
		SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
		
		if (settingsManager && GameIsPlaying () && collider)
		{
			if (startState == AC_OnOff.On)
			{
				collider.enabled = true;
			}
			else
			{
				collider.enabled = false;
			}
		}
	}
	
	
	public ColliderData SaveData ()
	{
		ColliderData colliderData = new ColliderData ();

		colliderData.objectID = constantID;
		colliderData.isOn = false;

		if (collider)
		{
			colliderData.isOn = collider.enabled;
		}

		return (colliderData);
	}
	
	
	public void LoadData (ColliderData data)
	{
		if (collider)
		{
			if (data.isOn)
			{
				collider.enabled = true;
			}
			else
			{
				collider.enabled = false;
			}
		}
	}

}


[System.Serializable]
public class ColliderData
{
	public int objectID;
	public bool isOn;

	public ColliderData () { }
}