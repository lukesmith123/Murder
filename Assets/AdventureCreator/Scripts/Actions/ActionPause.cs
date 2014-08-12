/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionPause.cs"
 * 
 *	This action pauses the game by a given amount.
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
public class ActionPause : Action
{
	
	public float timeToPause;

	
	public ActionPause ()
	{
		this.isDisplayed = true;
		title = "Engine: Pause game";
	}
	
	
	override public float Run ()
	{
		if (!isRunning)
		{
			isRunning = true;
			return timeToPause;
		}
		else
		{
			isRunning = false;
			return 0f;
		}
	}


	#if UNITY_EDITOR

	override public void ShowGUI ()
	{
		timeToPause = EditorGUILayout.Slider ("Wait time (s):", timeToPause, 0, 10f);
		AfterRunningOption ();
	}
	

	public override string SetLabel ()
	{
		string labelAdd = " (" + timeToPause + "s)";
		return labelAdd;
	}

	#endif
	
}