using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Employees.Models;
using Employees.Data;


namespace Employees.MVCModels
{
    public class EmployeesModel<IEmpoyeeModel> : CollectionModel<IEmployeeModel>
    {
        static private EmployeesModel<IEmployeeModel> _instance = new EmployeesModel<IEmployeeModel>();

        private DataManager _manager = new DataManager(GlobalDataContext.GetInstance().Context);


        static public EmployeesModel<IEmployeeModel> GetInstance()
        {
            return _instance;
        }

        protected override async Task<List<IEmployeeModel>> FetchList()
        {
            return await _manager.GetEmployees();
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
