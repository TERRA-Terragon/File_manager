using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Runtime.CompilerServices;
using System.IO.Pipes;
using System.Drawing;


//namespace 
public struct pos_cursour
{
    public int x;
    public int y;
}
class Program
{
    pos_cursour csr= new pos_cursour() { x = 0, y = 0 };
    string[] files = null;
    public List<string> zapros = new List<string>();
    public string sobraniy = "";
    int file_now = 0;
    public int last_y_pos = 0;
    public int last_x_pos = 0;
    public string selct="";
    public int sellect=0;
    private bool press_enter;

    //methods
    void svitc_coursor_pos(pos_cursour a)
    {
        Console.SetCursorPosition(a.x,a.y);
    }
    void update()
    {foreach (string a in zapros){ sobraniy = string.Concat(zapros); } }
    static void print<T>(T a){Console.Write(a?.ToString()); }
    static void print<T>(T a,int b) {Console.ForegroundColor = ConsoleColor.Green; Console.Write(a?.ToString()); Console.ResetColor(); }

    void clear_cmd() {
        for (int y = last_y_pos; y >= 0; y--)
        {
            Console.SetCursorPosition(0, y);
            print(new string(' ', Console.WindowWidth - 1));   // ← Width, не Height!
        }
        last_y_pos = 0;
    }
    static void sleep(int seconds) {Thread.Sleep(seconds);}
    void get_folder(string path,int mode){
        try
        {
            if (mode == 0)
            {
                files = Directory.GetFiles(path);
            }
            else
                files = Directory.GetDirectories(path);
        }
        catch
        {
            // Если путь неверный, просто обнуляем массив, чтобы программа не падала
            files = new string[0];
            
        }
    }
    //
    static void Main(string[] args)
    {
        Program app = new Program();

        Console.CursorVisible = false;
        while (true)
        {
            for (int y = app.last_y_pos; y >= 0; y--)
            {
                Console.SetCursorPosition(0, y);
                print(new string(' ', Console.WindowWidth - 1));   // очистка снизу вверх
            }

            Console.SetCursorPosition(0, 0);
            print("Файловый менеджер С#:\n");
            foreach (string a in app.zapros)
            {
                print(""+a);
            }
            print(new string(' ', 5));//подтираем лишнее
            if (Console.KeyAvailable)// нажатие клавиши
            {
                var key = Console.ReadKey(true);
                if(key.Key == ConsoleKey.Enter)
                { 
                   for (int x = 0; x < app.files.Length; x++)
                    {
                        if (x == app.sellect)
                        {
                            app.selct = app.files[x];
                            app.zapros.Clear();
                            for(int i=0; i<app.selct.Length;i++)
                            {
                                app.zapros.Add(app.selct[i].ToString());
                            }
                        }
                    }
                    app.zapros.Add(@"\");
                    app.get_folder(app.sobraniy, 1);
                    app.update();
                    app.sellect= 0;
                    ///прим енение папки
                }
                if (key.Key == ConsoleKey.Escape)
                    break;
                if (key.Key == ConsoleKey.Backspace)
                {
                    // Удаляем последний символ при нажатии Backspace
                    if (app.zapros.Count > 0)
                        app.zapros.RemoveAt(app.zapros.Count - 1);
                    app.update();

                }
                else
                if (key.Modifiers == ConsoleModifiers.Control && key.Key == ConsoleKey.Backspace)
                    for(int i = app.selct.Length; i>0;i--)
                    {
                        if(app.selct[i].ToString() == @"\");
                            break;
                    }

                else if (key.KeyChar >= 32) // 32 и выше - это печатные символы (буквы, цифры, пробел)
                {
                    app.zapros.Add(key.KeyChar.ToString());
                    app.update();
                }
                else
                    if (key.Key == ConsoleKey.UpArrow && app.sellect > 0)
                {
                    app.sellect--;
                }
                else
                    if (key.Key == ConsoleKey.DownArrow && app.sellect < (app.files.Length-1))
                    app.sellect++;
            }
            if (!string.IsNullOrEmpty(app.sobraniy))
            {
                app.get_folder(app.sobraniy,1);
                if (app.files.Length != 0)
                    foreach (string a in app.files)
                    {
                        app.csr = new pos_cursour() { x = Console.GetCursorPosition().Left, y = Console.GetCursorPosition().Top };
                        if(Array.IndexOf(app.files, a) ==app.sellect)
                            print("|->" + a + "<--\n",1);
                        else
                            print("|-" + a + "\n");
                        Console.SetCursorPosition(app.csr.x, app.csr.y + 1);
                        if (app.last_y_pos < app.csr.y)
                            app.last_y_pos = app.csr.y;
                    }

            }
            else
                app.clear_cmd();
            sleep(20);
        }
    }

}
