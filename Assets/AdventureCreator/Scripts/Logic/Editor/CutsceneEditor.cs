using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor(typeof(Cutscene))]

[System.Serializable]
public class CutsceneEditor : ActionListEditor
{

	public override void OnInspectorGUI ()
	{
		Cutscene _target = (Cutscene) target;

		EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Cutscene properties", EditorStyles.boldLabel);
			_target.actionListType = (ActionListType) EditorGUILayout.EnumPopup ("When running:", _target.actionListType);
			if (_target.actionListType == ActionListType.PauseGameplay)
			{
				_target.isSkippable = EditorGUILayout.Toggle ("Is skippable?", _target.isSkippable);
			}
			_target.triggerTime = EditorGUILayout.Slider ("Start delay (s):", _target.triggerTime, 0f, 10f);
			_target.autosaveAfter = EditorGUILayout.Toggle ("Auto-save after?", _target.autosaveAfter);
		EditorGUILayout.EndVertical ();

		DrawSharedElements ();

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
    }

}