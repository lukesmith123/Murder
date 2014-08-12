/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionCharRender.cs"
 * 
 *	This Action overrides Character
 *	render settings.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionCharRender : Action
{

	public int constantID = 0;
	public bool isPlayer;
	public Char _char;

	public RenderLock renderLock_sorting;
	public SortingMapType mapType;
	public int sortingOrder;
	public string sortingLayer;

	public RenderLock renderLock_scale;
	public int scale;

	public RenderLock renderLock_direction;
	public CharDirection direction;
	
	
	public ActionCharRender ()
	{
		this.isDisplayed = true;
		title = "Character: Change rendering";
	}
	
	
	override public float Run ()
	{
		if (isPlayer)
		{
			_char = GameObject.FindWithTag (Tags.player).GetComponent <Player>();
		}
		else if (isAssetFile && constantID != 0)
		{
			// Attempt to find the correct scene object
			ConstantID idObject = Serializer.returnComponent <ConstantID> (constantID);
			if (idObject != null && idObject.GetComponent <Char>())
			{
				_char = idObject.GetComponent <Char>();
			}
		}

		if (_char)
		{
			if (renderLock_sorting == RenderLock.Set)
			{
				if (mapType == SortingMapType.OrderInLayer)
				{
					_char.SetSorting (sortingOrder);
				}
				else if (mapType == SortingMapType.SortingLayer)
				{
					_char.SetSorting (sortingLayer);
				}
			}
			else if (renderLock_sorting == RenderLock.Release)
			{
				_char.ReleaseSorting ();
			}

			if (_char.animEngine == null)
			{
				_char.ResetAnimationEngine ();
			}
			
			if (_char.animEngine != null)
			{
				_char.animEngine.ActionCharRenderRun (this);
			}
		}

		return 0f;
	}
	
	
	#if UNITY_EDITOR
	
	override public void ShowGUI ()
	{
		isPlayer = EditorGUILayout.Toggle ("Is Player?", isPlayer);
		if (isPlayer)
		{
			if (Application.isPlaying)
			{
				_char = GameObject.FindWithTag (Tags.player).GetComponent <AC.Char>();
			}
			else
			{
				_char = AdvGame.GetReferences ().settingsManager.GetDefaultPlayer ();
			}
		}
		else
		{
			_char = (Char) EditorGUILayout.ObjectField ("Character:", _char, typeof (Char), true);
			
			if (_char && _char.GetComponent <ConstantID>())
			{
				constantID = _char.GetComponent <ConstantID>().constantID;
			}
		}

		if (_char)
		{
			EditorGUILayout.Space ();
			renderLock_sorting = (RenderLock) EditorGUILayout.EnumPopup ("Sorting:", renderLock_sorting);
			if (renderLock_sorting == RenderLock.Set)
			{
				mapType = (SortingMapType) EditorGUILayout.EnumPopup ("Sorting type:", mapType);
				if (mapType == SortingMapType.OrderInLayer)
				{
					sortingOrder = EditorGUILayout.IntField ("New order:", sortingOrder);
				}
				else if (mapType == SortingMapType.SortingLayer)
				{
					sortingLayer = EditorGUILayout.TextField ("New layer:", sortingLayer);
				}
			}

			if (_char.animEngine == null)
			{
				_char.ResetAnimationEngine ();
			}
			if (_char.animEngine)
			{
				_char.animEngine.ActionCharRenderGUI (this);
			}
		}
		else
		{
			EditorGUILayout.HelpBox ("This Action requires a Character before more options will show.", MessageType.Info);
		}

		EditorGUILayout.Space ();
		AfterRunningOption ();
	}
	
	
	public override string SetLabel ()
	{
		string labelAdd = "";
		
		if (isPlayer)
		{
			labelAdd = " (Player)";
		}
		else if (_char)
		{
			labelAdd = " (" + _char.name + ")";
		}

		return labelAdd;
	}
	
	#endif
	
}