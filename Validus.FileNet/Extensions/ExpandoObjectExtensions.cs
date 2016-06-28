using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Validus.FileNet
{
	public static class ExpandoObjectExtensions
	{
		public static void Add(this ExpandoObject value, IDictionary<string, object> source)
		{
			var expando = (IDictionary<string, object>)value;

			source.ToList().ForEach(kvp => expando.Add(kvp.Key, kvp.Value));
		}

		public static IDictionary<string, object> UnMapFromFileNet(this IDocument value, IDictionary<string, object> destination,
																   bool includeReadOnly = false, bool includeSystem = false)
		{
			var properties = value.GetType().GetProperties();

			foreach (var property in properties)
			{
				var attributes = property.GetCustomAttributes(typeof(FileNetFieldAttribute), true);
				var field = attributes.OfType<FileNetFieldAttribute>()
				                      .SingleOrDefault(f => !f.IsReadOnly | includeReadOnly &&
				                                            !f.IsSystem | includeSystem);

				if (field == null || field.Equals(default(FileNetFieldAttribute)))
					continue;

				if (!destination.ContainsKey(field.ID))
					destination.Add(field.ID, property.GetValue(value, null));
			}

			return destination;
		}

		// TODO: Switch value & destination and extend IDocument instead
		public static T MapToFileNet<T>(this IDictionary<string, object> value, T destination,
										bool includeReadOnly = false, bool includeSystem = false)
		{
			var properties = destination.GetType().GetProperties();

			foreach (var property in properties)
			{
				var attributes = property.GetCustomAttributes(typeof(FileNetFieldAttribute), true);
				var field = attributes.OfType<FileNetFieldAttribute>()
									  .SingleOrDefault(f => !f.IsReadOnly | includeReadOnly &&
															!f.IsSystem | includeSystem);

				if (field == null || string.IsNullOrEmpty(field.ID) || field.Equals(default(FileNetFieldAttribute)))
					continue;

				var fnProperty = value.SingleOrDefault(kvp => kvp.Key.ToLower() == field.ID.ToLower());

				if (fnProperty.Equals(default(KeyValuePair<string, object>)))
					continue;

				var fnValue = default(object);

				if (property.PropertyType == typeof(Guid) || property.PropertyType == typeof(Guid?))
				{
					var fnGuid = default(Guid);

					if (fnProperty.Value != null
					    && Guid.TryParse(fnProperty.Value.ToString(), out fnGuid)
						&& fnGuid != default(Guid))
						fnValue = fnGuid;
				}
				else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
				{
					var fnDateTime = default(DateTime);

					if (fnProperty.Value != null
						&& DateTime.TryParse(fnProperty.Value.ToString(), out fnDateTime)
						&& fnDateTime != default(DateTime))
						fnValue = fnDateTime;
				}
				else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
				{
					var fnInteger = default(int);

					if (fnProperty.Value != null
						&& int.TryParse(fnProperty.Value.ToString(), out fnInteger)
						&& fnInteger != default(int))
						fnValue = fnInteger;
				}
				else if (property.PropertyType == typeof(string))
				{
					fnValue = fnProperty.Value;
				}

				if (fnValue != default(object))
				{
					property.SetValue(destination, fnValue, null);
				}
			}

			return destination;
		}
	}
}