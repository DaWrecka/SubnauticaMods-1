﻿using System;
using System.Reflection;

using SMLHelper.V2.Options;

namespace Common.Config
{
	partial class Options: ModOptions
	{
		// implement this to create custom action when field is changes via options
		public interface IFieldCustomAction
		{
			void fieldCustomAction();
		}
		
		// Attribute for creating options UI elements
		[AttributeUsage(AttributeTargets.Field)]
		public class FieldAttribute: Config.FieldAttribute
		{
			string label = null;
			Type customActionType = null;

			public FieldAttribute(string Label = null, Type CustomActionType = null)
			{
				label = Label;
				customActionType = CustomActionType;
			}

			override public void process(object config, FieldInfo field)
			{																			$"Options.FieldAttribute.process fieldName:'{field.Name}' fieldType:{field.FieldType} label: '{label}'".logDbg();
				if (mainConfig == null)
					mainConfig = config as Config;

				if (label == null)
					label = field.Name;

				ModOption.InitParams initParams = new ModOption.InitParams{config = config, field = field, label = label};

				if (customActionType != null)
				{
					initParams.action = Activator.CreateInstance(customActionType) as IFieldCustomAction;
					
					if (initParams.action == null)
						$"Options.FieldAttribute: '{field.Name}' You need to implement IFieldCustomAction in CustomAction".logError();
				}

				if (field.FieldType == typeof(bool))
				{
					add(new ToggleOption(initParams));
				}
				else
				if (field.FieldType == typeof(UnityEngine.KeyCode))
				{
					add(new KeyBindOption(initParams));
				}
				else
				if (field.FieldType == typeof(float) || field.FieldType == typeof(int))
				{
					// creating ChoiceOption if we also have choice attribute
					ChoiceAttribute choice = GetCustomAttribute(field, typeof(ChoiceAttribute)) as ChoiceAttribute;
					if (choice != null && choice.choices.Length > 0)
					{
						add(new ChoiceOption(initParams, choice.choices));
						return;
					}

					// creating SliderOption if we also have bounds attribute
					Config.FieldBoundsAttribute bounds = GetCustomAttribute(field, typeof(Config.FieldBoundsAttribute)) as Config.FieldBoundsAttribute;
					if (bounds != null && bounds.isBothBoundsSet())
					{
						add(new SliderOption(initParams, bounds.min, bounds.max));
						return;
					}

					$"Options.FieldAttribute: '{field.Name}' For numeric option field you also need to add ChoiceAttribute or FieldBoundsAttribute".logError();
				}
				else
					$"Options.FieldAttribute: '{field.Name}' Unsupported field type".logError();
			}
		}
	}
}