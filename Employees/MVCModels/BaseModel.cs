using System;
using Microsoft.EntityFrameworkCore;


namespace Employees.MVCModels
{
    public class BaseModel : IModel
    {
        public event NewDataHandler NewData;
        public event ErrorHandler Error;


        public virtual void LoadData()
        {
        }

        protected void OnChanged()
        {
            NewData(true);
        }

        protected void OnError(ErrorCode code)
        {
            Error(code);
        }
    }
}
