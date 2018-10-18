using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GrammarChecker_WPF.ViewModels;


namespace GrammarChecker_WPF.AutoCompleteTextBox

{
    public static class AutoCompleteBehavior
    {
        private static TextChangedEventHandler onTextChanged = new TextChangedEventHandler(OnTextChanged);
        private static KeyEventHandler onKeyDown = new KeyEventHandler(OnPreviewKeyDown);
        private static string matchingString = "";
        private static string match;
        private static List<string> HintWords = new List<string>();
        private static string CurrentWord = "";
        private static int CurrentWordPosition = 0;
        private static TextBox textBox;

        public static readonly DependencyProperty AutoCompleteEnabledProperty =
            DependencyProperty.RegisterAttached("AutoCompleteEnabled",
                typeof(bool), typeof(AutoCompleteBehavior),
                new PropertyMetadata(false, OnAutoComplete));

        public static readonly DependencyProperty AutoCompleteIncorrectWordsProperty =
            DependencyProperty.RegisterAttached("IncorrectWords",
                typeof(IEnumerable<IncorrectWordViewModel>), typeof(AutoCompleteBehavior),
                new UIPropertyMetadata(null, OnIncorrectWordFound));

        private static void OnIncorrectWordFound(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
           
        }

        #region Items Source

        public static bool GetAutoCompleteEnabled(DependencyObject obj)
        {
            return (bool) obj.GetValue(AutoCompleteEnabledProperty);
        }

        public static void SetAutoCompleteEnabled(DependencyObject obj,
            bool value)
        {
            obj.SetValue(AutoCompleteEnabledProperty, value);
        }

        public static IEnumerable<IncorrectWordViewModel> GetIncorrectWords(DependencyObject obj)
        {
            object objRtn = obj.GetValue(AutoCompleteIncorrectWordsProperty);
            if (objRtn is IEnumerable<IncorrectWordViewModel>)
                return (objRtn as IEnumerable<IncorrectWordViewModel>);

            return new ObservableCollection<IncorrectWordViewModel>();
        }

        

        public static void SetIncorrectWords(DependencyObject obj, IEnumerable<IncorrectWordViewModel> value)
        {
            obj.SetValue(AutoCompleteIncorrectWordsProperty, value);
        }

        private static void OnAutoComplete(object sender, DependencyPropertyChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (sender == null)
                return;

            //If we're being removed, remove the callbacks
            //Remove our old handler, regardless of if we have a new one.
            tb.TextChanged -= onTextChanged;
            tb.PreviewKeyDown -= onKeyDown;
            if (e.NewValue != null)
            {
                //New source.  Add the callbacks
                tb.TextChanged += onTextChanged;
                tb.PreviewKeyDown += onKeyDown;
            }
        }

        #endregion





        static void CheckGrammar(object obj,string str)
        {

            TextBox tb = obj as TextBox;

            if (tb == null)
                return;
            var words = str.Split();
            var items = new ObservableCollection<IncorrectWordViewModel>();
            textBox = tb;
            foreach (var word in words)
            {
                var incorrectWord= new IncorrectWordViewModel(word);
                if (!WordDictionary.Contains(word))
                {
                    if (!items.Contains(incorrectWord))
                    {
                        incorrectWord.WordChanged += ChangeText;
                        items.Add(incorrectWord);
                    }
                }
            }
            SetIncorrectWords(tb, items);
        }

    

        static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox tb = e.OriginalSource as TextBox;
            if (!GetAutoCompleteEnabled(tb))
                return;

            if (tb == null)
                return;

            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                CheckGrammar(tb,tb.Text);
            }

            if (e.Key == Key.RightAlt && GetCurrentWord(tb.Text,tb.CaretIndex).StartsWith(matchingString))
            {
                tb.TextChanged -= onTextChanged;
                tb.Text = ReplaceToNext(tb.Text, tb.CaretIndex);
                tb.CaretIndex = CurrentWordPosition + CurrentWord.Length;
                tb.SelectionStart = CurrentWordPosition + matchingString.Length;
                tb.SelectionLength = CurrentWord.Length - matchingString.Length;
                tb.TextChanged += onTextChanged;
            }
                

            if (e.Key != Key.RightShift)
                return;

            if (tb.SelectionLength > 0 )
            {
                tb.CaretIndex = tb.SelectionStart = tb.SelectionStart + tb.SelectionLength;
                tb.SelectionLength = 0;
            }
        }

        static void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if(!GetAutoCompleteEnabled(sender as TextBox)) 
                return;

            if
            (
                (from change in e.Changes where change.RemovedLength > 0 select change).Any() &&
                (from change in e.Changes where change.AddedLength > 0 select change).Any() == false
            ) // procs on bacspace 
                return;

            TextBox tb = e.OriginalSource as TextBox;

            if(tb != null && tb.Text.EndsWith(" ")) 
                return;

            if (sender == null)
                return;

            if (String.IsNullOrEmpty(tb?.Text))
                return;

            matchingString = GetCurrentWord(tb.Text, tb.CaretIndex);

            HintWords = new List<string>(WordDictionary.GetWordContinuations(matchingString));

            if (!HintWords.Any())
            {
                return;
            }

            CurrentWordPosition = GetWordStartPosition(tb.Text, tb.CaretIndex); 

            if (String.IsNullOrEmpty(matchingString))
                return;

            Int32 wordLength = matchingString.Length;

            match =
            (
                from
                    value
                    in
                    (
                        from subvalue
                            in HintWords
                        where subvalue != null && subvalue.Length >= wordLength
                        select subvalue
                    )
                where value.Substring(0, wordLength).Equals(matchingString)
                select value.Substring(wordLength,
                    value.Length - wordLength)).FirstOrDefault();

            if (String.IsNullOrEmpty(match))
                return;

            int matchStart = (CurrentWordPosition + matchingString.Length);

            tb.TextChanged -= onTextChanged;
            tb.Text = tb.Text.Insert(tb.CaretIndex, match);
            CurrentWord = matchingString + match;
            tb.CaretIndex = matchStart;
            tb.SelectionStart = matchStart;

            tb.SelectionLength = (match.Length);
            tb.TextChanged += onTextChanged;
        }

        #region Helper Methods

        private static string GetCurrentWord(string text, int index)
        {
            var word = "";

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == ' ' || text[i] == '\r' || text[i] == '\n' || text[i] == '\t')
                {
                    if (i >= index - 1) return word.Trim();
                    word = "";
                }

                word += text[i];
            }

            return word.Trim();
        }

        private static int GetWordStartPosition(string text, int index)
        {
            if (index == 0) return 0;
            for (int i = index - 1; i > 0; i--)
            {
                if (text[i] == ' ' || text[i] == '\r' || text[i] == '\n' || text[i] == '\t')
                {
                    return i + 1;
                }

            }

            return 0;
        }

        private static string ReplaceToNext(string text, int index)
        {
            if (HintWords.Count == 0) return text;
            text =text.Remove(CurrentWordPosition, GetCurrentWord(text, index).Length);
            CurrentWord = GetNextWord();
            text = text.Insert(CurrentWordPosition, CurrentWord);
            return text;
        }


        private static int currentWordId = 1;
        private static string GetNextWord()
        {
            if (currentWordId >= HintWords.Count)
                currentWordId = 0;
            return HintWords[currentWordId++];
        }

        private static void ChangeText(string incorrectWord, string newWord)
        {
            
            textBox.Text = textBox.Text.Replace(incorrectWord, newWord);
        }
        #endregion

    }
}