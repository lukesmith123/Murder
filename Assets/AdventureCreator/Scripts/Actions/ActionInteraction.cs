/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionInteraction.cs"
 * 
 *	This Action can enable and disable
 *	a Hotspot's individual Interactions.
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
public class ActionInteraction : Action
{
	
	public Hotspot hotspot;
	public InteractionType interactionType;
	public enum ChangeType { Enable, Disable };
	public ChangeType changeType = ChangeType.Enable;
	public int number = 0;

	
	public ActionInteraction ()
	{
		this.isDisplayed = true;
		title = "Hotspot: Change interaction";
	}
	
	
	override public float Run ()
	{
		if (hotspot == null)
		{
			return 0f;
		}

		if (interactionType == InteractionType.Use)
		{
			if (AdvGame.GetReferences ().settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
			{
				ChangeButton (hotspot.useButton);
			}
			else
			{
				// Multiple use interactions
				if (hotspot.useButtons.Count > number)
				{
					ChangeButton (hotspot.useButtons [number]);
				}
			}
		}
		else if (interactionType == InteractionType.Examine)
		{
			ChangeButton (hotspot.lookButton);
		}
		else if (interactionType == InteractionType.Inventory)
		{
			if (hotspot.invButtons.Count > number)
			{
				ChangeButton (hotspot.invButtons [number]);
			}
		}

		return 0f;
	}


	private void ChangeButton (AC.Button button)
	{
		if (button == null)
		{
			return;
		}

		if (changeType == ChangeType.Enable)
		{
			button.isDisabled = false;
		}
		else if (changeType == ChangeType.Disable)
		{
			button.isDisabled = true;
		}
	}
	
	
	#if UNITY_EDITOR
	
	override public void ShowGUI ()
	{
		if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager)
		{
			hotspot = (Hotspot) EditorGUILayout.ObjectField ("Hotspot to change:", hotspot, typeof (Hotspot), true);
			interactionType = (InteractionType) EditorGUILayout.EnumPopup ("Interaction to change:", interactionType);

			if (hotspot != null)
			{
				if (AdvGame.GetReferences ().settingsManager.interactionMethod != AC_InteractionMethod.ContextSensitive && interactionType == InteractionType.Use)
				{
					if (AdvGame.GetReferences ().cursorManager)
					{
						// Multiple use interactions
						List<string> labelList = new List<string>();
						
						foreach (AC.Button button in hotspot.useButtons)
						{
							labelList.Add (hotspot.useButtons.IndexOf (button) + ": " + AdvGame.GetReferences ().cursorManager.GetLabelFromID (button.iconID));
						}
						
						number = EditorGUILayout.Popup ("Use interaction:", number, labelList.ToArray ());
					}
					else
					{
						EditorGUILayout.HelpBox ("A Cursor Manager is required.", MessageType.Warning);
					}
				}
				else if (interactionType == InteractionType.Inventory)
				{
					if (AdvGame.GetReferences ().inventoryManager)
					{
						List<string> labelList = new List<string>();

						foreach (AC.Button button in hotspot.invButtons)
						{
							labelList.Add (hotspot.invButtons.IndexOf (button) + ": " + AdvGame.GetReferences ().inventoryManager.GetLabel (button.invID));
						}

						number = EditorGUILayout.Popup ("Inventory interaction:", number, labelList.ToArray ());
					}
					else
					{
						EditorGUILayout.HelpBox ("An Inventory Manager is required.", MessageType.Warning);
					}
				}
			}

			changeType = (ChangeType) EditorGUILayout.EnumPopup ("Change to make:", changeType);
		}
		else
		{
			EditorGUILayout.HelpBox ("A Settings Manager is required for this Action.", MessageType.Warning);
		}

		AfterRunningOption ();
	}
	
	
	public override string SetLabel ()
	{
		string labelAdd = "";
		if (hotspot != null)
		{
			labelAdd = " (" + hotspot.name + " - " + changeType + " " + interactionType;
			labelAdd += ")";
		}
		return labelAdd;
	}
	
	#endif
	
}