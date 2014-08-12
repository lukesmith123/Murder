using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	[CustomEditor (typeof (ActionList))]

	[System.Serializable]
	public class ActionListEditor : Editor
	{

		private int categoryNumber;
		private int subCategoryNumber;
		private int typeNumber;
		private AC.Action actionToAffect = null;
		
		private ActionsManager actionsManager;
		
		
		public override void OnInspectorGUI ()
		{
			ActionList _target = (ActionList) target;
			
			DrawSharedElements ();
			
			if (GUI.changed)
			{
				EditorUtility.SetDirty (_target);
			}
		}
		
		
		protected void DrawSharedElements ()
		{
			if (AdvGame.GetReferences () == null)
			{
				Debug.LogError ("A References file is required - please use the Adventure Creator window to create one.");
				EditorGUILayout.LabelField ("No References file found!");
			}
			else
			{
				actionsManager = AdvGame.GetReferences ().actionsManager;
				
				ActionList _target = (ActionList) target;
				
				if (actionsManager)
				{
					int numActions = _target.actions.Count;
					if (numActions < 1)
					{
						numActions = 1;
						AC.Action newAction = GetDefaultAction ();
						_target.actions.Add (newAction);
					}
					
					EditorGUILayout.Space ();
					EditorGUILayout.BeginHorizontal ();
					if (GUILayout.Button ("Expand all", EditorStyles.miniButtonLeft))
					{
						Undo.RecordObject (_target, "Expand actions");
						foreach (AC.Action action in _target.actions)
						{
							action.isDisplayed = true;
						}
					}
					if (GUILayout.Button ("Collapse all", EditorStyles.miniButtonMid))
					{
						Undo.RecordObject (_target, "Collapse actions");
						foreach (AC.Action action in _target.actions)
						{
							action.isDisplayed = false;
						}
					}
					if (GUILayout.Button ("Action List Editor", EditorStyles.miniButtonRight))
					{
						ActionListEditorWindow window = (ActionListEditorWindow) EditorWindow.GetWindow (typeof (ActionListEditorWindow));
						window.Repaint ();
					}
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.Space ();
					
					for (int i=0; i<_target.actions.Count; i++)
					{
						EditorGUILayout.BeginVertical ("Button");
						typeNumber = GetTypeNumber (i);
						categoryNumber = GetCategoryNumber (typeNumber);
						subCategoryNumber = GetSubCategoryNumber (i, categoryNumber);
						
						EditorGUILayout.BeginHorizontal ();
						string actionLabel = " " + (i).ToString() + ": " + _target.actions[i].title + _target.actions[i].SetLabel ();
						_target.actions[i].isDisplayed = EditorGUILayout.Foldout (_target.actions[i].isDisplayed, actionLabel);
						if (!_target.actions[i].isEnabled)
						{
							EditorGUILayout.LabelField ("DISABLED", EditorStyles.boldLabel, GUILayout.MaxWidth (100f));
						}
						Texture2D icon = (Texture2D) AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/Graphics/Textures/inspector-use.png", typeof (Texture2D));
						if (GUILayout.Button (icon, GUILayout.Width (20f), GUILayout.Height (15f)))
						{
							ActionSideMenu (i);
						}

						_target.actions[i].isAssetFile = false;
						
						EditorGUILayout.EndHorizontal ();
						
						if (_target.actions[i].isDisplayed)
						{
							GUI.enabled = _target.actions[i].isEnabled;

							EditorGUILayout.BeginHorizontal ();
							EditorGUILayout.LabelField ("Action type:", GUILayout.Width (150));
							categoryNumber = EditorGUILayout.Popup(categoryNumber, actionsManager.GetActionCategories ());
							subCategoryNumber = EditorGUILayout.Popup(subCategoryNumber, actionsManager.GetActionSubCategories (categoryNumber));
							EditorGUILayout.EndVertical ();

							typeNumber = actionsManager.GetTypeNumber (categoryNumber, subCategoryNumber);
							//typeNumber = EditorGUILayout.Popup("Action type:", typeNumber, actionsManager.GetActionTitles ());

							EditorGUILayout.Space ();
							
							// Rebuild constructor if Subclass and type string do not match
							if (_target.actions[i].GetType().ToString() != actionsManager.GetActionName (typeNumber))
							{
								_target.actions[i] = ActionListEditor.RebuildAction (_target.actions[i], typeNumber);
							}
							ActionListEditor.ShowActionGUI (_target.actions[i], _target.gameObject);
						}
						
						if (_target.actions[i].endAction == AC.ResultAction.Skip || _target.actions[i].numSockets == 2)
						{
							_target.actions[i].SkipActionGUI (_target.actions, _target.actions[i].isDisplayed);
						}

						GUI.enabled = true;
						
						EditorGUILayout.EndVertical();
						EditorGUILayout.Space ();
					}
					
					if (GUILayout.Button("Add new action"))
					{
						Undo.RecordObject (_target, "Create action");
						numActions += 1;
					}
					
					_target.actions = ActionListEditor.ResizeList (_target.actions, numActions);
				}
			}
		}
		
		
		private int GetTypeNumber (int i)
		{
			ActionList _target = (ActionList) target;
			
			int number = 0;
			
			if (actionsManager)
			{
				for (int j=0; j<actionsManager.GetActionsSize(); j++)
				{
					try
					{
						if (_target.actions[i].GetType().ToString() == actionsManager.GetActionName(j))
						{
							number = j;
							break;
						}
					}
					
					catch
					{
						string defaultAction = actionsManager.GetDefaultAction ();
						_target.actions[i] = (AC.Action) CreateInstance (defaultAction);
					}
				}
			}
			
			return number;
		}	


		private int GetCategoryNumber (int i)
		{
			if (actionsManager)
			{
				return actionsManager.GetActionCategory (i);
			}
			
			return 0;
		}


		private int GetSubCategoryNumber (int i, int _categoryNumber)
		{
			ActionList _target = (ActionList) target;
			
			int number = 0;
			
			if (actionsManager)
			{
				return actionsManager.GetActionSubCategory (_target.actions[i].title, _categoryNumber);
			}
			
			return number;
		}
		
		
		public static void ShowActionGUI (AC.Action action, GameObject _target)
		{
			action.ShowGUI ();
		}
		
		
		public static AC.Action RebuildAction (AC.Action action, int number)
		{
			ActionsManager actionsManager = AdvGame.GetReferences ().actionsManager;
			
			if (actionsManager)
			{
				string ClassName = actionsManager.GetActionName (number);
				
				if (action.GetType().ToString() != ClassName)
				{
					action = (AC.Action) CreateInstance (ClassName);
				}
			}
			
			return action;
		}
		
		
		public static AC.Action GetDefaultAction ()
		{
			ActionsManager actionsManager = AdvGame.GetReferences ().actionsManager;
			
			if (actionsManager)
			{
				string defaultAction = actionsManager.GetDefaultAction ();
				return ((AC.Action) CreateInstance (defaultAction));
			}
			else
			{
				Debug.LogError ("Cannot create Action - no Actions Manager found.");
				return null;
			}
		}
		
		
		private void ActionSideMenu (int i)
		{
			ActionList _target = (ActionList) target;
			actionToAffect = _target.actions[i];
			GenericMenu menu = new GenericMenu ();
			
			if (_target.actions[i].isEnabled)
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
			if (_target.actions.Count > 1)
			{
				menu.AddItem (new GUIContent ("Delete"), false, Callback, "Delete");
			}
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
			ActionList t = (ActionList) target;
			ModifyAction (t, actionToAffect, obj.ToString ());
			EditorUtility.SetDirty (t);
		}
		
		
		public static void ModifyAction (ActionList _target, AC.Action _action, string callback)
		{
			int i = -1;
			if (_action != null && _target.actions.IndexOf (_action) > -1)
			{
				i = _target.actions.IndexOf (_action);
			}
			
			switch (callback)
			{
			case "Enable":
				Undo.RecordObject (_target, "Enable action");
				_action.isEnabled = true;
				break;
				
			case "Disable":
				Undo.RecordObject (_target, "Disable action");
				_action.isEnabled = false;
				break;
				
			case "Cut":
				Undo.RecordObject (_target, "Cut action");
				List<AC.Action> cutList = new List<AC.Action>();
				AC.Action cutAction = Object.Instantiate (_action) as AC.Action;
				cutList.Add (cutAction);
				AdvGame.copiedActions = cutList;
				_target.actions.Remove (_action);
				break;
				
			case "Copy":
				List<AC.Action> copyList = new List<AC.Action>();
				AC.Action copyAction = Object.Instantiate (_action) as AC.Action;
				copyAction.nodeRect = new Rect (0,0,300,60);
				copyList.Add (copyAction);
				AdvGame.copiedActions = copyList;
				break;
				
			case "Paste after":
				Undo.RecordObject (_target, "Paste actions");
				List<AC.Action> pasteList = AdvGame.copiedActions;
				_target.actions.InsertRange (i+1, pasteList);
				AdvGame.DuplicateActionsBuffer ();
				break;

			case "Insert end":
				Undo.RecordObject (_target, "Create action");
				AC.Action newAction = GetDefaultAction ();
				_target.actions.Add (newAction);
				break;
				
			case "Insert after":
				Undo.RecordObject (_target, "Create action");
				_target.actions.Insert (i+1, GetDefaultAction ());
				break;
				
			case "Delete":
				Undo.RecordObject (_target, "Delete action");
				_target.actions.Remove (_action);
				break;
				
			case "Move to top":
				Undo.RecordObject (_target, "Move action to top");
				_target.actions.Remove (_action);
				_target.actions.Insert (0, _action);
				break;
				
			case "Move up":
				Undo.RecordObject (_target, "Move action up");
				_target.actions.Remove (_action);
				_target.actions.Insert (i-1, _action);
				break;
				
			case "Move to bottom":
				Undo.RecordObject (_target, "Move action to bottom");
				_target.actions.Remove (_action);
				_target.actions.Insert (_target.actions.Count, _action);
				break;
				
			case "Move down":
				Undo.RecordObject (_target, "Move action down");
				_target.actions.Remove (_action);
				_target.actions.Insert (i+1, _action);
				break;
			}
		}
		

		public static void PushNodes (List<AC.Action> list, float xPoint, int count)
		{
			foreach (AC.Action action in list)
			{
				if (action.nodeRect.x > xPoint)
				{
					action.nodeRect.x += 350 * count;
				}
			}
		}
		
		
		public static List<AC.Action> ResizeList (List<AC.Action> list, int listSize)
		{
			ActionsManager actionsManager = AdvGame.GetReferences ().actionsManager;
			
			string defaultAction = "";
			
			if (actionsManager)
			{
				defaultAction = actionsManager.GetDefaultAction ();
			}
			
			if (list.Count < listSize)
			{
				// Increase size of list
				while (list.Count < listSize)
				{
					List<int> idArray = new List<int>();
					
					foreach (AC.Action _action in list)
					{
						idArray.Add (_action.id);
					}
					
					idArray.Sort ();
					
					list.Add ((AC.Action) CreateInstance (defaultAction));
					
					// Update id based on array
					foreach (int _id in idArray.ToArray())
					{
						if (list [list.Count -1].id == _id)
							list [list.Count -1].id ++;
					}
				}
			}
			else if (list.Count > listSize)
			{
				// Decrease size of list
				while (list.Count > listSize)
					list.RemoveAt (list.Count - 1);
			}
			
			return (list);
		}
		
	}

}