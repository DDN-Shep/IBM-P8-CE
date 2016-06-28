using System.Configuration;

namespace Validus.FileNet
{
	public class Config
	{
        public static string ServiceURL => ConfigurationManager.AppSettings["FileNetServiceURL"];

        public static string KerberosSPN => ConfigurationManager.AppSettings["FileNetKerberosSPN"];

        public static string DomainName => ConfigurationManager.AppSettings["FileNetDomain"];

        public static string LogonAsAdmin => ConfigurationManager.AppSettings["FileNetLogonAsAdmin"];

        public static string LogonAsAppPool => ConfigurationManager.AppSettings["FileNetLogonAsAppPool"];

        public static string AdminUsername => ConfigurationManager.AppSettings["FileNetAdminUsername"];

        public static string AdminPassword => ConfigurationManager.AppSettings["FileNetAdminPassword"];
    }
}