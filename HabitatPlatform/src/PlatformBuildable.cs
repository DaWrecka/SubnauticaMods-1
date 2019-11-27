﻿using UnityEngine;
using SMLHelper.V2.Crafting;
using Common.Crafting;

namespace HabitatPlatform
{
	class HabitatPlatform: CraftableObject
	{
		public static new TechType TechType { get; private set; } = 0;

		protected override TechData getTechData() => new TechData(new Ingredient(TechType.Titanium, 1));

		public override void patch()
		{
			TechType = register("Habitat platform", "Platform", SpriteManager.Get(TechType.AcidMushroom));

			addToGroup(TechGroup.Constructor, TechCategory.Constructor);
			addCraftingNode(CraftTree.Type.Constructor, "");

			unlockOnStart();
		}

		protected override GameObject getGameObject()
		{
			GameObject prefab = Object.Instantiate(CraftData.GetPrefabForTechType(TechType.RocketBase));

			//GameObject foundation =  Object.Instantiate(CraftData.GetPrefabForTechType(TechType.BaseRoom));

			//foundation.transform.parent = prefab.getChild("Base").transform;
			//foundation.transform.localPosition = Vector3.zero;

			//prefab.dump("!!platform");

			return prefab;
		}
	}
}