/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionsManager.cs"
 * 
 *	This script handles the "Inventory" tab of the main wizard.
 *	Inventory items are defined with this.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class InventoryManager : ScriptableObject
	{

		public List<InvItem> items;
		public List<InvBin> bins;
		public InvActionList unhandledCombine;
		public InvActionList unhandledHotspot;
		public List<Recipe> recipes;

		#if UNITY_EDITOR

		private Texture2D sideIcon;
		private SettingsManager settingsManager;
		private CursorManager cursorManager;
		
		private string filter = "";
		private InvItem selectedItem;
		private Recipe selectedRecipe;
		private int sideItem = -1;
		private int invNumber = 0;
		private int binNumber = -1;

		private static GUILayoutOption
			buttonWidth = GUILayout.MaxWidth (20f);

		private static GUIContent
			deleteContent = new GUIContent("-", "Delete item");
		

		public void ShowGUI ()
		{
			if (!sideIcon)
			{
				sideIcon = (Texture2D) AssetDatabase.LoadAssetAtPath ("Assets/AdventureCreator/Graphics/Textures/inspector-use.png", typeof (Texture2D));
			}

			if (AdvGame.GetReferences ())
			{
				if (AdvGame.GetReferences ().settingsManager)
				{
					settingsManager = AdvGame.GetReferences ().settingsManager;
				}
				if (AdvGame.GetReferences ().cursorManager)
				{
					cursorManager = AdvGame.GetReferences ().cursorManager;
				}
			}

			BinsGUI ();

			if (settingsManager == null || settingsManager.interactionMethod != AC_InteractionMethod.ChooseHotspotThenInteraction || (settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && settingsManager.inventoryInteractions == InventoryInteractions.Single))
			{
				EditorGUILayout.Space ();
				EditorGUILayout.LabelField ("Unhandled events", EditorStyles.boldLabel);
				unhandledCombine = (InvActionList) EditorGUILayout.ObjectField ("Combine:", unhandledCombine, typeof (InvActionList), false);
				unhandledHotspot = (InvActionList) EditorGUILayout.ObjectField ("Use on hotspot:", unhandledHotspot, typeof (InvActionList), false);
			}

			List<string> binList = new List<string>();
			foreach (InvBin bin in bins)
			{
				binList.Add (bin.label);
			}
			
			EditorGUILayout.Space ();
			CreateItemsGUI ();
			EditorGUILayout.Space ();

			if (selectedItem != null && items.Contains (selectedItem))
			{
				EditorGUILayout.LabelField ("Inventory item '" + selectedItem.label + "' properties", EditorStyles.boldLabel);

				EditorGUILayout.BeginVertical("Button");
					selectedItem.label = EditorGUILayout.TextField ("Name:", selectedItem.label);
					selectedItem.altLabel = EditorGUILayout.TextField ("Label (if not name):", selectedItem.altLabel);

					EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField ("Category:", GUILayout.Width (146f));
						if (bins.Count > 0)
						{
							binNumber = GetBinSlot (selectedItem.binID);
							binNumber = EditorGUILayout.Popup (binNumber, binList.ToArray());
							selectedItem.binID = bins[binNumber].id;
						}
						else
						{
							selectedItem.binID = -1;
							EditorGUILayout.LabelField ("No categories defined!", EditorStyles.miniLabel, GUILayout.Width (146f));
						}
					EditorGUILayout.EndHorizontal ();	

					selectedItem.tex = (Texture2D) EditorGUILayout.ObjectField ("Texture:", selectedItem.tex, typeof (Texture2D), false);

					if (settingsManager && (settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive || settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot || (settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && settingsManager.inventoryInteractions == InventoryInteractions.Single)))
					{
						selectedItem.activeTex = (Texture2D) EditorGUILayout.ObjectField ("Active texture:", selectedItem.activeTex, typeof (Texture2D), false);
					}

					selectedItem.carryOnStart = EditorGUILayout.Toggle ("Carry on start?", selectedItem.carryOnStart);
					selectedItem.canCarryMultiple = EditorGUILayout.Toggle ("Can carry multiple?", selectedItem.canCarryMultiple);
					if (selectedItem.carryOnStart && selectedItem.canCarryMultiple)
					{
						selectedItem.count = EditorGUILayout.IntField ("Quantity on start:", selectedItem.count);
					}
					else
					{
						selectedItem.count = 1;
					}

					EditorGUILayout.Space ();
					EditorGUILayout.LabelField ("Standard interactions", EditorStyles.boldLabel);
					if (settingsManager && settingsManager.interactionMethod != AC_InteractionMethod.ContextSensitive && settingsManager.inventoryInteractions == InventoryInteractions.Multiple && AdvGame.GetReferences ().cursorManager)
					{
						CursorManager cursorManager = AdvGame.GetReferences ().cursorManager;

						List<string> iconList = new List<string>();
						foreach (CursorIcon icon in cursorManager.cursorIcons)
						{
							iconList.Add (icon.label);
						}

						if (cursorManager.cursorIcons.Count > 0)
						{
							foreach (InvInteraction interaction in selectedItem.interactions)
							{
								EditorGUILayout.BeginHorizontal ();
								invNumber = GetIconSlot (interaction.icon.id);
								invNumber = EditorGUILayout.Popup (invNumber, iconList.ToArray());
								interaction.icon = cursorManager.cursorIcons[invNumber];

								interaction.actionList = (InvActionList) EditorGUILayout.ObjectField (interaction.actionList, typeof (InvActionList), false);
								
								if (GUILayout.Button (deleteContent, EditorStyles.miniButtonRight, buttonWidth))
								{
									Undo.RecordObject (this, "Delete interaction");
									selectedItem.interactions.Remove (interaction);
									break;
								}
								EditorGUILayout.EndHorizontal ();
							}
						}
						else
						{
							EditorGUILayout.HelpBox ("No interaction icons defined - please use the Cursor Manager", MessageType.Warning);
						}
						if (GUILayout.Button ("Add interaction"))
						{
							Undo.RecordObject (this, "Add new interaction");
							selectedItem.interactions.Add (new InvInteraction (cursorManager.cursorIcons[0]));
						}
					}
					else
					{
						selectedItem.useActionList = (InvActionList) EditorGUILayout.ObjectField ("Use:", selectedItem.useActionList, typeof (InvActionList), false);
						if (cursorManager && cursorManager.allowInteractionCursorForInventory && cursorManager.cursorIcons.Count > 0)
						{
							int useCursor_int = cursorManager.GetIntFromID (selectedItem.useIconID);
							useCursor_int = EditorGUILayout.Popup ("Use cursor icon:", useCursor_int, cursorManager.GetLabelsArray (useCursor_int));
							selectedItem.useIconID = cursorManager.cursorIcons[useCursor_int].id;
						}
						else
						{
							selectedItem.useIconID = 0;
						}
						selectedItem.lookActionList = (InvActionList) EditorGUILayout.ObjectField ("Examine:", selectedItem.lookActionList, typeof (InvActionList), false);
						selectedItem.unhandledActionList = (InvActionList) EditorGUILayout.ObjectField ("Unhandled use:", selectedItem.unhandledActionList, typeof (InvActionList), false);
						selectedItem.unhandledCombineActionList = (InvActionList) EditorGUILayout.ObjectField ("Unhandled combine:", selectedItem.unhandledCombineActionList, typeof (InvActionList), false);
					}
					
					EditorGUILayout.Space ();
					EditorGUILayout.LabelField ("Combine interactions", EditorStyles.boldLabel);
					for (int i=0; i<selectedItem.combineActionList.Count; i++)
					{
						EditorGUILayout.BeginHorizontal ();
							invNumber = GetArraySlot (selectedItem.combineID[i]);
							invNumber = EditorGUILayout.Popup (invNumber, GetLabelList ());
							selectedItem.combineID[i] = items[invNumber].id;
						
							selectedItem.combineActionList[i] = (InvActionList) EditorGUILayout.ObjectField (selectedItem.combineActionList[i], typeof (InvActionList), false);
						
							if (GUILayout.Button (deleteContent, EditorStyles.miniButtonRight, buttonWidth))
							{
								Undo.RecordObject (this, "Delete combine event");
								selectedItem.combineActionList.RemoveAt (i);
								selectedItem.combineID.RemoveAt (i);
								break;
							}
						EditorGUILayout.EndHorizontal ();
					}
					if (GUILayout.Button ("Add combine event"))
					{
						Undo.RecordObject (this, "Add new combine event");
						selectedItem.combineActionList.Add (null);
						selectedItem.combineID.Add (0);
					}
					
				EditorGUILayout.EndVertical();
				EditorGUILayout.Space ();
			}

			CraftingGUI ();
			
			if (GUI.changed)
			{
				EditorUtility.SetDirty (this);
			}
		}


		private void BinsGUI ()
		{
			EditorGUILayout.LabelField ("Categories", EditorStyles.boldLabel);
			
			foreach (InvBin bin in bins)
			{
				EditorGUILayout.BeginHorizontal ();
				bin.label = EditorGUILayout.TextField (bin.label);
				
				if (GUILayout.Button (deleteContent, EditorStyles.miniButton, GUILayout.MaxWidth(20f)))
				{
					Undo.RecordObject (this, "Delete category: " + bin.label);
					bins.Remove (bin);
					break;
				}
				EditorGUILayout.EndHorizontal ();

			}
			if (GUILayout.Button ("Create new category"))
			{
				Undo.RecordObject (this, "Add category");
				List<int> idArray = new List<int>();
				foreach (InvBin bin in bins)
				{
					idArray.Add (bin.id);
				}
				idArray.Sort ();
				bins.Add (new InvBin (idArray.ToArray ()));
			}
		}


		private void ResetFilter ()
		{
			filter = "";
		}


		private void CreateItemsGUI ()
		{
			EditorGUILayout.LabelField ("Inventory items", EditorStyles.boldLabel);

			filter = EditorGUILayout.TextField ("Filter by name:", filter);
			EditorGUILayout.Space ();

			foreach (InvItem item in items)
			{
				if (filter == "" || item.label.ToLower ().Contains (filter.ToLower ()))
				{
					EditorGUILayout.BeginHorizontal ();
					
					string buttonLabel = item.label;
					if (buttonLabel == "")
					{
						buttonLabel = "(Untitled)";	
					}

					if (GUILayout.Toggle (item.isEditing, item.id + ": " + buttonLabel, "Button"))
					{
						if (selectedItem != item)
						{
							DeactivateAllItems ();
							ActivateItem (item);
						}
					}

					if (GUILayout.Button (sideIcon, GUILayout.Width (20f), GUILayout.Height (15f)))
					{
						SideMenu (item);
					}

					EditorGUILayout.EndHorizontal ();
				}
			}

			if (GUILayout.Button("Create new item"))
			{
				Undo.RecordObject (this, "Create inventory item");

				ResetFilter ();
				InvItem newItem = new InvItem (GetIDArray ());
				items.Add (newItem);
				DeactivateAllItems ();
				ActivateItem (newItem);
			}
		}


		private void ActivateItem (InvItem item)
		{
			item.isEditing = true;
			selectedItem = item;
		}
		
		
		private void DeactivateAllItems ()
		{
			foreach (InvItem item in items)
			{
				item.isEditing = false;
			}
			selectedItem = null;
		}


		private void ActivateRecipe (Recipe recipe)
		{
			recipe.isEditing = true;
			selectedRecipe = recipe;
		}
		
		
		private void DeactivateAllRecipes ()
		{
			foreach (Recipe recipe in recipes)
			{
				recipe.isEditing = false;
			}
			selectedRecipe = null;
		}


		private void SideMenu (InvItem item)
		{
			GenericMenu menu = new GenericMenu ();
			sideItem = items.IndexOf (item);
			
			menu.AddItem (new GUIContent ("Insert after"), false, Callback, "Insert after");
			if (items.Count > 0)
			{
				menu.AddItem (new GUIContent ("Delete"), false, Callback, "Delete");
			}
			if (sideItem > 0 || sideItem < items.Count-1)
			{
				menu.AddSeparator ("");
			}
			if (sideItem > 0)
			{
				menu.AddItem (new GUIContent ("Move up"), false, Callback, "Move up");
			}
			if (sideItem < items.Count-1)
			{
				menu.AddItem (new GUIContent ("Move down"), false, Callback, "Move down");
			}
			
			menu.ShowAsContext ();
		}
		
		
		private void Callback (object obj)
		{
			if (sideItem >= 0)
			{
				ResetFilter ();
				InvItem tempItem = items[sideItem];

				switch (obj.ToString ())
				{
				case "Insert after":
					Undo.RecordObject (this, "Insert item");
					items.Insert (sideItem+1, new InvItem (GetIDArray ()));
					break;
					
				case "Delete":
					Undo.RecordObject (this, "Delete item");
					DeactivateAllItems ();
					items.RemoveAt (sideItem);
					break;
					
				case "Move up":
					Undo.RecordObject (this, "Move item up");
					items.RemoveAt (sideItem);
					items.Insert (sideItem-1, tempItem);
					break;
					
				case "Move down":
					Undo.RecordObject (this, "Move item down");
					items.RemoveAt (sideItem);
					items.Insert (sideItem+1, tempItem);
					break;
				}
			}

			EditorUtility.SetDirty (this);
			AssetDatabase.SaveAssets ();
			
			sideItem = -1;
		}


		private void CraftingGUI ()
		{
			EditorGUILayout.LabelField ("Crafting", EditorStyles.boldLabel);

			foreach (Recipe recipe in recipes)
			{
				EditorGUILayout.BeginHorizontal ();
				
				string buttonLabel = recipe.label;
				if (buttonLabel == "")
				{
					buttonLabel = "(Untitled)";	
				}
				
				if (GUILayout.Toggle (recipe.isEditing, recipe.id + ": " + buttonLabel, "Button"))
				{
					if (selectedRecipe != recipe)
					{
						DeactivateAllRecipes ();
						ActivateRecipe (recipe);
					}
				}
				
				if (GUILayout.Button ("-", GUILayout.Width (20f), GUILayout.Height (15f)))
				{
					Undo.RecordObject (this, "Delete recipe");
					DeactivateAllRecipes ();
					recipes.Remove (recipe);
					AssetDatabase.SaveAssets();
					break;
				}
				
				EditorGUILayout.EndHorizontal ();
			}
			
			if (GUILayout.Button("Create new recipe"))
			{
				Undo.RecordObject (this, "Create inventory recipe");
				
				Recipe newRecipe = new Recipe (GetIDArrayRecipe ());
				recipes.Add (newRecipe);
				DeactivateAllRecipes ();
				ActivateRecipe (newRecipe);
			}

			if (selectedRecipe != null && recipes.Contains (selectedRecipe))
			{
				EditorGUILayout.Space ();
				EditorGUILayout.LabelField ("Recipe '" + selectedRecipe.label + "' properties", EditorStyles.boldLabel);
				
				EditorGUILayout.BeginVertical("Button");
				selectedRecipe.label = EditorGUILayout.TextField ("Name:", selectedRecipe.label);

				EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Resulting item:", GUILayout.Width (146f));
					int i = GetArraySlot (selectedRecipe.resultID);
					i = EditorGUILayout.Popup (i, GetLabelList ());
					selectedRecipe.resultID = items[i].id;
				EditorGUILayout.EndHorizontal ();

				selectedRecipe.autoCreate = EditorGUILayout.Toggle ("Result is automatic?", selectedRecipe.autoCreate);
				selectedRecipe.useSpecificSlots = EditorGUILayout.Toggle ("Requires specific pattern?", selectedRecipe.useSpecificSlots);

				selectedRecipe.onCreateRecipe = (OnCreateRecipe) EditorGUILayout.EnumPopup ("When click on result:", selectedRecipe.onCreateRecipe);
				if (selectedRecipe.onCreateRecipe == OnCreateRecipe.RunActionList)
				{
					selectedRecipe.invActionList = (InvActionList) EditorGUILayout.ObjectField ("ActionList to run:", selectedRecipe.invActionList, typeof (InvActionList), false);
				}

				EditorGUILayout.Space ();
				EditorGUILayout.LabelField ("Ingredients", EditorStyles.boldLabel);

				foreach (Ingredient ingredient in selectedRecipe.ingredients)
				{
					EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField ("Ingredient:", GUILayout.Width (70f));
						i = GetArraySlot (ingredient.itemID);
						i = EditorGUILayout.Popup (i, GetLabelList ());
						ingredient.itemID = items[i].id;

						if (items[i].canCarryMultiple)
						{
							EditorGUILayout.LabelField ("Amount:", GUILayout.Width (50f));
							ingredient.amount = EditorGUILayout.IntField (ingredient.amount, GUILayout.Width (30f));
						}
						
						if (selectedRecipe.useSpecificSlots)
						{
							EditorGUILayout.LabelField ("Slot:", GUILayout.Width (30f));
							ingredient.slotNumber = EditorGUILayout.IntField (ingredient.slotNumber, GUILayout.Width (30f));
						}

						if (GUILayout.Button ("-", GUILayout.Width (20f), GUILayout.Height (15f)))
						{
							Undo.RecordObject (this, "Delete ingredient");
							selectedRecipe.ingredients.Remove (ingredient);
							AssetDatabase.SaveAssets();
							break;
						}

					EditorGUILayout.EndHorizontal ();
				}

				if (GUILayout.Button("Add new ingredient"))
				{
					Undo.RecordObject (this, "Add recipe ingredient");
					
					Ingredient newIngredient = new Ingredient ();
					selectedRecipe.ingredients.Add (newIngredient);
				}


				EditorGUILayout.EndVertical ();
			}
		}
		
		
		private int[] GetIDArray ()
		{
			List<int> idArray = new List<int>();
			foreach (InvItem item in items)
			{
				idArray.Add (item.id);
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}


		private int[] GetIDArrayRecipe ()
		{
			List<int> idArray = new List<int>();
			foreach (Recipe recipe in recipes)
			{
				idArray.Add (recipe.id);
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}


		private int GetIconSlot (int _id)
		{
			int i = 0;
			foreach (CursorIcon icon in AdvGame.GetReferences ().cursorManager.cursorIcons)
			{
				if (icon.id == _id)
				{
					return i;
				}
				i++;
			}

			return 0;
		}

		
		private int GetArraySlot (int _id)
		{
			int i = 0;
			foreach (InvItem item in items)
			{
				if (item.id == _id)
				{
					return i;
				}
				i++;
			}
			
			return 0;
		}


		private string[] GetLabelList ()
		{
			List<string> labelList = new List<string>();
			foreach (InvItem _item in items)
			{
				labelList.Add (_item.label);
			}
			return labelList.ToArray ();
		}


		private int GetBinSlot (int _id)
		{
			int i = 0;
			foreach (InvBin bin in bins)
			{
				if (bin.id == _id)
				{
					return i;
				}
				i++;
			}
			
			return 0;
		}

		#endif


		public string GetLabel (int _id)
		{
			string result = "";
			foreach (InvItem item in items)
			{
				if (item.id == _id)
				{
					result = item.label;
				}
			}
			
			return result;
		}
		
		
		public InvItem GetItem (int _id)
		{
			foreach (InvItem item in items)
			{
				if (item.id == _id)
				{
					return item;
				}
			}
			return null;
		}
		
		
		public bool CanCarryMultiple (int _id)
		{
			foreach (InvItem item in items)
			{
				if (item.id == _id)
				{
					return item.canCarryMultiple;
				}
			}
			
			return false;
		}

	}

}