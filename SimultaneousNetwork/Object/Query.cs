using System;
using System.Collections.Generic;
using System.Text;

namespace SimultaneousNetwork.Object
{
    public class Query
    {
        public IEnumerable<INetObj> Objs => _objs.Values;

        private Func<INetObj, bool> _exp;
        private Dictionary<Guid, INetObj> _objs;
        private INetObj _sub;

        public Query(INetObj sub, Func<INetObj, bool> exp)
        {
            _sub = sub;
            _exp = exp;
            _objs = new Dictionary<Guid, INetObj>();
        }

        internal void ObjAdded(INetObj obj)
        {
            if(_exp(obj))
            {
                _objs[obj.Id] = obj;
                _sub.Tell(_sub, new Added(obj, Objs));
            }
        }

        internal void ObjRemoved(INetObj obj)
        {
            if (_exp(obj) && _objs.ContainsKey(obj.Id))
            {
                _objs.Remove(obj.Id);
                _sub.Tell(_sub, new Removed(obj, Objs));
            }
        }


        public class Added : QueryChange
        {
            public Added(INetObj change, IEnumerable<INetObj> objs) : base(change, objs)
            {
            }
        }

        public class Removed : QueryChange
        {
            public Removed(INetObj change, IEnumerable<INetObj> objs) : base(change, objs)
            {
            }
        }

        public class QueryChange
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
}
