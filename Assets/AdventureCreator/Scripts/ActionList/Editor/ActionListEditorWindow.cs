using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using AC;

public class ActionListEditorWindow : EditorWindow
{
	
	private bool isMarquee = false;
	private Rect marqueeRect = new Rect (0f, 0f, 0f, 0f);
	private bool canMarquee = true;
	
	private float zoom = 1f;
	private float zoomMin = 0.5f;
	private float zoomMax = 1f;
	
	private Action actionChanging = null;
	private bool resultType;
	private int offsetChanging = 0;
	private int numActions = 0;
	private int deleteSkips = 0;
	
	private ActionList _target;
	private Vector2 scrollPosition = Vector2.zero;
	private Vector2 maxScroll;
	private Vector2 menuPosition;
	
	private Texture2D socketIn = (Texture2D) AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/Graphics/Textures/pointer-active.png", typeof (Texture2D));
	private Texture2D socketOut = (Texture2D) AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/Graphics/Textures/pointer.png", typeof (Texture2D));
	private Texture2D grey = (Texture2D) AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/Graphics/Textures/grey.png", typeof (Texture2D));
	private ActionsManager actionsManager;
	
	
	[MenuItem ("Adventure Creator/Editors/ActionList Editor")]
	static void Init ()
	{
		ActionListEditorWindow window = (ActionListEditorWindow) EditorWindow.GetWindow (typeof (ActionListEditorWindow));
		window.Repaint ();
		window.title = "ActionList Editor";
	}
	
	
	private void OnEnable ()
	{
		if (AdvGame.GetReferences () && AdvGame.GetReferences ().actionsManager)
		{
			actionsManager = AdvGame.GetReferences ().actionsManager;
		}
		UnmarkAll ();
	}
	
	
	private void PanAndZoomWindow ()
	{
		if (actionChanging)
		{
			return;
		}
		
		if (Event.current.type == EventType.ScrollWheel)
		{
			Vector2 screenCoordsMousePos = Event.current.mousePosition;
			Vector2 delta = Event.current.delta;
			float zoomDelta = -delta.y / 80.0f;
			float oldZoom = zoom;
			zoom += zoomDelta;
			zoom = Mathf.Clamp(zoom, zoomMin, zoomMax);
			scrollPosition += (screenCoordsMousePos - scrollPosition) - (oldZoom / zoom) * (screenCoordsMousePos - scrollPosition);
			
			Event.current.Use();
		}
		
		if (Event.current.type == EventType.MouseDrag && Event.current.button == 2)
		{
			Vector2 delta = Event.current.delta;
			delta /= zoom;
			scrollPosition -= delta;
			
			Event.current.Use();
		}
	}
	
	
	private void DrawMarquee ()
	{
		if (actionChanging)
		{
			return;
		}
		
		if (!canMarquee)
		{
			isMarquee = false;
			return;
		}
		
		if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && !isMarquee)
		{
			isMarquee = true;
			marqueeRect = new Rect (Event.current.mousePosition.x, Event.current.mousePosition.y, 0f, 0f);
		}
		else if (Event.current.type == EventType.MouseUp)
		{
			if (isMarquee)
			{
				MarqueeSelect ();
			}
			isMarquee = false;
		}
		
		if (isMarquee)
		{
			marqueeRect.width = Event.current.mousePosition.x - marqueeRect.x;
			marqueeRect.height = Event.current.mousePosition.y - marqueeRect.y;
			GUI.DrawTexture (marqueeRect, grey);
		}
	}
	
	
	private void MarqueeSelect ()
	{
		if (marqueeRect.width < 0f)
		{
			marqueeRect.x += marqueeRect.width;
			marqueeRect.width *= -1f;
		}
		if (marqueeRect.height < 0f)
		{
			marqueeRect.y += marqueeRect.height;
			marqueeRect.height *= -1f;
		}
		
		// Correct for panning
		marqueeRect.x += scrollPosition.x;
		marqueeRect.y += scrollPosition.y;
		
		// Correct for zooming
		marqueeRect.x /= zoom;
		marqueeRect.y /= zoom;
		marqueeRect.width /= zoom;
		marqueeRect.height /= zoom;
		
		foreach (Action action in _target.actions)
		{
			action.isMarked = false;
			if (action.nodeRect.x < (marqueeRect.x + marqueeRect.width) && (action.nodeRect.x + action.nodeRect.width) > marqueeRect.x &&
			    action.nodeRect.y < (marqueeRect.y + marqueeRect.height) && (action.nodeRect.y + action.nodeRect.height) > marqueeRect.y)
			{
				action.isMarked = true;
			}
		}
	}
	
	
	private void OnGUI ()
	{
		if (Selection.activeGameObject && Selection.activeGameObject.GetComponent <ActionList>() && !(Selection.activeGameObject.GetComponent <ActionList>() is RuntimeActionList))
		{
			_target = Selection.activeGameObject.GetComponent<ActionList>();
		}
		else
		{
			_target = null;
		}
		
		if (_target != null)
		{
			if (_target.actions.Count == 0 || (_target.actions.Count == 1 && _target.actions[0] == null))
			{
				_target.actions.Clear ();
				AC.Action newAction = ActionListEditor.GetDefaultAction ();
				_target.actions.Add (newAction);
			}
			
			PanAndZoomWindow ();
			NodesGUI ();
			DrawMarquee ();
			
			if (GUI.changed)
			{
				EditorUtility.SetDirty (_target);
			}
		}
		else
		{
			GUILayout.Space (10f);
			GUILayout.Label ("No ActionList object selected", EditorStyles.largeLabel);
		}
	}
	
	
	private void OnInspectorUpdate ()
	{
		Repaint();
	}
	
	
	private void PositionNode (Action action)
	{
		if (_target == null)
		{
			return;
		}
		
		int i = _target.actions.IndexOf (action);
		
		if (i == 0)
		{
			action.nodeRect.x = 50;
			action.nodeRect.y = 50;
		}
		else
		{
			action.nodeRect.x = _target.actions[i-1].nodeRect.x + 350;
			action.nodeRect.y = _target.actions[i-1].nodeRect.y;
		}
	}
	
	
	private void PositionAllNodes ()
	{
		if (_target != null && _target.actions.Count > 0)
		{
			foreach (Action action in _target.actions)
			{
				PositionNode (action);
			}
		}
	}
	
	
	private void NodeWindow (int i)
	{
		if (actionsManager == null)
		{
			OnEnable ();
		}
		if (actionsManager == null)
		{
			return;
		}
		
		GUI.enabled = _target.actions[i].isEnabled;
		
		int typeNumber = GetTypeNumber (i);
		int categoryNumber = GetCategoryNumber (typeNumber);
		int subCategoryNumber = GetSubCategoryNumber (i, categoryNumber);
		
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("Action type:", GUILayout.Width (100));
		categoryNumber = EditorGUILayout.Popup(categoryNumber, actionsManager.GetActionCategories ());
		subCategoryNumber = EditorGUILayout.Popup(subCategoryNumber, actionsManager.GetActionSubCategories (categoryNumber));
		EditorGUILayout.EndVertical ();
		
		typeNumber = actionsManager.GetTypeNumber (categoryNumber, subCategoryNumber);
		//typeNumber = EditorGUILayout.Popup("Action type:", typeNumber, actionsManager.GetActionTitles ());
		
		EditorGUILayout.Space ();
		
		// Rebuild constructor if Subclass and type string do not match
		if (_target.actions[i].GetType().ToString() != actionsManager.GetActionName (typeNumber))
		{
			Vector2 currentPosition = new Vector2 (_target.actions[i].nodeRect.x, _target.actions[i].nodeRect.y);
			_target.actions[i] = ActionListEditor.RebuildAction (_target.actions[i], typeNumber);
			_target.actions[i].nodeRect.x = currentPosition.x;
			_target.actions[i].nodeRect.y = currentPosition.y;
		}
		
		_target.actions[i].ShowGUI ();
		
		GUI.enabled = true;
		if (_target.actions[i].endAction == ResultAction.Skip || _target.actions[i].numSockets == 2)
		{
			_target.actions[i].SkipActionGUI (_target.actions, true);
		}
		
		if (i == 0)
		{
			_target.actions[i].nodeRect.x = 50;
			_target.actions[i].nodeRect.y = 50;
		}
		else if (deleteSkips == 0)
		{
			GUI.DragWindow ();
		}
	}
	
	
	private void LimitWindow (Action action)
	{
		if (action.nodeRect.x < 1)
		{
			action.nodeRect.x = 1;
		}
		
		if (action.nodeRect.y < 1)
		{
			action.nodeRect.y = 1;
		}
	}
	
	
	private void NodesGUI ()
	{
		if (_target.actions[0].nodeRect.x == 0 || _target.actions[0].nodeRect.y == 0)
		{
			PositionAllNodes ();
		}
		
		bool loseConnection = false;
		Event e = Event.current;
		if (e.isMouse && actionChanging != null && e.type == EventType.MouseUp)
		{
			loseConnection = true;
		}
		
		numActions = _target.actions.Count;
		if (numActions < 1)
		{
			numActions = 1;
			AC.Action newAction = ActionListEditor.GetDefaultAction ();
			_target.actions.Add (newAction);
		}
		
		EditorZoomArea.Begin (zoom, new Rect (0, 0, position.width / zoom, position.height / zoom));
		scrollPosition = GUI.BeginScrollView (new Rect (0, 0, position.width / zoom, position.height / zoom), scrollPosition, new Rect (0, 0, maxScroll.x, maxScroll.y), false, false);
		BeginWindows ();
		
		canMarquee = true;
		Vector2 newMaxScroll = Vector2.zero;
		for (int i=0; i<_target.actions.Count; i++)
		{
			FixConnections (i);
			
			if (_target.actions[i].nodeRect.x == 0 && _target.actions[i].nodeRect.y == 0)
			{
				PositionNode (_target.actions[i]);
			}
			
			Color tempColor = GUI.color;
			
			if (_target.actions[i].isRunning)
			{
				GUI.color = Color.cyan;
			}
			else if (_target.actions[i].isMarked)
			{
				GUI.color = Color.green;
			}
			
			_target.actions[i].nodeRect.height = 10;
			if (deleteSkips > 0)
			{
				deleteSkips --;
				Rect originalRect = _target.actions[i].nodeRect;
				_target.actions[i].nodeRect = GUILayout.Window (i, _target.actions[i].nodeRect, NodeWindow, i + ": " + _target.actions[i].title, GUILayout.Width (300));
				_target.actions[i].nodeRect.x = originalRect.x;
				_target.actions[i].nodeRect.y = originalRect.y;
			}
			else
			{
				_target.actions[i].nodeRect = GUILayout.Window (i, _target.actions[i].nodeRect, NodeWindow, i + ": " + _target.actions[i].title, GUILayout.Width (300));
			}
			
			GUI.color = tempColor;
			
			if (_target.actions[i].nodeRect.x + _target.actions[i].nodeRect.width + 20 > newMaxScroll.x)
			{
				newMaxScroll.x = _target.actions[i].nodeRect.x + _target.actions[i].nodeRect.width + 200;
			}
			if (_target.actions[i].nodeRect.height != 10)
			{
				if (_target.actions[i].nodeRect.y + _target.actions[i].nodeRect.height + 20 > newMaxScroll.y)
				{
					newMaxScroll.y = _target.actions[i].nodeRect.y + _target.actions[i].nodeRect.height + 200;
				}
			}
			
			LimitWindow (_target.actions[i]);
			DrawSockets (_target.actions[i]);
			
			if (_target.actions[i].endAction == ResultAction.Skip || _target.actions[i].numSockets == 2)
			{
				_target.actions[i].SkipActionGUI (_target.actions, false);
			}
			
			_target.actions = ActionListEditor.ResizeList (_target.actions, numActions);
			
			if (actionChanging != null && loseConnection && (new Rect (_target.actions[i].nodeRect.x - 20, _target.actions[i].nodeRect.y, 20, 20).Contains(e.mousePosition) || _target.actions[i].nodeRect.Contains(e.mousePosition)))
			{
				Reconnect (actionChanging, _target.actions[i]);
			}
			
			if (!isMarquee && _target.actions[i].nodeRect.Contains (e.mousePosition))
			{
				canMarquee = false;
			}
			
		}
		
		if (loseConnection && actionChanging != null)
		{
			EndConnect (actionChanging, e.mousePosition);
		}
		
		if (actionChanging != null)
		{
			AdvGame.DrawNodeCurve (actionChanging.nodeRect, e.mousePosition, Color.cyan, offsetChanging);
		}
		
		if (e.type == EventType.ContextClick && actionChanging == null && !isMarquee)
		{
			menuPosition = e.mousePosition;
			CheckEmptyMenu (e.mousePosition);
		}
		
		EndWindows ();
		GUI.EndScrollView ();
		EditorZoomArea.End();
		
		if (newMaxScroll.y != 0)
		{
			maxScroll = newMaxScroll;
		}
	}
	
	
	private void UnmarkAll ()
	{
		if (_target && _target.actions.Count > 0)
		{
			foreach (Action action in _target.actions)
			{
				if (action)
				{
					action.isMarked = false;
				}
			}
		}
	}
	
	
	private Action InsertAction (int i, Vector2 position)
	{
		ActionListEditor.ModifyAction (_target, _target.actions[i], "Insert after");
		numActions ++;
		UnmarkAll ();
		
		_target.actions[i+1].nodeRect.x = position.x;
		_target.actions[i+1].nodeRect.y = position.y;
		_target.actions[i+1].endAction = ResultAction.Stop;
		
		return _target.actions[i+1];
	}
	
	
	private void FixConnections (int i)
	{
		if (_target.actions[i].numSockets == 0)
		{
			_target.actions[i].endAction = ResultAction.Stop;
		}
		
		else if (_target.actions[i] is ActionCheck)
		{
			ActionCheck tempAction = (ActionCheck) _target.actions[i];
			if (tempAction.resultActionTrue == ResultAction.Skip && !_target.actions.Contains (tempAction.skipActionTrueActual))
			{
				if (tempAction.skipActionTrue >= _target.actions.Count)
				{
					tempAction.resultActionTrue = ResultAction.Stop;
				}
			}
			if (tempAction.resultActionFail == ResultAction.Skip && !_target.actions.Contains (tempAction.skipActionFailActual))
			{
				if (tempAction.skipActionFail >= _target.actions.Count)
				{
					tempAction.resultActionFail = ResultAction.Stop;
				}
			}
		}
		else
		{
			if (_target.actions[i].endAction == ResultAction.Skip && !_target.actions.Contains (_target.actions[i].skipActionActual))
			{
				if (_target.actions[i].skipAction >= _target.actions.Count)
				{
					_target.actions[i].endAction = ResultAction.Stop;
				}
			}
		}
	}
	
	
	private void EndConnect (Action action1, Vector2 mousePosition)
	{
		isMarquee = false;
		
		if (action1 is ActionCheck)
		{
			ActionCheck tempAction = (ActionCheck) action1;
			
			if (resultType)
			{
				if (_target.actions.IndexOf (action1) == _target.actions.Count - 1 && tempAction.resultActionTrue != ResultAction.Skip)
				{
					InsertAction (_target.actions.IndexOf (action1), mousePosition);
					tempAction.resultActionTrue = ResultAction.Continue;
				}
				else if (tempAction.resultActionTrue == ResultAction.Stop)
				{
					tempAction.resultActionTrue = ResultAction.Skip;
					tempAction.skipActionTrueActual = InsertAction (_target.actions.Count-1, mousePosition);
				}
				else
				{
					tempAction.resultActionTrue = ResultAction.Stop;
				}
			}
			else
			{
				if (_target.actions.IndexOf (action1) == _target.actions.Count - 1 && tempAction.resultActionFail != ResultAction.Skip)
				{
					InsertAction (_target.actions.IndexOf (action1), mousePosition);
					tempAction.resultActionFail = ResultAction.Continue;
				}
				else if (tempAction.resultActionFail == ResultAction.Stop)
				{
					tempAction.resultActionFail = ResultAction.Skip;
					tempAction.skipActionFailActual = InsertAction (_target.actions.Count-1, mousePosition);
				}
				else
				{
					tempAction.resultActionFail = ResultAction.Stop;
				}
			}
		}
		else
		{
			if (_target.actions.IndexOf (action1) == _target.actions.Count - 1 && action1.endAction != ResultAction.Skip)
			{
				InsertAction (_target.actions.IndexOf (action1), mousePosition);
				action1.endAction = ResultAction.Continue;
			}
			else if (action1.endAction == ResultAction.Stop)
			{
				action1.endAction = ResultAction.Skip;
				action1.skipActionActual = InsertAction (_target.actions.Count-1, mousePosition);
			}
			else
			{
				action1.endAction = ResultAction.Stop;
			}
		}
		
		actionChanging = null;
		offsetChanging = 0;
		
		EditorUtility.SetDirty (_target);
	}
	
	
	private void Reconnect (Action action1, Action action2)
	{
		isMarquee = false;
		
		if (action1 is ActionCheck)
		{
			ActionCheck actionCheck = (ActionCheck) action1;
			
			if (resultType)
			{
				actionCheck.resultActionTrue = ResultAction.Skip;
				if (action2 != null)
				{
					actionCheck.skipActionTrueActual = action2;
				}
			}
			else
			{
				actionCheck.resultActionFail = ResultAction.Skip;
				if (action2 != null)
				{
					actionCheck.skipActionFailActual = action2;
				}
			}
		}
		else
		{
			action1.endAction = ResultAction.Skip;
			action1.skipActionActual = action2;
		}
		
		actionChanging = null;
		offsetChanging = 0;
		
		EditorUtility.SetDirty (_target);
	}
	
	
	private Rect SocketIn (Action action)
	{
		return new Rect (action.nodeRect.x - 20, action.nodeRect.y, 20, 20);
	}
	
	
	private void DrawSockets (Action action)
	{
		int i = _target.actions.IndexOf (action);
		Event e = Event.current;
		
		if (SocketIn (action).Contains (e.mousePosition) && actionChanging)
		{
			GUI.Label (SocketIn (action), socketOut, "Label");
		}
		else
		{
			GUI.Label (SocketIn (action), socketIn, "Label");
		}
		
		if (action.numSockets == 0)
		{
			return;
		}
		
		int offset = 0;
		
		if (action is ActionCheck)
		{
			ActionCheck actionCheck = (ActionCheck) action;
			if (actionCheck.resultActionFail != ResultAction.RunCutscene)
			{
				Rect buttonRect = new Rect (action.nodeRect.x + action.nodeRect.width, action.nodeRect.y - 22 + action.nodeRect.height, 20, 20);
				
				if (e.isMouse && actionChanging == null && e.type == EventType.MouseDown && action.isEnabled && buttonRect.Contains(e.mousePosition))
				{
					offsetChanging = 10;
					resultType = false;
					actionChanging = action;
				}
				
				if (actionChanging == null && action.isEnabled && buttonRect.Contains (e.mousePosition))
				{
					GUI.Button (buttonRect, socketIn, "Label");
				}
				else
				{
					GUI.Button (buttonRect, socketOut, "Label");
				}
				
				if (actionCheck.resultActionFail == ResultAction.Skip)
				{
					offset = 17;
				}
			}
			if (actionCheck.resultActionTrue != ResultAction.RunCutscene)
			{
				Rect buttonRect = new Rect (action.nodeRect.x + action.nodeRect.width, action.nodeRect.y - 40 - offset + action.nodeRect.height, 20, 20);
				
				if (e.isMouse && actionChanging == null && e.type == EventType.MouseDown && action.isEnabled && buttonRect.Contains(e.mousePosition))
				{
					offsetChanging = 30 + offset;
					resultType = true;
					actionChanging = action;
				}
				
				if (actionChanging == null && action.isEnabled && buttonRect.Contains (e.mousePosition))
				{
					GUI.Button (buttonRect, socketIn, "Label");
				}
				else
				{
					GUI.Button (buttonRect, socketOut, "Label");
				}
			}
		}
		else
		{
			if (action.endAction != ResultAction.RunCutscene)
			{
				Rect buttonRect = new Rect (action.nodeRect.x + action.nodeRect.width, action.nodeRect.y - 22 + action.nodeRect.height, 20, 20);
				
				if (e.isMouse && actionChanging == null && e.type == EventType.MouseDown && action.isEnabled && buttonRect.Contains(e.mousePosition))
				{
					offsetChanging = 10;
					actionChanging = action;
				}
				
				if (actionChanging == null && action.isEnabled && buttonRect.Contains (e.mousePosition))
				{
					GUI.Button (buttonRect, socketIn, "Label");
				}
				else
				{
					GUI.Button (buttonRect, socketOut, "Label");
				}
			}
		}
		
		action.DrawOutWires (_target.actions, i, offset);
	}
	
	
	private int GetTypeNumber (int i)
	{
		ActionsManager actionsManager = AdvGame.GetReferences ().actionsManager;
		
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
					_target.actions[i] = (Action) CreateInstance (defaultAction);
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
		int number = 0;
		
		if (actionsManager)
		{
			return actionsManager.GetActionSubCategory (_target.actions[i].title, _categoryNumber);
		}
		
		return number;
	}
	
	
	private bool AreAnyActionsMarked ()
	{
		foreach (Action action in _target.actions)
		{
			if (action.isMarked)
			{
				return true;
			}
		}
		
		return false;
	}
	
	
	private void CheckEmptyMenu (Vector2 mousePosition)
	{
		if (_target.actions[0].nodeRect.Contains (mousePosition))
		{
			return;
		}
		
		CreateEmptyMenu ();
	}
	
	
	private void CreateEmptyMenu ()
	{
		GenericMenu menu = new GenericMenu ();
		menu.AddItem (new GUIContent ("Add new Action"), false, EmptyCallback, "Insert end");
		if (AdvGame.copiedActions != null && AdvGame.copiedActions.Count > 0)
		{
			menu.AddSeparator ("");
			menu.AddItem (new GUIContent ("Paste"), false, EmptyCallback, "Paste");
		}
		
		menu.AddSeparator ("");
		menu.AddItem (new GUIContent ("Select all"), false, EmptyCallback, "Mark all");
		
		if (AreAnyActionsMarked ())
		{
			menu.AddItem (new GUIContent ("Deselect all"), false, EmptyCallback, "Unmark all");
			menu.AddSeparator ("");
			menu.AddItem (new GUIContent ("Copy selected"), false, EmptyCallback, "Copy marked");
			menu.AddItem (new GUIContent ("Delete selected"), false, EmptyCallback, "Delete marked");
			menu.AddSeparator ("");
			menu.AddItem (new GUIContent ("Enable selected"), false, EmptyCallback, "Enable marked");
			menu.AddItem (new GUIContent ("Disable selected"), false, EmptyCallback, "Disable marked");
		}
		
		menu.ShowAsContext ();
	}
	
	
	private void ActionCallback (object obj)
	{
		Action actionToAffect = null;
		
		foreach (Action action in _target.actions)
		{
			if (action.nodeRect.Contains (new Vector2 (menuPosition.x + 50, menuPosition.y)))
			{
				actionToAffect = action;
				break;
			}
		}
		
		if (actionToAffect == null)
		{
			return;
		}
		
		ActionListEditor.ModifyAction (_target, actionToAffect, obj.ToString ());
		EditorUtility.SetDirty (_target);
	}
	
	
	private void EmptyCallback (object obj)
	{
		if (obj.ToString () == "Insert end")
		{
			ActionListEditor.ModifyAction (_target, null, obj.ToString ());
			_target.actions[_target.actions.Count-1].nodeRect.x = menuPosition.x;
			_target.actions[_target.actions.Count-1].nodeRect.y = menuPosition.y;
		}
		else if (obj.ToString () == "Paste")
		{
			if (AdvGame.copiedActions.Count == 0)
			{
				return;
			}
			Undo.RecordObject (_target, "Paste actions");
			List<Action> pasteList = AdvGame.copiedActions;
			Vector2 firstPosition = new Vector2 (pasteList[0].nodeRect.x, pasteList[0].nodeRect.y);
			foreach (Action pasteAction in pasteList)
			{
				if (pasteList.IndexOf (pasteAction) == 0)
				{
					pasteAction.nodeRect.x = menuPosition.x;
					pasteAction.nodeRect.y = menuPosition.y;
				}
				else
				{
					pasteAction.nodeRect.x = menuPosition.x + (pasteAction.nodeRect.x - firstPosition.x);
					pasteAction.nodeRect.y = menuPosition.y + (pasteAction.nodeRect.y - firstPosition.y);
				}
				_target.actions.Add (pasteAction);
			}
			AdvGame.DuplicateActionsBuffer ();
		}
		else if (obj.ToString () == "Mark all")
		{
			foreach (Action action in _target.actions)
			{
				action.isMarked = true;
			}
		}
		else if (obj.ToString () == "Unmark all")
		{
			foreach (Action action in _target.actions)
			{
				action.isMarked = false;
			}
		}
		else if (obj.ToString () == "Copy marked")
		{
			List<Action> copyList = new List<Action>();
			foreach (Action action in _target.actions)
			{
				if (action.isMarked)
				{
					action.isMarked = false;
					Action copyAction = Object.Instantiate (action) as Action;
					copyList.Add (copyAction);
				}
			}
			AdvGame.copiedActions = copyList;
		}
		else if (obj.ToString () == "Delete marked")
		{
			while (AreAnyActionsMarked ())
			{
				foreach (Action action in _target.actions)
				{
					if (action.isMarked)
					{
						_target.actions.Remove (action);
						deleteSkips = _target.actions.Count;
						numActions --;
						DestroyImmediate (action);
						break;
					}
				}
			}
			if (_target.actions.Count == 0)
			{
				_target.actions.Add (ActionListEditor.GetDefaultAction ());
			}
		}
		else if (obj.ToString () == "Enable marked")
		{
			foreach (Action action in _target.actions)
			{
				if (action.isMarked)
				{
					action.isEnabled = true;
				}
			}
		}
		else if (obj.ToString () == "Disable marked")
		{
			foreach (Action action in _target.actions)
			{
				if (action.isMarked)
				{
					action.isEnabled = false;
				}
			}
		}
		EditorUtility.SetDirty (_target);
	}
	
}