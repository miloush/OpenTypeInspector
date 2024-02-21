namespace OpenTypeInspector
{
    using System.Windows;
    using System.Windows.Controls;

    public partial class GlyphBox : UserControl
    {
        public static readonly DependencyProperty GlyphSizeProperty = DependencyProperty.Register("GlyphSize", typeof(double), typeof(GlyphBox), new FrameworkPropertyMetadata(50.0, OnGlyphSizeChanged));
        public static readonly DependencyProperty GlyphItemProperty = DependencyProperty.Register("GlyphItem", typeof(GlyphItem), typeof(GlyphBox), new PropertyMetadata());
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(GlyphBox), new PropertyMetadata(Orientation.Horizontal));

        private static void OnGlyphSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GlyphBox @this = (GlyphBox)d;

            @this.PathBox.Width = @this.PathBox.Height = (double)e.NewValue;
        }
        
        public double GlyphSize
        {
            get { return (double)GetValue(GlyphSizeProperty); }
            set { SetValue(GlyphSizeProperty, value); }
        }

        public GlyphItem GlyphItem
        {
            get { return (GlyphItem)GetValue(GlyphItemProperty); }
            set { SetValue(GlyphItemProperty, value); }
        }

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public GlyphBox()
        {
            InitializeComponent();
        }
    }
}
