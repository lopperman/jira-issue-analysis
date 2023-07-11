namespace JTIS
{
    public class JCustomField
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string[] Values { get; set; }

        public JCustomField(string id, string name, string[] values)
        {
            Id = id;
            Name = name;
            Values = values;
        }
    }
}