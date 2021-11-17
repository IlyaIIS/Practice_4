using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    class Program
    {
        static void Main(string[] args)
        {
            /*Table table = new Table("TestTable.txt");
            /*
            Table sortedTable = table.GetSortedTable("Result.txt", true, "SecondColumn");
            Table.GetFilteredTable(sortedTable, "FiltredResul.txt", new Condition(sortedTable, "FirstColumn", (value) =>
                {
                    return (int)value >= 3;
                }));*//*
            table.GetSortedTable("Result.txt", true, "FirstColumn", SortType.Multipath);*/

            AskUserForSort();
        }

        static void AskUserForSort()
        {
            Console.Write("Введите имя файла для сортировки: ");
            Table inputTable = new Table(@"..\..\..\" + Console.ReadLine() + ".txt");

            Console.Write("Введите имя результирующиего файла: ");
            string outputFile = @"..\..\..\" + Console.ReadLine() + ".txt";

            Console.Write("Введите название столбца для сортировки: ");
            string attributeName = Console.ReadLine();

            Console.Write("Выберите порядок сортировки (1 - по возростонию, 2 - по убыванию): ");
            bool ascending = int.Parse(Console.ReadLine()) == 1;

            Console.Write("Выберите тип сортировки (1 - прямая, 2 - натуральная, 3 - многопутевая): ");
            SortType sortType = (SortType)(int.Parse(Console.ReadLine()) - 1);

            Condition condition = AskCondition(inputTable);


            if (condition != null)
                inputTable = Table.GetFilteredTable(inputTable, @"..\..\..\temp.txt", condition);

            inputTable.GetSortedTable(outputFile, ascending, attributeName, sortType);
        }
        static Condition AskCondition(Table table)
        {
            Console.Write("Введите название столбца для условия (оставьте пустым, если условия нет): ");
            string conditionAttributeName = Console.ReadLine();

            if (conditionAttributeName.Length != 0)
            {
                Console.Write("Введите условие (шаблон: x = Химикаты): ");
                string textOfFunctuon = Console.ReadLine();
                if (textOfFunctuon[^1] != ' ')
                    textOfFunctuon += ' ';

                string[] words = ParseLine(textOfFunctuon);
                IComparable operand = GetOpernd(words[2]);

                ConditionDelegate function = GetFunction(words[1], operand);

                return new Condition(table, conditionAttributeName, function);
            }
            else
            {
                return null;
            }

            string[] ParseLine(string line)
            {
                List<string> output = new List<string>();
                StringBuilder subStr = new StringBuilder();
                foreach(char el in line)
                {
                    if (el != ' ')
                    {
                        subStr.Append(el);
                    }
                    else
                    {
                        output.Add(subStr.ToString());
                        subStr.Clear();
                    }
                }
                return output.ToArray();
            }
            IComparable GetOpernd(string operand)
            {
                if (int.TryParse(operand, out int result))
                    return result;
                else
                    return operand;
            }
            ConditionDelegate GetFunction(string operatorStr, IComparable operand)
            {
                switch (operatorStr)
                {
                    case ">":
                        return (value) => { return value.CompareTo(operand) == 1; };
                    case "<":
                        return (value) => { return value.CompareTo(operand) == -1; };
                    case "=":
                    case "==":
                        return (value) => { return value.CompareTo(operand) == 0; };
                    case ">=":
                        return (value) => { return value.CompareTo(operand) != -1; };
                    case "<=":
                        return (value) => { return value.CompareTo(operand) != 1; };
                    default:
                        throw new Exception("Некорректные данные");
                }
            }
        }
    }
}
