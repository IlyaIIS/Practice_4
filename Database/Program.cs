using System;

namespace Database
{
    class Program
    {
        static void Main(string[] args)
        {
            Table table = new Table("TestTable.txt");
            /*
            Table sortedTable = table.GetSortedTable("Result.txt", true, "SecondColumn");
            Table.GetFilteredTable(sortedTable, "FiltredResul.txt", new Condition(sortedTable, "FirstColumn", (value) =>
                {
                    return (int)value >= 3;
                }));*/
            table.GetSortedTable("Result.txt", true, "FirstColumn", SortType.Multipath);
        }
    }
}
