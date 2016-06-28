using System;
using System.ComponentModel;

namespace Validus.FileNet
{
	public enum VersionStatus
	{
		[Description("Released")]
		Released = 1,
		[Description("In Process")]
		InProcess = 2,
		[Description("Reservation")]
		Reservation = 3,
		[Description("Superseded")]
		Superseded = 4
	}
}