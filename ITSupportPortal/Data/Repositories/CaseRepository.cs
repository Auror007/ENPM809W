using ITSupportPortal.Data.Enums;
using ITSupportPortal.Interfaces;
using ITSupportPortal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace ITSupportPortal.Data.Repositories
{
    public class CaseRepository : ICaseRepository
    {
        private readonly ApplicationDbContext _context;

        public CaseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        //Insert new case in database
        public Case CreateCase(string title, string description, EnumProduct e, string customer_id, string? employee_id)
        {
            var entry = new Case
            {
                CustomerID = customer_id,
                EmployeeID = employee_id,
                Title = title,
                State = CaseState.Open,
                CreationTime = DateTime.Now,
                Description = description,
                ProductCategory = e,
                UploadedFile = false,
                UploadedFileHash = null,
            };

            entry = _context.Case.Add(entry).Entity;
            _context.SaveChanges();
            return entry;
        }

        //Update File Data in database
        public Case UpdateFileData(string id, string hash)
        {
            var current_case = _context.Case.Where(c => c.Id == id).FirstOrDefault();
            current_case.UploadedFile = true;
            current_case.UploadedFileHash = hash;
            _context.SaveChanges();
            return current_case;
        }

        //Get all cases for a customer
        public List<Case> GetAllCases(string customerId)
        {
            return _context.Case
                           .Where(c => c.CustomerID == customerId)
                           .ToList();
        }

        //Get all cases for a agent
        public List<Case> GetAllAssignedCases(string employeeid)
        {
            return _context.Case
                           .Where(c => c.EmployeeID == employeeid && c.State == CaseState.Open)
                           .ToList();
        }

        //Get All Open cases.
        public IEnumerable<Case> GetAllOpenCases()
        {
            return _context.Case
                           .Where(c => c.State == CaseState.Open && c.EmployeeID.Length == 0)
                           .OrderBy(c => c.CreationTime)
                           .ToList();
        }

        //Assign the case to employee and check before
        public Case AssignEmployeeToCase(string case_id, string employee_id)
        {

            var current_case = _context.Case.FindAsync(case_id).Result;
            if (current_case.EmployeeID.Length == 0)
            {
                current_case.EmployeeID = employee_id;
                _context.SaveChanges();
                return current_case;
            }
            else return null;

        }


        //Change state to closed
        public Case CloseCase(string case_id)
        {
            var current_case = _context.Case.FindAsync(case_id).Result;
            if (current_case.State == CaseState.Open)
            {
                current_case.State = CaseState.Closed;
                _context.SaveChanges();
                return current_case;
            }
            else return null;
        }

        //Re-open case
        public Case ReOpenCase(string case_id)
        {
            var current_case = _context.Case.FindAsync(case_id).Result;
            if (current_case.State == CaseState.Closed)
            {
                current_case.State = CaseState.Open;
                current_case.EmployeeID = "";
                _context.SaveChanges();
                return current_case;
            }
            else return null;
        }

    }
}
