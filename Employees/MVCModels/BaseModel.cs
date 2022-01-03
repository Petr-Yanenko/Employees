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
            if (NewData != null)
                NewData(true, client);
        }

        protected void OnError(ErrorCode code, Object client)
        {
            if (Error != null)
                Error(code, client);
        }
    }
}
