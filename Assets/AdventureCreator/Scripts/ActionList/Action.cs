/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Action.cs"
 * 
 *	This is the base class from which all Actions derive.
 *	We need blank functions Run, ShowGUI and SetLabel,
 *	which will be over-ridden by the subclasses.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	abstract public class Action : ScriptableObject
	{

		public int numSockets = 1;
		public bool willWait;
		public float defaultPauseTime = 0.1f;
		
		public bool isRunning;
		public int id;
		
		public bool isDisplayed;
		public string title;
		
		public ResultAction endAction = ResultAction.Continue;
		public int skipAction = -1;
		public AC.Action skipActionActual;
		public Cutscene linkedCutscene;

		public bool isMarked = false;
		public bool isEnabled = true;
		public bool isAssetFile = false;

		public Rect nodeRect = new Rect (0,0,300,60);
		
		
		public Action ()
		{
			this.isDisplayed = true;
		}
		
		
		public virtual float Run ()
		{
			return defaultPauseTime;
		}


		public virtual void Skip ()
		{
			Run ();
		}
		
		
		public virtual void ShowGUI () {}
		
		
		public virtual int End (List<Action> actions)
		{
			if (endAction == ResultAction.Stop)
			{
				return -1;
			}
			else if (endAction == ResultAction.Skip)
			{
				int skip = skipAction;
				if (skipActionActual && actions.Contains (skipActionActual))
				{
					skip = actions.IndexOf (skipActionActual);
				}
				else if (skip == -1)
				{
					skip = 0;
				}

				return (skip);
			}
			else if (endAction == ResultAction.RunCutscene && linkedCutscene)
			{	
				return -1;
			}
			
			// Continue as normal
			return -3;
		}
		
		
		#if UNITY_EDITOR
		
		protected void AfterRunningOption ()
		{
			EditorGUILayout.Space ();
			endAction = (ResultAction) EditorGUILayout.EnumPopup ("After running:", (ResultAction) endAction);
			
			if (endAction == ResultAction.RunCutscene)
			{
				linkedCutscene = (Cutscene) EditorGUILayout.ObjectField ("Cutscene to run", linkedCutscene, typeof (Cutscene), true);
			}
		}
		
		
		public virtual void SkipActionGUI (List<Action> actions, bool showGUI)
		{
			if (skipAction == -1)
			{
				// Set default
				int i = actions.IndexOf (this);
				if (actions.Count > i+1)
				{
					skipAction = i+1;
				}
				else
				{
					skipAction = i;
				}
			}

			int tempSkipAction = skipAction;
			List<string> labelList = new List<string>();

			if (skipActionActual)
			{
				bool found = false;

				for (int i = 0; i < actions.Count; i++)
				{
					labelList.Add (i.ToString () + ": " + actions [i].title);

					if (skipActionActual == actions [i])
					{
						skipAction = i;
						found = true;
					}
				}
				
				if (!found)
				{
					skipAction = tempSkipAction;
				}
			}

			if (skipAction >= actions.Count)
			{
				skipAction = actions.Count - 1;
			}

			if (showGUI)
			{
				if (actions.Count > 1)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField ("  Action to skip to:");
					tempSkipAction = EditorGUILayout.Popup (skipAction, labelList.ToArray());
					EditorGUILayout.EndHorizontal();
					skipAction = tempSkipAction;
				}
				else
				{
					EditorGUILayout.HelpBox ("Cannot skip action - no further Actions available", MessageType.Warning);
					return;
				}
			}

			skipActionActual = actions [skipAction];
		}


		public virtual string SetLabel ()
		{
			return ("");
		}


		public virtual void DrawOutWires (List<Action> actions, int i, int offset)
		{
			if (endAction == ResultAction.Continue || (endAction == ResultAction.Skip && skipAction == i+1))
			{
				if (actions.Count > i+1)
				{
					AdvGame.DrawNodeCurve (nodeRect, actions[i+1].nodeRect, Color.blue, 10);
				}
			}
			else if (endAction == ResultAction.Skip)
			{
				if (actions.Contains (skipActionActual))
				{
					AdvGame.DrawNodeCurve (nodeRect, skipActionActual.nodeRect, Color.blue, 10);
				}
			}
		}

		#endif
		
	}
	
}
