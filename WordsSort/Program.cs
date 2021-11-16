using System;
using System.Text;

namespace WordsSort
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] words = Sorter.GetSortedWordsArrayFromPdf(@"Tolkien_The_Fellowship_of_the_Ring.pdf", SortType.Inserts);

            NumberedSet<string> set = new NumberedSet<string>(words);

            foreach(var element in set)
            {
                Console.Write(element.Value);
                Console.Write(" " + new string('-', 15 - Console.CursorLeft) + " ");
                Console.WriteLine(element.Count);
            }
        }
    }
}
