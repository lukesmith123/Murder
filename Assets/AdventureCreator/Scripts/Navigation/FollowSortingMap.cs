/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"FollowSortingMap.cs"
 * 
 *	This script causes any attached Sprite Renderer
 *	to change according to the scene's Sorting Map.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

public class FollowSortingMap : MonoBehaviour
{

	public bool lockSorting = false;
	public bool affectChildren = true;
	public bool followSortingMap = false;
	public bool offsetOriginal = false;

	private int offset;
	private int sortingOrder = 0;
	private string sortingLayer = "";
	private SortingMap sortingMap;


	public void Awake ()
	{
		UpdateSortingMap ();

		if (offsetOriginal && GetComponent <SpriteRenderer>())
		{
			offset = GetComponent <SpriteRenderer>().sortingOrder;
		}
		else
		{
			offset = 0;
		}
	}


	private void OnLevelWasLoaded ()
	{
		UpdateSortingMap ();
	}


	public void UpdateSortingMap ()
	{
		if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>() && GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>().sortingMap != null)
		{
			sortingMap = GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>().sortingMap;
		}
		else
		{
			Debug.Log ("Cannot find sorting map to follow!");
		}
	}


	private void Update ()
	{
		if (lockSorting)
		{
			return;
		}

		if (sortingMap == null)
		{
			return;
		}

		if (followSortingMap && sortingMap.sortingAreas.Count > 0)
		{
			for (int i=0; i<sortingMap.sortingAreas.Count; i++)
			{
				// Determine angle between SortingMap's normal and relative position - if <90, must be "behind" the plane
				if (Vector3.Angle (sortingMap.transform.forward, sortingMap.GetAreaPosition (i) - transform.position) < 90f)
				{
					if (sortingMap.mapType == SortingMapType.OrderInLayer)
					{
						sortingOrder = sortingMap.sortingAreas [i].order;
					}
					else if (sortingMap.mapType == SortingMapType.SortingLayer)
					{
						sortingLayer = sortingMap.sortingAreas [i].layer;
					}

					break;
				}
			}
		}

		if (renderer)
		{
			if (sortingMap.mapType == SortingMapType.OrderInLayer)
			{
				renderer.sortingOrder = sortingOrder;
				
				if (offsetOriginal)
				{
					renderer.sortingOrder += offset;
				}
			}
			else if (sortingMap.mapType == SortingMapType.SortingLayer)
			{
				renderer.sortingLayerName = sortingLayer;
			}
		}

		if (!affectChildren)
		{
			return;
		}

		Renderer[] renderers = GetComponentsInChildren <Renderer>();
		foreach (Renderer _renderer in renderers)
		{
			if (sortingMap.mapType == SortingMapType.OrderInLayer)
			{
				_renderer.sortingOrder = sortingOrder;
				
				if (offsetOriginal)
				{
					_renderer.sortingOrder += offset;
				}
			}
			else if (sortingMap.mapType == SortingMapType.SortingLayer)
			{
				_renderer.sortingLayerName = sortingLayer;
			}
		}
	}


	public void LockSortingOrder (int order)
	{
		lockSorting = true;
		if (renderer)
		{
			renderer.sortingOrder = order;
		}

		if (!affectChildren)
		{
			return;
		}
		
		Renderer[] renderers = GetComponentsInChildren <Renderer>();
		foreach (Renderer _renderer in renderers)
		{
			_renderer.sortingOrder = order;
		}
	}


	public void LockSortingLayer (string layer)
	{
		lockSorting = true;
		if (renderer)
		{
			renderer.sortingLayerName = layer;
		}

		if (!affectChildren)
		{
			return;
		}
		
		Renderer[] renderers = GetComponentsInChildren <Renderer>();
		foreach (Renderer _renderer in renderers)
		{
			_renderer.sortingLayerName = layer;
		}
	}


	public float GetLocalScale ()
	{
		if (followSortingMap && sortingMap != null && sortingMap.affectScale)
		{
			return (sortingMap.GetScale (transform.position) / 100f);
		}

		return 0f;
	}


	public float GetLocalSpeed ()
	{
		if (followSortingMap && sortingMap != null && sortingMap.affectScale && sortingMap.affectSpeed)
		{
			return (sortingMap.GetScale (transform.position) / 100f);
		}
		
		return 1f;
	}

}
