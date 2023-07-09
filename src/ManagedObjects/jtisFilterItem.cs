namespace JTIS.Data 
{

    public class jtisFilterItem<T> where T:notnull, IComparable<T>
    {
        public T Value {get;private set;}
        public string? Description {get;private set;}
        public jtisFilterItem(T value, string? desc = null)
        {
            Value = value;
            Description = desc;
        }

        public override string ToString()
        {
            return Description == null ? Value.ToString() : $"{Value.ToString()} ({Description})";
        }
    }

    public class jtisFilterItems<T> where T:notnull, IComparable<T>
    {
        private List<jtisFilterItem<T>> _filterItems = new List<jtisFilterItem<T>>();

        public IReadOnlyList<jtisFilterItem<T>> Items
        {
            get {
                return _filterItems ;
            }
        }
        public void Clear()
        {
            _filterItems.Clear();
        }
        public void AddFilterItem(T filterValue, string? desc = null)
        {
            if (IsFiltered(filterValue)==false)
            {
                _filterItems.Add(new jtisFilterItem<T>(filterValue,desc));
            }
        }
        public void AddFilterItem(jtisFilterItem<T> item)
        {
            if (!IsFiltered(item.Value))
            {
                _filterItems.Add(item);
            }
        }
        public void AddFilterItems(IEnumerable<jtisFilterItem<T>> items)
        {
            foreach (var item in items)
            {
                AddFilterItem(item);
            }
        }
        public List<T> FilterValues{
            get{
                return _filterItems.Select(x=>x.Value).ToList();
            }
        }

        public bool IsFiltered(T filterValue)
        {            
            return _filterItems.Any(x=>x.Value.CompareTo(filterValue) == 0);
        }

        public int Count
        {
            get{
                return _filterItems.Count();
            }
        }
    }

}