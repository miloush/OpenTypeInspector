namespace OpenTypeInspector
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;

    public class LookupItem
    {
        public GlyphTypefaceInspector.LookupTableInspector Inspector { get; }
        public int LookupIndex { get; set; }
        public object LookupType { get; set; }

        public LookupItem(GlyphTypefaceInspector.LookupTableInspector inspector)
        {
            Inspector = inspector;
            LookupIndex = -1;
        }
        public LookupItem(GlyphTypefaceInspector.LookupTableInspector inspector, object lookup) : this(inspector)
        {
            Initialize(lookup);
        }
        public LookupItem(GlyphTypefaceInspector.LookupTableInspector inspector, ushort lookupIndex) : this(inspector)
        {
            LookupIndex = lookupIndex;

            object lookup = inspector.GetLookup(lookupIndex);
            Initialize(lookup);
        }
        private void Initialize(object lookup)
        {
            LookupType = Inspector.GetLookupTypeEnum(lookup) + " (" + Inspector.GetLookupSubtableCount(lookup) + ")";
        }

        public TextBlock LookupBlock
        {
            get
            {
                TextBlock text = new TextBlock();
                text.Inlines.Add(new Hyperlink(new Run(LookupIndex.ToString())) { Command = ApplicationCommands.Find, CommandParameter = this });
                text.Inlines.Add(new Run(" "));
                text.Inlines.Add(new Run(LookupType.ToString()));
                return text;
            }
        }
    }
}
