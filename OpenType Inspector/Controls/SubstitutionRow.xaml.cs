namespace OpenTypeInspector
{
    using System.Windows;
    using System.Windows.Controls;

    public partial class SubstitutionRow : UserControl
    {
        public static readonly DependencyProperty GlyphSizeProperty = DependencyProperty.Register("GlyphSize", typeof(double), typeof(SubstitutionRow), new PropertyMetadata(20.0));
        public static readonly DependencyProperty SubstitutionItemProperty = DependencyProperty.Register("SubstitutionItem", typeof(SubstitutionItem), typeof(SubstitutionRow), new PropertyMetadata());

        public double GlyphSize
        {
            get { return (double)GetValue(GlyphSizeProperty); }
            set { SetValue(GlyphSizeProperty, value); }
        }

        public SubstitutionItem SubstitutionItem
        {
            get { return (SubstitutionItem)GetValue(SubstitutionItemProperty); }
            set { SetValue(SubstitutionItemProperty, value); }
        }

        public SubstitutionRow()
        {
            InitializeComponent();
        }
    }
}
