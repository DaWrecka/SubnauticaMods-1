﻿using System;
using System.Reflection;

namespace Common.Configuration
{
	partial class Config
	{
		// for use with class attributes
		// attributes can be used on config class and inner classes
		public interface IConfigAttribute { void process(object config); }

		// for use with config field attributes
		// attributes can be used on config's fields and inner classes fields
		public interface IFieldAttribute  { void process(object config, FieldInfo field); }

		// for use in attributes that need to know root config
		// can be used with both class attributes and field attributes
		public interface IRootConfigInfo  { void setRootConfig(Config config); }


		// for use with non-primitive types, inner fields will not be searched for attributes
		// (other attributes of the field will still be processed)
		[AttributeUsage(AttributeTargets.Field)]
		public class SkipRecursiveAttrProcessing: Attribute {}

		public static bool _isFieldValidForRecursiveAttrProcessing(FieldInfo field) =>
			field.FieldType.IsClass && !field.IsStatic && !field.checkAttribute<SkipRecursiveAttrProcessing>();


		void processAttributes()
		{
			_processAttributes(this); // recursive

			void _processAttributes(object config)
			{
				if (config == null)
					return;

				// processing attributes for config class
				foreach (var attr in Attribute.GetCustomAttributes(config.GetType()))
				{
					(attr as IRootConfigInfo)?.setRootConfig(this); // need to be first
					(attr as IConfigAttribute)?.process(config);
				}

				// processing attributes for fields and nested classes (don't process static fields)
				foreach (var field in config.GetType().fields())
				{																															$"Checking field '{field.Name}' for attributes".logDbg();
					foreach (var attr in Attribute.GetCustomAttributes(field))
					{
						(attr as IRootConfigInfo)?.setRootConfig(this); // need to be first
						(attr as IFieldAttribute)?.process(config, field);
					}

					if (_isFieldValidForRecursiveAttrProcessing(field))
						_processAttributes(field.GetValue(config));
				}
			}
		}
	}
}