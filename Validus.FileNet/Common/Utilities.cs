using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Validus.FileNet
{
	public static class Utilities
	{
        public static ModifiablePropertyType[] GetPropertyCollection(IDictionary<string, object> dmsPropertyValues) => dmsPropertyValues.Select(GetProperty).ToArray();

        public static ModifiablePropertyType GetProperty(KeyValuePair<string, object> dmsPropertyValue)
		{
			var dmsProperty = default(ModifiablePropertyType);

			if (dmsPropertyValue.Value == null)
			{
				dmsProperty = new SingletonString { Value = null };
			}
			else
			{
				var propertyType = dmsPropertyValue.Value.GetType();

				if (propertyType == typeof(string))
				{
					dmsProperty = new SingletonString
					{
						Value = Convert.ToString(dmsPropertyValue.Value)
					};
				}
				else if (propertyType == typeof(bool) || propertyType == typeof(bool?))
				{
					dmsProperty = new SingletonBoolean
					{
						Value = Convert.ToBoolean(dmsPropertyValue.Value),
						ValueSpecified = true
					};
				}
				else if (propertyType == typeof(int) || propertyType == typeof(int?))
				{
					dmsProperty = new SingletonInteger32
					{
						Value = Convert.ToInt32(dmsPropertyValue.Value),
						ValueSpecified = true
					};
				}
				else if (propertyType == typeof(float) || propertyType == typeof(float?) ||
						 propertyType == typeof(double) || propertyType == typeof(double?))
				{
					dmsProperty = new SingletonFloat64
					{
						Value = Convert.ToDouble(dmsPropertyValue.Value),
						ValueSpecified = true
					};
				}
				else if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
				{
					dmsProperty = new SingletonDateTime
					{
						Value = Convert.ToDateTime(dmsPropertyValue.Value),
						ValueSpecified = true
					};
				}
				else if (propertyType == typeof(Guid) || propertyType == typeof(Guid?))
				{
					dmsProperty = new SingletonId
					{
						Value = ((Guid)dmsPropertyValue.Value).ToString()
					};
				}
				else if (propertyType == typeof(string[]))
				{
					dmsProperty = new ListOfString
					{
						Value = (string[])dmsPropertyValue.Value
					};
				}
				else if (propertyType == typeof(List<string>))
				{
					dmsProperty = new ListOfString
					{
						Value = ((List<string>)dmsPropertyValue.Value).ToArray()
					};
				}
				else
				{
					throw new NotImplementedException(propertyType.FullName);
				}
			}
				
			dmsProperty.propertyId = dmsPropertyValue.Key;

			return dmsProperty;
		}

		public static Localization GetTimezone()
		{
			var timeZone = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);

			return new Localization
			{
				Timezone = (timeZone.Hours >= 0) // Timezone format should be '+|-HH:MM' (e.g., -07:00)
				    ? string.Format("+{0}:{1}", timeZone.Hours.ToString("D2"), timeZone.Minutes.ToString("D2"))
				    : string.Format("{0}:{1}", timeZone.Hours.ToString("D2"), timeZone.Minutes.ToString("D2"))
			};
		}

		public static string GetPropertyValue(PropertyType dmsProperty)
		{
			string propertyValue = null;

			if (dmsProperty != null)
			{
				var propertyType = dmsProperty.GetType();

				if (propertyType == typeof(SingletonString))
				{
					propertyValue = ((SingletonString)dmsProperty).Value ?? string.Empty;
				}
				else if (propertyType == typeof(SingletonBoolean))
				{
					propertyValue = ((SingletonBoolean)dmsProperty).Value.ToString();
				}
				else if (propertyType == typeof(SingletonDateTime))
				{
					propertyValue = ((SingletonDateTime)dmsProperty).Value.ToString();
				}
				else if (propertyType == typeof(SingletonFloat64))
				{
					propertyValue = ((SingletonFloat64)dmsProperty).Value.ToString();
				}
				else if (propertyType == typeof(SingletonId))
				{
					propertyValue = ((SingletonId)dmsProperty).Value;
				}
				else if (propertyType == typeof(SingletonInteger32))
				{
					propertyValue = ((SingletonInteger32)dmsProperty).Value.ToString();
				}
				else if (propertyType == typeof(SingletonObject))
				{
					var dmsObject = (SingletonObject)dmsProperty;

					if ((dmsObject.Value != null) && (dmsObject.Value.GetType() == typeof(ObjectReference)))
					{
						propertyValue = ((ObjectReference)dmsObject.Value).objectId;
					}
				}
				else if (propertyType == typeof(EnumOfObject))
				{
					var dmsObject = (EnumOfObject)dmsProperty;

					if (dmsObject.Value != null)
					{
						foreach (var dmsValue in dmsObject.Value)
						{
							propertyValue = string.Concat(propertyValue, dmsValue.objectId, ";", Environment.NewLine);
						}
					}
				}
				else if (propertyType == typeof(ListOfString))
				{
					var dmsObject = (ListOfString)dmsProperty;

					if (dmsObject.Value != null)
					{
                        propertyValue = string.Join(";", dmsObject.Value);
                    }
				}
				else if (propertyType == typeof(ListOfObject))
				{
					if (dmsProperty.propertyId == "ContentElements")
					{
						propertyValue = dmsProperty.propertyId;
					}
					else
					{
						var dmsObject = (ListOfObject)dmsProperty;

						if (dmsObject.Value != null)
						{
							foreach (var dmsValue in dmsObject.Value)
							{
								propertyValue = string.Concat(propertyValue, dmsValue.classId, ";", Environment.NewLine);
							}
						}
					}
				}
				//else if (propertyType == typeof(UnevaluatedCollection))
				//{
				//	var dmsObject = ((UnevaluatedCollection)dmsProperty).propertyId;

				//	if (dmsObject != null)
				//	{
				//		//foreach (var dmsValue in dmsObject.Value)
				//		//{
				//		//	propertyValue = string.Concat(propertyValue, dmsValue, ";", Environment.NewLine);
				//		//}
				//	}
				//}

				if (propertyValue == null)
				{
					propertyValue = dmsProperty.ToString();
				}
			}

			return propertyValue;
		}

		public static void AddSQLParameter(ref SqlCommand sqlCommand, string paramName, object paramValue)
		{
			if (paramValue == null)
				return;

			if (paramValue is string)
			{
				if (string.IsNullOrEmpty(paramValue.ToString()) == true)
					return;

				paramValue = paramValue.ToString().Replace("'", "''");
			}

			sqlCommand.Parameters.Add(new SqlParameter(paramName, paramValue));
		}
	}
}