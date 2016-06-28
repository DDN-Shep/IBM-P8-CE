using System;
using System.ComponentModel;

namespace Validus.FileNet
{
	public static class EnumExtensions
	{
		public static string GetDescription(this Enum value)
		{
			var type = value.GetType();
			var name = Enum.GetName(type, value);
			var field = type.GetField(name);

			var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

			return attribute != null ? attribute.Description : name;
		}
	}
}