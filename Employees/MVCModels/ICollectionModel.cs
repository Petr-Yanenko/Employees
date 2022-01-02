using System;


namespace Employees.MVCModels
{
    public interface ICollectionModel<T> : IModel
    {
        public T[] CopyData(Object client);
        public void Append(T item, Object client);
        public void Delete(T item, Object client);
    }
}
