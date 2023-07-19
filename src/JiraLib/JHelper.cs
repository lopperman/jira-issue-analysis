namespace JTIS
{
    public static class JHelper
    {
        public static T GetValue<T>(String value)
        {
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Unable to convert '{0}' to Type: {1}\r\n\r\n{2}", value, typeof(T).FullName,ex.Message));
            }
        }

    }
}