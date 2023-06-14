



namespace JiraCon
{
    public interface IMenuConsole
    {        
        public JTISConfig ActiveConfig {get; set;}
        public bool DoMenu();
        public bool ProcessKey(ConsoleKey key);        
    }


    public static class MenuManager
    {
        public static void Start(JTISConfig cfg)
        {
            while (DoMenu(new MenuMain(cfg)))
            {

            }
        }
        public static bool DoMenu(IMenuConsole menu)
        {
            if (menu.ActiveConfig != JTISConfigHelper.config )
            {
                menu.ActiveConfig = JTISConfigHelper.config;
            }
            while (menu.DoMenu())
            {

            }
            return false;
        }
    }


}