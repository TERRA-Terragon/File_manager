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
    string LOGO = @"00000000  0         
000  0000 0         
000   000 0         
000  0000 0         
0000000   0         
          0         
00000000000000000000
          0         
          0  0000000
          0  000    
          0  0000000
          0  000    
          0  000    ";
    pos_cursour csr= new pos_cursour() { x = 0, y = 0 };
    string[] files = null; //перечень всех файлов в дирректории ,которая открыта
    public List<string> zapros = new List<string>(); //список по букве
    public string sobraniy = ""; //результат  финальный после сборки по частям(путь)
    public int last_y_pos = 0; //получение последней координаты по y(наиболее большой)
    public int last_x_pos = 0; //получение последней координаты по x(наиболее большой)
    public string selct=""; //текущая выбранная дирректория
    public int sellect=0; //выбранная

    //methods
    //добавление пути
    void add_to_path(Program app)
    {
        for (int x = 0; x < app.files.Length; x++)
        {
            if (x == app.sellect)
            {
                app.selct = app.files[x];
                app.zapros.Clear();
                for (int i = 0; i < app.selct.Length; i++)
                {
                    app.zapros.Add(app.selct[i].ToString());
                }
            }
        }
    }
    //добавление пути
    void add_to_path(Program app,string b)
    {
         app.zapros.Clear();
         for (int i = 0; i < b.Length; i++)
         {
            app.zapros.Add(app.selct[i].ToString());
         }
    }
    //возврат на 1 дирректорию вниз
    void back_to_path(Program app)
    {
        for (int i = app.zapros.Count()-1; i > 0; i--)
        {
            if (app.zapros[i].ToString() != @"\")
                app.zapros.RemoveAt(app.zapros.Count - 1);
            else
            if(app.zapros[app.zapros.Count() - 1].ToString() != @"\")
                app.zapros.RemoveAt(app.zapros.Count - 1);
            else
                break;
        }
        app.get_folder(app.sobraniy, 1);
        app.update();
    }
    //вывод в определённых координатах
    void svitc_coursor_pos(pos_cursour a)
    {
        Console.SetCursorPosition(a.x,a.y);
    }
    //обновление запроса
    void update()
    {foreach (string a in zapros){ sobraniy = string.Concat(zapros); } }
    //шаблон вывода текста
    static void print<T>(T a){Console.Write(a?.ToString()); }
    //шаблон вывода цветного текста
    static void print<T>(T a,int b) {Console.ForegroundColor = ConsoleColor.Green; Console.Write(a?.ToString()); Console.ResetColor(); }
    //очистка экран
    void clear_cmd() {
        for (int y = last_y_pos; y >= 0; y--)
        {
            Console.SetCursorPosition(0, y);
            print(new string(' ', Console.WindowWidth - 1));   // ← Width, не Height!
        }

    }
    //ожидание потока
    static void sleep(int seconds) {Thread.Sleep(seconds);}
    //получение списка папок
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
    static void Main(string[] args)
    {
        Program app = new Program();
        Console.SetWindowSize(200,60); //размер консоли
        Console.CursorVisible = false;
        //тело основного цикла
        while (true)
        {
            //очистка экрана 9 полностью)
            for (int y = app.last_y_pos; y >= 0; y--)
            {
                Console.SetCursorPosition(0, y);
                print(new string(' ', Console.WindowWidth - 1));
            }
            //начало вывода 
            Console.SetCursorPosition(0, 0);
            print(app.LOGO + "\n");
            foreach (string a in app.zapros) //вывод пути
            {
                print(""+a ,1);
            }
            ///////////////////////////////////////////////КНОПОЧКИ
            if (Console.KeyAvailable)// нажатие клавиши
            {
                var key = Console.ReadKey(true);
                if(key.Key == ConsoleKey.Enter)
                { 
                    //добавление по индексу

                    app.zapros.Add(@"\");
                    app.add_to_path(app);
                    app.get_folder(app.sobraniy, 1);
                    app.update();
                    app.sellect= 0;
                    ///прим енение папки
                }
                if (key.Key == ConsoleKey.Escape)
                    break;
                if (key.Modifiers == ConsoleModifiers.Control && key.Key == ConsoleKey.Backspace)
                    app.back_to_path(app);
                else if (key.Key == ConsoleKey.Backspace)
                {
                    // Удаляем последний символ при нажатии Backspace
                    if (app.zapros.Count > 0)
                        app.zapros.RemoveAt(app.zapros.Count - 1);
                    app.update();

                }

                else if (key.KeyChar >= 32) // 32 и выше - это печатные символы (буквы, цифры, пробел)
                {
                    app.zapros.Add(key.KeyChar.ToString());
                    app.update();
                }
                else
                    if (app.files != null && key.Key == ConsoleKey.UpArrow && app.sellect > 0) //стрелочка вниз
                {
                    app.sellect--;
                }
                else
                    if (app.files != null && key.Key == ConsoleKey.DownArrow && app.sellect < (app.files.Length - 1)) //стрелочка вверх
                    app.sellect++;
            }
            ///////////////////////////////////////////////ВЫВОД ВСЕХ ДИРРЕКТОРИЙ
            if (!string.IsNullOrEmpty(app.sobraniy))
            {
                app.get_folder(app.sobraniy,1);
                if (app.files.Length != 0)
                    foreach (string a in app.files)
                    {
                        app.csr = new pos_cursour() { x = Console.GetCursorPosition().Left, y = Console.GetCursorPosition().Top };
                        if(Array.IndexOf(app.files, a) ==app.sellect)
                            print("  -|=>" + a + "<=\n",1);
                        else
                            print("   |-" + a + "\n");
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
