/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"_Camera.cs"
 * 
 *	This is the base class for GameCamera and FirstPersonCamera.
 * 
 */


using UnityEngine;
using System.Collections;

namespace AC
{

	public class _Camera : MonoBehaviour
	{

		protected virtual void Awake ()
		{
			if (GameObject.FindWithTag (Tags.mainCamera) && GameObject.FindWithTag (Tags.mainCamera).GetComponent <MainCamera>())
			{
				this.camera.enabled = false;
			}
		}


		public virtual void ResetTarget ()
		{ }


		public Vector3 PositionRelativeToCamera (Vector3 _position)
		{
			return (_position.x * ForwardVector ()) + (_position.z * RightVector ());
		}
		
		
		public Vector3 RightVector ()
		{
			return (transform.right);
		}
		
		
		public Vector3 ForwardVector ()
		{
			Vector3 camForward;
			
			camForward = transform.forward;
			camForward.y = 0;
			
			return (camForward);
		}
		
		
		public virtual void MoveCameraInstant ()
		{ }
		
		
		protected float ConstrainAxis (float desired, Vector2 range)
		{
			if (range.x < range.y)
			{
				desired = Mathf.Clamp (desired, range.x, range.y);
			}
			
			else if (range.x > range.y)
			{
				desired = Mathf.Clamp (desired, range.y, range.x);
			}
			
			else
			{
				desired = range.x;
			}
				
			return desired;
		}


		public void SetSplitScreen (MenuOrientation splitOrientation, bool isTopLeftSplit)
		{
			camera.enabled = true;

			float borderWidth = GameObject.FindWithTag (Tags.mainCamera).GetComponent <MainCamera>().borderWidth;
			float split = 0.49f;
			
			// Pillarbox
			if (GameObject.FindWithTag (Tags.mainCamera).GetComponent <MainCamera>().borderOrientation == MenuOrientation.Vertical)
			{
				if (splitOrientation == MenuOrientation.Horizontal)
				{
					if (!isTopLeftSplit)
					{
						camera.rect = new Rect (borderWidth, 0f, 1f - (2*borderWidth), split);
					}
					else
					{
						camera.rect = new Rect (borderWidth, 1f - split, 1f - (2*borderWidth), split);
					}
				}
				else
				{
					if (isTopLeftSplit)
					{
						camera.rect = new Rect (borderWidth, 0f, split - borderWidth, 1f);
					}
					else
					{
						camera.rect = new Rect (1f - split, 0f, split - borderWidth, 1f);
					}
				}
			}
			// Letterbox
			else
			{
				if (splitOrientation == MenuOrientation.Horizontal)
				{
					if (isTopLeftSplit)
					{
						camera.rect = new Rect (0f, 1f - split, 1f, split - borderWidth);
					}
					else
					{
						camera.rect = new Rect (0f, borderWidth, 1f, split - borderWidth);
					}
				}
				else
				{
					if (isTopLeftSplit)
					{
						camera.rect = new Rect (0f, borderWidth, split, 1f - (2*borderWidth));
					}
					else
					{
						camera.rect = new Rect (1f - split, borderWidth, split, 1f - (2*borderWidth));
					}
				}
			}
		}


		public void RemoveSplitScreen ()
		{
			if (camera.enabled)
			{
				camera.rect = new Rect (0f, 0f, 1f, 1f);
				camera.enabled = false;
			}
		}
		
	}

}