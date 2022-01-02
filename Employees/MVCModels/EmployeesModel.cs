using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Employees.Models;
using Employees.Data;


namespace Employees.MVCModels
{
    public class EmployeesModel : CollectionModel<IEmployeeModel>
    { 
        private DataManager _manager = new DataManager();


        protected override async Task<List<IEmployeeModel>> FetchList()
        {
            List<IEmployeeModel> employees = await _manager.GetEmployees();

            return employees;
        }

        protected override IEmployeeModel CopyItem(IEmployeeModel item)
        {
            return item.Copy();
        }

        protected override async Task<IEmployeeModel> InsertItem(IEmployeeModel item)
        {
            IEmployeeModel employee = await _manager.AddEmployee(item);

            return employee;
        }

        protected override async Task<bool> DeleteFromStore(IEmployeeModel item)
        {
            return await _manager.RemoveEmployee(item);
        }
    }
}
