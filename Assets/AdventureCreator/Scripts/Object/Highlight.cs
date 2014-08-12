/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Highlight.cs"
 * 
 *	This script is attached to any gameObject that glows
 *	when a cursor is placed over it's associated interaction
 *	object.  These are not always the same object.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	public class Highlight : MonoBehaviour
	{

		private float maxHighlight = 2f;
		private float highlight = 1f;
		private bool doHightlight = false;
		private int direction = 1;
		private float fadeStartTime;
		private float fadeTime = 0.3f;
		private bool isFlashing = false;
		
		private List<Color> originalColors = new List<Color>();
		
		
		private void Awake ()
		{
			// Go through own materials
			if (renderer)
			{
				foreach (Material material in renderer.materials)
				{
					if (material.HasProperty ("_Color"))
					{
						originalColors.Add (material.color);
					}
				}
			}
			
			// Go through any child materials
			Component[] children;
			children = GetComponentsInChildren <Renderer>();
			foreach (Renderer childRenderer in children)
			{
				foreach (Material material in childRenderer.materials)
				{
					if (material.HasProperty ("_Color"))
					{
						originalColors.Add (material.color);
					}
				}
			}

			if (guiTexture)
			{
				originalColors.Add (guiTexture.color);
			}
		}
		
		
		private void FixedUpdate ()
		{
			if (doHightlight)
			{	
				if (direction == 1)
				{
					// Add highlight
					highlight = Mathf.Lerp (1f, maxHighlight, AdvGame.Interpolate (fadeStartTime, fadeTime, MoveMethod.Linear));

					if (highlight >= maxHighlight)
					{
						highlight = maxHighlight;
						
						if (isFlashing)
						{
							direction = -1;
							fadeStartTime = Time.time;
						}
						else
						{
							doHightlight = false;
						}
					}
				}
				else
				{
					// Remove highlight
					highlight = Mathf.Lerp (maxHighlight, 1f, AdvGame.Interpolate (fadeStartTime, fadeTime, AC.MoveMethod.Linear));

					if (highlight <= 1f)
					{
						highlight = 1f;
						doHightlight = false;
						isFlashing = false;
					}
				}

				int i = 0;
				float alpha;

				// Go through own materials
				if (renderer)
				{
					foreach (Material material in renderer.materials)
					{
						if (material.HasProperty ("_Color"))
						{
							alpha = material.color.a;
							Color newColor = originalColors[i] * highlight;
							newColor.a = alpha;
							material.color = newColor;
							i++;
						}
					}
				}
				
				// Go through any child materials
				Component[] children;
				children = GetComponentsInChildren <Renderer>();
				foreach (Renderer childRenderer in children)
				{
					foreach (Material material in childRenderer.materials)
					{
						if (originalColors.Count <= i)
						{
							break;
						}

						if (material.HasProperty ("_Color"))
						{
							alpha = material.color.a;
							Color newColor = originalColors[i] * highlight;
							newColor.a = alpha;
							material.color = newColor;
							i++;
						}
					}
				}

				if (guiTexture)
				{
					alpha = Mathf.Lerp(0.2f, 1f, highlight - 1f); // highlight is between 1 and 2
					Color newColor = originalColors[i];
					newColor.a = alpha;
					guiTexture.color = newColor;
				}
			}
		}
		
		
		public void HighlightOn ()
		{
			doHightlight = true;
			isFlashing = false;
			direction = 1;
			fadeStartTime = Time.time;
			
			if (highlight > 1f)
			{
				fadeStartTime -= (highlight - 1f) / (maxHighlight - 1f) * fadeTime;
			}
			else
			{
				highlight = 1f;
			}
		}
		
		
		public void HighlightOff ()
		{
			doHightlight = true;
			isFlashing = false;
			direction = -1;
			fadeStartTime = Time.time;
			
			if (highlight < maxHighlight)
			{
				fadeStartTime -= (maxHighlight - highlight) / (maxHighlight - 1) * fadeTime;
			}
			else
			{
				highlight = maxHighlight;
			}
		}
		
		
		public void Flash ()
		{
			if (!isFlashing && (!doHightlight || (doHightlight && direction == -1)))
			{
				doHightlight = true;
				isFlashing = true;
				highlight = 1f;
				direction = 1;
				fadeStartTime = Time.time;
			}
		}


		public float GetHighlightAlpha ()
		{
			return (highlight - 1f);
		}
		
		
		public void HighlightOffInstant ()
		{
			doHightlight = false;
			isFlashing = false;
			
			// Go through any child materials
			int i=0;
			Component[] children;
			children = GetComponentsInChildren <Renderer>();
			foreach (Renderer childRenderer in children)
			{
				foreach (Material material in childRenderer.materials)
				{
					if (material.HasProperty ("_Color"))
					{
						Color newColor = originalColors[i];
						material.color = newColor;
						i++;
					}
				}
			}

			if (guiTexture)
			{
				Color newColor = originalColors[i];
				guiTexture.color = newColor;
			}
		}
		
	}

}