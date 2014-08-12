/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionPlayMaker.cs"
 * 
 *	This action interacts with the popular
 *	PlayMaker FSM-manager.
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
public class ActionPlayMaker : Action
{

	public GameObject linkedObject;
	public string eventName;


	public ActionPlayMaker ()
	{
		this.isDisplayed = true;
		title = "Third-Party: PlayMaker";
	}


	override public float Run ()
	{
		if (linkedObject != null && eventName != "")
		{
			PlayMakerIntegration.CallEvent (linkedObject, eventName);
		}

		return 0f;
	}
	
	
	#if UNITY_EDITOR
	
	override public void ShowGUI ()
	{
		if (PlayMakerIntegration.IsDefinePresent ())
		{
			linkedObject = (GameObject) EditorGUILayout.ObjectField ("PlayMaker FSM:", linkedObject, typeof (GameObject), true);
			eventName = EditorGUILayout.TextField ("Event to call:", eventName);
		}
		else
		{
			EditorGUILayout.HelpBox ("The 'PlayMakerIsPresent' Scripting Define Symbol must be listed in the\nPlayer Settings. Please set it from Edit -> Project Settings -> Player", MessageType.Warning);
		}

		AfterRunningOption ();
	}
	
	
	public override string SetLabel ()
	{
		string labelAdd = "";
		return labelAdd;
	}
	
	#endif
}
