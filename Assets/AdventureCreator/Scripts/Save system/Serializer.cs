/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Serializer.cs"
 * 
 *	This script serializes saved game data and performs the file handling.
 * 
 * 	It is partially based on Zumwalt's code here:
 * 	http://wiki.unity3d.com/index.php?title=Save_and_Load_from_XML
 *  and uses functions by Nitin Pande:
 *  http://www.eggheadcafe.com/articles/system.xml.xmlserialization.asp 
 * 
 */

using AC;
using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml; 
using System.Xml.Serialization;


public class Serializer : MonoBehaviour
{
	
	public static T returnComponent <T> (int constantID) where T : MonoBehaviour
	{
		T result = null;
		
		if (constantID != 0)
		{
			T[] components = FindObjectsOfType (typeof(T)) as T[];
			
			foreach (T component in components)
			{
				if (component.GetComponent <ConstantID>())
				{
					if (component.GetComponent <ConstantID>().constantID == constantID)
					{
						// Found it
						result = component;
						break;
					}
				}
			}
		}
		
		return result;
	}
	
	
	public static int GetConstantID (GameObject _gameObject)
	{
		if (_gameObject.GetComponent <ConstantID>())
		{
			if (_gameObject.GetComponent <ConstantID>().constantID != 0)
			{
				return (_gameObject.GetComponent <ConstantID>().constantID);
			}
			else
			{	
				Debug.LogWarning ("GameObject " + _gameObject.name + " was not saved because it does not have an ID.");
			}
		}
		else
		{
			Debug.LogWarning ("GameObject " + _gameObject.name + " was not saved because it does not have a constant ID script!");
		}
		
		return 0;
	}
	
	
	public static string SerializeObjectBinary (object pObject)
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		MemoryStream memoryStream = new MemoryStream ();
		binaryFormatter.Serialize (memoryStream, pObject);
		return (Convert.ToBase64String (memoryStream.GetBuffer ()));
	}
	
	
	public static T DeserializeObjectBinary <T> (string pString)
	{
		if (pString.Contains ("<?xml"))
		{
			// Fix converted Options Data
			PlayerPrefs.DeleteKey ("Options");
			return default (T);
		}
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		MemoryStream memoryStream = new MemoryStream (Convert.FromBase64String (pString));
		return (T) binaryFormatter.Deserialize (memoryStream);
	}
	
	
	public static string SerializeObjectXML <T> (object pObject) 
	{ 
		string XmlizedString = null; 
		
		MemoryStream memoryStream = new MemoryStream(); 
		XmlSerializer xs = new XmlSerializer (typeof (T)); 
		XmlTextWriter xmlTextWriter = new XmlTextWriter (memoryStream, Encoding.UTF8); 
		
		xs.Serialize (xmlTextWriter, pObject); 
		memoryStream = (MemoryStream) xmlTextWriter.BaseStream; 
		XmlizedString = UTF8ByteArrayToString (memoryStream.ToArray());
		
		return XmlizedString;
	}
	
	
	public static object DeserializeObjectXML <T> (string pXmlizedString) 
	{ 
		XmlSerializer xs = new XmlSerializer (typeof (T)); 
		MemoryStream memoryStream = new MemoryStream (StringToUTF8ByteArray (pXmlizedString)); 
		return xs.Deserialize(memoryStream); 
	}
	
	
	private static string UTF8ByteArrayToString (byte[] characters) 
	{		
		UTF8Encoding encoding = new UTF8Encoding(); 
		string constructedString = encoding.GetString (characters); 
		return (constructedString); 
	}
	
	
	private static byte[] StringToUTF8ByteArray (string pXmlString) 
	{ 
		UTF8Encoding encoding = new UTF8Encoding(); 
		byte[] byteArray = encoding.GetBytes (pXmlString); 
		return byteArray; 
	}
	
	
	public static List<SingleLevelData> DeserializeRoom (string pString)
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		MemoryStream memoryStream = new MemoryStream (Convert.FromBase64String (pString));
		return (List<SingleLevelData>) binaryFormatter.Deserialize (memoryStream);
	}
	
	
	public static void CreateSaveFile (string fullFileName, string _data)
	{
		#if UNITY_WEBPLAYER
		
		PlayerPrefs.SetString (fullFileName, _data);
		Debug.Log ("PlayerPrefs key written: " + fullFileName);
		
		#else
		
		StreamWriter writer;
		
		FileInfo t = new FileInfo (fullFileName);
		
		if (!t.Exists)
		{
			writer = t.CreateText ();
		}
		
		else
		{
			t.Delete ();
			writer = t.CreateText ();
		}
		
		writer.Write (_data);
		writer.Close ();
		
		Debug.Log ("File written: " + fullFileName);
		#endif
	}
	
	
	public static string LoadSaveFile (string fullFileName)
	{
		string _data = "";
		
		#if UNITY_WEBPLAYER
		
		_data = PlayerPrefs.GetString(fullFileName, "");
		
		#else
		
		StreamReader r = File.OpenText (fullFileName);
		
		string _info = r.ReadToEnd ();
		r.Close ();
		_data = _info;
		
		#endif
		
		Debug.Log ("File Read: " + fullFileName);
		return (_data);
	}
	
	
	public static Paths RestorePathData (Paths path, string pathData)
	{
		if (pathData == null)
		{
			return null;
		}
		
		path.affectY = true;
		path.pathType = AC_PathType.ForwardOnly;
		path.nodePause = 0;
		path.nodes = new List<Vector3>();
		
		if (pathData.Length > 0)
		{
			string[] nodesArray = pathData.Split ("|"[0]);
			
			foreach (string chunk in nodesArray)
			{
				string[] chunkData = chunk.Split (":"[0]);
				
				float _x = 0;
				float.TryParse (chunkData[0], out _x);
				
				float _y = 0;
				float.TryParse (chunkData[1], out _y);
				
				float _z = 0;
				float.TryParse (chunkData[2], out _z);
				
				path.nodes.Add (new Vector3 (_x, _y, _z));
			}
		}
		
		return path;
	}
	
	
	public static string CreatePathData (Paths path)
	{
		System.Text.StringBuilder pathString = new System.Text.StringBuilder ();
		
		foreach (Vector3 node in path.nodes)
		{
			pathString.Append (node.x.ToString ());
			pathString.Append (":");
			pathString.Append (node.y.ToString ());
			pathString.Append (":");
			pathString.Append (node.z.ToString ());
			pathString.Append ("|");
		}
		
		if (path.nodes.Count > 0)
		{
			pathString.Remove (pathString.Length-1, 1);
		}
		
		return pathString.ToString ();
	}
	
}
