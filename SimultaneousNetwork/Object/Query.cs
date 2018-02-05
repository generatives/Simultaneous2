using System;
using System.Collections.Generic;
using System.Text;

namespace SimultaneousNetwork.Object
{
    public class Query
    {
        public event EventHandler<QueryChange> OnObjAdded;
        public event EventHandler<QueryChange> OnObjRemoved;
        public IEnumerable<INetObj> Objs => _objs.Values;

        private Func<INetObj, bool> _exp;
        private Dictionary<Guid, INetObj> _objs;

        public Query(Func<INetObj, bool> exp)
        {
            _exp = exp;
            _objs = new Dictionary<Guid, INetObj>();
        }

        internal void ObjAdded(INetObj obj)
        {
            if(_exp(obj))
            {
                _objs[obj.Id] = obj;
                OnObjAdded?.Invoke(this, new QueryChange(obj, Objs));
            }
        }

        internal void ObjRemoved(INetObj obj)
        {
            if (_exp(obj) && _objs.ContainsKey(obj.Id))
            {
                _objs.Remove(obj.Id);
                OnObjRemoved?.Invoke(this, new QueryChange(obj, Objs));
            }
        }
    }

    public class QueryChange : EventArgs
    {
        public IEnumerable<INetObj> Objs { get; private set; }
        public INetObj Change { get; private set; }

        public QueryChange(INetObj change, IEnumerable<INetObj> objs)
        {
            Change = change;
            Objs = objs;
        }
    }
}
