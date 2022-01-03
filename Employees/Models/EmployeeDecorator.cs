using System;
using System.ComponentModel.DataAnnotations;


namespace Employees.Models
{
    public class EmployeeDecorator : IEmployeeModel
    {
        private IEmployeeModel _decorated;
        private long _timeStamp;

        public int Id { get { return _decorated.Id; } }
        public string FirstName { get { return _decorated.FirstName; } }
        public string LastName { get { return _decorated.LastName; } }
        public string Patronymic { get { return _decorated.Patronymic; } }

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get { return _decorated.DateOfBirth; } }
        public string Position { get { return _decorated.Position; } }

        public long TimeStamp { get { return _timeStamp; } }


        public EmployeeDecorator(IEmployeeModel decorated, long timeStamp)
        {
            _decorated = decorated;
            _timeStamp = timeStamp;
        }

        public IEmployeeModel Copy()
        {
            return new EmployeeDecorator(_decorated.Copy(), _timeStamp);
        }
    }
}
