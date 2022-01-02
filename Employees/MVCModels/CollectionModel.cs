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
        protected ImmutableList<T> _copy;
        protected List<T> _list;

        protected ActiveObject _worker = new ActiveObject();


        public CollectionModel()
        {
            _list = new List<T>();
            _copy = _list.ToImmutableList();

            LoadData();
        }

        public override void LoadData()
        {
            base.LoadData();
            _worker.EnQueueTask(() => FetchData());
        }

        public T[] CopyData()
        {
            return _copy.ToArray();
        }

        public void Append(T item)
        {
            _worker.EnQueueTask(() => AppendItem(item));
        }

        public void Delete(T item)
        {
            _worker.EnQueueTask(() => DeleteItem(item));
        }

        protected virtual void FetchData()
        {
            _list = FetchList().Result;
            SetCopy();

        }

        protected abstract Task<List<T>> FetchList();

        protected virtual void AppendItem(T item)
        {
            T employee = InsertItem(item).Result;
            if (employee != null)
            {
                _list.Add(employee);
                SetCopy();
            }
        }

        protected virtual void DeleteItem(T item)
        {
            if (DeleteFromStore(item).Result)
            {
                _list.Remove(item);
                SetCopy();
            }
        }

        protected ImmutableList<T> CopyList()
        {
            List<T> copy = new List<T>();

            foreach (T data in _list)
            {
                copy.Add(CopyItem(data));
            }

            return copy.ToImmutableList();
        }

        protected abstract T CopyItem(T item);

        protected void SetCopy()
        {
            _copy = CopyList();
            OnChanged();
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
