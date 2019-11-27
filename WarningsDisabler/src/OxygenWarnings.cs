﻿using Harmony;
using Common.Configuration;

namespace WarningsDisabler
{
	// Disabling low oxygen warnings
	static class OxygenWarnings
	{
		static int hintMessageHash = 0;
		
		// for hiding popup message when changing option in game
		public class HideOxygenHint: Config.Field.ICustomAction
		{
			public void customAction()
			{
				if (!Main.config.oxygenWarningsEnabled)
				{
					uGUI_PopupMessage popup = Hint.main?.message;
					
					if (popup && popup.isShowingMessage && popup.messageHash == hintMessageHash)
						popup.Hide();
				}
			}
		}
		
		// to make sure we hide proper popup
		[HarmonyPatch(typeof(HintSwimToSurface), "OnLanguageChanged")]
		static class HintSwimToSurface_OnLanguageChanged_Patch
		{
			static void Postfix(HintSwimToSurface __instance) => hintMessageHash = __instance.messageHash;
		}
		
		[HarmonyPatch(typeof(HintSwimToSurface), "Update")]
		static class HintSwimToSurface_Update_Patch
		{
			static bool Prefix() => Main.config.oxygenWarningsEnabled;
		}
	
		[HarmonyPatch(typeof(LowOxygenAlert), "Update")]
		static class LowOxygenAlert_Update_Patch
		{
			static bool Prefix() => Main.config.oxygenWarningsEnabled;
		}
	}
}