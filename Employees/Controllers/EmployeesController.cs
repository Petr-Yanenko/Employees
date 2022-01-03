using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
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
        private ErrorCode _errorCode = ErrorCode.NoError;

        private NewDataHandler _newData;
        private ErrorHandler _error;


        public EmployeesController()
        {
            _newData = (newData, client) =>
            {
                if (client == this)
                {
                    _evnt.Set();
                }
            };

            _error = (error, client) =>
            {
                if (client == this)
                {
                    GlobalDataContext.GetInstance().HandleError(error);
                    _errorCode = error;
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
            return View(_model.CopyData(this));
        }

        // GET: Employees/Details/5
        public IActionResult Details(int? id)
        {
            var employee = FirstOrDefault(id);
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
            var employee = FirstOrDefault(id);
            if (employee == null)
            {
                return NotFound();
            }

            _model.RequestDeleting((int)id, this);
            _evnt.WaitOne();

            return GetViewResult(() => View(employee));
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _model.Delete(id, this);
            _evnt.WaitOne();

            return GetViewResult(() => RedirectToAction(nameof(Index)));
        }

        // GET: Employees/Edit/5
        public IActionResult Edit(int? id)
        {
            IEmployeeModel employee = FirstOrDefault(id);
            if (employee == null)
            {
                return NotFound();
            }

            long timeStamp = _model.RequestUpdating(employee, this);
            _evnt.WaitOne();

            return GetViewResult(() => View(new EmployeeDecorator(employee, timeStamp)));
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, long timeStamp, [Bind("Id,FirstName,LastName,Patronymic,DateOfBirth,Position")] EmployeeModel employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _model.Update(employee, timeStamp, this);
                _evnt.WaitOne();
                                    
                return RedirectToAction(nameof(Index));
            }
            return View(new EmployeeDecorator(employee, timeStamp));
        }

        private IEmployeeModel FirstOrDefault(int? id)
        {
            if (id == null)
            {
                return null;
            }

            return _model.CopyData(this).FirstOrDefault(item => item.Id == id);
        }

        delegate IActionResult GetSuccessViewResult();

        private IActionResult GetViewResult(GetSuccessViewResult success)
        {
            if (_errorCode == ErrorCode.NoError)
            {
                return success();
            }
            else if (_errorCode == ErrorCode.ResourceMissing)
            {
                return Conflict();
            }
            else if (_errorCode == ErrorCode.ResourceBusy)
            {
                return Conflict();
            }
            else
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
