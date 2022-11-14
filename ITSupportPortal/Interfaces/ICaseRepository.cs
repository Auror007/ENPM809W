using ITSupportPortal.Data.Enums;
using ITSupportPortal.Models;

namespace ITSupportPortal.Interfaces
{
    public interface ICaseRepository
    {
        Case AssignEmployeeToCase(string case_id, string employee_id);
        Case CloseCase(string case_id);
        Case CreateCase(string title, string description, EnumProduct e, string customer_id, string? employee_id);
        List<Case> GetAllAssignedCases(string employeeid);
        List<Case> GetAllCases(string customerId);
        IEnumerable<Case> GetAllOpenCases();
        Case ReOpenCase(string case_id);
        Case UpdateFileData(string id, string hash);
    }
}