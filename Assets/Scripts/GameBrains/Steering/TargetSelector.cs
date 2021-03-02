#region Copyright © ThotLab Games 2011. Licensed under the terms of the Microsoft Reciprocal Licence (Ms-RL).

// Microsoft Reciprocal License (Ms-RL)
//
// This license governs use of the accompanying software. If you use the software, you accept this
// license. If you do not accept the license, do not use the software.
//
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same
// meaning here as under U.S. copyright law.
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
//
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and
// limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free
// copyright license to reproduce its contribution, prepare derivative works of its contribution,
// and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and
// limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free
// license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or
// otherwise dispose of its contribution in the software or derivative works of the contribution in
// the software.
//
// 3. Conditions and Limitations
// (A) Reciprocal Grants- For any file you distribute that contains code from the software (in
// source code or binary format), you must provide recipients the source code to that file along
// with a copy of this license, which license will govern that file. You may license other files
// that are entirely your own work and do not contain code from the software under any terms you
// choose.
// (B) No Trademark License- This license does not grant you rights to use any contributors' name,
// logo, or trademarks.
// (C) If you bring a patent claim against any contributor over patents that you claim are
// infringed by the software, your patent license from such contributor to the software ends
// automatically.
// (D) If you distribute any portion of the software, you must retain all copyright, patent,
// trademark, and attribution notices that are present in the software.
// (E) If you distribute any portion of the software in source code form, you may do so only under
// this license by including a complete copy of this license with your distribution. If you
// distribute any portion of the software in compiled or object code form, you may only do so under
// a license that complies with this license.
// (F) The software is licensed "as-is." You bear the risk of using it. The contributors give no
// express warranties, guarantees or conditions. You may have additional consumer rights under your
// local laws which this license cannot change. To the extent permitted under your local laws, the
// contributors exclude the implied warranties of merchantability, fitness for a particular purpose
// and non-infringement.

#endregion Copyright © ThotLab Games 2011. Licensed under the terms of the Microsoft Reciprocal Licence (Ms-RL).

using GameBrains.Cameras;
using GameBrains.GUI;
using GameBrains.Motors;
using UnityEngine;

// Add to the component menu.
namespace GameBrains.Steering
{
	[AddComponentMenu("Scripts/Steering/Target Selector")]

	public class TargetSelector : WindowManager
	{
		public GUISkin customSkin;
		public SteeringBehaviour steeringBehaviour;
		public GameObject[] targets;
		public TargetedCamera[] targetedCameras;
		public Vector2 windowPositionOffset = new Vector2(0, 50);
		public int rowsPerColumn = 3;

		private Motor motor;

		private int width;
		private int height;
		private Rect windowRectangle;

		private GUIStyle centeredLabelStyle;

		private string windowTitle = "Target Selector";

		// After all objects are initialized, Awake is called when the script
		// is being loaded. This occurs before any Start calls.
		// Use Awake instead of the constructor for initialization.
		public void Awake()
		{
			motor = GetComponent<Motor>();
			if (motor == null)
			{
				Debug.Log("No Motor");
			}
			motor.AddSteeringScript();
			steeringBehaviour = motor.steeringBehaviour;
			if (steeringBehaviour == null)
			{
				Debug.Log("No Steering behaviour");
			}	

			GameObject mainCamera = GameObject.Find("Main Camera");
			if (mainCamera != null)
			{
				targetedCameras = mainCamera.GetComponents<TargetedCamera>();
			}

			rowsPerColumn = Mathf.Max(3, rowsPerColumn);
		}

		// If this behavior is enabled, OnGUI is called for rendering and handling GUI events.
		// It might be called several times per frame (one call per event).
		public void OnGUI()
		{
			UnityEngine.GUI.skin = customSkin;

			if (width != Screen.width || height != Screen.height)
			{
				width = Screen.width;
				height = Screen.height;
				windowRectangle = new Rect(Screen.width * 0.02f + windowPositionOffset.x, Screen.height * 0.02f + windowPositionOffset.y, 480, 0); // GUILayout will determine height
			}

			windowRectangle = GUILayout.Window(windowId, windowRectangle, WindowFunction, windowTitle);
		}

		// This creates the GUI inside the window.
		// It requires the id of the window it's currently making GUI for.
		private void WindowFunction(int windowID)
		{
			// Draw any Controls inside the window here.

			if (steeringBehaviour == null)
			{
				Debug.Log("Getting Steering Behavior");
				steeringBehaviour = GetComponent<SteeringBehaviour>();
			}

			if (steeringBehaviour == null)
			{
				Debug.Log("No Steering Behavior");
				return;
			}

			if (motor == null)
			{
				Debug.Log("Getting Motor");
				motor = GetComponent<Motor>();
			}

			if (motor == null)
			{
				Debug.Log("No Motor");
				return;
			}

			centeredLabelStyle ??= new GUIStyle(UnityEngine.GUI.skin.GetStyle("Label"))
			{
				alignment = TextAnchor.MiddleCenter
			};

			GUILayout.BeginHorizontal();

			GUILayout.Label(name, centeredLabelStyle);

			if (GUILayout.Button(motor.isAiControlled ? "is an AI" : "is a Player"))
			{
				motor.isAiControlled = !motor.isAiControlled;
				steeringBehaviour.targetObject = null;
				steeringBehaviour.targetPosition = transform.position;
			}

			if (targetedCameras != null && GUILayout.Button("Watch"))
			{
				foreach (TargetedCamera targetedCamera in targetedCameras)
				{
					targetedCamera.target = transform;
				}
			}

			GUILayout.EndHorizontal();

			int targetIndex = 0;

			GUILayout.BeginHorizontal();

			GUILayout.BeginVertical();

			if (GUILayout.Button("None"))
			{
				steeringBehaviour.targetObject = null;
				steeringBehaviour.targetPosition = transform.position;
			}

			if (GUILayout.Button("Origin"))
			{
				steeringBehaviour.targetObject = null;
				steeringBehaviour.targetPosition = Vector3.zero;
			}

			if (GUILayout.Button("Random"))
			{
				steeringBehaviour.targetObject = null;
				steeringBehaviour.targetPosition = Random.insideUnitSphere * 50;
			}

			var row = 3;

			while (row < rowsPerColumn && targetIndex < targets.Length)
			{
				if (GUILayout.Button(targets[targetIndex].name))
				{
					steeringBehaviour.targetObject = targets[targetIndex];
				}

				row++;
				targetIndex++;
			}

			GUILayout.EndVertical();

			while (targetIndex < targets.Length)
			{
				GUILayout.BeginVertical();

				row = 0;

				while (row < rowsPerColumn && targetIndex < targets.Length)
				{
					if (GUILayout.Button(targets[targetIndex].name))
					{
						steeringBehaviour.targetObject = targets[targetIndex];
					}

					row++;
					targetIndex++;
				}

				GUILayout.EndVertical();
			}

			GUILayout.EndHorizontal();

			// Make the windows be draggable.
			UnityEngine.GUI.DragWindow();
		}
	}
}