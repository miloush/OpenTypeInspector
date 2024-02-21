namespace OpenTypeInspector
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Threading;

    public partial class MainWindow : Window
    {
        private static readonly string FoldersPath = Path.Combine(Path.GetTempPath(), "OTI.txt");

        private volatile int _loadingIndex = -1;

        private ObservableCollection<FontItem> _fonts;
        private CollectionView _fontsView;
        private CollectionView _fontsSearchView;

        public CollectionView FontsView { get { return _fontsView; } }
        public CollectionView FontsSearchView { get { return _fontsSearchView; } }

        public MainWindow()
        {
            _fonts = new ObservableCollection<FontItem>();

            _fontsView = new ListCollectionView(_fonts);
            _fontsView.GroupDescriptions.Add(new PropertyGroupDescription("FamilyName"));

            _fontsSearchView = new ListCollectionView(_fonts);
            _fontsSearchView.Filter = SearchFilter;

            InitializeComponent();

            FolderBox.Items.Insert(0, Environment.GetFolderPath(Environment.SpecialFolder.Fonts));

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string localFonts = Path.Combine(appData, "Microsoft", "Windows", "Fonts");
            if (Directory.Exists(localFonts))
                FolderBox.Items.Insert(1, localFonts);

            if (File.Exists(FoldersPath))
                try
                {
                    foreach (string line in File.ReadAllLines(FoldersPath))
                        FolderBox.Items.Insert(FolderBox.Items.Count - 1, line);
                }
                catch { }

            FolderBox.SelectedIndex = 0;
        }

        private void Load(object dirarg)
        {
            string dir = (string)dirarg;
            int loadingIndex = _loadingIndex;

            if (!Directory.Exists(dir))
                return;

            foreach (string path in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories))
            {
                if (path.Contains("BadFonts"))
                    continue;

                if (loadingIndex != _loadingIndex)
                    break;

                Load(path, loadingIndex, force: false);
            }
        }
        private void Load(string path, int matchingIndex, bool force)
        {
            GlyphTypeface typeface = null;

            try
            {
                typeface = new GlyphTypeface(new Uri(path));

                FontItem item = new FontItem(typeface);

                if (force || item.Substitutions.Counts.Count > 0) // otherwise we get plenty fonts with no substitutions, however, it is confusing we ignore such fonts on explicit file drop
                    Dispatcher.Invoke(new Action<FontItem, int>((font, index) =>
                    {
                        if (index < 0 || index == _loadingIndex)
                            _fonts.Add(font);
                    }), DispatcherPriority.Background, item, matchingIndex);
            }
            catch (Exception e) {
                if (force)
                    MessageBox.Show(this, e.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
                                                                                                        
                Debug.WriteLine($"{path}: {e.GetBaseException().Message}");
            }
        }

        private void OnSubGridSorting(object sender, DataGridSortingEventArgs e)
        {
            OnGridSorting(e.Column, FamilyNameSubColumn);
        }
        private void OnPosGridSorting(object sender, DataGridSortingEventArgs e)
        {
            OnGridSorting(e.Column, FamilyNamePosColumn);
        }
        private void OnDefGridSorting(object sender, DataGridSortingEventArgs e)
        {
            OnGridSorting(e.Column, FamilyNameDefColumn);
        }
        private void OnGridSorting(DataGridColumn sortedColumn, DataGridTextColumn familyNameColumn)
        {
            if (sortedColumn == familyNameColumn)
            {
                familyNameColumn.Visibility = Visibility.Collapsed;

                _fontsView.GroupDescriptions.Add(new PropertyGroupDescription("FamilyName"));
            }
            else
            {
                _fontsView.GroupDescriptions.Clear();

                familyNameColumn.Visibility = Visibility.Visible;
            }
        }

        private void OnCharacterRowDoubleClick(object sender, MouseEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            if (row == null)
                return;

            TypingBox.AppendText(((CharacterItem)row.Item).Character);
        }

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                _loadingIndex = -1; // cancel loading
        }

        // consider remembering when the drag is from DataGrid and disable drop to avoid loading already loaded file
        // (this and the next method had if (e.Source is DataGrid) return; but it was removed, unclear why)
        private void OnFontDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;

            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
                e.Effects = DragDropEffects.Link;
        }
        private void OnFontDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, true);

                foreach (string file in files)
                    Load(file, -1, true);
            }
        }
        private void OnFontDrag(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DataGridRow row = (DataGridRow)sender;

                FontItem item = (FontItem)row.DataContext;
                DragDrop.DoDragDrop(row, new DataObject(DataFormats.FileDrop, new string[] { item.Typeface.FontUri.AbsolutePath }, true), DragDropEffects.Copy);
            }
        }

        private void OnFindItem(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO: support positionining features
            FontItem item = FontsSubGrid.SelectedItem as FontItem;
            if (item == null)
                return;

            //FontItem.LookupTableCounts tableCounts = item.Substitutions;

            //switch (e.Parameter)
            //{
            //    case LookupItem lookup:
            //        tableCounts = lookup.Inspector is GlyphSubstitutionsInspector ? item.Substitutions : item.Positioning;
            //        break;
            //    case FeatureItem feature:
            //        tableCounts = feature.Inspector is GlyphSubstitutionsInspector ? item.Substitutions : item.Positioning;
            //        break;
            //}

            //object view = tableCounts.FindView(lookup);
            //if (view == null)
            //    return;

            //foreach (TabItem tab in DetailTabs.Items)
            //{
            //    FrameworkElement element = tab.Content as FrameworkElement;
            //    if (element.DataContext == view)
            //    {
            //        DetailTabs.SelectedItem = tab;
            //        break;
            //    }
            //}
        }

        private void OnFolderSelected(object sender, SelectionChangedEventArgs e)
        {

            if (FolderBox.SelectedItem is ComboBoxItem)
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                var result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string dir = dialog.SelectedPath;
                    try { File.AppendAllText(FoldersPath, dir + Environment.NewLine); }
                    catch { }

                    FolderBox.Items.Insert(FolderBox.Items.Count - 1, dir);
                    FolderBox.SelectedIndex = FolderBox.Items.Count - 2;
                }
            }
            else
            {
                _loadingIndex = FolderBox.SelectedIndex;
                _fonts.Clear();
                string dir = (string)FolderBox.SelectedItem;
                new Thread(Load) { IsBackground = true }.Start(dir);
            }
        }

        private string _scriptPattern;
        private string _languagePattern;
        private string _featurePattern;
        private string _charactersPattern;
        private SortedSet<int> _codepointsPattern;

        private void Search(object sender, RoutedEventArgs e)
        {
            _scriptPattern = SearchScript.Text;
            _languagePattern = SearchLanguage.Text;
            _featurePattern = SearchFeature.Text;

            string searchCharacters = SearchCharacters.Text;

            _charactersPattern = string.Empty;
            _codepointsPattern = new SortedSet<int>();
            if (!string.IsNullOrEmpty(searchCharacters))
                foreach (string token in searchCharacters.Split(null as char[], StringSplitOptions.RemoveEmptyEntries))
                {
                    int number;
                    if (int.TryParse(token, out number) && number >= 0)
                        _codepointsPattern.Add(number);

                    if (int.TryParse(token, NumberStyles.AllowHexSpecifier, null, out number) && number >= 0)
                        _codepointsPattern.Add(number);

                    else
                        _charactersPattern += token;
                }

            _fontsSearchView.Refresh();
        }
        private bool SearchFilter(object obj)
        {
            FontItem item = (FontItem)obj;
            GlyphTypeface typeface = item.Typeface;

            bool scriptMatch = string.IsNullOrEmpty(_scriptPattern);
            bool languageMatch = string.IsNullOrEmpty(_languagePattern);
            bool featureMatch = string.IsNullOrEmpty(_featurePattern);
            bool charactersMatch = string.IsNullOrEmpty(_charactersPattern);

            if (!scriptMatch)
                foreach (TaggedObject script in item.TypefaceInspector.Substitutions.EnumerateScripts())
                    if (Regex.IsMatch(GlyphTypefaceInspector.ToTagString(script.Tag), _scriptPattern))
                    {
                        scriptMatch = true;
                        break;
                    }
            if (!scriptMatch) return false;

            if (!languageMatch)
                foreach (TaggedObject script in item.TypefaceInspector.Substitutions.EnumerateScripts())
                    foreach (TaggedObject language in item.TypefaceInspector.Substitutions.EnumerateLanguages(script.Object))
                        if (Regex.IsMatch(GlyphTypefaceInspector.ToTagString(language.Tag), _languagePattern))
                        {
                            languageMatch = true;
                            break;
                        }
            if (!languageMatch) return false;

            if (!featureMatch)
                foreach (TaggedObject feature in item.TypefaceInspector.Substitutions.EnumerateFeatures())
                    if (Regex.IsMatch(GlyphTypefaceInspector.ToTagString(feature.Tag), _featurePattern))
                    {
                        featureMatch = true;
                        break;
                    }
            if (!featureMatch) return false;

            if (!charactersMatch)
            {
                foreach (int codepoint in _codepointsPattern)
                    if (item.Typeface.CharacterToGlyphMap.ContainsKey(codepoint))
                    {
                        charactersMatch = true;
                        break;
                    }

                if (!charactersMatch)
                    for (int i = 0; i < _charactersPattern.Length; i++)
                        if (!char.IsHighSurrogate(_charactersPattern, i) || (i < _charactersPattern.Length - 1 && char.IsLowSurrogate(_charactersPattern, i + 1)))
                        {
                            int codepoint = char.ConvertToUtf32(_charactersPattern, i);
                            if (item.Typeface.CharacterToGlyphMap.ContainsKey(codepoint))
                            {
                                charactersMatch = true;
                                break;
                            }

                            if (codepoint > char.MaxValue)
                                i++; //surrogate catch up
                        }
            }
            if (!charactersMatch) return false;

            return true;
        }

        private void TransformHex(object sender, KeyEventArgs e)
        {
            if (e.SystemKey == Key.X)
                if (sender is TextBox box)
                    if (box.SelectionLength > 0) // if something is selected, toggle between hex and chars
                    {
                        if (box.SelectionLength >= 4 && box.SelectionLength <= 8 && int.TryParse(box.SelectedText, NumberStyles.HexNumber, null, out int codepoint))
                        {
                            box.SelectedText = char.ConvertFromUtf32(codepoint);
                            e.Handled = true;
                            return;
                        }
                        else if (box.SelectedText.Length == 2 && char.IsSurrogatePair(box.SelectedText, 0) || box.SelectedText.Length == 1)
                        {
                            box.SelectedText = char.ConvertToUtf32(box.SelectedText, 0).ToString("X4");
                            e.Handled = true;
                            return;
                        }
                    }
                    else if (box.CaretIndex > 0) // if nothing is selected, convert the last char into hex (one way only)
                    {
                        int currentIndex = box.CaretIndex;
                        string replacement;
                        if (box.CaretIndex > 1 && char.IsSurrogatePair(box.Text, box.CaretIndex - 2))
                        {
                            currentIndex -= 2;
                            replacement = char.ConvertToUtf32(box.Text, box.CaretIndex - 2).ToString("X4");
                            box.Text = box.Text.Substring(0, box.CaretIndex - 2) + replacement + box.Text.Substring(box.CaretIndex);
                        }
                        else
                        {
                            currentIndex--;
                            replacement = ((int)box.Text[box.CaretIndex - 1]).ToString("X4");
                            box.Text = box.Text.Substring(0, box.CaretIndex - 1) + replacement + box.Text.Substring(box.CaretIndex);
                        }

                        box.CaretIndex += currentIndex + replacement.Length;
                        e.Handled = true;
                    }
        }

        private void OnCopyFilePaths(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem copyItem)
                if (copyItem.Parent is ContextMenu menu)
                    if (menu.PlacementTarget is MultiSelector selector)
                    {
                        string paths = string.Join("\r\n", selector.SelectedItems.OfType<FontItem>().Select(item => item.FileInfo.FullName));
                        Clipboard.SetText(paths);
                    }
        }
    }
}

