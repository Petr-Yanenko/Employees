using System;
using Microsoft.EntityFrameworkCore;


namespace Employees.MVCModels
{
    public class BaseModel : IModel
    {
        public event NewDataHandler NewData;
        public event ErrorHandler Error;


        public virtual void LoadData(Object client)
        {
        }

        protected void OnChanged(Object client)
        {
            NewData(true, client);
        }

        protected void OnError(ErrorCode code, Object client)
        {
            Error(code, client);
        }
    }
}
