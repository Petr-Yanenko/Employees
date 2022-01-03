using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Employees.MVCModels
{
    public abstract class CollectionModel<T> : BaseModel, ICollectionModel<T>
    {
        protected BlockingCollection<T> _copy = new BlockingCollection<T>();
        protected List<T> _list = new List<T>();

        protected ActiveObject _worker = new ActiveObject();


        public CollectionModel()
        {
            LoadData(null);
        }

        public override void LoadData(Object client)
        {
            base.LoadData(client);
            _worker.EnqueueTask(() => FetchData(client));
        }

        public T[] CopyData(Object client)
        {
            return _copy.ToArray();
        }

        public void Append(T item, Object client)
        {
            _worker.EnqueueTask(() => AppendItem(item, client));
        }

        public void RequestDeleting(int id, Object client)
        {
            _worker.EnqueueTask(() => RequestDeletingItem(id, client));
        }

        public void Delete(int id, Object client)
        {
            _worker.EnqueueTask(() => DeleteItem(id, client));
        }

        public long RequestUpdating(T item, Object client)
        {
            long milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            _worker.EnqueueTask(() => RequestUpdating(item, milliseconds, client));

            return milliseconds;
        }

        public void Update(T item, long timeStamp, Object client)
        {
            _worker.EnqueueTask(() => UpdateItem(item, timeStamp, client));
        }

        protected virtual async void FetchData(Object client)
        {
            Task<List<T>> dataTask = FetchList();

            _list = await dataTask;

            if (_list == null)
            {
                _list = new List<T>();
            }

            SetCopy(client);

        }

        protected abstract Task<List<T>> FetchList();

        protected virtual async void AppendItem(T item, Object client)
        {
            Task<T> employeeTask = InsertItem(item);
            T employee = await employeeTask;

            if (employee != null)
            {
                _list.Add(employee);
                SetCopy(client);
            }
            else
            {
                OnError(ErrorCode.UnknownError, client);
            }
        }

        protected virtual void RequestDeletingItem(int id, Object client)
        {
            T found = FindUpdatedItem(id);

            if (found != null)
            {
                OnError(ErrorCode.ResourceBusy, client);
            }
            else
            {
                OnChanged(client);
            }
        }

        protected virtual async void DeleteItem(int id, Object client)
        {
            Task<T> deletedTask = DeleteFromStore(id);
            T deleted = await deletedTask;

            if (deleted != null)
            {
                _list.Remove(deleted);
                SetCopy(client);
            }
            else
            {
                OnError(ErrorCode.ResourceMissing, client);
            }
        }

        protected abstract void RequestUpdating(T item, long timeStamp, Object client);

        protected abstract void UpdateItem(T item, long timeStamp, Object client);

        protected BlockingCollection<T> CopyList()
        {
            BlockingCollection<T> copy = new BlockingCollection<T>();

            foreach (T data in _list)
            {
                copy.Add(CopyItem(data));
            }
            copy.CompleteAdding();

            return copy;
        }

        protected abstract T CopyItem(T item);

        protected void SetCopy(Object client)
        {
            _copy = CopyList();
            OnChanged(client);
        }

        protected abstract Task<T> InsertItem(T item);
        
        protected abstract Task<T> DeleteFromStore(int id);

        protected abstract T FindUpdatedItem(int id);


        public delegate void ModelTask();

        public class ActiveObject
        {
            protected BlockingCollection<ModelTask> _tasks = new BlockingCollection<ModelTask>();
            private Thread _thread;


            public ActiveObject()
            {
                _thread = new Thread(Start);
                try
                {
                    _thread.Start();
                }
                catch(Exception ex)
                {
                    GlobalDataContext.GetInstance().HandleException(ex);
                    throw ex;
                }
            }

            private void Start()
            {
                while(true)
                {
                    ModelTask task = DequeueTask();
                    if (task != null)
                    {
                        task();
                    }
                }
            }

            public void EnqueueTask(ModelTask task)
            {
                _tasks.Add(task);
            }

            ModelTask DequeueTask()
            {
                return _tasks.Take();
            }
        }
    }
}
