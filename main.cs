using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Serialization;


/*namespace  System
*/public struct pos_cursour
{
    public int x;
    public int y;
}
class Program
{
    private List<string> dir = new List<string>{@"1111          "
,@"1  1111111111 "
,@"1111111111111 "
,@"1           1 "
,@"1111111111111 "};
    private string LOGO = @"00000000  0         
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
    public class FolderPin
    {
        private string name;
        private string path;
        public string give_path() { return path; }
        public string give_name() { return name; }
        public FolderPin(string path_,string name_) { name = name_; path = path_; }
    }
    private List<FolderPin> pinFolders_list= new List<FolderPin>(); //закреплёныне папки
    private bool pressed_key;
    pos_cursour csr= new pos_cursour() { x = 0, y = 0 };
    private string[] files = null; //перечень всех файлов в дирректории ,которая открыта
    private List<string> zapros = new List<string>(); //список по букве
    private string sobraniy = ""; //результат  финальный после сборки по частям(путь)
    private int last_y_pos = 0; //получение последней координаты по y(наиболее большой)
    private int last_x_pos = 0; //получение последней координаты по x(наиболее большой)
    private string selct=""; //текущая выбранная дирректория
    private int sellect=0; //выбранная
    private DateTime lastRamUpdate = DateTime.MinValue; //время для регистрации вызовов
    private bool mode_search = false; //рыжим просмотра папком и файлов true -файлы , else - папки

    ////
    /////
    /////                             METHODS
    /////  

    void add_to_path(Program app)    //добавление пути
    {
        for (int x = 0; x < app.files.Length; x++)
        {
            if (x == app.sellect && app.sellect>=0)
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

    void add_to_path(Program app,string b)    //добавление пути
    {
         app.zapros.Clear();
         for (int i = 0; i < b.Length; i++)
         {
            app.zapros.Add(app.selct[i].ToString());
         }
    }

    void back_to_path(Program app)    //воаврат на 1 дирректорию вниз
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

    }

    void switch_coursor_pos(pos_cursour a,pos_cursour b,string c)    //вывод в определённых координатах
    {
        Console.SetCursorPosition(a.x,a.y);
        print(c);
        Console.SetCursorPosition(b.x, b.y);
    }
    void switch_coursor_pos_for(pos_cursour a, pos_cursour b, List<string> c,int AA)    //вывод в определённых координатах
    {
        Console.SetCursorPosition(a.x+15, a.y-2);
        print("Pins Folders:"+"["+pinFolders_list.Count()+ "/7]",1);c:
        short dlina=1; //сколько элементов(вниз)
        if(pinFolders_list.Count >= 1) //вывод всех закреплённых папок
            foreach(FolderPin Q in pinFolders_list)
            {
                Console.SetCursorPosition(a.x + 15, a.y+ dlina);
                if(sellect != 1 && sellect !=2 && sellect != 3 && sellect != 4 && sellect != 5 && sellect != 6 && sellect != 7 && Math.Abs(sellect)== dlina)
                    print("=>"+Q.give_name()+"<=", 1);
                else
                    print(Q.give_name(), 3);
                dlina++;
            }

        for (int i = 0; i < AA; i++)
        {
            Console.SetCursorPosition(a.x+1, a.y-2+i+1);
            print(c[i],1);
        }
        Console.SetCursorPosition(b.x, b.y);
    }
    void switch_coursor_pos(pos_cursour a, pos_cursour b, string c,int g)
    {
        Console.SetCursorPosition(a.x, a.y);
        print(c,g);
        Console.SetCursorPosition(b.x, b.y);
    }

    void update()    //обновление запроса
    { foreach (string a in zapros){ sobraniy = string.Concat(zapros); } }    //шаблон вывода текста
    static void print<T>(T a){Console.Write(a?.ToString()); }    //шаблон вывода цветного текста
    static void print<T>(T a,int b) { if (b == 1) Console.ForegroundColor = ConsoleColor.Green; else if (b ==2) Console.ForegroundColor = ConsoleColor.Red; else if (b == 3) Console.ForegroundColor = ConsoleColor.Blue; Console.Write(a?.ToString()); Console.ResetColor(); }
    //очистка экран
    void clear_cmd(Program app) {
        for (int y = app.last_y_pos+1; y >= 0; y--)
        {
            Console.SetCursorPosition(0, y);
            print(new string(' ', Console.WindowWidth));   // ← Width, не Height!
        }
        app.last_y_pos = 0;
    }
    void clear_cmd_all_screen(Program app)
    {
        for (int y = Console.WindowHeight-1; y >= 0; y--)
        {
            Console.SetCursorPosition(0, y);
            print(new string(' ', Console.WindowWidth));   // ← Width, не Height!
        }
        app.last_y_pos = 0;
    }

    static void sleep(int seconds) {Thread.Sleep(seconds); }    //ожидание потока
    void get_folder(string path,int mode){    //получение списка папок

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
            if (app.pressed_key == false)
            {
                //очистка экрана 9 полностью)
                for (int y = app.last_y_pos; y >= 0; y--)
                {
                    Console.SetCursorPosition(0, y);
                    print(new string(' ', Console.WindowWidth - 1));
                }
                //начало вывода 
                Console.SetCursorPosition(0, 0);
                print(app.LOGO + "\n", 2);
                app.switch_coursor_pos(new pos_cursour() { x = 22, y = 11 }, new pos_cursour() { x = Console.GetCursorPosition().Left, y = Console.GetCursorPosition().Top }, $"Process used RAM:   {Process.GetCurrentProcess().WorkingSet64 / 1024.0 / 1024.0:F2} МБ");

                app.switch_coursor_pos_for(new pos_cursour() { x = 23, y = 2 }, new pos_cursour() { x = Console.GetCursorPosition().Left, y = Console.GetCursorPosition().Top }, app.dir, app.dir.Count());



                foreach (string a in app.zapros) //вывод пути
                {
                    print("" + a, 1);
                }
                ///////////////////////////////////////////////КНОПОЧКИ
                if (Console.KeyAvailable)// нажатие клавиши
                {
                    app.pressed_key = true;
                    var key = Console.ReadKey(true);
                    //print($"Key={key.Key} Char={(int)key.KeyChar} Mod={key.Modifiers}",2);

                    if (key.Key == ConsoleKey.Enter)                        ///прим енение папки
                    {
                        if (app.sellect >= 0)
                        {
                            app.zapros.Add(@"\");
                            app.add_to_path(app);
                            app.get_folder(app.sobraniy, Convert.ToInt16(app.mode_search));
                        }
                        else
                        {
                            app.zapros.Clear();
                            for (int i = 0; i < app.pinFolders_list.Count; i++)
                            {
                                app.zapros.Add(app.pinFolders_list[i].give_path());
                            }
                        }
                        app.sellect = 0;

                    }
                    else
                        if (key.Key == ConsoleKey.Tab)                        ///прим енение папки
                    {
                        app.mode_search= !app.mode_search;
                       // print(Convert.ToInt16( app.mode_search));
                        //app.get_folder(app.sobraniy, Convert.ToInt16(app.mode_search));

                    }
                    if (key.Key == ConsoleKey.Escape)                            //выход
                        break;
                    if (key.Modifiers == ConsoleModifiers.Control && key.Key == ConsoleKey.Backspace)
                        app.back_to_path(app);
                    else if (key.Key == ConsoleKey.Backspace) //delet text
                    {
                        // Удаляем последний символ при нажатии Backspace
                        if (app.zapros.Count > 0)
                            app.zapros.RemoveAt(app.zapros.Count - 1);
                        print(new string(' ', Console.WindowWidth - 1));
                        app.clear_cmd(app);


                    }
                    else if (app.files != null && key.KeyChar == '`') // пробел для ЗАКРЕПЛЕНИЕ папок         ///////////////////////////
                    {
                        if (!app.pinFolders_list.Any(p => p.give_path() == app.sobraniy) && app.pinFolders_list.Count < 8) {
                            app.pinFolders_list.Add(new FolderPin(app.sobraniy, app.sobraniy)); }
                        else
                        {
                            app.pinFolders_list.RemoveAll(p => p.give_path() == app.sobraniy);
                            print("\nYou can't pin this fodler! Folder was unbound.", 2);
                            sleep(1000);
                        }
                    }
                else if (key.KeyChar >= 32 && key.KeyChar != '`') // 32 и выше - это печатные символы (буквы, цифры, пробел)
                {
                    app.zapros.Add(key.KeyChar.ToString());

                }


                else
                    if (app.files != null && key.Key == ConsoleKey.UpArrow && app.sellect > -app.pinFolders_list.Count()) //стрелочка вниз
                {
                    app.sellect--;

                }
                else
                    if (app.files != null && key.Key == ConsoleKey.DownArrow && app.sellect < (app.files.Length - 1)) //стрелочка вверх
                {
                    app.sellect++;
                }
                else
                    if (app.files != null && key.Key == ConsoleKey.LeftArrow && app.sellect - 20 > 0) //стрелочка влево прыжок на 20
                {
                    app.sellect = app.sellect - 20;
                }
                else
                    if (app.files != null && key.Key == ConsoleKey.RightArrow && app.sellect + 20 <= (app.files.Length - 1)) //стрелочка вверх
                {
                    app.sellect = app.sellect + 20;
                }
                }
                ///////////////////////////////////////////////ВЫВОД ВСЕХ ДИРРЕКТОРИЙ
                if (!string.IsNullOrEmpty(app.sobraniy))
                {
                    app.get_folder(app.sobraniy, 1);
                    if (app.files.Length != 0)
                    {
                        print("   -");
                        short file_pass=0;
                        short colum = 1;
                        foreach (string a in app.files)
                        {
                            
                            if (file_pass <= 20)
                            {
                                app.csr = new pos_cursour() { x = Console.GetCursorPosition().Left, y = Console.GetCursorPosition().Top }; //запоминаем есто курсора для сдел. вывода
                            }
                            else { 
                                app.csr = new pos_cursour() { x = colum * 30, y = Console.GetCursorPosition().Top - 20 }; //запоминаем есто курсора для сдел. вывода
                                colum++;
                                file_pass = 0;
                                }
                            if (Array.IndexOf(app.files, a) == app.sellect && app.sellect>=0)
                            {
                                print("|-=>" + a + "<=\n", 1);
                                if (file_pass == 20)
                                {
                                    Console.SetCursorPosition(Console.GetCursorPosition().Left + ((colum - 1) * 30) + 8, Console.GetCursorPosition().Top + 1);
                                    print("Page:" + colum, 2);
                                }
                            }

                            else
                            {
                                print("|-" + a + "\n");
                                if (file_pass == 20)
                                {
                                    Console.SetCursorPosition(Console.GetCursorPosition().Left + ((colum - 1) * 30) + 8, Console.GetCursorPosition().Top + 1);
                                    print("Page:" + colum);
                                }
                            }
                            Console.SetCursorPosition(app.csr.x, app.csr.y + 1);
                            if (app.last_y_pos < app.csr.y+1)
                                app.last_y_pos = app.csr.y+1;
                            if (app.last_x_pos < app.csr.x)
                                app.last_x_pos = app.csr.x;
                            file_pass++;

                        }
                    }

                }
                app.update();
            }
            sleep(22); //задержка , для снятия нагрузки
            if (app.pressed_key == true) app.pressed_key = false;

            if ((DateTime.Now - app.lastRamUpdate).TotalMilliseconds > 1000) //очистка всего экрана
            {
                app.clear_cmd_all_screen(app);
                //    app.switch_coursor_pos(new pos_cursour() { x = 22, y = 11 }, new pos_cursour() { x = Console.GetCursorPosition().Left, y = Console.GetCursorPosition().Top }, $"Process used RAM:   {Process.GetCurrentProcess().WorkingSet64 / 1024.0 / 1024.0:F2} МБ");
                app.lastRamUpdate = DateTime.Now;
            }
            else //частичная очистка
            {
                app.clear_cmd(app);
                app.last_y_pos = 0;
            }
        }
    }

}
