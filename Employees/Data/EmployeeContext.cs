using System;
using Microsoft.EntityFrameworkCore;


namespace Employees.Data
{
    public class EmployeeContext : DbContext
    {
        public EmployeeContext(DbContextOptions<EmployeeContext> options) : base(options)
        {
        }

        public DbSet<Employees.Models.EmployeeModel> Employees { get; set; }
    }
}
