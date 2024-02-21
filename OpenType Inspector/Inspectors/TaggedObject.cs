namespace OpenTypeInspector
{
    public struct TaggedObject
    {
        public uint Tag { get; set; }
        public string TagString => GlyphTypefaceInspector.ToTagString(Tag);

        public object Object { get; set; }

        public TaggedObject() { }
        public TaggedObject(uint tag, object obj)
        {
            Tag = tag;
            Object = obj;
        }

        public override string ToString() => TagString;
    }
}
