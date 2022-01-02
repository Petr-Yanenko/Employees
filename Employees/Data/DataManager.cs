using System;
using Employees.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Threading.Tasks;


namespace Employees.Data
{
    public class DataManager
    {
        private EmployeeContext _context;

        public DataManager()
        {
            string connectionString = "Data Source=EmployeeContext.db";
            var contextOptions = new DbContextOptionsBuilder<EmployeeContext>()
                .UseSqlite(connectionString).Options;

            _context = new EmployeeContext(contextOptions);
        }

        public async Task<List<IEmployeeModel>> GetEmployees()
        {
            List<EmployeeModel> models = await _context.Employees.ToListAsync();

            List<IEmployeeModel> employees = new List<IEmployeeModel>();

            foreach (EmployeeModel data in models)
            {
                employees.Add(data);
            }

            return employees;
        }

        public async Task<IEmployeeModel> AddEmployee(IEmployeeModel newEmployee)
        {
            EmployeeModel data = new EmployeeModel();
            data.FirstName = newEmployee.FirstName;
            data.LastName = newEmployee.LastName;
            data.Patronymic = newEmployee.Patronymic;
            data.DateOfBirth = newEmployee.DateOfBirth;
            data.Position = newEmployee.Position;

            _context.Add<EmployeeModel>(data);
            await _context.SaveChangesAsync();

            if (data.Id == 0)
            {
                return null;
            }

            return data;
        }

        public async Task<bool> RemoveEmployee(IEmployeeModel employee)
        {
            if (employee != null)
            {
                EntityEntry removeRes = _context.Remove<EmployeeModel>((EmployeeModel)employee);
                await _context.SaveChangesAsync();

                return removeRes.State == EntityState.Detached;
            }

            return false;
        }
    }
}
