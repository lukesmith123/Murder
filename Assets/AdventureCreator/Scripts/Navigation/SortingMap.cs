﻿/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"SortingMap.cs"
 * 
 *	This script is used to change the sorting order of
 *	2D Character sprites based on their Z-position.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AC;

public class SortingMap : MonoBehaviour
{

	public SortingMapType mapType = SortingMapType.OrderInLayer;
	public List <SortingArea> sortingAreas = new List<SortingArea>();
	public bool affectScale = false;
	public bool affectSpeed = true;
	public int originScale = 100;


	private void OnDrawGizmos ()
	{
		for (int i=0; i<sortingAreas.Count; i++)
		{
			Gizmos.DrawIcon (GetAreaPosition (i), "", true);

			Gizmos.color = sortingAreas [i].color;
			if (i == 0)
			{
				Gizmos.DrawLine (transform.position, GetAreaPosition (i));
			}
			else
			{
				Gizmos.DrawLine (GetAreaPosition (i-1), GetAreaPosition (i));
			}
		}
	}


	public Vector3 GetAreaPosition (int i)
	{
		return (transform.position + (transform.forward * sortingAreas [i].z));
	}


	public float GetScale (Vector3 followPosition)
	{
		if (!affectScale)
		{
			return 1f;
		}

		if (sortingAreas.Count == 0)
		{
			return (float) originScale;
		}

		// Behind first?
		if (Vector3.Angle (transform.forward, transform.position - followPosition) < 90f)
		{
			return (float) originScale;
		}

		// In front of last?
		if (Vector3.Angle (transform.forward, GetAreaPosition (sortingAreas.Count-1) - followPosition) > 90f)
		{
			return (float) sortingAreas [sortingAreas.Count-1].scale;
		}

		// In between two?
		for (int i=0; i<sortingAreas.Count; i++)
		{
			float angle = Vector3.Angle (transform.forward, GetAreaPosition (i) - followPosition);
			if (angle < 90f)
			{
				float prevZ = 0;
				if (i > 0)
				{
					prevZ = sortingAreas [i-1].z;
				}

				float proportionAlong = 1 - Vector3.Distance (GetAreaPosition (i), followPosition) / (sortingAreas [i].z - prevZ) * Mathf.Cos (Mathf.Deg2Rad * angle);
				float previousScale = (float) originScale;
				if (i > 0)
				{
					previousScale = sortingAreas [i-1].scale;
				}

				return (previousScale + proportionAlong * ((float) sortingAreas [i].scale - previousScale));
			}
		}

		return 1f;
	}


	public void SetInBetweenScales ()
	{
		if (sortingAreas.Count < 2)
		{
			return;
		}

		float finalScale = sortingAreas [sortingAreas.Count-1].scale;
		float finalZ = sortingAreas [sortingAreas.Count-1].z;

		for (int i=0; i<sortingAreas.Count-1; i++)
		{
			float newScale = ((sortingAreas [i].z / finalZ) * ((float) finalScale - (float) originScale)) + (float) originScale;
			sortingAreas [i].scale = (int) newScale;
		}
	}
	
}