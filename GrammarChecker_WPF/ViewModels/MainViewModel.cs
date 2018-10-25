using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;
using DevExpress.Mvvm;

namespace GrammarChecker_WPF.ViewModels
{
    class MainViewModel:ViewModelBase
    {
        public ObservableCollection<IncorrectWordViewModel> IncorrectWords { get; set; }
        public HashSet<string> Dictionary => WordDictionary.HashSet;

        public string Text { get; set; }
       
    }
}
