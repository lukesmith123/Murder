using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor(typeof(AC_Trigger))]

[System.Serializable]
public class AC_TriggerEditor : CutsceneEditor
{
	private string[] Options = { "On enter", "Continuous", "On exit" };

	public override void OnInspectorGUI()
    {
		AC_Trigger _target = (AC_Trigger) target;
   
		EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Trigger properties", EditorStyles.boldLabel);
			_target.actionListType = (ActionListType) EditorGUILayout.EnumPopup ("When running:", _target.actionListType);
			if (_target.actionListType == ActionListType.PauseGameplay)
			{
				_target.isSkippable = EditorGUILayout.Toggle ("Is skippable?", _target.isSkippable);
			}
			_target.triggerType = EditorGUILayout.Popup ("Trigger type:", _target.triggerType, Options);
			_target.cancelInteractions = EditorGUILayout.Toggle ("Cancel interactions?", _target.cancelInteractions);
		EditorGUILayout.EndVertical ();

		DrawSharedElements ();

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
    }

}
