using System;

namespace Validus.FileNet
{
	public interface IP8Document
	{
		Guid ObjectID { get; set; }

		Guid VersionSeriesID { get; set; }

		Guid? ReservationID { get; set; }

		object ContentElements { get; set; }

		byte[] InlineContent { get; set; }

		int VersionStatus { get; set; }

		int MajorVersionNumber { get; set; }

		int MinorVersionNumber { get; set; }

		IP8Document[] Related { get; set; }
	}
}