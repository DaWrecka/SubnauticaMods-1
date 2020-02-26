﻿using System;
using SMLHelper.V2.Options;

namespace Common.Configuration
{
	partial class Options: ModOptions
	{
		public class SliderOption: ModOption
		{
			readonly float min, max;

			public SliderOption(Config.Field cfgField, string label, float _min, float _max): base(cfgField, label)
			{
				min = _min;
				max = _max;
			}

			public override void addOption(Options options)
			{
				options.AddSliderOption(id, label, min, max, cfgField.value.toFloat());
			}

			public override void onEvent(EventArgs e)
			{
				cfgField.value = (e as SliderChangedEventArgs)?.Value;
			}
		}
	}
}