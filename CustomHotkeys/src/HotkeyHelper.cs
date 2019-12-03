﻿using System;
using System.Runtime.InteropServices;

using UnityEngine;

using Common;

namespace CustomHotkeys
{
	static class HotkeyHelper
	{
		static GameObject gameObject = null;
		
		[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
		private static extern bool SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
		
		[DllImport("user32.dll", EntryPoint = "FindWindow")]
		public static extern IntPtr FindWindow(string className, string windowName);

		static void setPos()
		{
			IntPtr p = FindWindow(null, "Subnautica");
			if (p != null)
				p.ToString().onScreen();
			
			SetWindowPos(FindWindow(null, "Subnautica"), 0, 10, 500, 0, 0, 0x0001);
		}

		class Hotkeys: MonoBehaviour
		{
			void Update()
			{
				if (Input.GetKeyDown(KeyCode.F1))
				{
					DisplayManager.SetResolution(1280, 720, false);
					setPos();
				}
				
				if (Input.GetKeyDown(KeyCode.F2))
					DisplayManager.SetResolution(2560, 1440, true);

				if (Input.GetKeyDown(KeyCode.F3))
					DayNightCycle.main._dayNightSpeed = 100f;
				
				if (Input.GetKeyDown(KeyCode.F4))
					DayNightCycle.main._dayNightSpeed = 1f;

				if (Input.GetKey(KeyCode.F5))
					DevConsole.instance.Submit("warpforward " + Main.config.warpStep);
			}
		}


		public static void init()
		{
			if (gameObject == null)
				gameObject = UnityHelper.createPersistentGameObject<Hotkeys>("CustomHotkeys");
		}
	}
}
