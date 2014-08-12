using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using AC;

[CustomEditor(typeof(InvActionList))]

[System.Serializable]
public class InvActionListEditor : Editor
{
	private int typeNumber = -1;
	private int categoryNumber;
	private int subCategoryNumber;
	private AC.Action actionToAffect = null;
	
	private ActionsManager actionsManager;
	
	private static GUILayoutOption
		labelWidth = GUILayout.MaxWidth(50f);

	
	public override void OnInspectorGUI()
	{
		InvActionList _target = (InvActionList) target;
		
		actionsManager = AdvGame.GetReferences ().actionsManager;
			
		foreach (AC.Action action in _target.actions)
		{
			action.isAssetFile = true;

			int i = _target.actions.IndexOf (action);

			EditorGUILayout.BeginVertical("Button");
			
				string actionLabel = " " + (i).ToString() + ": " + action.title + action.SetLabel ();
				
				EditorGUILayout.BeginHorizontal ();
					action.isDisplayed = EditorGUILayout.Foldout (action.isDisplayed, actionLabel);
					if (!action.isEnabled)
					{
						EditorGUILayout.LabelField ("DISABLED", EditorStyles.boldLabel, GUILayout.Width (100f));
					}
					if (GUILayout.Button ("❖", GUILayout.Width (25f)))
					{
						ActionSideMenu (action);
					}
				EditorGUILayout.EndHorizontal ();
	
				if (action.isDisplayed)
				{
					EditorGUILayout.Space ();
					GUI.enabled = action.isEnabled;
					action.ShowGUI ();
					GUI.enabled = true;
				}

				if (action.endAction == AC.ResultAction.Skip || action is ActionCheck)
				{
					action.SkipActionGUI (_target.actions, action.isDisplayed);
				}

			EditorGUILayout.EndVertical();
			EditorGUILayout.Space ();
		}
		
		EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Type:", labelWidth);
			if (typeNumber == -1)
			{
				typeNumber = actionsManager.defaultClass;
			}
			
			categoryNumber = GetCategoryNumber (typeNumber);
			categoryNumber = EditorGUILayout.Popup(categoryNumber, actionsManager.GetActionCategories ());

			if (subCategoryNumber >= actionsManager.GetActionSubCategories (categoryNumber).Length)
			{
				subCategoryNumber = actionsManager.GetActionSubCategories (categoryNumber).Length - 1;
			}
			if (subCategoryNumber < 0)
			{
				subCategoryNumber = 0;
			}

			subCategoryNumber = EditorGUILayout.Popup(subCategoryNumber, actionsManager.GetActionSubCategories (categoryNumber));
			
			typeNumber = actionsManager.GetTypeNumber (categoryNumber, subCategoryNumber);
			
			if (GUILayout.Button("Add new"))
			{
				AddAction (actionsManager.GetActionName (typeNumber), _target.actions.Count);
			}
		EditorGUILayout.EndHorizontal ();
		
		if (GUI.changed)
		{
			EditorUtility.SetDirty (_target);
		}
	}
	

	private void DeleteAction (AC.Action action)
	{
		UnityEngine.Object.DestroyImmediate (action, true);
		AssetDatabase.SaveAssets ();
		
		InvActionList _target = (InvActionList) target;
		_target.actions.Remove (action);
	}
	
	
	private void AddAction (string className, int i)
	{
		InvActionList _target = (InvActionList) target;
		
		List<int> idArray = new List<int>();
		
		foreach (AC.Action _action in _target.actions)
		{
			idArray.Add (_action.id);
		}

		idArray.Sort ();
		
		AC.Action newAction = (AC.Action) CreateInstance (className);
		
		// Update id based on array
		foreach (int _id in idArray.ToArray())
		{
			if (newAction.id == _id)
				newAction.id ++;
		}

		newAction.name = newAction.title;
		
		_target.actions.Insert (i, newAction);
		AssetDatabase.AddObjectToAsset (newAction, _target);
		AssetDatabase.ImportAsset (AssetDatabase.GetAssetPath (newAction));
	}


	private void ActionSideMenu (AC.Action action)
	{
		InvActionList _target = (InvActionList) target;
		
		int i = _target.actions.IndexOf (action);
		actionToAffect = action;
		GenericMenu menu = new GenericMenu ();
		
		if (action.isEnabled)
		{
			menu.AddItem (new GUIContent ("Disable"), false, Callback, "Disable");
		}
		else
		{
			menu.AddItem (new GUIContent ("Enable"), false, Callback, "Enable");
		}
		menu.AddSeparator ("");
		if (_target.actions.Count > 1)
		{
			menu.AddItem (new GUIContent ("Cut"), false, Callback, "Cut");
		}
		menu.AddItem (new GUIContent ("Copy"), false, Callback, "Copy");
		if (AdvGame.copiedActions.Count > 0)
		{
			menu.AddItem (new GUIContent ("Paste after"), false, Callback, "Paste after");
		}
		menu.AddSeparator ("");
		menu.AddItem (new GUIContent ("Insert after"), false, Callback, "Insert after");
		menu.AddItem (new GUIContent ("Delete"), false, Callback, "Delete");
		if (i > 0 || i < _target.actions.Count-1)
		{
			menu.AddSeparator ("");
		}
		if (i > 0)
		{
			menu.AddItem (new GUIContent ("Re-arrange/Move to top"), false, Callback, "Move to top");
			menu.AddItem (new GUIContent ("Re-arrange/Move up"), false, Callback, "Move up");
		}
		if (i < _target.actions.Count-1)
		{
			menu.AddItem (new GUIContent ("Re-arrange/Move down"), false, Callback, "Move down");
			menu.AddItem (new GUIContent ("Re-arrange/Move to bottom"), false, Callback, "Move to bottom");
		}
		
		menu.ShowAsContext ();
	}
	
	
	private void Callback (object obj)
	{
		InvActionList _target = (InvActionList) target;
		int i = _target.actions.IndexOf (actionToAffect);
		
		switch (obj.ToString ())
		{
		case "Enable":
			Undo.RecordObject (_target, "Enable action");
			_target.actions [i].isEnabled = true;
			break;
			
		case "Disable":
			Undo.RecordObject (_target, "Disable action");
			_target.actions [i].isEnabled = false;
			break;
			
		case "Cut":
			Undo.RecordObject (_target, "Cut action");
			List<AC.Action> cutList = new List<AC.Action>();
			AC.Action cutAction = Object.Instantiate (actionToAffect) as AC.Action;
			cutList.Add (cutAction);
			AdvGame.copiedActions = cutList;
			_target.actions.Remove (actionToAffect);
			UnityEngine.Object.DestroyImmediate (actionToAffect, true);
			AssetDatabase.SaveAssets();
			break;
			
		case "Copy":
			List<AC.Action> copyList = new List<AC.Action>();
			AC.Action copyAction = Object.Instantiate (actionToAffect) as AC.Action;
			copyAction.name = copyAction.name.Replace ("(Clone)", "");
			copyList.Add (copyAction);
			AdvGame.copiedActions = copyList;
			break;
			
		case "Paste after":
			Undo.RecordObject (_target, "Paste actions");
			List<AC.Action> pasteList = AdvGame.copiedActions;
			int j=i+1;
			foreach (AC.Action action in pasteList)
			{
				AC.Action pastedAction = Object.Instantiate (action) as AC.Action;
				pastedAction.name = pastedAction.name.Replace ("(Clone)", "");
				_target.actions.Insert (j, pastedAction);
				j++;
				AssetDatabase.AddObjectToAsset (pastedAction, _target);
				AssetDatabase.ImportAsset (AssetDatabase.GetAssetPath (pastedAction));
			}
			break;
			
		case "Insert after":
			Undo.RecordObject (_target, "Create action");
			AddAction (actionsManager.GetActionName (typeNumber), i+1);
			break;
			
		case "Delete":
			Undo.RecordObject (_target, "Delete action");
			_target.actions.Remove (actionToAffect);
			UnityEngine.Object.DestroyImmediate (actionToAffect, true);
			AssetDatabase.SaveAssets();
			break;
			
		case "Move to top":
			Undo.RecordObject (_target, "Move action to top");
			_target.actions.Remove (actionToAffect);
			_target.actions.Insert (0, actionToAffect);
			break;
			
		case "Move up":
			Undo.RecordObject (_target, "Move action up");
			_target.actions.Remove (actionToAffect);
			_target.actions.Insert (i-1, actionToAffect);
			break;
			
		case "Move to bottom":
			Undo.RecordObject (_target, "Move action to bottom");
			_target.actions.Remove (actionToAffect);
			_target.actions.Insert (_target.actions.Count, actionToAffect);
			break;
			
		case "Move down":
			Undo.RecordObject (_target, "Move action down");
			_target.actions.Remove (actionToAffect);
			_target.actions.Insert (i+1, actionToAffect);
			break;
		}
	}


	private int GetCategoryNumber (int i)
	{
		if (actionsManager)
		{
			return actionsManager.GetActionCategory (i);
		}
		
		return 0;
	}
	
}