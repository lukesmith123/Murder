/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionRunActionList.cs"
 * 
 *	This is a blank action template.
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
public class ActionRunActionList : Action
{
	
	public enum ListSource { InScene, AssetFile };
	public ListSource listSource = ListSource.InScene;

	public ActionList actionList;
	public InvActionList invActionList;
	public int constantID = 0;

	public bool runFromStart = true;
	public int jumpToAction;
	public AC.Action jumpToActionActual;
	public bool runInParallel = false;



	public ActionRunActionList ()
	{
		this.isDisplayed = true;
		title = "Engine: Run ActionList";
		numSockets = 0;
	}


	override public float Run ()
	{
		ActionListManager actionListManager = GameObject.FindWithTag (Tags.gameEngine).GetComponent <ActionListManager>();

		if (!isRunning)
		{
			isRunning = true;

			if (listSource == ListSource.InScene)
			{
				if (isAssetFile && constantID != 0)
				{
					// Attempt to find the correct scene object
					actionList = Serializer.returnComponent <ActionList> (constantID);
				}

				if (actionList != null)
				{
					if (actionList is RuntimeActionList)
					{
						Debug.LogWarning (actionList.name + " cannot be used by this Action.");
						return 0f;
					}
					
					actionListManager.EndList (actionList);
					
					if (runFromStart)
					{
						actionList.Interact ();
					}
					else
					{
						int skip = jumpToAction;
						if (jumpToActionActual && actionList.actions.IndexOf (jumpToActionActual) > 0)
						{
							skip = actionList.actions.IndexOf (jumpToActionActual);
						}
						
						actionList.Interact (skip);
					}
				}
			}
			else if (listSource == ListSource.AssetFile && invActionList != null)
			{
				GameObject.FindWithTag (Tags.gameEngine).GetComponent <RuntimeActionList>().Play (invActionList);
			}

			if (runInParallel && willWait)
			{
				if (!isAssetFile || listSource == ListSource.InScene)
				{
					return defaultPauseTime;
				}
			}
		}
		else
		{
			if (listSource == ListSource.InScene && actionList != null)
			{
				if (actionListManager.IsListRunning (actionList))
				{
					return defaultPauseTime;
				}
				else
				{
					isRunning = false;
				}
			}
			else if (listSource == ListSource.AssetFile && invActionList != null)
			{
				if (GameObject.FindWithTag (Tags.gameEngine).GetComponent <RuntimeActionList>().IsRunning ())
				{
					return defaultPauseTime;
				}
				else
				{
					isRunning = false;
				}
			}
		}

		return 0f;
	}


	override public void Skip ()
	{
		ActionListManager actionListManager = GameObject.FindWithTag (Tags.gameEngine).GetComponent <ActionListManager>();
		
		if (listSource == ListSource.InScene && actionList != null)
		{
			if (actionList is RuntimeActionList)
			{
				return;
			}
			
			actionListManager.EndList (actionList);
			
			if (runFromStart)
			{
				actionList.Interact ();
			}
			else
			{
				int skip = jumpToAction;
				if (jumpToActionActual && actionList.actions.IndexOf (jumpToActionActual) > 0)
				{
					skip = actionList.actions.IndexOf (jumpToActionActual);
				}
				
				actionList.Interact (skip);
			}
		}
		else if (listSource == ListSource.AssetFile && invActionList != null)
		{
			GameObject.FindWithTag (Tags.gameEngine).GetComponent <RuntimeActionList>().Play (invActionList);
		}
	}


	override public int End (List<AC.Action> actions)
	{
		if (runInParallel)
		{
			if (!isAssetFile || listSource == ListSource.InScene)
			{
				return (base.End (actions));
			}
		}

		return -1;
	}
	
	
	#if UNITY_EDITOR
	
	override public void ShowGUI ()
	{
		listSource = (ListSource) EditorGUILayout.EnumPopup ("Source:", listSource);
		if (listSource == ListSource.InScene)
		{
			if (isAssetFile)
			{
				constantID = EditorGUILayout.IntField ("ActionList (ID):", constantID);
				runFromStart = true;
			}
			else
			{
				actionList = (ActionList) EditorGUILayout.ObjectField ("ActionList:", actionList, typeof (ActionList), true);
				runFromStart = EditorGUILayout.Toggle ("Run from start?", runFromStart);

				if (!runFromStart && actionList != null && actionList.actions.Count > 1)
				{
					JumpToActionGUI (actionList.actions);
				}
			}

			if (actionList != null)
			{
				if (actionList is RuntimeActionList)
				{
					EditorGUILayout.HelpBox ("This ActionList cannot be used by this Action.", MessageType.Warning);
				}
			}
		}
		else if (listSource == ListSource.AssetFile)
		{
			invActionList = (InvActionList) EditorGUILayout.ObjectField ("InvActionList asset:", invActionList, typeof (InvActionList), true);
		}

		if (!isAssetFile || listSource == ListSource.InScene)
		{
			runInParallel = EditorGUILayout.Toggle ("Run in parallel?", runInParallel);
		}

		if (runInParallel)
		{
			willWait = EditorGUILayout.Toggle ("Pause until finish?", willWait);
			numSockets = 1;
			AfterRunningOption ();
		}
		else
		{
			numSockets = 0;
		}
	}


	private void JumpToActionGUI (List<Action> actions)
	{
		int tempSkipAction = jumpToAction;
		List<string> labelList = new List<string>();
		
		if (jumpToActionActual)
		{
			bool found = false;
			
			for (int i = 0; i < actions.Count; i++)
			{
				labelList.Add (i.ToString () + ": " + actions [i].title);
				
				if (jumpToActionActual == actions [i])
				{
					jumpToAction = i;
					found = true;
				}
			}

			if (!found)
			{
				jumpToAction = tempSkipAction;
			}
		}
		
		if (jumpToAction < 0)
		{
			jumpToAction = 0;
		}
		
		if (jumpToAction >= actions.Count)
		{
			jumpToAction = actions.Count - 1;
		}
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField ("  Action to skip to:");
		tempSkipAction = EditorGUILayout.Popup (jumpToAction, labelList.ToArray());
		jumpToAction = tempSkipAction;
		EditorGUILayout.EndHorizontal();
		jumpToActionActual = actions [jumpToAction];
	}
	
	
	public override string SetLabel ()
	{
		string labelAdd = "";
		
		if (listSource == ListSource.InScene && actionList != null)
		{
			labelAdd += " (" + actionList.name + ")";
		}
		else if (listSource == ListSource.AssetFile && invActionList != null)
		{
			labelAdd += " (" + invActionList.name + ")";
		}
		
		return labelAdd;
	}
	
	#endif
	
}