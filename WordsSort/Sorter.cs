using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SautinSoft;

namespace WordsSort
{
    static class Sorter
    {
        public static string[] GetSortedWordsArrayFromPdf(string fileName, SortType sortType)
        {
            string textFilePath = fileName.Replace(".pdf", ".txt");

            //if (!File.Exists(textFilePath))
            {
                PdfFocus f = new PdfFocus();
                f.OpenPdf(@"..\..\..\" + fileName);
                f.ToText(textFilePath,2,10);
            }

            return GetSortedWordsArrayFromFile(textFilePath, sortType);
        }

        public static string[] GetSortedWordsArrayFromFile(string filePath, SortType sortType)
        {
            if (sortType == SortType.Merges)
            {
                List<string> output = new List<string>();

                List<Queue<string>> subArrays = GetSortedSubArrays(filePath);

                string minWord = GetMinWord(ref subArrays);
                while (minWord != null)
                {
                    //
                    if (output.Count % 100 == 0)
                        Console.Write("\r" + subArrays.Count);
                    //

                    output.Add(minWord);
                    minWord = GetMinWord(ref subArrays);
                }

                return output.ToArray();
            }
            else if (sortType == SortType.Inserts)
            {
                return GetInsertsSortedArray(filePath);
            }
            else
            {
                throw new Exception("Нереализованный тип сортировки");
            }
        }

        private static List<Queue<string>> GetSortedSubArrays(string filePath)
        {
            char[] punctuationMarks = { '.', ',', ';', ':', '!', '?', '(', ')', '\'', '"' };

            List<Queue<string>> output = new List<Queue<string>>();
            
            string pastWord = string.Empty;
            StreamReader file = new StreamReader(filePath);
            StringBuilder subString = new StringBuilder();
            output.Add(new Queue<string>());
            int characterInt = 0;
            while (characterInt != -1)
            {
                do
                {
                    characterInt = file.Read();
                } while (characterInt != -1 && ((characterInt >= 8216 && characterInt <= 8223) || characterInt == 13 ||
                        punctuationMarks.Contains((char)characterInt)));
                char character = (char)characterInt;

                if (character != ' ' && character != '\n' && characterInt != -1)
                {
                    subString.Append(character);
                }
                else
                {
                    if (pastWord.Length != 0)
                        output[^1].Enqueue(pastWord);

                    if (pastWord.CompareTo(subString.ToString()) == 1)
                        output.Add(new Queue<string>());
                    
                    pastWord = subString.ToString();
                    subString.Clear();
                }
            }
            output[^1].Enqueue(pastWord);
            output[0].Dequeue();
            if (output[0].Count == 0)
                output.RemoveAt(0);

            DeliteEmptyArrays();

            return output;

            void DeliteEmptyArrays()
            {
                for (int i = 0; i < output.Count; i++)
                {
                    if (output[i].Count == 0)
                    {
                        output.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
        private static string GetMinWord(ref List<Queue<string>> subArrays)
        {
            string minWord = null;
            int subArrayNum = -1;
            for (int i = 0; i < subArrays.Count; i++)
            {
                string checkedWord = subArrays[i].Peek();
                if (minWord == null || checkedWord.CompareTo(minWord) == -1)
                {
                    subArrayNum = i;
                    minWord = checkedWord;
                }
            }

            if (subArrayNum == -1)
                return null;

            subArrays[subArrayNum].Dequeue();
            if (subArrays[subArrayNum].Count == 0)
                subArrays.RemoveAt(subArrayNum);

            return minWord;
        }

        private static string[] GetInsertsSortedArray(string filePath)
        {
            char[] punctuationMarks = { '.', ',', ';', ':', '!', '?', '(', ')', '\'', '"' };

            LinkedList<string> output = new LinkedList<string>();

            StreamReader file = new StreamReader(filePath);
            StringBuilder subString = new StringBuilder();
            int characterInt = 0;
            while (characterInt != -1)
            {
                do
                {
                    characterInt = file.Read();
                } while (characterInt != -1 && ((characterInt >= 8216 && characterInt <= 8223) || characterInt == 13 ||
                        punctuationMarks.Contains((char)characterInt)));
                char character = (char)characterInt;

                if (character != ' ' && character != '\n' && characterInt != -1)
                {
                    subString.Append(character);
                }
                else
                {
                    string word = subString.ToString();
                    if (word.Length != 0)
                        InsertWordInSortedOrder(word);
                    subString.Clear();
                }
            }

            file.Close();

            return output.ToArray();

            void InsertWordInSortedOrder(string word)
            {
                var node = output.First;
                while(node != output.Last)
                {
                    if (node.Value.CompareTo(word) != -1)
                    {
                        
                        output.AddBefore(node, word);
                        return;
                    }
                    node = node.Next;
                }

                output.AddLast(word);
            }
        }
    }
    enum SortType
    {
        Inserts,
        Merges
    }
}
