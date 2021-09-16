using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using System.Threading;

namespace ToDoListCMD
{
    class Program
    {
        //ToDo Class
        static ToDo todoHandler = null;
        static List<ToDo> todoList = null;

        //Color & Drawing
        static ConsoleColor ColorOnComplete = ConsoleColor.Green;
        static ConsoleColor ColorOnProgress = ConsoleColor.White;

        //Global
        static bool ChangeInList = true;
        static bool ShowTaskCompleteMenu = true;
        static bool StatusExit = false;

        //Helper variables
        static int TodoCount = 0;

        //DELEGATE & EVENT
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(SetConsoleCtrlEventHandler handler, bool add);
        private delegate bool SetConsoleCtrlEventHandler(CtrlType sig);
        private enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        /// <summary>
        /// Spec:  
        /*
             * Aktuális Teendők({ száma}):
             * * 1 valami1 /t Kész + zöld szín
             * * 2 valami2
             *
             * Menü(ha először van megnyitva):
             * inputba: a sorszám és akkor az kész
             * ha nem szám, akkor menü(ha átnavigál)
             * 
             * Menü(ha átnavigál) :
             * 0 - Read List - (ha először van megnyitva)
             * 1 - Add Todo -> automat index |feladat | automat nincs kész
             * 2 - Edit Todo
             * 3 - Delete Todo
           */
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //Variables
            todoHandler = new ToDo();
            todoList = new List<ToDo>();
            todoList = todoHandler.Load();
            TodoCount = todoList.Count;

            //Events
            SetConsoleCtrlHandler(Handler, true); // Register the handle 

            //List<ToDo> list = todoHandler.Load();

            //MAIN loop
            while (true)
            {
                if (ChangeInList == true)
                {
                    OnRead();

                    Console.WriteLine();
                }

                if (ShowTaskCompleteMenu)
                    MenuOnFirst();
                else
                    MenuOnNavigate();

                if (StatusExit)
                {
                    OnExit();
                }
            }
            
            
        }

        //BONUS METHODS
        private static void Sleep(int ticks = 100)
        {
            Thread.Sleep(ticks);
        }

        //EVENTS
        private static bool Handler(CtrlType signal)
        {
            switch (signal)
            {
                case CtrlType.CTRL_BREAK_EVENT:
                    todoHandler.Save(todoList);
                    break;
                case CtrlType.CTRL_C_EVENT:
                    todoHandler.Save(todoList);
                    break;
                case CtrlType.CTRL_LOGOFF_EVENT:
                    todoHandler.Save(todoList);
                    break;
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                    todoHandler.Save(todoList);
                    break;
                case CtrlType.CTRL_CLOSE_EVENT:
                    todoHandler.Save(todoList);
                    break;
            }

            return true;
        }


        //CRUD FUNCTIONS
        private static void OnDelete()
        {
            if (todoList == null)
            {
                return;
            }

            bool run = true;
            int taskId = 0;

            while (run)
            {
                try
                {
                    Console.Write("Add meg a törölni kívánt elem indexét: ");
                    taskId = int.Parse(Console.ReadLine());

                }
                catch (FormatException)
                {
                    Console.WriteLine("Rossz értéket adtál meg.\nPróbáld meg újra!");
                }

                run = false;
            }

            ToDo deletableTask = todoList.Where(task => task.Id == taskId).First();

            todoList.Remove(deletableTask);

            for (int i = 0; i < todoList.Count; i++)
            {
                todoList[i].Id = i;
            }

            Console.WriteLine("Feladat sikeresen törölve!");
            Sleep(500);

            Console.Clear();
            ShowTaskCompleteMenu = true;
        }
        private static void OnInsert()
        {
            if(todoList == null)
            {
                return;
            }
            ToDo newTodo = new ToDo();
            string TaskName ="";
            DateTime TaskDate=new DateTime();
            bool run = true;

            while (run)
            {
                try
                {
                    Console.Write("Add meg a feladat címét: ");
                    TaskName = Console.ReadLine();
                    Console.WriteLine();
                    Console.Write("Add meg a feladat dátumát: ");
                    TaskDate = Convert.ToDateTime(Console.ReadLine());
                }
                catch (FormatException)
                {
                    Console.Clear();
                }

                run = false;
            }

            newTodo.Id = TodoCount;
            newTodo.Task = TaskName;
            newTodo.Time = TaskDate;

            todoList.Add(newTodo);

            Console.WriteLine("Feladat sikeresen hozzáadva");
            Sleep(500);
            Console.Clear();
            ShowTaskCompleteMenu = true;
        }
        private static void OnRead()
        {
            WriteList();
        }
        private static void OnEdit()
        {
            if(todoList == null)
            {
                return;
            }

            bool run = true;
            int taskId = 0;

            while (run)
            {
                try
                {
                    Console.Write("Add meg a módosítani kívánt feladat indexét: ");
                    taskId = int.Parse(Console.ReadLine());
                }
                catch (FormatException)
                {
                    Console.Clear();
                }

                run = false;
            }

            ToDo selectedTask = todoList.Where(task => task.Id == taskId).First();

            if(selectedTask == null)
            {
                Console.WriteLine("Nem találtam ilyen azonosítójú feladatot");
                Sleep(750);
                Console.Clear();
                ShowTaskCompleteMenu = true;
            }
            else
            {
                string selectedTaskTitle = "üres";
                DateTime selectedTaskDate = new DateTime();
                bool selectedTaskIsCompleted = selectedTask.IsCompleted;

                run = true;
                bool formatWasOk = false;
                while (run)
                {
                    try
                    {
                        Console.WriteLine($"(1).: Feladat jelenleg címe: {selectedTask.Task}\n" +
                            $"(2).: Feladat jelenlegi dátuma: {selectedTask.Time}\n " +
                            "(3).: Feladat jelenlegi státusza: ",selectedTask.IsCompleted == false ? "Nincs kész" : "Kész");
                        Console.WriteLine("\nAdd meg a fenti ( 1-2-3 ) számok közül, melyiket módosítanád!");
                        int selectedRow = int.Parse(Console.ReadLine());
                        if(selectedRow == 1)
                        {
                            Console.Write("Add meg az új nevét a feladatnak: ");
                            selectedTaskTitle = Console.ReadLine();
                            formatWasOk = true;
                        }
                        else if(selectedRow == 2)
                        {
                            Console.Write("Add meg az új teljesítési dátumát a feladatnak\nPélda formátum: 2021.06.07.: ");
                            selectedTaskDate = Convert.ToDateTime(Console.ReadLine());
                            formatWasOk = true;
                        }
                        else if(selectedRow == 3)
                        {
                            Console.Write("Add meg az új státuszát a feladatnak\n0 ha nincs kész, 1 ha kész: ");
                            selectedTaskIsCompleted = int.Parse(Console.ReadLine()) == 0 ? false : true;
                            formatWasOk = true;
                        }
                    }
                    catch (FormatException)
                    {
                        formatWasOk = false;
                    }

                    if(formatWasOk == true)
                    {
                        run = false;
                    }
                }

                if(selectedTaskTitle != "")
                {
                    selectedTask.Task = selectedTaskTitle;
                }
                if(selectedTaskDate != new DateTime())
                {
                    selectedTask.Time = selectedTaskDate;
                }
                if(selectedTask.IsCompleted != selectedTaskIsCompleted)
                {
                    selectedTask.IsCompleted = selectedTaskIsCompleted;
                }
            }

            Console.WriteLine("Módosítás sikeresen végrehajtva!");
            Sleep(500);
            ShowTaskCompleteMenu = true;
        }


        //APPLICATION HANDLER FUNCTIONS
        private static void OnExit()
        {
            todoHandler.Save(todoList);
            System.Environment.Exit(1);
        }


        //TODO LIST HANDLER FUNCTIONS    
        private static void WriteList()
        {
            if(todoList == null)
            {
                return;
            }

            Console.Clear();

            Console.WriteLine($"Aktuális teendők ({todoList.Where(task => task.IsCompleted == false).Count()})");

            for (int i = 0; i < todoList.Count; i++)
            {
                WriteTask(todoList[i]);
            }

            TodoCount = todoList.Count;
        }
        private static void WriteTask(ToDo task)
        {

            if(task.IsCompleted == false)
            {
                Console.ForegroundColor = ColorOnProgress;
                Console.WriteLine($"{task.Id}. {task.Task} {task.Time:d} Nincs kész");
            }
            else if(task.IsCompleted == true)
            {
                 Console.ForegroundColor = ColorOnComplete;
                 Console.WriteLine($"{task.Id}. {task.Task} {task.Time:d} Kész");              
            }
        }
        private static void UserInputComplete(int userInput)
        {
            if (todoList == null)
            {
                return;
            }

            for (int i = 0; i < todoList.Count; i++)
            {
                if(userInput == todoList[i].Id &&
                    todoList[i].IsCompleted == false)
                {
                    todoList[i].IsCompleted = true;
                }
                else if(userInput == todoList[i].Id &&
                    todoList[i].IsCompleted == true)
                {
                    todoList[i].IsCompleted = false;
                }
            }
        }


        //MENU HANDLER FUNCTIONS
        private static void MenuOnFirst()
        {
            Console.ForegroundColor = ColorOnProgress;

            int userInput = 0;
            Console.Write("Válaszd ki hanyas számú feladattal végeztél\nVagy nyomj meg bármilyen gombot a menü eléréséhez: ");
            try
            {
                userInput = int.Parse(Console.ReadLine());

            }
            catch (FormatException)
            {
                ShowTaskCompleteMenu = false;
            }
            catch(Exception x)
            {
                Console.Clear();
            }

            UserInputComplete(userInput);
            ChangeInList = true;
        }
        private static void MenuOnNavigate()
        {
            Console.ForegroundColor = ColorOnProgress;

            if (ShowTaskCompleteMenu == false)
            {
                string userMenuInput = "";

                Console.WriteLine($"Beolvasás: (r) | Hozzáadás: (a) | Szerkesztés: (e) | Törlés: (d)\nVissza a feladat menüre (v)\nKilépés (x) vagy (q)");

                try
                {
                    userMenuInput = Console.ReadLine();
                }
                catch (FormatException)
                {
                    Console.WriteLine("Ilyen menüpont nincsen, adj meg egy elérhetőt!\nPéldául nyomd meg az r betűt, majd egy entert");
                }
                catch(Exception x)
                {
                    Console.Clear();
                    Console.WriteLine(x.InnerException.ToString());

                }
                if (userMenuInput == "v")
                {
                    ChangeInList = true;
                    ShowTaskCompleteMenu = true;
                }
                else if(userMenuInput == "q" || userMenuInput == "x")
                {
                    StatusExit = true;
                }
                else if(userMenuInput == "r")
                {
                    ChangeInList = true;
                }
                else if(userMenuInput == "a")
                {
                    OnInsert();
                }
                else if (userMenuInput == "e")
                {
                    OnEdit();
                }
                else if (userMenuInput == "d")
                {
                    OnDelete();
                }
            }
            else
            {
                return;
            }
        }

    }

    [Serializable()]
    class ToDo
    {
        //Properties
        public int Id { get; set; }
        public string Task { get; set; } = "üres";
        public DateTime Time
        {
            get
            {
                return time;
            }
            set
            {
                if(value.Year < DateTime.Now.Year||
                    value.Month<DateTime.Now.Month||
                    value.Day < DateTime.Now.Day)
                {
                    time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                }
                else
                {
                    time = value;
                }
            }
        }
        public bool IsCompleted { get; set; } = false;

        private DateTime time;


        //Constructor
        public ToDo() { }


        //Property operations
        public void Save(List<ToDo> tuples)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, tuples);
                ms.Position = 0;
                byte[] buffer = new byte[(int)ms.Length];
                ms.Read(buffer, 0, buffer.Length);
                Properties.Settings.Default.Tasks = Convert.ToBase64String(buffer);
                Properties.Settings.Default.Save();
            }
        }

        public List<ToDo> Load()
        {
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(Properties.Settings.Default.Tasks)))
            {
                BinaryFormatter bf = new BinaryFormatter();
                return (List<ToDo>)bf.Deserialize(ms);
            }
        }
    }
}
