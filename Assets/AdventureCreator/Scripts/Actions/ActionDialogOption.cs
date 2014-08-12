/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013
 *	
 *	"ActionDialogOption.cs"
 * 
 *	This action changes the visibility of dialogue options.
 * 
*/

using UnityEngine;
using System.Collections;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionDialogOption : Action
{
	
	public enum SwitchType { On, Off, OnForever, OffForever };
	public SwitchType switchType;
	public int optionNumber;
	
	public Conversation linkedConversation;
	
	
	public ActionDialogOption ()
	{
		this.isDisplayed = true;
		title = "Dialogue: Toggle option";
	}
	
	override public float Run ()
	{
		bool setOption = false;
		bool clampOption = false;
		
		if (switchType == SwitchType.On || switchType == SwitchType.OnForever)
		{
			setOption = true;
		}
		
		if (switchType == SwitchType.OffForever || switchType == SwitchType.OnForever)
		{
			clampOption = true;
		}
		
		if (linkedConversation)
		{
			linkedConversation.SetOption (optionNumber, setOption, clampOption);
		}
		
		return 0f;
	}
	
	
	#if UNITY_EDITOR
	
	public override void ShowGUI ()
	{
		linkedConversation = (Conversation) EditorGUILayout.ObjectField ("Conversation:", linkedConversation, typeof (Conversation), true);
		
		Conversation conv = linkedConversation;
		if (conv)
		{
			ButtonDialog[] optionsArray = conv.options.ToArray ();
			string[] options = new string[optionsArray.Length];
			
			for (int j=0; j<options.Length; j++)
			{
				options[j] = j.ToString () + ": ";
				if (optionsArray[j].label == "")
				{
					options[j] += "(Untitled option)";
				}
				else
				{
					options[j] += optionsArray[j].label;
				}
			}
			
			if (optionNumber > options.Length-1)
			{
				// Cap max if some were removed
				optionNumber = options.Length-1;
			}
			
			optionNumber = EditorGUILayout.Popup (optionNumber, options);
		}
		
		switchType = (SwitchType) EditorGUILayout.EnumPopup ("Set to:", switchType);
		
		AfterRunningOption ();
	}
	
	#endif
	
}