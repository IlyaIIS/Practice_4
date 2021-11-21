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
        public static string[] GetSortedWordsArrayFromPdf(string fileName, SortType sortType, int pageCount)
        {
            string textFilePath = fileName.Replace(".pdf", ".txt");
            textFilePath = textFilePath.Insert(textFilePath.Length - 4, "_" + pageCount);

            if (!File.Exists(textFilePath))
            {
                PdfFocus f = new PdfFocus();
                f.OpenPdf(@"..\..\..\" + fileName);
                f.ToText(textFilePath,2,pageCount);
            }
            DateTime time = DateTime.Now;
            string[] output = GetSortedWordsArrayFromFile(textFilePath, sortType);
            Console.WriteLine(output.Length + " - " + Math.Round(DateTime.Now.Subtract(time).TotalMilliseconds));
            return output;
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
                    /*
                    if (output.Count % 100 == 0)
                        Console.Write("\r" + subArrays.Count);
                    //*/

                    output.Add(minWord);
                    minWord = GetMinWord(ref subArrays);
                }

                return output.ToArray();
            }
            else if (sortType == SortType.Inserts)
            {
                return GetInsertsSortedArray(filePath);
            }
            else if (sortType == SortType.MSD)
            {
                return GetMsdSortedArray(filePath).ToArray();
            }
            else
            {
                throw new Exception("Нереализованный тип сортировки");
            }
        }

        private static List<string> GetMsdSortedArray(string filePath)
        {
            List<string> words = new List<string>();

            StreamReader file = new StreamReader(filePath);

            StringBuilder subString = new StringBuilder();
            string pastWord = string.Empty;
            int characterInt = 0;
            while (characterInt != -1)
            {
                do
                {
                    characterInt = file.Read();
                } while (characterInt != -1 && !(
                (characterInt >= 65 && characterInt <= 90) || 
                (characterInt >= 97 && characterInt <= 122) || 
                characterInt == 32 || characterInt == 13));
                char character = Char.ToLower((char)characterInt);

                if (character != ' ' && character != '\n' && characterInt != -1)
                {
                    subString.Append(character);
                }
                else
                {
                    if (pastWord.Length != 0)
                    {
                        words.Add(pastWord);
                    }

                    pastWord = subString.ToString();
                    subString.Clear();
                }
            }
            if (pastWord.Length != 0)
                words.Add(pastWord);

            return GetRecursiveMsdSortedArray(words, 0);
        }

        private static List<string> GetRecursiveMsdSortedArray(List<string> array, int depth)
        {
            if (array.Count > 1)
            {
                Dictionary<char, List<string>> baskets = new Dictionary<char, List<string>>();
                baskets.Add((char)96, new List<string>());
                foreach (string word in array)
                {
                    if (depth < word.Length)
                    {
                        if (baskets.ContainsKey(word[depth]))
                            baskets[word[depth]].Add(word);
                        else
                            baskets.Add(word[depth], new List<string> { word });
                    }
                    else
                    {
                        baskets[(char)96].Add(word);
                    }
                }

                if (baskets[(char)96].Count == array.Count)
                    return array;

                List<string> output = new List<string>();
                for (int i = 96; i <= 122; i++)
                {
                    if (baskets.ContainsKey((char)i))
                    {
                        foreach (string word in GetRecursiveMsdSortedArray(baskets[(char)i], depth + 1))
                            output.Add(word);
                    }
                }

                return output;
            }
            else
            {
                return array;
            }
        }

        private static List<Queue<string>> GetSortedSubArrays(string filePath)
        {
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
                } while (characterInt != -1 && !(
                (characterInt >= 65 && characterInt <= 90) ||
                (characterInt >= 97 && characterInt <= 122) ||
                characterInt == 32 || characterInt == 13));
                char character = Char.ToLower((char)characterInt);

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
            LinkedList<string> output = new LinkedList<string>();

            StreamReader file = new StreamReader(filePath);
            StringBuilder subString = new StringBuilder();
            int characterInt = 0;
            while (characterInt != -1)
            {
                do
                {
                    characterInt = file.Read();
                } while (characterInt != -1 && !(
                (characterInt >= 65 && characterInt <= 90) ||
                (characterInt >= 97 && characterInt <= 122) ||
                characterInt == 32 || characterInt == 13));
                char character = Char.ToLower((char)characterInt);

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
        Merges,
        MSD
    }
}
