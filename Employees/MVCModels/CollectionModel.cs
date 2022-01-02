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
            _worker.EnQueueTask(() => FetchData(client));
        }

        public T[] CopyData(Object client)
        {
            return _copy.ToArray();
        }

        public void Append(T item, Object client)
        {
            _worker.EnQueueTask(() => AppendItem(item, client));
        }

        public void Delete(T item, Object client)
        {
            _worker.EnQueueTask(() => DeleteItem(item, client));
        }

        protected virtual void FetchData(Object client)
        {
            _list = FetchList().Result;
            SetCopy(client);

        }

        protected abstract Task<List<T>> FetchList();

        protected virtual void AppendItem(T item, Object client)
        {
            T employee = InsertItem(item).Result;
            if (employee != null)
            {
                _list.Add(employee);
                SetCopy(client);
            }
        }

        protected virtual void DeleteItem(T item, Object client)
        {
            if (DeleteFromStore(item).Result)
            {
                _list.Remove(item);
                SetCopy(client);
            }
        }

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
        
        protected abstract Task<bool> DeleteFromStore(T item);


        public delegate void ModelTask();

        public class ActiveObject
        {
            //protected List<ModelTask> _tasks = new List<ModelTask>();
            //private Object _lock = new Object();
            protected BlockingCollection<ModelTask> _tasks = new BlockingCollection<ModelTask>();
            private Thread _thread;
            private static AutoResetEvent _evnt = new AutoResetEvent(true);


            public ActiveObject()
            {
                _thread = new Thread(this.Start);
            }

            private void Start()
            {
                while(true)
                {
                    _evnt.WaitOne();
                    ModelTask task = DequeueTask();
                    if (task != null)
                    {
                        task();
                    }
                }
            }

            public void EnQueueTask(ModelTask task)
            {
                //lock (_lock)
                //{
                //    _tasks.Add(task);
                //    _evnt.Set();
                //}
                _tasks.Add(task);
            }

            ModelTask DequeueTask()
            {
                //lock (_lock)
                //{
                //    int count = _tasks.Count;
                //    if (count > 0)
                //    {
                //        ModelTask next = _tasks[0];
                //        _tasks.RemoveAt(0);

                //        return next;
                //    }

                //    return null;
                //}
                return _tasks.Take();
            }
        }
    }
}
