/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionCharPortrait.cs"
 * 
 *	This action picks a new portrait for the chosen Character.
 *	Written for the AC community by Guran.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionCharPortrait : Action
{
	
	public int constantID = 0;
	public bool isPlayer;
	public Char _char;
	public Texture2D newPortraitGraphic;


	public ActionCharPortrait ()
	{
		this.isDisplayed = true;
		title = "Character: Switch Portrait";
	}
	
	
	override public float Run ()
	{
		if (isPlayer)
		{
			_char = GameObject.FindWithTag (Tags.player).GetComponent <Player>();
		}
		else if (isAssetFile && constantID != 0)
		{
			// Attempt to find the correct scene object
			ConstantID idObject = Serializer.returnComponent <ConstantID> (constantID);
			if (idObject != null && idObject.GetComponent <Char>())
			{
				_char = idObject.GetComponent <Char>();
			}
		}
		
		if (_char)
		{
			_char.portraitGraphic = newPortraitGraphic;
		}
		
		return 0f;
	}
	
	
	#if UNITY_EDITOR

	override public void ShowGUI ()
	{
		// Action-specific Inspector GUI code here
		isPlayer = EditorGUILayout.Toggle ("Is Player?", isPlayer);
		if (isPlayer)
		{
			if (Application.isPlaying)
			{
				_char = GameObject.FindWithTag (Tags.player).GetComponent <AC.Char>();
			}
			else
			{
				_char = AdvGame.GetReferences ().settingsManager.GetDefaultPlayer ();
			}
		}
		else
		{
			_char = (Char) EditorGUILayout.ObjectField ("Character:", _char, typeof (Char), true);
			
			if (_char && _char.GetComponent <ConstantID>())
			{
				constantID = _char.GetComponent <ConstantID>().constantID;
			}
		}
		
		newPortraitGraphic = (Texture2D) EditorGUILayout.ObjectField ("New Portrait graphic:", newPortraitGraphic, typeof (Texture2D), true);
		AfterRunningOption ();
	}
	

	public override string SetLabel ()
	{
		string labelAdd = "";

		if (isPlayer)
		{
			labelAdd = " (Player)";
		}
		else if (_char)
		{
			labelAdd = " (" + _char.name + ")";
		}
		
		return labelAdd;
	}

	#endif
	
}