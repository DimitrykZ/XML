using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Data;
using System.IO;
using System.Threading;

namespace XML_1Console
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlCopy.CopyFiles(args);
        }

    }

    class XmlCopy      // Класс который работает с XML файлами
    {
        private static int iThread = 0;           // Переменная отвечает за контроль количества паралельных потоков
        private static int numberS = 0;             //Переменная которая хранит номера строк с которыми работают
        private static string xmlName = "XML.xml";  //xmlName хранит имя XML файла, если в строке вызова не ввести имя файла который нужен
        private static  List<string[]> list = new List<string[]>();    //Создание экземпляра коллекции list                                         // то переменная хранит имя файла по умолчанию, который должен находится с exe файлом в одной папке
        public static void CopyFiles(string[] args) //Основной метод, который запускает все нужные шаги для работы
        {

            if (args.Length > 0)                    //Проверка вводидись ли какие либо данные в командной строке
                xmlName = args[0];                      // если вводились, то xmlName присваивается введеный адресс 

                   
            GetList();                                  // Создание коллекции List при помощи метода GetList
            foreach (string[] st in list)             //Для каждого элемента коллекции выполняется следующий набор операций
            {
                Thread th1 = new Thread(new ParameterizedThreadStart(Copy)); //Создается параллельный поток
                while (iThread >= 4)                                         //Если количество потоков = 4, то выполянется ожидание 
                {                                                           //пока не какой либо поток на не закончится.
                    Thread.Sleep(10);
                }
                th1.Start(st);                                                 //запуск паралелльного потока
                Thread.Sleep(10);                                               // Временная пауза чтобы проинициализировались переменные в паралельном потоке
            }

        }

                          
        public static void GetList()                                //Метод для заполнения коллекции List
        {
            XmlTextReader textReader = new XmlTextReader(xmlName);   //Созадется объект textReader отвечающий за чтение XML файла

            //List<string[]> list = new List<string[]>();                 //Создание экземпляра коллекции list

            try
            {
                while (textReader.Read())                           //Пока чтение файла возможно =true
                {
                    XmlNodeType nType = textReader.NodeType;        //nType служит для работы с данные, прочитанные в XML файле

                    if (nType == XmlNodeType.Element)              //Когда чтение файла доход то типа Element
                    {                                               // выполняется процедура чтения и записи  атрибутов данного элемента

                        string[] str = new string[3];               //массив строк хранящий три значения атрибутов имя файла, исходный и конечный путь
                        if (textReader.HasAttributes)
                            while (textReader.MoveToNextAttribute()) //Проход по атрибуттам
                            {
                                if (textReader.Name == "FileName") // Если имя аттрибута равно FileName, то записываем его в нулевой индекс массива
                                    str[0] = textReader.Value;
                                if (textReader.Name == "directoryBegin")// Если имя аттрибута равно directoryBegin, то записываем его в первый индекс массива
                                    str[1] = textReader.Value;
                                if (textReader.Name == "directoryLast")// Если имя аттрибута равно directoryLast, то записываем его в второй индекс массива
                                    str[2] = textReader.Value;
                            }

                       
                        if (str[0]!=null) // Проверка на наличие имени файла
                        {
                            //foreach (string s in str)//Данные действие нужно для защиты от элементов с пустыми атрибутами
                             //   Console.Write(s);
                           // Console.WriteLine();
                            list.Add(str);
                        }
                       

                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
           
        }

        public static void Copy(object ob)                      //Метод отвечающий за копирование файла
        {
            iThread++;                                          //Увеличение переменной дает понять, что запущен поток
            int sn = ++numberS;                                     //sn хранит номер элемента (строки) коллекции, данная информация может потребоваться при выводе ошибок
            string[] st = (string[])ob;                               //st -строка которая хранит имя  и пути 
            string fileName = st[0];                      //fileName -имя файла
            string directoryBegin = st[1];              // directoryBegin - исходный путь
            string directoryLast = st[2];               //directoryLast - конечный путь


            if (IsDirectory(directoryLast, sn) && FreeMemory(directoryBegin, fileName, directoryLast))
                //Проверка есть ли папка в которую нужно переместить файл, если нет то создает ее, а также наличие свободного места на диске
                try
                {
                    using (FileStream file = File.OpenRead((directoryBegin) + "\\" + fileName))  //Поток чтения файла
                    {
                        using (FileStream file2 = File.OpenWrite(directoryLast + "\\" + fileName)) //Поток записи файла
                            file.CopyTo(file2);                                                     //Копирование файла
                        Console.WriteLine("Файл {0} скопирован из папки {1} в папку {2}", fileName,
                            directoryBegin, directoryLast); //Сообщение о проделанной работе
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + "Строка номер " + sn);
                }

            iThread--;   // Уменьшает счетчик работающих потоков, дает понять, что поток завершен.

        }

        public static Boolean IsDirectory(string strDirectory, int sn)   //Метод отвечающий наличие папки, куда будет копироваться файл
        {
            if (!Directory.Exists(strDirectory))                        //Если каталога нет, то создает его
            {
                try
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(strDirectory);
                    dirInfo.Create();
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + "Строка номер " + sn);
                    return false;
                }
            }
            return true;

        }

        public static bool FreeMemory(string directoryBegin, string fileName, string directoryLast)  //метод проверяет хватает ли места для копирования файла
        {
            try
            {
                FileInfo file = new FileInfo((directoryBegin) + "\\" + fileName); //Создание экземпляра объекта FileInfo, чтобы узнать размер файла
                DriveInfo disk = new DriveInfo(directoryLast.First().ToString()); //Создание экземпляра объекта DriveInfo, чтобы узнать кол-во свободного места

                                      
                if (file.Length > disk.AvailableFreeSpace)   //Если места недостаточно, то выдается соответствующее сообщение.
                {
                    Console.WriteLine("Не хватает места на диске {0}", disk.Name);
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;                        //Если все проверки прошли,то возвращается true, то есть разрешение на копирование
        }

    }

}
