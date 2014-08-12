﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor (typeof (RememberTransform), true)]
public class RememberTransformEditor : ConstantIDEditor
{
	
	public override void OnInspectorGUI()
	{
		RememberTransform _target = (RememberTransform) target;

		EditorGUILayout.BeginVertical ("Button");
		EditorGUILayout.LabelField ("Transform", EditorStyles.boldLabel);
		_target.saveParent = EditorGUILayout.Toggle ("Save change in Parent?", _target.saveParent);
		EditorGUILayout.EndVertical ();

		SharedGUI ();
	}
	
}
