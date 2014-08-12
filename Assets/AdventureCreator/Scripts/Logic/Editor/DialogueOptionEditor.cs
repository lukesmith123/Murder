﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor (typeof(DialogueOption))]

[System.Serializable]
public class DialogueOptionEditor : CutsceneEditor
{

	public override void OnInspectorGUI()
    {
		DialogueOption _target = (DialogueOption) target;

		EditorGUILayout.BeginVertical ("Button");
		EditorGUILayout.LabelField ("Dialogue Option properties", EditorStyles.boldLabel);
		if (_target.actionListType == ActionListType.PauseGameplay)
		{
			_target.isSkippable = EditorGUILayout.Toggle ("Is skippable?", _target.isSkippable);
		}
		EditorGUILayout.EndVertical ();

		// Draw all GUI elements that buttons and triggers share
		DrawSharedElements ();

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
    }

}
