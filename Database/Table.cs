using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Database
{
    class Table
    {
        public const char SeparatingSign = '	';
        public readonly string FilePath;
        public int ColumnCount { get; }
        public int RowCount { get; private set; }
        public string[] Attributes { get; }
        public ColumnType[] Types { get; }
        public Table(string path)
        {
            FilePath = path;
            if (File.Exists(path))
            {
                StreamReader file = new StreamReader(path);

                string[] firstLine = ParseLine(file.ReadLine(), 2);
                ColumnCount = int.Parse(firstLine[0]);
                RowCount = int.Parse(firstLine[1]);
                Attributes = ParseLine(file.ReadLine(), ColumnCount);
                Types = ParseToType(ParseLine(file.ReadLine(), ColumnCount));
                file.Close();
            }
            else
            {
                throw new Exception("Файл не найден");
            }
        }
        public Table(string path, Table cloneableTable, bool cloneData)
        {
            FilePath = path;
            ColumnCount = cloneableTable.ColumnCount;
            Attributes = cloneableTable.Attributes;
            Types = cloneableTable.Types;

            StreamWriter newFile = new StreamWriter(path);
            StreamReader cloneableFile = new StreamReader(cloneableTable.FilePath);

            if (cloneData)
                newFile.WriteLine(cloneableFile.ReadLine());
            else
                newFile.WriteLine(ParseLine(cloneableFile.ReadLine(), 2)[0] + SeparatingSign + "0" + SeparatingSign);
            newFile.WriteLine(cloneableFile.ReadLine());
            newFile.WriteLine(cloneableFile.ReadLine());

            if (cloneData)
            {
                while (!cloneableFile.EndOfStream)
                {
                    newFile.WriteLine(cloneableFile.ReadLine());
                }
            }

            newFile.Close();
            cloneableFile.Close();
        }
        static string[] ParseLine(string line, int columnCount)
        {
            string[] output = new string[columnCount];
            StringBuilder substring = new StringBuilder();
            int i = 0;
            foreach (char el in line)
            {
                if (el == SeparatingSign)
                {
                    output[i] = substring.ToString();
                    substring = substring.Clear();
                    i++;
                }
                else
                {
                    substring.Append(el);
                }
            }

            return output;
        }

        ColumnType[] ParseToType(string[] words)
        {
            ColumnType[] output = new ColumnType[ColumnCount];
            for (int i = 0; i < ColumnCount; i++)
            {
                output[i] = (ColumnType)int.Parse(words[i]);
            }
            return output;
        }

        static TableElement[] ParseToElements(string[] words, int columnCount, ColumnType[] types)
        {
            TableElement[] output = new TableElement[columnCount];
            for (int i = 0; i < columnCount; i++)
            {
                output[i] = new TableElement(words[i], types[i]);
            }

            return output;
        }

        public Table GetSortedTable(string outputPath, bool ascending, string attribute, SortType sortType)
        {
            int attributeNum = -1;
            for (int i = 0; i < ColumnCount; i++)
            {
                if (Attributes[i] == attribute)
                    attributeNum = i;
            }
            if (attributeNum == -1)
                throw new Exception("Неверное имя атрибута");

            if (sortType == SortType.Direct)
            {

                SubSortDirectly(outputPath, ascending, attributeNum, 0);

                return (new Table(outputPath));

            }
            else if (sortType == SortType.Natural)
            {
                string directoryPath = "temp";

                SplitIntoTablesNaturally(directoryPath, attributeNum, ascending);

                string newDirectoryPath = directoryPath;
                int j = 0;
                do
                {
                    string[] files = Directory.GetFiles(newDirectoryPath);
                    newDirectoryPath = directoryPath + @"\temp_" + j;
                    Directory.CreateDirectory(newDirectoryPath);

                    for (int i = 1; i < files.Length; i += 2)
                    {
                        MergeSortedTables(newDirectoryPath + @"\temp_" + i / 2, new string[] { files[i - 1], files[i] }, attributeNum, ascending);
                    }
                    if (files.Length % 2 == 1)
                        File.Copy(files[^1], newDirectoryPath + @"\temp_" + files.Length / 2);

                    j++;
                } while (Directory.GetFiles(newDirectoryPath).Length > 1);

                File.Copy(Directory.GetFiles(newDirectoryPath)[0], outputPath, true);

                return (new Table(outputPath));
            }
            else if (sortType == SortType.Multipath)
            {
                string directoryPath = "temp";

                SplitIntoTablesNaturally(directoryPath, attributeNum, ascending);
                MergeSortedTables(outputPath, Directory.GetFiles(directoryPath), attributeNum, ascending);

                return (new Table(outputPath));
            }
            else
            {
                throw new Exception("Нереализованный тип сортировки");
            }
        }
        void SubSortDirectly(string outputPath, bool ascending, int columnNum, int depth)
        {
            if (RowCount > 1)
            {
                string path1 = @"temp\temp_" + depth.ToString() + "_1";
                string path2 = @"temp\temp_" + depth.ToString() + "_2";
                SplitIntoTwoTableDirectly(path1, path2);
                Table table1 = new Table(path1);
                Table table2 = new Table(path2);
                table1.SubSortDirectly(path1, ascending, columnNum, depth + 1);
                table2.SubSortDirectly(path2, ascending, columnNum, depth + 1);
                MergeSortedTables(outputPath, new string[] { path1, path2 }, columnNum, ascending);
            }
            else
            {
                return;
            }
        }

        void SplitIntoTwoTableDirectly(string outputPath1, string outputPath2)
        {
            if (!File.Exists(outputPath1))
                File.Delete(outputPath1);
            if (!File.Exists(outputPath2))
                File.Delete(outputPath2);

            Table table1 = new Table(outputPath1, this, false);
            Table table2 = new Table(outputPath2, this, false);

            StreamWriter file1 = new StreamWriter(outputPath1);
            StreamWriter file2 = new StreamWriter(outputPath2);
            StreamReader originFile = new StreamReader(FilePath);

            for (int i = 0; i < 3; i++)
            {
                string line = originFile.ReadLine();
                file1.WriteLine(line);
                file2.WriteLine(line);
            }

            int j = 0;
            while (!originFile.EndOfStream)
            {
                if (j % 2 == 0)
                {
                    file1.WriteLine(originFile.ReadLine());
                }
                else
                {
                    file2.WriteLine(originFile.ReadLine());
                }
                j++;
            }

            file1.Close();
            file2.Close();
            originFile.Close();

            table1.SetRowCount(RowCount / 2 + RowCount % 2);
            table2.SetRowCount(RowCount / 2);
        }
        public void SplitIntoTablesNaturally(string outputDirectoryPath, int checkedColumnNum, bool ascending)
        {
            int dir = ascending ? 1 : -1;
            List<Table> tables = new List<Table>();

            StreamReader originFile = new StreamReader(FilePath);

            string[] metadata = new string[3];
            for (int i = 0; i < 3; i++)
            {
                metadata[i] = originFile.ReadLine();
            }

            if (Directory.Exists(outputDirectoryPath))
                Directory.Delete(outputDirectoryPath, true);
            Directory.CreateDirectory(outputDirectoryPath);

            int j = 0;
            StreamWriter currentFile;
            string pastLine = originFile.ReadLine();
            TableElement[] pastElements = ParseToElements(ParseLine(pastLine, ColumnCount), ColumnCount, Types);
            while (pastLine != null)
            {
                string path = outputDirectoryPath + @"\temp_" + j + ".txt";
                tables.Add(new Table(path, this, false));
                currentFile = new StreamWriter(path);
                int rowCount = 0;

                foreach (string line in metadata)
                    currentFile.WriteLine(line);

                while (true)
                {
                    currentFile.WriteLine(pastLine);
                    rowCount++;

                    if (!originFile.EndOfStream)
                    {
                        string line = originFile.ReadLine();
                        TableElement[] elements = ParseToElements(ParseLine(line, tables[j].ColumnCount), tables[j].ColumnCount, tables[j].Types);
                        if (pastElements[checkedColumnNum].CompareTo(elements[checkedColumnNum]) == dir)
                        {
                            pastLine = line;
                            pastElements = elements;
                            break;
                        }
                        pastLine = line;
                        pastElements = elements;
                    }
                    else
                    {
                        pastLine = null;
                        break;
                    }
                }

                currentFile.Close();
                tables[j].SetRowCount(rowCount);
                j++;
            }

            originFile.Close();
        }
        void MergeSortedTables(string outputPath, string[] inputPath, int columnNum, bool ascending)
        {
            int dir = ascending ? 1 : -1;
            Table[] tables = new Table[inputPath.Length];
            for (int i = 0; i < inputPath.Length; i++)
                tables[i] = new Table(inputPath[i]);
            Table outputTable = new Table(outputPath, tables[0], false);

            StreamReader[] files = new StreamReader[inputPath.Length];
            for (int i = 0; i < inputPath.Length; i++)
                files[i] = new StreamReader(inputPath[i]);
            StreamWriter outputFile = new StreamWriter(outputPath);

            for (int j = 0; j < 3; j++)
                outputFile.WriteLine(files[0].ReadLine());
            for (int i = 1; i < inputPath.Length; i++)
                for (int j = 0; j < 3; j++)
                {
                    files[i].ReadLine();
                }

            string[] lines = new string[inputPath.Length];
            for (int i = 0; i < inputPath.Length; i++)
                lines[i] = files[i].ReadLine();
            TableElement[][] elements = new TableElement[inputPath.Length][];
            for (int i = 0; i < inputPath.Length; i++)
                elements[i] = ParseToElements(ParseLine(lines[i], ColumnCount), ColumnCount, Types);
            do
            {
                int j = GetMinOrMaxElementNum(elements);
                if (j != -1)
                {
                    outputFile.WriteLine(lines[j]);
                    if (!files[j].EndOfStream)
                    {
                        lines[j] = files[j].ReadLine();
                        elements[j] = ParseToElements(ParseLine(lines[j], ColumnCount), ColumnCount, Types);
                    }
                    else
                    {
                        lines[j] = null;
                        elements[j] = null;
                    }
                }
                else
                {
                    break;
                }
            } while (true);

            foreach (var file in files)
                file.Close();
            outputFile.Close();

            int totalRowCount = 0;
            foreach (Table table in tables)
                totalRowCount += table.RowCount;
            outputTable.SetRowCount(totalRowCount);

            int GetMinOrMaxElementNum(TableElement[][] elements)
            {
                TableElement[] currentLine = null;
                int output = -1;
                for (int i = 0; i < inputPath.Length; i++)
                {
                    if (elements[i] != null && (currentLine == null || (currentLine[columnNum].CompareTo(elements[i][columnNum]) == dir)))
                    {
                        currentLine = elements[i];
                        output = i;
                    }
                }
                return output;
            }
        }
        void SetRowCount(int count)
        {
            RowCount = count;
            RewriteLine(FilePath, 0, ColumnCount.ToString() + SeparatingSign + RowCount.ToString() + SeparatingSign);
        }

        public static Table GetFilteredTable(Table table, string outputPath, Condition condition)
        {
            if (File.Exists(outputPath))
                File.Delete(outputPath);

            Table outputTable = new Table(outputPath, table, false);

            StreamWriter outputFile = new StreamWriter(outputPath);
            StreamReader inputFile = new StreamReader(table.FilePath);

            for (int i = 0; i < 3; i++)
            {
                outputFile.WriteLine(inputFile.ReadLine());
            }
            int j = 0;
            while (!inputFile.EndOfStream)
            {
                string line = inputFile.ReadLine();
                TableElement[] elements = ParseToElements(ParseLine(line, table.ColumnCount), table.ColumnCount, table.Types);
                if (condition.Satisfies(elements))
                {
                    outputFile.WriteLine(line);
                    j++;
                }
            }

            outputFile.Close();
            inputFile.Close();

            outputTable.SetRowCount(j);

            return outputTable;
        }

        private static void RewriteLine(string path, int lineIndex, string newValue)
        {
            int i = 0;
            string tempPath = path + ".tmp";
            using (StreamReader sr = new StreamReader(path))
            using (StreamWriter sw = new StreamWriter(tempPath))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (lineIndex == i)
                    {
                        sw.WriteLine(newValue);
                    }
                    else
                    {
                        sw.WriteLine(line);
                    }
                    i++;
                }
            }
            File.Delete(path);
            File.Move(tempPath, path);
        }
    }

    delegate bool ConditionDelegate(IComparable value);
    class Condition
    {
        string Attribute { get; }
        int attributeNum;
        ConditionDelegate function;
        public Condition(Table table, string attribute, ConditionDelegate conditionFunction)
        {
            Attribute = attribute;
            function = conditionFunction;
            attributeNum = Array.IndexOf(table.Attributes, Attribute);
        }
        public bool Satisfies(TableElement[] elements)
        {
            return function((IComparable)elements[attributeNum].Value);
        }
    }

    struct TableElement : IComparable
    {
        public Object Value { get; }
        public ColumnType Type { get; }
        public TableElement(string value, ColumnType type)
        {
            Type = type;
            switch (type)
            {
                case ColumnType.Integer:
                    Value = int.Parse(value);
                    break;
                case ColumnType.String:
                    Value = value;
                    break;
                default:
                    throw new Exception("Тип не реализован");
            }
        }

        public int CompareTo(object obj)
        {
            if (obj.GetType() == typeof(TableElement))
            {
                TableElement otherElement = (TableElement)obj;
                if (Type == otherElement.Type)
                {
                    IComparable value1 = (IComparable)Value;
                    IComparable value2 = (IComparable)otherElement.Value;
                    return value1.CompareTo(value2);
                }
                else
                {
                    throw new Exception("Попытка сравнить элементы, хронящие разные типы данных");
                }
            }
            else
            {
                throw new Exception("Нельзя сравнить с другими типами");
            }
        }
    }

    enum ColumnType
    {
        Integer,
        String
    }

    enum SortType
    {
        Direct,
        Natural,
        Multipath
    }
}

//Формат таблицы:
//строка 1: количество стобцов
//строка 2: список атбибутов
//строка 3: список типов атрибутов
//строки 4...: строки с данными
//для разделения используется символ "	" (символ табуляции)