/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RuntimeActionList.cs"
 * 
 *	This is a special derivative of ActionList, attached to the GameEngine.
 *	It is used to run InvActionLists and MenuActionLists, which are assets defined outside of the scene.
 *	This type of asset's actions are copied here and run locally.
 *	When a MenuActionList is copied, the menu it is called from is recorded, so that the game returns
 *	to the appropriate state after running.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class RuntimeActionList : ActionList
	{
		
		[HideInInspector] public bool pauseAfterEnd = false;


		public void Play (InvActionList invActionList)
		{
			if (invActionList.actions.Count > 0)
			{
				pauseAfterEnd = false;
				
				actions.Clear ();
				
				foreach (AC.Action action in invActionList.actions)
				{
					actions.Add (action);
				}

				Interact ();
			}
		}
		
		
		public void Play (InvActionList menuActionList, Menu menuRunFrom)
		{
			if (menuActionList.actions.Count > 0)
			{
				if (menuRunFrom.appearType == AppearType.Manual || menuRunFrom.appearType == AppearType.OnInputKey)
				{
					pauseAfterEnd = menuRunFrom.pauseWhenEnabled;
					
					if (pauseAfterEnd)
					{
						stateHandler.BackupLastGameplayState ();
					}
				}
				
				actions.Clear ();
				
				foreach (AC.Action action in menuActionList.actions)
				{
					actions.Add (action);
				}
				
				Interact ();
			}
		}


		public bool IsRunning ()
		{
			if (nextActionNumber > -1)
			{
				return true;
			}
			
			return false;
		}
	
	}

}
