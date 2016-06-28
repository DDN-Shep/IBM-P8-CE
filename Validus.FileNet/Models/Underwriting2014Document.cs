using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Validus.FileNet
{
	[Serializable, DataContract]
	public class Underwriting2014Document : Document, IUnderwriting2014Document
	{
		[DataMember, FileNetField(ID = "uwPolicyID", Name = "Policy IDs")]
        public string PolicyIDs { get; set; }
        //public List<string> PolicyIDs { get; set; }

        [DataMember, FileNetField(ID = "uwCOB", Name = "COB")]
		public string COB { get; set; }

		[DataMember, FileNetField(ID = "uwDocType", Name = "Document Type")]
		public string DocumentType { get; set; }

		[DataMember, FileNetField(ID = "uwSubDocType", Name = "Sub-Document Type")]
		public string SubDocumentType { get; set; }

		[DataMember, FileNetField(ID = "Description", Name = "Description")]
		public string Description { get; set; }

		[DataMember, FileNetField(ID = "uwInsuredName", Name = "Insured Name")]
		public string InsuredName { get; set; }

		[DataMember, FileNetField(ID = "uwUnderwriter", Name = "Underwriter")]
		public string Underwriter { get; set; }

		[DataMember, FileNetField(ID = "uwBrokerPSU", Name = "Broker")]
		public string Broker { get; set; }

		[DataMember, FileNetField(ID = "uwInceptionDate", Name = "Inception Date")]
		public DateTime? InceptionDate { get; set; }

		[DataMember, FileNetField(ID = "uwWrittenDate", Name = "Written Date")]
		public DateTime? WrittenDate { get; set; }

		[DataMember, FileNetField(ID = "AccountingYear", Name = "Accounting Year")]
		public string AccountingYear { get; set; }

		[DataMember, FileNetField(ID = "uwStatus", Name = "Status")]
		public string Status { get; set; }

		[DataMember, FileNetField(ID = "uwEntryStatus", Name = "Entry Status")]
		public string EntryStatus { get; set; }

		[DataMember, FileNetField(ID = "uwIndexPerson", Name = "Scanner User")]
		public string ScannerUser { get; set; }

		[DataMember, FileNetField(ID = "uwInputDeviceID", Name = "Input Device ID")]
		public string InputDeviceID { get; set; }

		[DataMember, FileNetField(ID = "WSTriggerStatus", Name = "Workflow Status")]
		public string WorkflowStatus { get; set; }

		[DataMember, FileNetField(ID = "WorkflowGUID", Name = "Workflow ID")]
		public Guid? WorkflowGUID { get; set; }

		public Underwriting2014Document()
			: base(ObjectStore.Underwriting, DocumentClass.Underwriting2014)
		{ }
	}
}