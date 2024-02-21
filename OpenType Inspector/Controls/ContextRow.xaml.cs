namespace OpenTypeInspector
{
    using System.Windows;
    using System.Windows.Controls;

    public partial class ContextRow : UserControl
    {
        public static readonly DependencyProperty GlyphSizeProperty = DependencyProperty.Register("GlyphSize", typeof(double), typeof(ContextRow), new PropertyMetadata(20.0));
        public static readonly DependencyProperty ContextItemProperty = DependencyProperty.Register("ContextItem", typeof(ContextItem), typeof(ContextRow), new PropertyMetadata());
        public static readonly DependencyProperty MaxRowSizeProperty = DependencyProperty.Register("MaxRowSize", typeof(double), typeof(ContextRow), new PropertyMetadata(150.0));

        public double MaxRowSize
        {
            get { return (double)GetValue(MaxRowSizeProperty); }
            set { SetValue(MaxRowSizeProperty, value); }
        }

        public double GlyphSize
        {
            get { return (double)GetValue(GlyphSizeProperty); }
            set { SetValue(GlyphSizeProperty, value); }
        }

        public ContextItem ContextItem
        {
            get { return (ContextItem)GetValue(ContextItemProperty); }
            set { SetValue(ContextItemProperty, value); }
        }

        public ContextRow()
        {
            InitializeComponent();
        }
    }
}
