﻿/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Container.cs"
 * 
 *	This script is used to store a set of
 *	Inventory items in the scene, to be
 *	either taken or added to by the player.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	public class Container : MonoBehaviour
	{

		public List<ContainerItem> items;


		public void Interact ()
		{
			if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>())
			{
				GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>().activeContainer = this;
			}
		}


		public void Add (int _id, int amount)
		{
			InventoryManager inventoryManager = AdvGame.GetReferences ().inventoryManager;

			// Raise "count" by 1 for appropriate ID
			foreach (ContainerItem containerItem in items)
			{
				if (containerItem.linkedID == _id)
				{
					if (inventoryManager.CanCarryMultiple (containerItem.linkedID))
					{
						containerItem.count += amount;
					}
					return;
				}
			}

			// Not already carrying the item
			foreach (InvItem assetItem in AdvGame.GetReferences ().inventoryManager.items)
			{
				if (assetItem.id == _id)
				{
					if (!inventoryManager.CanCarryMultiple (_id))
					{
						amount = 1;
					}

					items.Add (new ContainerItem (_id, amount, GetIDArray ()));
				}
			}
		}
		
		
		public void Remove (int _id, int amount)
		{
			// Reduce "count" by 1 for appropriate ID
			
			foreach (ContainerItem item in items)
			{
				if (item.linkedID == _id)
				{
					if (item.count > 0)
					{
						item.count -= amount;
					}
					if (item.count < 1)
					{
						items.Remove (item);
					}
					return;
				}
			}
		}


		public int GetCount (int _id)
		{
			foreach (ContainerItem item in items)
			{
				if (item.linkedID == _id)
				{
					return (item.count);
				}
			}
			
			return 0;
		}


		public int[] GetIDArray ()
		{
			// Returns a list of id's in the list
			
			List<int> idArray = new List<int>();
			
			foreach (ContainerItem item in items)
			{
				idArray.Add (item.id);
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}

	}

}