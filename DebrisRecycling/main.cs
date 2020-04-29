﻿using Common;
using Common.Crafting;
using Common.Configuration;

namespace DebrisRecycling
{
	public static class Main
	{
		internal static readonly ModConfig config = Mod.init<ModConfig>();

		public static void patch()
		{
			HarmonyHelper.patchAll();
			CraftHelper.patchAll();

			LanguageHelper.init(); // after CraftHelper

			DebrisPatcher.init(config.prefabsConfig, Mod.loadConfig<PrefabIDs>("prefabs_config.json", Config.LoadOptions.None));
		}
	}
}