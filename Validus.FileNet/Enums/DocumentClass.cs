using System.ComponentModel;

namespace Validus.FileNet
{
	public enum DocumentClass
	{
		[Description("")]
		Any,
		[Description("Claims")]
		Claims,
		[Description("Compliance")]
		Compliance,
		[Description("DevDocument")]
		Development,
		[Description("Document")]
		Document,
		[Description("IT")]
		IT,
		[Description("Underwriting")]
		Underwriting,
		[Description("Underwriting2014")]
		Underwriting2014,
		[Description("URSL")]
		URSL
	}
}