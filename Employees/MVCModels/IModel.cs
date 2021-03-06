using System;


namespace Employees.MVCModels
{
    public enum ErrorCode
    {
        NoError = -1,
        UnknownError = 0,
        ResourceBusy = 1,
        ResourceMissing = 2,
    }


    public delegate void NewDataHandler(bool newData, Object client);
    public delegate void ErrorHandler(ErrorCode code, Object client);

    public interface IModel
    {
        public event NewDataHandler NewData;
        public event ErrorHandler Error;

        public void LoadData(Object client);
    }
}
