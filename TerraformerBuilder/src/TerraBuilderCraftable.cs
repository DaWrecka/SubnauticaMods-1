﻿using System.Collections.Generic;
using UnityEngine;
using SMLHelper.V2.Crafting;

using Common;
using Common.Crafting;

namespace TerraformerBuilder
{
	class TerraBuilder: CraftableObject
	{
		public static new TechType TechType { get; private set; } = 0;

		protected override GameObject getGameObject()
		{
			GameObject prefab = Object.Instantiate(CraftData.GetPrefabForTechType(TechType.Terraformer));

			Terraformer trfCmp = prefab.GetComponent<Terraformer>();
			BuilderTool bldCmp = prefab.AddComponent<BuilderTool>();

			bldCmp.copyValuesFrom(trfCmp, "rightHandIKTarget", "leftHandIKTarget", "ikAimRightArm", "ikAimLeftArm", "mainCollider", "pickupable", "useLeftAimTargetOnPlayer");

			Object.DestroyImmediate(trfCmp);

			bldCmp.animator = prefab.getChild("terraformer_anim").GetComponent<Animator>();

			prefab.AddComponent<TerraBuilderControl>();

			VFXFabricating vfxFab = prefab.GetComponentInChildren<VFXFabricating>();
			vfxFab.eulerOffset = new Vector3(-10f, 90f, 0f);
			vfxFab.posOffset = new Vector3(-0.4f, 0.11f, 0f);
			vfxFab.localMaxY = 0.24f;

			return prefab;
		}
		
		protected override TechData getTechData() => new TechData() { craftAmount = 1, Ingredients = new List<Ingredient>
		{
			new Ingredient(TechType.ComputerChip, 1),
			new Ingredient(TechType.WiringKit, 1),
			new Ingredient(TechType.Battery, 1)
		}};

		public override void patch()
		{
			TechType = register("Habitat builder",
								"Fabricates habitat compartments and appliances from raw materials.",
								AssetsHelper.loadSprite(ClassID));

			setPDAGroup(TechGroup.Personal, TechCategory.Tools);
			addToCraftingNode(CraftTree.Type.Fabricator, "Personal/Tools");
			setEquipmentType(EquipmentType.Hand);
				
			setCraftingTime(7f);
			
			if (Main.config.bigInInventory)
				setItemSize(2, 2);

			unlockOnStart();
		}
	}
}