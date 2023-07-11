using System.Diagnostics;

namespace JTIS.Extensions
{
    public abstract class Property<P>
    {
        public readonly string Name;
        public readonly Type Type;

        public int Index { get; internal set; }

        protected Property(string name, Type type)
        {
            Name = name;
            Type = type;
        }
    }

    public class Property<PInstance, PProperty> : Property<PInstance> where PInstance : ExtendableObject<PInstance>
    {
        public Property(string name) : base(name, typeof(PProperty))
        {
        }

        public PProperty Get(PInstance instance)
        {
            return instance.GetProperty(this);
        }

        public void Set(PInstance instance, PProperty value)
        {
            instance.SetProperty(this, value);
        }
    }

     public class ExtendableObject<T> where T : ExtendableObject<T>
    {
        private readonly object[] properties;

        public ExtendableObject()
        {
            properties = new object[Properties.Count];
        }

        internal TProperty GetProperty<TProperty>(Property<T, TProperty> property)
        {
            var index = property.Index;
            Debug.Assert(properties.Length >= index, $"Property {property.Name} should be registered !");
            var value = properties[index];
            return (TProperty)value;
        }

        internal void SetProperty<TProperty>(Property<T, TProperty> property, TProperty value)
        {
            var index = property.Index;
            Debug.Assert(properties.Length >= index, $"Property {property.Name} should be registered !");
            properties[index] = value;
        }

        private static readonly List<Property<T>> Properties = new List<Property<T>>();
        public static void RegisterProperty(Property<T> property)
        {
            var name = property.Name;
            Debug.Assert(Properties.All(p => p.Name != name));
            property.Index = Properties.Count;
            Properties.Add(property);
        }
    }    

}


