using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor (typeof(Interaction))]

[System.Serializable]
public class InteractionEditor : CutsceneEditor
{

	public override void OnInspectorGUI()
    {
		Interaction _target = (Interaction) target;
		
		EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Interaction properties", EditorStyles.boldLabel);
			_target.actionListType = (ActionListType) EditorGUILayout.EnumPopup ("When running:", _target.actionListType);
			if (_target.actionListType == ActionListType.PauseGameplay)
			{
				_target.isSkippable = EditorGUILayout.Toggle ("Is skippable?", _target.isSkippable);
			}
		EditorGUILayout.EndVertical ();

		DrawSharedElements ();

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}

    }

}
