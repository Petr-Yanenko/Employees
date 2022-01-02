using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Employees.MVCModels;
using Employees.Models;
using Employees.Data;


namespace Employees.Controllers
{
    public class EmployeesController : Controller
    {
        private EmployeesModel _model = GlobalDataContext.GetInstance().Model;
        private AutoResetEvent _evnt = new AutoResetEvent(false);
        private IEmployeeModel[] _data;

        private NewDataHandler _newData;
        private ErrorHandler _error;


        public EmployeesController()
        {
            _newData = (newData, client) =>
            {
                if (client == this)
                {
                    _data = _model.CopyData(this);
                    _evnt.Set();
                }
            };

            _error = (error, client) =>
            {
                if (client == this)
                {
                    GlobalDataContext.GetInstance().HandleError(error);
                    _evnt.Set();
                }
            };

            _model.NewData += _newData;
            _model.Error += _error;
        }

        ~EmployeesController()
        {
            _model.NewData -= _newData;
            _model.Error -= _error;
        }

        // GET: Employees
        public IActionResult Index()
        {
            _data = _model.CopyData(this);

            return View(_data);
        }

        // GET: Employees/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = _model.CopyData(this)
                .FirstOrDefault(item => item.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Employees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,FirstName,LastName,Patronymic,DateOfBirth,Position")] EmployeeModel employee)
        {
            if (ModelState.IsValid)
            {
                _model.Append(employee, this);
                _evnt.WaitOne();

                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        // GET: Employees/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = _model.CopyData(this).FirstOrDefault(item => item.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _model.Delete(id, this);
            _evnt.WaitOne();

            return RedirectToAction(nameof(Index));
        }
    }
}
