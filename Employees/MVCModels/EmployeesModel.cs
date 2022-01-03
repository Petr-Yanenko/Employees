using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Employees;
using Employees.Models;
using Employees.Data;


namespace Employees.MVCModels
{
    public class EmployeesModel : CollectionModel<IEmployeeModel>
    { 
        private DataManager _manager = new DataManager();

        private List<EmployeeDecorator> _updated = new List<EmployeeDecorator>();


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

        protected override async Task<IEmployeeModel> DeleteFromStore(int id)
        {
            EmployeeDecorator found = FindUpdatedEmployee(id);
            if (found == null)
            {
                IEmployeeModel employee = _list.Find(e => CheckID(id, e));
                bool deleted = await _manager.RemoveEmployee(employee);

                return deleted ? employee : null;
            }

            return null;
        }

        protected override void RequestUpdating(IEmployeeModel item, long timeStamp, Object client)
        {
            EmployeeDecorator updated = FindUpdatedEmployee(item.Id, timeStamp);
            if (updated == null)
            {
                IEmployeeModel found = _list.Find(e => CheckID(item.Id, e));
                if (found != null)
                {
                    _updated.Add(new EmployeeDecorator(found, timeStamp));
                    OnChanged(client);

                    return;
                }

                OnError(ErrorCode.ResourceMissing, client);

                return;
            }

            OnError(ErrorCode.ResourceBusy, client);
        }

        protected override async void UpdateItem(IEmployeeModel item, long timeStamp, Object client)
        {
            EmployeeDecorator found = FindUpdatedEmployee(item.Id, timeStamp);
            if (found != null && found.TimeStamp == timeStamp)
            {
                _updated.Remove(found);
                IEmployeeModel employee = _list.Find(employee => CheckID(item.Id, employee));
                bool updated = await _manager.UpdateEmployee(employee, item);

                if (updated)
                {
                    SetCopy(client);
                }
                else
                {
                    OnError(ErrorCode.UnknownError, client);
                }

                return;
            }

            OnError(ErrorCode.ResourceBusy, client);
        }

        protected override IEmployeeModel FindUpdatedItem(int id)
        {
            return FindUpdatedEmployee(id);
        }

        private bool CheckTimeStamp(long timeStamp, EmployeeDecorator item)
        {
            return timeStamp - item.TimeStamp > GlobalDataContext.kUpdateTimeout;
        }

        private bool CheckID(int id, IEmployeeModel item)
        {
            return id == item.Id;
        }

        private EmployeeDecorator FindUpdatedEmployee(int id)
        {
            return FindUpdatedEmployee(id, DateTimeOffset.Now.ToUnixTimeMilliseconds());
        }
        private EmployeeDecorator FindUpdatedEmployee(int id, long timeStamp)
        {
            _updated.RemoveAll(item => CheckTimeStamp(timeStamp, item));
            EmployeeDecorator found = _updated.Find(e => CheckID(id, e));

            return found;
        }
    }
}
