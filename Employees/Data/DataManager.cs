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
            return await DoOperationOnContext(employee, EntityState.Deleted, item =>
            {
                return _context.Remove(item);
            });
        }

        public async Task<bool> UpdateEmployee(IEmployeeModel employee, IEmployeeModel updated)
        {
            IEmployeeModel old = employee.Copy();
            ChangeEmployee(employee, updated);

            bool updateRes = await DoOperationOnContext(employee, EntityState.Modified, item =>
            {
                return _context.Update(item);
            });

            if (!updateRes)
            {
                ChangeEmployee(employee, old);
            }

            return updateRes;
        }

        delegate EntityEntry ContextOperation(IEmployeeModel item);

        private async Task<bool> DoOperationOnContext(
            IEmployeeModel item,
            EntityState state,
            ContextOperation operation)
        {
            if (item != null)
            {
                EntityEntry updateRes = operation(item);
                if (updateRes.State == state)
                {
                    int updatedNum = await _context.SaveChangesAsync();

                    return updatedNum > 0;
                }
            }

            return false;
        }

        private void ChangeEmployee(IEmployeeModel employee, IEmployeeModel update)
        {
            EmployeeModel changed = (EmployeeModel)employee;

            if (changed != null)
            {
                changed.FirstName = update.FirstName;
                changed.LastName = update.LastName;
                changed.Patronymic = update.Patronymic;
                changed.DateOfBirth = update.DateOfBirth;
                changed.Position = update.Position;
            }
        }
    }
}
