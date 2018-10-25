using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using DevExpress.Mvvm.POCO;

namespace GrammarChecker_WPF
{

    class DictionaryItem:IComparable
    {
        public readonly string Word;
        public readonly int Frequency;

        public DictionaryItem(string word, int frequency)
        {
            Word = word;
            Frequency = frequency;
        }

        public int CompareTo(object obj)
        {
            var a = (DictionaryItem)obj;
            
            if (Frequency > a.Frequency)
                return -1;
            if (Frequency < a.Frequency)
                return 1;
            else
                return 0;
        }

        public override string ToString()
        {
            return Word + "\t" + Frequency;
        }
    }

    static class WordDictionary
    {
        public static HashSet<string> HashSet = new HashSet<string>();
        private static SortedList<char, List<DictionaryItem>> dictionary;
        private static string path =
            @"C:\Users\nnuda\source\repos\LABS_sem5\GrammarChecker_WPF\GrammarChecker_WPF\Dictionary\WordDictionary.txt";

        static WordDictionary()
        {
            dictionary=new SortedList<char, List<DictionaryItem>>();
            LoadDictionary();

        }
        private static void LoadDictionary()
        {
            using (StreamReader reader = new StreamReader(new FileStream(path,FileMode.Open),Encoding.Default))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().ToLower().Trim()
                        .Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                    var word = line[0];
                    var firstLetter = word[0];
                    var frequency = Convert.ToInt32(line[1]);
                    if (!dictionary.ContainsKey(firstLetter))
                    {
                        var newList = new List<DictionaryItem>();
                        dictionary.Add(firstLetter, newList);
                    }
                    HashSet.Add(word);
                    dictionary[firstLetter].Add(new DictionaryItem(word, frequency));
                }
            }
            foreach (var list in dictionary.Values)
            {
                list.Sort();
            }
        }

        public static void SaveDictionary()
        {
            using (var writer = new StreamWriter(new FileStream(path, FileMode.Open), Encoding.Default))
            {
                foreach (var list in dictionary.Values)
                {
                    foreach (var item in list)
                    {
                        writer.WriteLine();
                    }
                }   
            }
        }

        public static List<string> GetWordContinuations(string word)
        {
            var continuationWords = new List<string>();
            if (word.Length != 0 && dictionary.ContainsKey(word.First())) 
            foreach (var wordFromDictionary in dictionary[word.First()])
            {
                if (continuationWords.Count >=100) return continuationWords;
                if(wordFromDictionary.Word.StartsWith(word))
                    continuationWords.Add(wordFromDictionary.Word);
            }
            return continuationWords;
        }

        static bool ContainsLoop(List<DictionaryItem> list, string value)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Word == value)
                {
                    return true;
                }
            }
            return false;
        }

        public static IEnumerable<string> GetSimularWords(string str)
        {
            var simularWords = new List<string>();
            
            if ( str.Length == 0)
                return simularWords;
            if (!dictionary.ContainsKey(str[0]))
            return simularWords;

            var list = dictionary[str[0]];

            for (int k = 0; k < str.Length; k++)
            {
                var regexWord = new StringBuilder(str);
                regexWord.Remove(k, 1);   
                regexWord.Insert(k, "\\w{1}");
         
                var regex = new Regex(regexWord.ToString());
               
                simularWords.AddRange(from t in list where regex.IsMatch(t.Word) && str.Length == t.Word.Length select t.Word);
            }
            return simularWords;
        }

        public static void Add(string word)
        {
            if (word.Length == 0) return;
            if (dictionary.ContainsKey(word[0]))
                dictionary[word[0]].Add(new DictionaryItem(word,1));
        }

        public static bool Contains(string word)
        {
            if (word.Length == 0) return true;
            if(dictionary.ContainsKey(word[0]))
            if (ContainsLoop(dictionary[word[0]],word))
                return true;
            return false;
        }
    }
}
