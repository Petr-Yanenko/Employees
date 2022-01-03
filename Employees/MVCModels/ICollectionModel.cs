using System;


namespace Employees.MVCModels
{
    public interface ICollectionModel<T> : IModel
    {
        public T[] CopyData(Object client);
        public void Append(T item, Object client);
        public void RequestDeleting(int id, Object client);
        public void Delete(int id, Object client);
        public long RequestUpdating(T item, Object client);
        public void Update(T item, long timeStamp, Object client);
    }
}
