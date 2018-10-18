using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DevExpress.Mvvm;

namespace GrammarChecker_WPF.ViewModels
{
    public class IncorrectWordViewModel
    {
        public string Word { get; set; }
        public IList<string> CorrectedWords { get; private set; }

        public IncorrectWordViewModel(string word)
        {
            CorrectedWords=new List<string>(WordDictionary.GetSimularWords(word));
            CorrectedWords.Insert(0,"Добавить в словарь");
            Word = word;
        }



        public ICommand IncorrectWordCommand
        {
            get
            {
                return new DelegateCommand<object>((obj) =>
                {
                    var str = obj as string;
                    if (str == "Добавить в словарь")
                    {
                        WordDictionary.Add(Word);
                    }
                    else
                    {
                        WordChanged?.Invoke(Word,str);
                    }

                });
            }
        }

        public event Action<string, string> WordChanged;
    }
}
