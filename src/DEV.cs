using JTIS.Console;
using Spectre.Console;

namespace DEV; 

public static class TEST
{
    public static void test1()
    {
        AnsiConsole.Clear();
        ConsoleUtil.WriteAppTitle();
        AnsiConsole.WriteLine();
        for (int i = 0 ; i <9; i ++)
        {
            AnsiConsole.Write(new Rule($"TESTING SOMETHING {i:00000}").RuleStyle(new Style(Color.Blue,Color.LightYellow3)));

        }
        var clearFromLine = System.Console.GetCursorPosition().Top - 2;

        while (ConsoleUtil.Confirm("keep going?",true))
        {
            var curTop = System.Console.GetCursorPosition().Top;
            for (int i = 1; i < curTop - 1; i ++)
            {
                System.Console.SetCursorPosition(1,i);
                System.Console.Write(i);
            }
            clearFromLine +=1;            
            ConsoleUtil.FillLines(Color.LightCoral,System.Console.GetCursorPosition().Top - clearFromLine,1);
            AnsiConsole.Write($"{System.Console.GetCursorPosition().Top.ToString()}, {clearFromLine}");
        }

        AnsiConsole.Write(new Rule());
        ConsoleUtil.PressAnyKeyToContinue();
    }
}