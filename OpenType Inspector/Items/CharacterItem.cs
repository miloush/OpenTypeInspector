namespace OpenTypeInspector
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows.Media;

    public class CharacterItem
    {
        private static Dictionary<int, string> _characterNames;

        static CharacterItem()
        {
            _characterNames = new Dictionary<int, string>(24428);
            byte[] characterNamesResource = UnicodeResources.CharacterNames;

            using (BinaryReader reader = new BinaryReader(new MemoryStream(characterNamesResource)))
                while (reader.BaseStream.Position < characterNamesResource.Length)
                    _characterNames[reader.ReadInt32()] = reader.ReadString();
        }

        private GlyphTypeface _typeface;
        public Uri FontUri { get { return _typeface.FontUri; } }

        private readonly ushort[] _glyphID;
        public ushort GlyphID { get { return _glyphID[0]; } }

        public ushort[] GlyphIndices { get { return _glyphID; } }

        private int _codepoint;
        public int Codepoint { get { return _codepoint; } }
        public string Character { get { return char.ConvertFromUtf32(_codepoint); } }

        public string CharacterName
        {
            get
            {
                string name = null;

                if (_characterNames.TryGetValue(_codepoint, out name))
                    return name;

                return null;
            }
        }

        public CharacterItem(int codepoint, GlyphTypeface typeface)
        {
            _codepoint = codepoint;
            _typeface = typeface;
            _glyphID = new ushort[1] { typeface.CharacterToGlyphMap[codepoint] };
        }
    }
}
