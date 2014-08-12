/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"InvActionList.cs"
 * 
 *	This script stores a list of Actions in an asset file.
 *	It is used to handle inventory actions that are irregardless of scene.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class InvActionList : ScriptableObject
{
	public List<AC.Action> actions = new List<AC.Action>();
}
