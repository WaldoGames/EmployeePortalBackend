using Serilog;

namespace EmployeePortalBackend.Logger
{
    public static class AccessLog
    {
        private static readonly Serilog.ILogger _userLogger = Log.ForContext("LogType", "UserDataAccess");
        private static readonly Serilog.ILogger _TicketLogger = Log.ForContext("LogType", "TicketDataAccess");
        public static void UserDataAccessed(string employeeId, string userId)
        {
            _userLogger.Information("Employee {EmployeeId} accessed {UserId}", employeeId, userId);
        }

        public static void SensitiveUserDataAccess(string employeeId, string userId)
        {
            _userLogger.Information("Employee {EmployeeId} accessed {UserId} sensitiveData", employeeId, userId);
        }
        public static void TicketDataAccess(string employeeId, string ticketId)
        {
            _TicketLogger.Information("Employee {EmployeeId} accessed ", employeeId, ticketId);
        }
        public static void TicketOverviewDataAccess(string employeeId)
        {
            _TicketLogger.Information("Employee {EmployeeId} accessed the ticket over view", employeeId);
        }

    }
}
