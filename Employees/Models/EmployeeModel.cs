using System;
using System.ComponentModel.DataAnnotations;


namespace Employees.Models
{
    public interface IEmployeeModel
    {
        public int Id { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Patronymic { get; }

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; }
        public string Position { get; }

        public EmployeeModel Copy();
    }

    public class EmployeeModel : IEmployeeModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Patronymic { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        public string Position { get; set; }

        public EmployeeModel Copy()
        {
            EmployeeModel copy = new EmployeeModel();

            copy.Id = this.Id;
            copy.FirstName = this.FirstName;
            copy.LastName = this.LastName;
            copy.Patronymic = this.Patronymic;
            copy.DateOfBirth = this.DateOfBirth;
            copy.Position = this.Position;

            return copy;
        }
    }
}
