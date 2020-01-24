﻿using System.Collections.Generic;

using Harmony;
using UnityEngine;

using Common;
using Common.GameSerialization;

namespace DayNightSpeed
{
	static partial class DayNightSpeedControl
	{
		[HarmonyHelper.PatchClass]
		class StoryGoalsListener: MonoBehaviour
		{
			const float shortGoalDelay = 60f;
			const string saveName = "goals";

			static StoryGoalsListener instance = null;
			List<string> goals = new List<string>(); // goals with delay less than shortGoalDelay

			SaveLoadHelper slHelper;
			class SaveData { public List<string> goals; }

			void Awake()
			{
				if (instance != null && $"StoryGoalsListener already created!".logError())
				{
					Destroy(this);
					return;
				}

				instance = this;
				slHelper = new SaveLoadHelper(null, onSave);
			}

			void OnDestroy() => instance = null;

			void Update() => slHelper.update();

			public static void load() => instance?.onLoad();

			void onLoad()
			{
				goals = SaveLoad.load<SaveData>(saveName)?.goals ?? new List<string>();
				forcedNormalSpeed = (goals.Count != 0);
			}

			void onSave()
			{
				SaveLoad.save(saveName, new SaveData { goals = goals });
			}

			[HarmonyPatch(typeof(Story.StoryGoalScheduler), "Schedule")][HarmonyPostfix]
			static void onAddGoal(Story.StoryGoal goal)
			{
				if (goal == null || goal.delay > shortGoalDelay)
					return;

				forcedNormalSpeed = true;

				instance.goals.Add(goal.key);	$"StoryGoalsListener: goal added '{goal.key}'".logDbg();
			}

			[HarmonyPatch(typeof(Story.StoryGoal), "Execute")][HarmonyPostfix]
			static void onRemoveGoal(string key)
			{
				if (instance.goals.RemoveAll(g => g == key) > 0 && instance.goals.Count == 0)
					forcedNormalSpeed = false;
			}
		}
	}
}