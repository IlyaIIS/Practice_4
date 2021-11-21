using System;
using System.Text;

namespace WordsSort
{
    class Program
    {
        static void Main(string[] args)
        {
            for (int i = 15; i <= 750; i += 15)
            {
                //Sorter.GetSortedWordsArrayFromPdf(@"Tolkien_The_Fellowship_of_the_Ring.pdf", SortType.Inserts, i);
                //Sorter.GetSortedWordsArrayFromPdf(@"Tolkien_The_Fellowship_of_the_Ring.pdf", SortType.Merges, i);
                for (int j = 0; j < 5; j++)
                {
                    Sorter.GetSortedWordsArrayFromPdf(@"Tolkien_The_Fellowship_of_the_Ring.pdf", SortType.MSD, i);
                }
                Console.WriteLine();
            }
            

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            /*Console.WriteLine(words.Length);
            foreach (string word in words)
                Console.WriteLine(word);*/

            /*NumberedSet<string> set = new NumberedSet<string>(words);

            foreach(var element in set)
            {
                Console.Write(element.Value);
                Console.Write(" " + new string('-', 25 - Console.CursorLeft) + " ");
                Console.WriteLine(element.Count);
            }*/
        }
    }
}
