/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"GVar.cs"
 * 
 *	This script is a data class for project-wide variables.
 * 
 */

using UnityEngine;
using AC;

[System.Serializable]
public class GVar
{
	
	public string label;
	public int val;				// For bools, 0 = false, 1 = true
	public float floatVal;
	public string textVal;
	public VariableType type;
	public int id;				// Internal ID to allow order-independence
	public bool isEditing = false;

	public VarLink link = VarLink.None;
	public string pmVar;
	public bool updateLinkOnStart = false;
	
	
	public GVar ()
	{
		val = 0;
		floatVal = 0f;
		textVal = "";
		type = VariableType.Boolean;
		id = 0;
		link = VarLink.None;
		pmVar = "";
		updateLinkOnStart = false;

		label = "Variable " + (id + 1).ToString ();
	}
	
	
	public GVar (int[] idArray)
	{
		val = 0;
		floatVal = 0f;
		textVal = "";
		type = VariableType.Boolean;
		id = 0;
		link = VarLink.None;
		pmVar = "";
		updateLinkOnStart = false;

		// Update id based on array
		foreach (int _id in idArray)
		{
			if (id == _id)
			{
				id ++;
			}
		}
		
		label = "Variable " + (id + 1).ToString ();
	}
	
	
	public GVar (GVar assetVar)
	{
		// Duplicates Asset to Runtime instance
		// (Do it the long way to ensure no connections remain to the asset file)
		
		val = assetVar.val;
		floatVal = assetVar.floatVal;
		textVal = assetVar.textVal;
		type = assetVar.type;
		id = assetVar.id;
		label = assetVar.label;
		link = assetVar.link;
		pmVar = assetVar.pmVar;
		updateLinkOnStart = assetVar.updateLinkOnStart;
	}


	public void Download ()
	{
		if (link == VarLink.PlaymakerGlobalVariable && pmVar != "")
		{
			if (!PlayMakerIntegration.IsDefinePresent ())
			{
				return;
			}

			if (type == VariableType.Integer)
			{
				val = PlayMakerIntegration.GetGlobalInt (pmVar);
			}
			else if (type == VariableType.Boolean)
			{
				bool _val = PlayMakerIntegration.GetGlobalBool (pmVar);
				if (_val)
				{
					val = 1;
				}
				else
				{
					val = 0;
				}
			}
			else if (type == VariableType.String)
			{
				textVal = PlayMakerIntegration.GetGlobalString (pmVar);
			}
			else if (type == VariableType.Float)
			{
				floatVal = PlayMakerIntegration.GetGlobalFloat (pmVar);
			}
		}
	}


	public void Upload ()
	{
		if (link == VarLink.PlaymakerGlobalVariable && pmVar != "")
		{
			if (!PlayMakerIntegration.IsDefinePresent ())
			{
				return;
			}
		
			if (type == VariableType.Integer)
			{
				PlayMakerIntegration.SetGlobalInt (pmVar, val);
			}
			else if (type == VariableType.Boolean)
			{
				if (val == 1)
				{
					PlayMakerIntegration.SetGlobalBool (pmVar, true);
				}
				else
				{
					PlayMakerIntegration.SetGlobalBool (pmVar, false);
				}
			}
			else if (type == VariableType.String)
			{
				PlayMakerIntegration.SetGlobalString (pmVar, textVal);
			}
			else if (type == VariableType.Float)
			{
				PlayMakerIntegration.SetGlobalFloat (pmVar, floatVal);
			}
		}
		else if (link == VarLink.OptionsData)
		{
			GameObject.FindWithTag (Tags.persistentEngine).GetComponent <Options>().SavePrefs ();
		}
	}


	
	public void SetValue (string newValue)
	{
		textVal = newValue;
	}
	
	
	public void SetValue (float newValue, SetVarMethod setVarMethod)
	{
		if (setVarMethod == SetVarMethod.IncreaseByValue)
		{
			floatVal += newValue;
		}
		else if (setVarMethod == SetVarMethod.SetAsRandom)
		{
			floatVal = Random.Range (0f, newValue);
		}
		else
		{
			floatVal = newValue;
		}
	}
	
	
	public void SetValue (int newValue, SetVarMethod setVarMethod)
	{
		if (setVarMethod == SetVarMethod.IncreaseByValue)
		{
			val += newValue;
		}
		else if (setVarMethod == SetVarMethod.SetAsRandom)
		{
			val = Random.Range (0, newValue);
		}
		else
		{
			val = newValue;
		}
		
		if (type == VariableType.Boolean)
		{
			if (val > 0)
			{
				val = 1;
			}
			else
			{
				val = 0;
			}
		}
	}


	
	public string GetValue ()
	{
		if (type == VariableType.Integer)
		{
			return val.ToString ();
		}
		else if (type == VariableType.String)
		{
			return textVal;
		}
		else if (type == VariableType.Float)
		{
			return floatVal.ToString ();
		}
		else
		{
			if (val == 0)
			{
				return "False";
			}
			else
			{
				return "True";
			}
		}
	}
	
}