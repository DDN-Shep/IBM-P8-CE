using System;
using System.Runtime.Serialization;

namespace Validus.FileNet
{
	[Serializable, DataContract]
	public class DateRange
	{
		[IgnoreDataMember]
		public const DateTimeKind Kind = DateTimeKind.Utc;

		[IgnoreDataMember]
		public const string FromFormat = "yyyyMMddT000000Z";

		[IgnoreDataMember]
		public const string ToFormat = "yyyyMMddT235959Z";

		[IgnoreDataMember]
		public string Name { get; set; }

		[DataMember]
		public DateTime? From { get; set; }

		[DataMember]
		public DateTime? To { get; set; }
	}
}