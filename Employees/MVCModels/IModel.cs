using System;


namespace Employees.MVCModels
{
    public enum ErrorCode
    {
        NoError = -1,
        UnknownError = 0,
    }


    public delegate void NewDataHandler(bool newData);
    public delegate void ErrorHandler(ErrorCode code);

    public interface IModel
    {
        public event NewDataHandler NewData;
        public event ErrorHandler Error;

        public void LoadData();
        public void Reset();
        public void Cancel();
    }
}
