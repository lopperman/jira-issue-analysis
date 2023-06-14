



namespace JiraCon
{
    public interface IMenuConsole
    {        
        public bool DoMenu();
        public void BuildMenu();
        public bool ProcessKey(ConsoleKey key);        
    }


    public static class MenuManager
    {
        public static void Start(JTISConfig cfg)
        {
            MenuMain mnu = new MenuMain(cfg);
            while(mnu.DoMenu())
            {

            }
        }
    }

}