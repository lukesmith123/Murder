using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor(typeof(ArrowPrompt))]
public class ArrowPromptEditor : Editor
{
	
	public override void OnInspectorGUI()
	{
		ArrowPrompt _target = (ArrowPrompt) target;
		
		_target.arrowPromptType = (ArrowPromptType) EditorGUILayout.EnumPopup ("Input type:", _target.arrowPromptType);
			
		EditorGUILayout.BeginVertical ("Button");
		GUILayout.Label ("Up arrow", EditorStyles.boldLabel);
		ArrowGUI (_target.upArrow);
		EditorGUILayout.EndVertical ();
		
		EditorGUILayout.BeginVertical ("Button");
		GUILayout.Label ("Left arrow", EditorStyles.boldLabel);
		ArrowGUI (_target.leftArrow);
		EditorGUILayout.EndVertical ();
		
		EditorGUILayout.BeginVertical ("Button");
		GUILayout.Label ("Right arrow", EditorStyles.boldLabel);
		ArrowGUI (_target.rightArrow);
		EditorGUILayout.EndVertical ();
		
		EditorGUILayout.BeginVertical ("Button");
		GUILayout.Label ("Down arrow", EditorStyles.boldLabel);
		ArrowGUI (_target.downArrow);
		EditorGUILayout.EndVertical ();

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}
	
	
	public void ArrowGUI (Arrow arrow)
	{
		if (arrow != null)
		{
			arrow.isPresent = EditorGUILayout.Toggle ("Provide?", arrow.isPresent);
		
			if (arrow.isPresent)
			{
				arrow.texture = (Texture2D) EditorGUILayout.ObjectField ("Icon texture:", arrow.texture, typeof (Texture2D), true);
				arrow.linkedCutscene = (Cutscene) EditorGUILayout.ObjectField ("Linked Cutscene:", arrow.linkedCutscene, typeof (Cutscene), true);
			}
		}	
	}

}
