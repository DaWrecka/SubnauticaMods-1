﻿using System;
using System.Reflection;

using SMLHelper.V2.Options;

namespace Common.Config
{
	partial class Options: ModOptions
	{
		abstract class ModOption
		{
			public struct InitParams
			{
				public object config;
				public FieldInfo field;
				public string label;
				public IFieldCustomAction action;
			}
			
			public readonly string id;
			protected readonly string label;

			readonly object config;
			readonly FieldInfo field;
			readonly IFieldCustomAction action;

			protected object fieldValue
			{
				get => field.GetValue(config);

				set
				{
					try
					{
						field.SetValue(config, Convert.ChangeType(value, field.FieldType));
					}
					catch (Exception e)
					{
						Log.msg(e);
					}
				}
			}

			public ModOption(InitParams p)
			{
				field = p.field;
				config = p.config;
				action = p.action;

				id = field.Name;
				label = p.label;
			}
			
			public ModOption(object _config, FieldInfo _field, string _label)
			{
				field = _field;
				config = _config;

				id = field.Name;
				label = _label;
			}

			abstract public void addOption(Options options);

			virtual public void onEvent(EventArgs e)
			{
				mainConfig?.save();
				action?.fieldCustomAction();
			}
		}


		class ToggleOption: ModOption
		{
			//public ToggleOption(object _config, FieldInfo _fieldInfo, string _label):
			//	base(_config, _fieldInfo, _label)
			//{
			//}
			
			public ToggleOption(InitParams prms): base(prms)
			{
			}

			override public void addOption(Options options)
			{
				options.AddToggleOption(id, label, fieldValue.toBool());
			}

			override public void onEvent(EventArgs e)
			{
				fieldValue = (e as ToggleChangedEventArgs)?.Value;
				base.onEvent(e);
			}
		}


		class SliderOption: ModOption
		{
			float min, max;

			public SliderOption(object _config, FieldInfo _fieldInfo, string _label, float _min, float _max):
				base(_config, _fieldInfo, _label)
			{
				min = _min;
				max = _max;
			}
			
			public SliderOption(InitParams prms, float _min, float _max): base(prms)
			{
				min = _min;
				max = _max;
			}

			override public void addOption(Options options)
			{
				options.AddSliderOption(id, label, min, max, fieldValue.toFloat());
			}

			override public void onEvent(EventArgs e)
			{
				fieldValue = (e as SliderChangedEventArgs)?.Value;
				base.onEvent(e);
			}
		}


		class ChoiceOption: ModOption
		{
			string[] choices = null;

			public ChoiceOption(object _config, FieldInfo _fieldInfo, string _label, string[] _choices):
				base(_config, _fieldInfo, _label)
			{
				choices = _choices;
			}

			override public void addOption(Options options)
			{
				options.AddChoiceOption(id, label, choices, fieldValue.toInt());
			}

			override public void onEvent(EventArgs e)
			{
				fieldValue = (e as ChoiceChangedEventArgs)?.Index;
				base.onEvent(e);
			}
		}


		class KeyBindOption: ModOption
		{
			public KeyBindOption(object _config, FieldInfo _fieldInfo, string _label):
				base(_config, _fieldInfo, _label)
			{
			}

			override public void addOption(Options options)
			{
				options.AddKeybindOption(id, label, GameInput.Device.Keyboard, (UnityEngine.KeyCode)fieldValue.toInt());
			}

			override public void onEvent(EventArgs e)
			{
				fieldValue = (e as KeybindChangedEventArgs)?.Key;
				base.onEvent(e);
			}
		}
	}
}