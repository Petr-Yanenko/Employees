using System;


namespace Employees.MVCModels
{
    public interface ICollectionModel<T> : IModel
    {
        public T[] CopyData();
        public void Append(T item);
        public void Delete(T item);
    }
}
