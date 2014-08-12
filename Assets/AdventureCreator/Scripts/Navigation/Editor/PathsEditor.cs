using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using AC;

[CustomEditor(typeof(Paths))]
public class PathsEditor : Editor
{
	
	private static GUIContent
		insertContent = new GUIContent("+", "Insert node"),
		deleteContent = new GUIContent("-", "Delete node");

	private static GUILayoutOption
		buttonWidth = GUILayout.MaxWidth(20f);
	
	
	public override void OnInspectorGUI()
	{
		Paths _target = (Paths) target;

		if (_target.GetComponent <AC.Char>())
		{
			return;
		}

		int numNodes = _target.nodes.Count;
		if (numNodes < 1)
		{
			numNodes = 1;
			_target.nodes = ResizeList (_target.nodes, numNodes);
		}

		//EditorGUILayout.LabelField("Nodes: " + (numNodes - 1).ToString());
		_target.nodePause = EditorGUILayout.FloatField ("Wait time (s):", _target.nodePause);
 		_target.pathSpeed = (PathSpeed) EditorGUILayout.EnumPopup("Walk or run:", (PathSpeed) _target.pathSpeed);
		_target.pathType = (AC_PathType) EditorGUILayout.EnumPopup ("Path type:", (AC_PathType) _target.pathType);
		_target.affectY = EditorGUILayout.Toggle ("Override gravity?", _target.affectY);

		// List nodes
		for (int i=1; i<_target.nodes.Count; i++)
		{

			EditorGUILayout.BeginVertical("Button");
				EditorGUILayout.BeginHorizontal ();
					_target.nodes[i] = EditorGUILayout.Vector3Field ("Node " + i + ": ", _target.nodes[i]);
					
					if (GUILayout.Button (insertContent, EditorStyles.miniButtonLeft, buttonWidth))
					{
						Undo.RecordObject (_target, "Add path node");
						Vector3 newNodePosition;
						newNodePosition = _target.nodes[i] + new Vector3 (1.0f, 0f, 0f);
						_target.nodes.Insert (i+1, newNodePosition);
						numNodes += 1;
					}
					if (GUILayout.Button (deleteContent, EditorStyles.miniButtonRight, buttonWidth))
					{
						Undo.RecordObject (_target, "Delete path node");
						_target.nodes.RemoveAt (i);
						numNodes -= 1;
					}
				EditorGUILayout.EndHorizontal ();
			EditorGUILayout.EndVertical();
		}

		if (numNodes == 1 && GUILayout.Button("Add node"))
		{
			Undo.RecordObject (_target, "Add path node");
			numNodes += 1;
		}
		
		_target.nodes[0] = _target.transform.position;
		_target.nodes = ResizeList (_target.nodes, numNodes);

		if (GUI.changed)
		{
			EditorUtility.SetDirty ((Paths) target);
		}
	}
	
	
	private void OnSceneGUI ()
	{
		Paths _target = (Paths) target;
		
		// Go through each element in the nodesArray array and display its stuff
		for (int i=0; i<_target.nodes.Count; i++)
		{
			if (i>0 && !Application.isPlaying)
			{
				_target.nodes[i] = Handles.PositionHandle (_target.nodes[i], Quaternion.identity);
			}
			
			Handles.Label (_target.nodes[i], i.ToString());
		}
		
		if (GUI.changed)
			EditorUtility.SetDirty (_target);
	}

	
	private List<Vector3> ResizeList (List<Vector3> list, int listSize)
	{
		if (list.Count < listSize)
		{
			// Increase size of list
			while (list.Count < listSize)
			{
				Vector3 newNodePosition;
				if (list.Count > 0)
				{
					newNodePosition = list[list.Count-1] + new Vector3 (1.0f, 0f, 0f);
				}
				else
				{
					newNodePosition = Vector3.zero;
				}
				list.Add (newNodePosition);
			}
		}
		else if (list.Count > listSize)
		{
			// Decrease size of list
			while (list.Count > listSize)
			{
				list.RemoveAt (list.Count - 1);
			}
		}
		return (list);
	}

}
