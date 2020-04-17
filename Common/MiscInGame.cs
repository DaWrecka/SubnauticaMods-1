﻿using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Common
{
	static partial class StringExtensions
	{
		public static string onScreen(this string s)
		{
			if (!SaveLoadManager.main.isLoading && Time.timeScale != 0f)
				ErrorMessage.AddDebug(s);

			return s;
		}

		public static void onScreen(this List<string> list, string msg = "", int maxCount = 30)
		{
			List<string> listToPrint = list.Count > maxCount? list.GetRange(0, maxCount): list;

			if (list.Count > maxCount)
				$"List is too large ({list.Count} entries), printing first {maxCount} entries".onScreen();

			listToPrint.ForEach(s => ErrorMessage.AddDebug(msg + s));
		}
	}

	static class Strings
	{
		public static class Mouse
		{
			static string _str(int utf32) => "<color=#ADF8FFFF>" + char.ConvertFromUtf32(utf32) + "</color>";

			public static readonly string rightButton	= _str(57404);
			public static readonly string middleButton	= _str(57405);
			public static readonly string scrollUp		= _str(57406);
			public static readonly string scrollDown	= _str(57407);
		}

		public static readonly string modName = Assembly.GetExecutingAssembly().GetName().Name;
	}

	static partial class SpriteHelper // extended in other Common projects
	{
		public static Atlas.Sprite getSprite(object spriteID)
		{
			$"TechSpriteHelper.getSprite({spriteID.GetType()}) is not implemented!".logError();
			return SpriteManager.defaultSprite;
		}
	}

	static class GameUtils
	{
		// can't use vanilla GetVehicle in OnPlayerModeChange after 06.11 update :(
		public static Vehicle getVehicle(this Player player) => player.GetComponentInParent<Vehicle>();

		public static TechType getHeldToolType() => Inventory.main?.GetHeldTool()?.pickupable.GetTechType() ?? TechType.None;
	}

	static class MiscInGameExtensions
	{
		public static int getArgsCount(this NotificationCenter.Notification n) => n?.data?.Count ?? 0;
		public static object getArg(this NotificationCenter.Notification n, int index) => n.data[index]; // do not check for null at that point
		public static object getArgSafe(this NotificationCenter.Notification n, int index) => n?.data?.Count > index? n.data[index]: null;
	}


	// base class for console commands which are exists between scenes
	abstract class PersistentConsoleCommands: MonoBehaviour
	{
		const string cmdPrefix = "OnConsoleCommand_";

		readonly List<string> cmdNames = new List<string>();

		public static GameObject createGameObject<T>(string name = "ConsoleCommands") where T: PersistentConsoleCommands
		{
			return UnityHelper.createPersistentGameObject<T>(name);
		}

		void init()
		{																													"PersistentConsoleCommands.init cmdNames already inited!".logDbgError(cmdNames.Count > 0);
			// searching for console commands methods in derived class
			GetType().methods().Where(m => m.Name.StartsWith(cmdPrefix)).forEach(m => cmdNames.Add(m.Name.Replace(cmdPrefix, "")));
		}

		void registerCommands()
		{
			foreach (var cmdName in cmdNames)
			{
				// double registration is checked inside DevConsole
				DevConsole.RegisterConsoleCommand(this, cmdName);															$"PersistentConsoleCommands: {cmdName} is registered".logDbg();
			}
		}

		void Awake()
		{
			init();
			SceneManager.sceneUnloaded += onSceneUnloaded;

			registerCommands();
		}

		void OnDestroy()
		{
			SceneManager.sceneUnloaded -= onSceneUnloaded;
		}

		// notifications are cleared between some scenes, so we need to reregister commands
		void onSceneUnloaded(Scene scene)
		{
			registerCommands();
		}
	}
}