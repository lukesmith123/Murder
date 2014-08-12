﻿/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Parallax2D.cs"
 * 
 *	Attach this script to a GameObject when making a 2D game,
 *	to make it scroll as the camera moves.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class Parallax2D : MonoBehaviour
	{

		public GameCamera2D camera2D;
		public float depth;
		public bool xScroll;
		public bool yScroll;
		public float xOffset;
		public float yOffset;

		private float xStart;
		private float yStart;
		private float xDesired;
		private float yDesired;
		private MainCamera mainCamera;
		private Vector2 perspectiveOffset;

		private bool isUnity2D;


		private void Awake ()
		{
			if (GameObject.FindWithTag (Tags.mainCamera) && GameObject.FindWithTag (Tags.mainCamera).GetComponent <MainCamera>())
			{
				mainCamera = GameObject.FindWithTag (Tags.mainCamera).GetComponent <MainCamera>();
			}

			xStart = transform.localPosition.x;
			yStart = transform.localPosition.y;

			xDesired = xStart;
			yDesired = yStart;

			if (AdvGame.GetReferences ().settingsManager)
			{
				isUnity2D = AdvGame.GetReferences ().settingsManager.IsUnity2D ();
			}
		}


		private void Update ()
		{
			if (mainCamera)
			{
				if (isUnity2D)
				{
					perspectiveOffset = new Vector2 (mainCamera.transform.position.x, mainCamera.transform.position.y);
				}
				else
				{
					perspectiveOffset = mainCamera.perspectiveOffset;
				}
			}

			xDesired = xStart;
			if (xScroll)
			{
				xDesired += perspectiveOffset.x * depth;
				xDesired += xOffset;
			}

			yDesired = yStart;
			if (yScroll)
			{
				yDesired += perspectiveOffset.y * depth;
				yDesired += yOffset;
			}

			transform.localPosition = new Vector3 (xDesired, yDesired, transform.localPosition.z);
		}

	}

}