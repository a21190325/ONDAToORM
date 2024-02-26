using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Dtos.Errors
{
	public static class ErrorCodes
	{
		public const string REQUIRED_FIELD_IS_EMPTY = "REQUIRED_FIELD_IS_EMPTY";
		public const string UNAUTHORIZED_ACTION_FOR_CALLING_USER = "UNAUTHORIZED_ACTION_FOR_CALLING_USER";
		public const string APPLICATION_ERROR = "APPLICATION_ERROR";
		public const string ERROR_PROCESSING_SQL_ON_DB_SERVER = "ERROR_PROCESSING_SQL_ON_DB_SERVER";
		public const string ERROR_EXECUTING_SCAFFOLDING_COMMAND = "ERROR_EXECUTING_SCAFFOLDING_COMMAND";
	}
}
