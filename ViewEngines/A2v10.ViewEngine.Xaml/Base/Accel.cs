﻿// Copyright © 2019-2023 Oleksandr Kukhtin. All rights reserved.

using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace A2v10.Xaml
{
	[TypeConverter(typeof(AccelConverter))]
	public class Accel : XamlElement
	{
		public String? Key { get; set; }

		// CASM - modifiers (control, alt, shift, meta)
		// Ctrl + A => C___:KeyA
		public String GetKeyCode()
		{
			if (Key == null)
				return String.Empty;
			if (Key.Contains('+'))
			{
				var modifiers = new StringBuilder("____:");
				var x = Key.Split('+');
				var keyName = KeyName2EventCode(x[^1]); // last char
				for (Int32 i = 0; i < x.Length - 1; i++)
				{
					switch (x[i].Trim())
					{
						case "Shift":
							modifiers[2] = 'S';
							break;
						case "Control":
						case "Ctrl":
							modifiers[0] = 'C';
							break;
						case "Alt":
							modifiers[1] = 'A';
							break;
						case "Meta":
							modifiers[3] = 'M';
							break;
						default:
							throw new XamlException($"Invalid key modifier value: '{x[i]}' ");
					}
				}
				return modifiers.ToString() + keyName;
			}
			return "____:" + KeyName2EventCode(Key);
		}

		static String KeyName2EventCode(String keyName)
		{
			keyName = keyName.Trim();
			if (keyName.Length == 1 && keyName[0] >= 'A' && keyName[0] <= 'Z')
				return $"Key{keyName}";
			else if (keyName.Length == 1 && keyName[0] >= '0' && keyName[0] <= '9')
				return $"Digit{keyName}";
			else if (keyName == "Left" || keyName == "Right" || keyName == "Up" || keyName == "Down")
				return $"Arrow{keyName}";
			else if (keyName.Length > 1 && keyName.StartsWith('F'))
				return keyName;
			return keyName;
		}
	}

	public class AccelConverter : TypeConverter
	{
		public override Boolean CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
		{
			if (sourceType == typeof(String))
				return true;
			else if (sourceType == typeof(Accel))
				return true;
			return false;
		}

		public override Object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, Object value)
		{
			if (value == null)
				return null;
			if (value is String)
				return new Accel() { Key = value.ToString()! };
			else if (value is Accel)
				return value as Accel;
			throw new XamlException($"Invalid Accel value '{value}'");
		}
	}


	public class AccelCommand : XamlElement
	{
		public Accel? Accel { get; set; }
		public Command? Command { get; set; }

		public void RenderElement(RenderContext context)
		{
			if (Accel == null || String.IsNullOrEmpty(Accel.Key))
				return;
			var cmd = GetBindingCommand(nameof(Command));
			if (cmd == null)
				return;
			var ac = new TagBuilder("a2-accel-command");
			ac.MergeAttribute("accel", Accel.GetKeyCode());
			ac.MergeAttribute(":command", $"() => {cmd.GetCommand(context)}"); // FUNCTION!!!
			ac.Render(context);
		}
	}


	[TypeConverter(typeof(AccelCommandCollectionConverter))]
	public class AccelCommandCollection : List<AccelCommand>
	{
	}

	public class AccelCommandCollectionConverter : TypeConverter
	{
		public override Boolean CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
		{
			if (sourceType == typeof(AccelCommand))
				return true;
			return base.CanConvertFrom(context, sourceType);
		}

		public override Object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, Object value)
		{
			if (value == null)
				return null;
			if (value is AccelCommand accelCommand)
			{
				var x = new AccelCommandCollection
				{
					accelCommand
				};
				return x;
			}
			return base.ConvertFrom(context, culture, value);
		}
	}
}
