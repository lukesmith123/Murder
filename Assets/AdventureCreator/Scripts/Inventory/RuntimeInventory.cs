/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RuntimeInventory.cs"
 * 
 *	This script creates a local copy of the InventoryManager's items.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	public class RuntimeInventory : MonoBehaviour
	{

		[HideInInspector] public List<InvItem> localItems;
		[HideInInspector] public InvActionList unhandledCombine;
		[HideInInspector] public InvActionList unhandledHotspot;
		
		[HideInInspector] public bool isLocked = true;
		[HideInInspector] public InvItem selectedItem = null;
		
		private InventoryManager inventoryManager;
		private RuntimeActionList runtimeActionList;

		
		public void Awake ()
		{
			selectedItem = null;
			GetReferences ();

			localItems.Clear ();
			GetItemsOnStart ();
			
			if (inventoryManager)
			{
				unhandledCombine = inventoryManager.unhandledCombine;
				unhandledHotspot = inventoryManager.unhandledHotspot;
			}
			else
			{
				Debug.LogError ("An Inventory Manager is required - please use the Adventure Creator window to create one.");
			}
			
		}
		
		
		private void GetReferences ()
		{
			if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <RuntimeActionList>())
			{
				runtimeActionList = GameObject.FindWithTag (Tags.gameEngine).GetComponent <RuntimeActionList>();
			}
			
			if (AdvGame.GetReferences () && AdvGame.GetReferences ().inventoryManager)
			{
				inventoryManager = AdvGame.GetReferences ().inventoryManager;
			}
		}

		
		public void SetNull ()
		{
			selectedItem = null;
			PlayerMenus.ResetInventoryBoxes ();
		}
		
		
		public void SelectItemByID (int _id)
		{
			foreach (InvItem item in localItems)
			{
				if (item.id == _id)
				{
					selectedItem = item;
					PlayerMenus.ResetInventoryBoxes ();
					return;
				}
			}
			
			SetNull ();
			GetReferences ();
			Debug.LogWarning ("Want to select inventory item " + inventoryManager.GetLabel (_id) + " but player is not carrying it.");
		}
		
		
		public void SelectItem (InvItem item)
		{
			if (selectedItem == item)
			{
				selectedItem = null;
				GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerCursor>().ResetSelectedCursor ();
			}
			else
			{
				selectedItem = item;
			}

			PlayerMenus.ResetInventoryBoxes ();
		}
		
		
		private void GetItemsOnStart ()
		{
			if (inventoryManager)
			{
				foreach (InvItem item in inventoryManager.items)
				{
					if (item.carryOnStart)
					{
						if (!item.canCarryMultiple)
						{
							item.count = 1;
						}
						item.recipeSlot = -1;
						isLocked = false;
						localItems.Add (item);
					}
				}
			}
			else
			{
				Debug.LogError ("No Inventory Manager found - please use the Adventure Creator window to create one.");
			}
		}
		
		
		public void Add (int _id, int amount)
		{
			// Raise "count" by 1 for appropriate ID
			
			if (localItems.Count == 0)
			{
				isLocked = false;
			}
			
			foreach (InvItem item in localItems)
			{
				if (item.id == _id)
				{
					if (item.canCarryMultiple)
					{
						item.count += amount;
					}

					return;
				}
			}
			
			GetReferences ();

			if (inventoryManager)
			{
				// Not already carrying the item
				foreach (InvItem assetItem in inventoryManager.items)
				{
					if (assetItem.id == _id)
					{
						InvItem newItem = assetItem;
						
						if (!newItem.canCarryMultiple)
						{
							amount = 1;
						}

						newItem.recipeSlot = -1;
						newItem.count = amount;
						localItems.Add (newItem);
					}
				}
			}
		
		}
		
		
		public void Remove (int _id, int amount)
		{
			// Reduce "count" by 1 for appropriate ID

			foreach (InvItem item in localItems)
			{
				if (item.id == _id)
				{
					if (!item.canCarryMultiple)
					{
						localItems.Remove (item);
					}
					else
					{
						if (item.count > 0)
						{
							item.count -= amount;
						}
						if (item.count < 1)
						{
							localItems.Remove (item);
						}
					}					
					if (localItems.Count == 0)
					{
						isLocked = true;
					}
					
					break;
				}
			}
		}
		
		
		public string GetLabel (InvItem item)
		{
			if (Options.GetLanguage () > 0)
			{
				return (SpeechManager.GetTranslation (item.lineID, Options.GetLanguage ()));
			}
			else if (item.altLabel != "")
			{
				return (item.altLabel);
			}

			return (item.label);
		}
		
		
		public int GetCount (int _id)
		{
			foreach (InvItem item in localItems)
			{
				if (item.id == _id)
				{
					return (item.count);
				}
			}
			
			return 0;
		}


		public InvItem GetItem (int _id)
		{
			foreach (InvItem item in localItems)
			{
				if (item.id == _id)
				{
					return item;
				}
			}

			return null;
		}


		public void Look (InvItem item)
		{
			GetReferences ();
			
			if (runtimeActionList && item.lookActionList)
			{
				runtimeActionList.Play (item.lookActionList);
			}
		}
		
		
		public void Use (InvItem item)
		{
			GetReferences ();
			
			if (runtimeActionList)
			{
				if (item.useActionList)
				{
					selectedItem = null;
					runtimeActionList.Play (item.useActionList);
				}
				else
				{
					SelectItem (item);
				}
			}
		}


		public void RunInteraction (InvItem invItem, int iconID)
		{
			GetReferences ();
			
			if (runtimeActionList)
			{
				foreach (InvInteraction interaction in invItem.interactions)
				{
					if (interaction.icon.id == iconID)
					{
						if (interaction.actionList)
						{
							runtimeActionList.Play (interaction.actionList);
						}
						break;
					}
				}
			}
		}


		public void RunInteraction (int iconID)
		{
			GetReferences ();

			if (runtimeActionList)
			{
				foreach (InvInteraction interaction in selectedItem.interactions)
				{
					if (interaction.icon.id == iconID)
					{
						if (interaction.actionList)
						{
							runtimeActionList.Play (interaction.actionList);
						}
						break;
					}
				}
			}

			selectedItem = null;
		}


		public void ShowInteractions (InvItem item)
		{
			selectedItem = item;

			PlayerMenus playerMenus = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>();
			playerMenus.SetInteractionMenus (true);
		}
		
		
		public void Combine (InvItem item)
		{
			GetReferences ();
			
			if (item == selectedItem)
			{
				selectedItem = null;

				SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
				if ((settingsManager.interactionMethod != AC_InteractionMethod.ChooseHotspotThenInteraction || settingsManager.inventoryInteractions == InventoryInteractions.Single) && settingsManager.inventoryDragDrop && settingsManager.inventoryDropLook)
				{
					Look (item);
				}
			}
			else if (runtimeActionList)
			{
				for (int i=0; i<item.combineID.Count; i++)
				{
					if (item.combineID[i] == selectedItem.id && item.combineActionList[i])
					{
						PlayerMenus.ForceOffAllMenus (true);
						selectedItem = null;
						runtimeActionList.Play (item.combineActionList [i]);
						return;
					}
				}
				
				// Found no combine match
				if (selectedItem.unhandledCombineActionList)
				{
					InvActionList unhandledActionList = selectedItem.unhandledCombineActionList;
					selectedItem = null;
					runtimeActionList.Play (unhandledActionList);	
				}
				else if (unhandledCombine)
				{
					selectedItem = null;
					PlayerMenus.ForceOffAllMenus (true);
					runtimeActionList.Play (unhandledCombine);
				}
				else
				{
					selectedItem = null;
				}
			}

			GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerCursor>().ResetSelectedCursor ();
		}


		public List<InvItem> GetSelected ()
		{
			List<InvItem> items = new List<InvItem>();

			if (selectedItem != null)
			{
				items.Add (selectedItem);
			}

			return items;
		}


		public bool IsItemCarried (InvItem _item)
		{
			foreach (InvItem item in localItems)
			{
				if (item == _item)
				{
					return true;
				}
			}

			return false;
		}


		public void RemoveRecipes ()
		{
			foreach (InvItem item in localItems)
			{
				item.recipeSlot = -1;
			}
			PlayerMenus.ResetInventoryBoxes ();
		}


		public List<InvItem> MatchInteractions ()
		{
			List<InvItem> items = new List<InvItem>();

			if (GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInteraction>())
			{
				PlayerInteraction playerInteraction = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInteraction>();
				
				if (playerInteraction.hotspot)
				{
					foreach (Button button in playerInteraction.hotspot.invButtons)
					{
						foreach (InvItem item in localItems)
						{
							if (item.id == button.invID && !button.isDisabled)
							{
								items.Add (item);
								break;
							}
						}
					}
				}

				else if (selectedItem != null)
				{
					foreach (int combineID in selectedItem.combineID)
					{
						foreach (InvItem item in localItems)
						{
							if (item.id == combineID)
							{
								items.Add (item);
								break;
							}
						}
					}
				}
			}

			return items;
		}


		public Recipe CalculateRecipe (bool autoCreateMatch)
		{
			if (inventoryManager == null)
			{
				return null;
			}

			foreach (Recipe recipe in inventoryManager.recipes)
			{
				if (autoCreateMatch != recipe.autoCreate)
				{
					return null;
				}

				// Are any invalid ingredients present?
				foreach (InvItem item in localItems)
				{
					if (item.recipeSlot >= 0)
					{
						bool found = false;
						foreach (Ingredient ingredient in recipe.ingredients)
						{
							if (ingredient.itemID == item.id)
							{
								found = true;
							}
						}
						if (!found)
						{
							// Not present in recipe
							return null;
						}
					}
				}

				bool canCreateRecipe = true;
				while (canCreateRecipe)
				{
					foreach (Ingredient ingredient in recipe.ingredients)
					{
						// Is ingredient present (and optionally, in correct slot)
						InvItem ingredientItem = GetItem (ingredient.itemID);
						if (ingredientItem == null)
						{
							canCreateRecipe = false;
							break;
						}

						if (ingredientItem.recipeSlot >= 0)
						{
							if ((recipe.useSpecificSlots && ingredientItem.recipeSlot == (ingredient.slotNumber -1)) || !recipe.useSpecificSlots)
							{
								if ((ingredientItem.canCarryMultiple && ingredientItem.count >= ingredient.amount) || !ingredientItem.canCarryMultiple)
								{
									if (canCreateRecipe && recipe.ingredients.IndexOf (ingredient) == (recipe.ingredients.Count -1))
									{
										return recipe;
									}
								}
								else canCreateRecipe = false;
							}
							else canCreateRecipe = false;
						}
						else canCreateRecipe = false;
					}
				}
			}

			return null;
		}


		public InvItem PerformCrafting (Recipe recipe)
		{
			foreach (Ingredient ingredient in recipe.ingredients)
			{
				Remove (ingredient.itemID, ingredient.amount);
			}

			Add (recipe.resultID, 1);

			foreach (InvItem item in localItems)
			{
				if (item.id == recipe.resultID)
				{
					return item;
				}
			}

			return null;
		}
		
		
		private void OnEnable ()
		{
			runtimeActionList = null;
			inventoryManager = null;
		}

	}

}