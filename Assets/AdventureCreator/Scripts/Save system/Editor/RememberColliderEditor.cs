using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor (typeof (RememberCollider), true)]
public class RememberColliderEditor : ConstantIDEditor
{
	
	public override void OnInspectorGUI()
	{
		RememberCollider _target = (RememberCollider) target;
		
		if (_target.GetComponent <Collider>() == null)
		{
			EditorGUILayout.HelpBox ("This script expects a Collider component!", MessageType.Warning);
		}
		
		SharedGUI ();
	}
	
}
