using System;
using System.Collections.Generic;

namespace Validus.FileNet
{
	interface IUnderwriting2014Document : IDocument
	{
		//List<string> PolicyIDs { get; set; }
        string PolicyIDs { get; set; }

        string COB { get; set; }

		//string DocumentType { get; set; }

		//string SubDocumentType { get; set; }

		string Description { get; set; }

		string InsuredName { get; set; }

		string Underwriter { get; set; }

		string Broker { get; set; }

		DateTime? InceptionDate { get; set; }

		DateTime? WrittenDate { get; set; }

		string AccountingYear { get; set; }

		string Status { get; set; }

		string EntryStatus { get; set; }

		string ScannerUser { get; set; }

		string InputDeviceID { get; set; }

		string WorkflowStatus { get; set; }

		Guid? WorkflowGUID { get; set; }
	}
}