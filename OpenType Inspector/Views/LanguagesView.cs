namespace OpenTypeInspector
{
    using System.Linq;
    using System.Collections.Generic;
    using System.Windows.Data;

    public class LanguagesView
    {
        private List<LanguageItem> _items;
        private ListCollectionView _itemsView;
        private List<LanguageItem> Items => _items;
        public ListCollectionView ItemsView => _itemsView ??= CreateCollectionView(Items);

        public LanguagesView(IEnumerable<ScriptItem> items)
        {
            _items = items.SelectMany(item => item.AllLanguages).ToList();
        }

        protected ListCollectionView CreateCollectionView(List<LanguageItem> items)
        {
            ListCollectionView itemsView = new ListCollectionView(items);
            itemsView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(LanguageItem.GroupingHeader)));
            return itemsView;
        }
    }
}
