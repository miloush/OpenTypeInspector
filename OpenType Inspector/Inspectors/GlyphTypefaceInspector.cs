namespace OpenTypeInspector
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Windows.Media;
    using IEnumerable = System.Collections.IEnumerable;

    public partial class GlyphTypefaceInspector
    {
        public enum ContextSubtableType : ushort
        {
            GlyphBased = 1,
            ClassBased = 2,
            CoverageBased = 3
        }

        #region Known Feature Names

        private static readonly Dictionary<uint, string> KnownFeatureNames = new Dictionary<uint, string>
        {
            { 1633774708, "Access All Alternates" },
            { 1633842790, "Above-base Forms" },
            { 1633842797, "Above-base Mark Positioning" },
            { 1633842803, "Above-base Substitutions" },
            { 1634103907, "Alternative Fractions" },
            { 1634429038, "Akhands" },
            { 1651275622, "Below-base Forms" },
            { 1651275629, "Below-base Mark Positioning" },
            { 1651275635, "Below-base Substitutions" },
            { 1667329140, "Contextual Alternates" },
            { 1667330917, "Case-Sensitive Forms" },
            { 1667460464, "Glyph Composition / Decomposition" },
            { 1667654002, "Conjunct Form After Ro" },
            { 1667916660, "Conjunct Forms" },
            { 1668049255, "Contextual Ligatures" },
            { 1668309876, "Centered CJK Punctuation" },
            { 1668313968, "Capital Spacing" },
            { 1668511592, "Contextual Swash" },
            { 1668641395, "Cursive Positioning" },
            { 1668689969, "Character Variants" },
            { 1664249955, "Petite Capitals From Capitals" },
            { 1664250723, "Small Capitals From Capitals" },
            { 1684632436, "Distances" },
            { 1684826471, "Discretionary Ligatures" },
            { 1684959085, "Denominators" },
            { 1702391924, "Expert Forms" },
            { 1717660788, "Final Glyph on Line Alternates" },
            { 1718185522, "Terminal Forms #2" },
            { 1718185523, "Terminal Forms #3" },
            { 1718185569, "Terminal Forms" },
            { 1718772067, "Fractions" },
            { 1719101796, "Full Widths" },
            { 1751215206, "Half Forms" },
            { 1751215214, "Halant Forms" },
            { 1751215220, "Alternate Half Widths" },
            { 1751741300, "Historical Forms" },
            { 1751871073, "Horizontal Kana Alternates" },
            { 1751935335, "Historical Ligatures" },
            { 1752065900, "Hangul" },
            { 1752132207, "Hojo Kanji Forms (JIS X 0212-1990 Kanji Forms)" },
            { 1752656228, "Half Widths" },
            { 1768843636, "Initial Forms" },
            { 1769172844, "Isolated Forms" },
            { 1769234796, "Italics" },
            { 1784769652, "Justification Alternates" },
            { 1785739064, "JIS78 Forms" },
            { 1785739315, "JIS83 Forms" },
            { 1785739568, "JIS90 Forms" },
            { 1785737268, "JIS2004 Forms" },
            { 1801810542, "Kerning" },
            { 1818649188, "Left Bounds" },
            { 1818847073, "Standard Ligatures" },
            { 1818914159, "Leading Jamo Forms" },
            { 1819178349, "Lining Figures" },
            { 1819239276, "Localized Forms" },
            { 1819570785, "Left-to-right alternates" },
            { 1819570797, "Left-to-right mirrored forms" },
            { 1835102827, "Mark Positioning" },
            { 1835361330, "Medial Forms #2" },
            { 1835361385, "Medial Forms" },
            { 1835496043, "Mathematical Greek" },
            { 1835756907, "Mark to Mark Positioning" },
            { 1836279156, "Mark Positioning via Substitution" },
            { 1851878516, "Alternate Annotation Forms" },
            { 1852597099, "NLC Kanji Forms" },
            { 1853188980, "Nukta Forms" },
            { 1853189490, "Numerators" },
            { 1869509997, "Oldstyle Figures" },
            { 1869636196, "Optical Bounds" },
            { 1869767790, "Ordinals" },
            { 1869770349, "Ornaments" },
            { 1885432948, "Proportional Alternate Widths" },
            { 1885561200, "Petite Capitals" },
            { 1886088801, "Proportional Kana" },
            { 1886287213, "Proportional Figures" },
            { 1886545254, "Pre-Base Forms" },
            { 1886545267, "Pre-base Substitutions" },
            { 1886614630, "Post-base Forms" },
            { 1886614643, "Post-base Substitutions" },
            { 1886873956, "Proportional Widths" },
            { 1903651172, "Quarter Widths" },
            { 1918987876, "Randomize" },
            { 1919644262, "Rakar Forms" },
            { 1919707495, "Required Ligatures" },
            { 1919969382, "Reph Forms" },
            { 1920229988, "Right Bounds" },
            { 1920232545, "Right-to-left alternates" },
            { 1920232557, "Right-to-left mirrored forms" },
            { 1920295545, "Ruby Notation Forms" },
            { 1935764596, "Stylistic Alternates" },
            { 1936289382, "Scientific Inferiors" },
            { 1936292453, "Optical size" },
            { 1936548720, "Small Capitals" },
            { 1936552044, "Simplified Forms" },
            { 1937072755, "Subscript" },
            { 1937076339, "Superscript" },
            { 1937208168, "Swash" },
            { 1953068140, "Titling" },
            { 1953131887, "Trailing Jamo Forms" },
            { 1953390957, "Traditional Name Forms" },
            { 1953396077, "Tabular Figures" },
            { 1953653092, "Traditional Forms" },
            { 1953982820, "Third Widths" },
            { 1970170211, "Unicase" },
            { 1986096244, "Alternate Vertical Metrics" },
            { 1986098293, "Vattu Variants" },
            { 1986359924, "Vertical Writing" },
            { 1986552172, "Alternate Vertical Half Metrics" },
            { 1986686319, "Vowel Jamo Forms" },
            { 1986752097, "Vertical Kana Alternates" },
            { 1986753134, "Vertical Kerning" },
            { 1987076460, "Proportional Alternate Vertical Metrics" },
            { 1987212338, "Vertical Alternates and Rotation" },
            { 2053468783, "Slashed Zero" }
        };

        #endregion

        #region Known Scripts

        private static readonly Dictionary<uint, string> KnownScripts = new Dictionary<uint, string>
        {
            { 1634885986, "Arabic" },
            { 1634889070, "Armenian" },
            { 1635152756, "Avestan" },
            { 1650551913, "Balinese" },
            { 1650552181, "Bamum" },
            { 1650553963, "Batak" },
            { 1650814567, "Bengali" },
            { 1651402546, "Bengali v.2" },
            { 1651470447, "Bopomofo" },
            { 1651663209, "Braille" },
            { 1651663208, "Brahmi" },
            { 1651861353, "Buginese" },
            { 1651861604, "Buhid" },
            { 1652128365, "Byzantine Music" },
            { 1667329651, "Canadian Syllabics" },
            { 1667330665, "Carian" },
            { 1667328877, "Chakma" },
            { 1667785069, "Cham" },
            { 1667786098, "Cherokee" },
            { 1751215721, "CJK Ideographic" },
            { 1668247668, "Coptic" },
            { 1668313716, "Cypriot Syllabary" },
            { 1668903532, "Cyrillic" },
            { 1145457748, "Default" },
            { 1685287540, "Deseret" },
            { 1684371041, "Devanagari" },
            { 1684370994, "Devanagari v.2" },
            { 1701280112, "Egyptian heiroglyphs" },
            { 1702127721, "Ethiopic" },
            { 1734700914, "Georgian" },
            { 1735156071, "Glagolitic" },
            { 1735357544, "Gothic" },
            { 1735550315, "Greek" },
            { 1735748210, "Gujarati" },
            { 1735029298, "Gujarati v.2" },
            { 1735750261, "Gurmukhi" },
            { 1735750194, "Gurmukhi v.2" },
            { 1751215719, "Hangul" },
            { 1784769903, "Hangul Jamo" },
            { 1751215727, "Hanunoo" },
            { 1751474802, "Hebrew" },
            // { 1801547361, "Hiragana" },
            { 1634889065, "Imperial Aramaic" },
            { 1885891689, "Inscriptional Pahlavi" },
            { 1886549097, "Inscriptional Parthian" },
            { 1784772193, "Javanese" },
            { 1802791017, "Kaithi" },
            { 1802396769, "Kannada" },
            { 1802396722, "Kannada v.2" },
            { 1801547361, "Katakana / Hiragana" },
            { 1801546857, "Kayah Li" },
            { 1802002802, "Kharosthi" },
            { 1802005874, "Khmer" },
            { 1818324768, "Lao" },
            { 1818326126, "Latin" },
            { 1818587235, "Lepcha" },
            { 1818848610, "Limbu" },
            { 1818848866, "Linear B" },
            { 1818850165, "Lisu (Fraser)" },
            { 1819894633, "Lycian" },
            { 1819894889, "Lydian" },
            { 1835825517, "Malayalam" },
            { 1835822386, "Malayalam v.2" },
            { 1835101796, "Mandaic, Mandaean" },
            { 1835103336, "Mathematical Alphanumeric Symbols" },
            { 1836344681, "Meitei Mayek (Meithei, Meetei)" },
            { 1835364963, "Meroitic Cursive" },
            { 1835364975, "Meroitic Hieroglyphs" },
            { 1836019303, "Mongolian" },
            { 1836413795, "Musical Symbols" },
            { 1836674418, "Myanmar" },
            { 1952541813, "New Tai Lue" },
            { 1852534560, "N'Ko" },
            { 1869046125, "Ogham" },
            { 1869374315, "Ol Chiki" },
            { 1769234796, "Old Italic" },
            { 2020631919, "Old Persian Cuneiform" },
            { 1935766114, "Old South Arabian" },
            { 1869769576, "Old Turkic, Orkhon Runic" },
            { 1869773153, "Odia (formerly Oriya)" },
            { 1869773106, "Odia v.2 (formerly Oriya v.2)" },
            { 1869835617, "Osmanya" },
            { 1885888871, "Phags-pa" },
            { 1885892216, "Phoenician" },
            { 1919577703, "Rejang" },
            { 1920298610, "Runic" },
            { 1935764850, "Samaritan" },
            { 1935766898, "Saurashtra" },
            { 1936224868, "Sharada" },
            { 1936220535, "Shavian" },
            { 1936289384, "Sinhala" },
            { 1936683617, "Sora Sompeng" },
            { 2020832632, "Sumero-Akkadian Cuneiform" },
            { 1937075812, "Sundanese" },
            { 1937337455, "Syloti Nagri" },
            { 1937338979, "Syriac" },
            { 1952935015, "Tagalog" },
            { 1952540514, "Tagbanwa" },
            { 1952541797, "Tai Le" },
            { 1818324577, "Tai Tham (Lanna)" },
            { 1952544372, "Tai Viet" },
            { 1952541554, "Takri" },
            { 1952542060, "Tamil" },
            { 1953328178, "Tamil v.2" },
            { 1952803957, "Telugu" },
            { 1952803890, "Telugu v.2" },
            { 1952997729, "Thaana" },
            { 1952997737, "Thai" },
            { 1953063540, "Tibetan" },
            { 1952869991, "Tifinagh" },
            { 1969709426, "Ugaritic Cuneiform" },
            { 1986095392, "Vai" },
            { 2036932640, "Yi" },
        };

        #endregion

        #region Known Languages

        private static readonly Dictionary<uint, string> KnownLanguages = new Dictionary<uint, string>        
        {
            { 1684434036, "Default" },
            { 1094861088, "Abaza" },
            { 1094863648, "Abkhazian" },
            { 1094998304, "Adyghe" },
            { 1095125792, "Afrikaans" },
            { 1095127584, "Afar" },
            { 1095194400, "Agaw" },
            { 1095521056, "Alsatian" },
            { 1095521312, "Altai" },
            { 1095583776, "Amharic" },
            { 1095782472, "Phonetic transcription—Americanist conventions" },
            { 1095909664, "Arabic" },
            { 1095911712, "Aari" },
            { 1095912224, "Arakanese" },
            { 1095978272, "Assamese" },
            { 1096042528, "Athapaskan" },
            { 1096176160, "Avar" },
            { 1096237344, "Awadhi" },
            { 1096371488, "Aymara" },
            { 1096434976, "Azeri" },
            { 1111573536, "Badaga" },
            { 1111574304, "Baghelkhandi" },
            { 1111575584, "Balkar" },
            { 1111577888, "Baule" },
            { 1111642656, "Berber" },
            { 1111705632, "Bench" },
            { 1111708192, "Bible Cree" },
            { 1111837728, "Belarussian" },
            { 1111837984, "Bemba" },
            { 1111838240, "Bengali" },
            { 1111970336, "Bulgarian" },
            { 1112033568, "Bhili" },
            { 1112035104, "Bhojpuri" },
            { 1112099616, "Bikol" },
            { 1112099872, "Bilen" },
            { 1112229408, "Blackfoot" },
            { 1112295712, "Balochi" },
            { 1112296992, "Balante" },
            { 1112298528, "Balti" },
            { 1112359456, "Bambara" },
            { 1112362016, "Bamileke" },
            { 1112494880, "Bosnian" },
            { 1112687904, "Breton" },
            { 1112688672, "Brahui" },
            { 1112688928, "Braj Bhasha" },
            { 1112689952, "Burmese" },
            { 1112754208, "Bashkir" },
            { 1112820000, "Beti" },
            { 1128354848, "Catalan" },
            { 1128612384, "Cebuano" },
            { 1128809760, "Chechen" },
            { 1128810272, "Chaha Gurage" },
            { 1128810528, "Chattisgarhi" },
            { 1128810784, "Chichewa" },
            { 1128811296, "Chukchi" },
            { 1128812576, "Chipewyan" },
            { 1128813088, "Cherokee" },
            { 1128813856, "Chuvash" },
            { 1129140768, "Comorian" },
            { 1129271328, "Coptic" },
            { 1129272096, "Corsican" },
            { 1129465120, "Cree" },
            { 1129468448, "Carrier" },
            { 1129468960, "Crimean Tatar" },
            { 1129532448, "Church Slavonic" },
            { 1129535776, "Czech" },
            { 1145130528, "Danish" },
            { 1145131552, "Dargwa" },
            { 1145262624, "Woods Cree" },
            { 1145394464, "German" },
            { 1145524768, "Dogri" },
            { 1145591328, "Dhivehi" },
            { 1145656864, "Dhivehi" },
            { 1145721376, "Djerma" },
            { 1145980704, "Dangme" },
            { 1145981728, "Dinka" },
            { 1146243360, "Dari" },
            { 1146441248, "Dungan" },
            { 1146768928, "Dzongkha" },
            { 1161972000, "Ebira" },
            { 1162039840, "Eastern Cree" },
            { 1162104608, "Edo" },
            { 1162234144, "Efik" },
            { 1162628128, "Greek" },
            { 1162757920, "English" },
            { 1163024928, "Erzya" },
            { 1163087904, "Spanish" },
            { 1163151648, "Estonian" },
            { 1163219232, "Basque" },
            { 1163283232, "Evenki" },
            { 1163284000, "Even" },
            { 1163347232, "Ewe" },
            { 1178684960, "French Antillean" },
            { 1178685984, "Farsi" },
            { 1179209248, "Finnish" },
            { 1179273504, "Fijian" },
            { 1179403552, "Flemish" },
            { 1179534624, "Forest Nenets" },
            { 1179602464, "Fon" },
            { 1179603744, "Faroese" },
            { 1179795744, "French" },
            { 1179797792, "Frisian" },
            { 1179798560, "Friulian" },
            { 1179926816, "Futa" },
            { 1179995168, "Fulani" },
            { 1195459616, "Ga" },
            { 1195459872, "Gaelic" },
            { 1195460384, "Gagauz" },
            { 1195461664, "Galician" },
            { 1195463200, "Garshuni" },
            { 1195464480, "Garhwali" },
            { 1195727392, "Ge'ez" },
            { 1195985952, "Gilyak" },
            { 1196251680, "Gumuz" },
            { 1196379680, "Gondi" },
            { 1196576288, "Greenlandic" },
            { 1196576544, "Garo" },
            { 1196769568, "Guarani" },
            { 1196771872, "Gujarati" },
            { 1212238112, "Haitian" },
            { 1212238880, "Halam" },
            { 1212240416, "Harauti" },
            { 1212241184, "Hausa" },
            { 1212241696, "Hawaiin" },
            { 1212304928, "Hammer-Banna" },
            { 1212763168, "Hiligaynon" },
            { 1212763680, "Hindi" },
            { 1213022496, "High Mari" },
            { 1213088800, "Hindko" },
            { 1213145120, "Ho" },
            { 1213352224, "Harari" },
            { 1213355552, "Croatian" },
            { 1213550112, "Hungarian" },
            { 1213809952, "Armenian" },
            { 1229082400, "Igbo" },
            { 1229606688, "Ijo" },
            { 1229737760, "Ilokano" },
            { 1229866016, "Indonesian" },
            { 1229866784, "Ingush" },
            { 1229870368, "Inuktitut" },
            { 1230000200, "Phonetic transcription—IPA conventions" },
            { 1230129440, "Irish" },
            { 1230132256, "Irish Traditional" },
            { 1230195744, "Icelandic" },
            { 1230196000, "Inari Sami" },
            { 1230258464, "Italian" },
            { 1230459424, "Hebrew" },
            { 1245795872, "Javanese" },
            { 1246316832, "Yiddish" },
            { 1245793824, "Japanese" },
            { 1247101984, "Judezmo" },
            { 1247104032, "Jula" },
            { 1262567968, "Kabardian" },
            { 1262568224, "Kachchi" },
            { 1262570528, "Kalenjin" },
            { 1262571040, "Kannada" },
            { 1262572064, "Karachay" },
            { 1262572576, "Georgian" },
            { 1262574112, "Kazakh" },
            { 1262830112, "Kebena" },
            { 1262961952, "Khutsuri Georgian" },
            { 1263026464, "Khakass" },
            { 1263029024, "Khanty-Kazim" },
            { 1263029536, "Khmer" },
            { 1263031072, "Khanty-Shurishkar" },
            { 1263031840, "Khanty-Vakhi" },
            { 1263032096, "Khowar" },
            { 1263094560, "Kikuyu" },
            { 1263096352, "Kirghiz" },
            { 1263096608, "Kisii" },
            { 1263226400, "Kokni" },
            { 1263291680, "Kalmyk" },
            { 1263354400, "Kamba" },
            { 1263357472, "Kumaoni" },
            { 1263357728, "Komo" },
            { 1263358752, "Komso" },
            { 1263424032, "Kanuri" },
            { 1263485984, "Kodagu" },
            { 1263487008, "Korean Old Hangul" },
            { 1263487776, "Konkani" },
            { 1263488544, "Kikongo" },
            { 1263489056, "Komi-Permyak" },
            { 1263489568, "Korean" },
            { 1263491616, "Komi-Zyrian" },
            { 1263553568, "Kpelle" },
            { 1263683872, "Krio" },
            { 1263684384, "Karakalpak" },
            { 1263684640, "Karelian" },
            { 1263684896, "Karaim" },
            { 1263685152, "Karen" },
            { 1263686688, "Koorete" },
            { 1263749152, "Kashmiri" },
            { 1263749408, "Khasi" },
            { 1263750432, "Kildin Sami" },
            { 1263880480, "Kui" },
            { 1263881248, "Kulvi" },
            { 1263881504, "Kumyk" },
            { 1263882784, "Kurdish" },
            { 1263883552, "Kurukh" },
            { 1263884576, "Kuy" },
            { 1264143136, "Koryak" },
            { 1279345696, "Ladin" },
            { 1279346720, "Lahuli" },
            { 1279347488, "Lak" },
            { 1279348000, "Lambani" },
            { 1279348512, "Lao" },
            { 1279349792, "Latin" },
            { 1279351328, "Laz" },
            { 1279480352, "L-Cree" },
            { 1279544096, "Ladakhi" },
            { 1279613472, "Lezgi" },
            { 1279872544, "Lingala" },
            { 1280131360, "Low Mari" },
            { 1280131616, "Limbu" },
            { 1280136992, "Lomwe" },
            { 1280524832, "Lower Sorbian" },
            { 1280527648, "Lule Sami" },
            { 1280591904, "Lithuanian" },
            { 1280596512, "Luxembourgish" },
            { 1280655904, "Luba" },
            { 1280657184, "Luganda" },
            { 1280657440, "Luhya" },
            { 1280659232, "Luo" },
            { 1280723232, "Latvian" },
            { 1296124448, "Majang" },
            { 1296124704, "Makua" },
            { 1296124960, "Malayalam Traditional" },
            { 1296125472, "Mansi" },
            { 1296125984, "Mapudungun" },
            { 1296126496, "Marathi" },
            { 1296127776, "Marwari" },
            { 1296191008, "Mbundu" },
            { 1296255008, "Manchu" },
            { 1296257568, "Moose Cree" },
            { 1296319776, "Mende" },
            { 1296387616, "Me'en" },
            { 1296652832, "Mizo" },
            { 1296778272, "Macedonian" },
            { 1296844064, "Male" },
            { 1296844576, "Malagasy" },
            { 1296846368, "Malinke" },
            { 1296847392, "Malayalam Reformed" },
            { 1296849184, "Malay" },
            { 1296974880, "Mandinka" },
            { 1296975648, "Mongolian" },
            { 1296976160, "Manipuri" },
            { 1296976672, "Maninka" },
            { 1296980000, "Manx Gaelic" },
            { 1297041440, "Mohawk" },
            { 1297042208, "Moksha" },
            { 1297042464, "Moldavian" },
            { 1297042976, "Mon" },
            { 1297044000, "Moroccan" },
            { 1297238304, "Maori" },
            { 1297369120, "Maithili" },
            { 1297371936, "Maltese" },
            { 1297436192, "Mundari" },
            { 1312900896, "Naga-Assamese" },
            { 1312902688, "Nanai" },
            { 1312903968, "Naskapi" },
            { 1313034784, "N-Cree" },
            { 1313096224, "Ndebele" },
            { 1313097504, "Ndonga" },
            { 1313165344, "Nepali" },
            { 1313167136, "Newari" },
            { 1313296928, "Nagari" },
            { 1313358624, "Norway House Cree" },
            { 1313428256, "Nisi" },
            { 1313428768, "Niuean" },
            { 1313557536, "Nkole" },
            { 1313558304, "N'Ko" },
            { 1313621024, "Dutch" },
            { 1313818400, "Nogai" },
            { 1313821216, "Norwegian" },
            { 1314082080, "Northern Sami" },
            { 1314144544, "Northern Tai" },
            { 1314148128, "Esperanto" },
            { 1314475552, "Nynorsk" },
            { 1329809696, "Occitan" },
            { 1329812000, "Oji-Cree" },
            { 1330266656, "Ojibway" },
            { 1330792736, "Odia (formerly Oriya)" },
            { 1330794272, "Oromo" },
            { 1330860832, "Ossetian" },
            { 1346453792, "Palestinian Aramaic" },
            { 1346456608, "Pali" },
            { 1346457120, "Punjabi" },
            { 1346457632, "Palpa" },
            { 1346458400, "Pashto" },
            { 1346851360, "Polytonic Greek" },
            { 1346980896, "Filipino" },
            { 1347176224, "Palaung" },
            { 1347177248, "Polish" },
            { 1347571488, "Provencal" },
            { 1347700512, "Portuguese" },
            { 1363758624, "Chin" },
            { 1380010528, "Rajasthani" },
            { 1380143648, "R-Cree" },
            { 1380078880, "Russian Buriat" },
            { 1380532512, "Riang" },
            { 1380799264, "Rhaeto-Romanic" },
            { 1380928800, "Romanian" },
            { 1380931872, "Romany" },
            { 1381194016, "Rusyn" },
            { 1381318944, "Ruanda" },
            { 1381323552, "Russian" },
            { 1396786208, "Sadri" },
            { 1396788768, "Sanskrit" },
            { 1396790304, "Santali" },
            { 1396791584, "Sayisi" },
            { 1397050144, "Sekota" },
            { 1397050400, "Selkup" },
            { 1397182240, "Sango" },
            { 1397247520, "Shan" },
            { 1397309984, "Sibe" },
            { 1397310496, "Sidamo" },
            { 1397311264, "Silte Gurage" },
            { 1397445408, "Skolt Sami" },
            { 1397446944, "Slovak" },
            { 1397506336, "Slavey" },
            { 1397511712, "Slovenian" },
            { 1397574688, "Somali" },
            { 1397575456, "Samoan" },
            { 1397637408, "Sena" },
            { 1397638176, "Sindhi" },
            { 1397639200, "Sinhalese" },
            { 1397639968, "Soninke" },
            { 1397704480, "Sodo Gurage" },
            { 1397707808, "Sotho" },
            { 1397836064, "Albanian" },
            { 1397899808, "Serbian" },
            { 1397902112, "Saraiki" },
            { 1397903904, "Serer" },
            { 1397967904, "South Slavey" },
            { 1397968160, "Southern Sami" },
            { 1398100512, "Suri" },
            { 1398161696, "Svan" },
            { 1398162720, "Swedish" },
            { 1398227232, "Swadaya Aramaic" },
            { 1398229792, "Swahili" },
            { 1398233632, "Swazi" },
            { 1398297632, "Sutu" },
            { 1398362656, "Syriac" },
            { 1413562912, "Tabasaran" },
            { 1413564960, "Tajiki" },
            { 1413565728, "Tamil" },
            { 1413567520, "Tatar" },
            { 1413698080, "TH-Cree" },
            { 1413827616, "Telugu" },
            { 1413959200, "Tongan" },
            { 1413960224, "Tigre" },
            { 1413962016, "Tigrinya" },
            { 1414021408, "Thai" },
            { 1414026272, "Tahitian" },
            { 1414087200, "Tibetan" },
            { 1414221088, "Turkmen" },
            { 1414352416, "Temne" },
            { 1414414624, "Tswana" },
            { 1414415648, "Tundra Nenets" },
            { 1414416160, "Tonga" },
            { 1414480928, "Todo" },
            { 1414679328, "Turkish" },
            { 1414743840, "Tsonga" },
            { 1414873376, "Turoyo Aramaic" },
            { 1414876192, "Tulu" },
            { 1414878752, "Tuvin" },
            { 1415006496, "Twi" },
            { 1430539552, "Udmurt" },
            { 1430999584, "Ukrainian" },
            { 1431454752, "Urdu" },
            { 1431519776, "Upper Sorbian" },
            { 1431914272, "Uyghur" },
            { 1431978528, "Uzbek" },
            { 1447382560, "Venda" },
            { 1447646240, "Vietnamese" },
            { 1463885856, "Wa" },
            { 1463895840, "Wagdi" },
            { 1464029728, "West-Cree" },
            { 1464159264, "Welsh" },
            { 1464616480, "Wolof" },
            { 1480737824, "Tai Lue" },
            { 1481134880, "Xhosa" },
            { 1497451296, "Sakha" },
            { 1497514272, "Yoruba" },
            { 1497584160, "Y-Cree" },
            { 1497973536, "Yi Classic" },
            { 1497976096, "Yi Modern" },
            { 1514686496, "Chinese, Hong Kong SAR" },
            { 1514688544, "Chinese Phonetic" },
            { 1514689312, "Chinese Simplified" },
            { 1514689568, "Chinese Traditional" },
            { 1515078688, "Zande" },
            { 1515539488, "Zulu" },
        };

        #endregion

        private static readonly Type OpenTypeTableTag;
        private static readonly Type OpenTypeTags;        
        
        static GlyphTypefaceInspector()
        {
            Assembly PresentationCore = typeof(GlyphTypeface).Assembly;

            OpenTypeTableTag = PresentationCore.GetType("MS.Internal.Text.TextInterface.OpenTypeTableTag");
            OpenTypeTags = PresentationCore.GetType("MS.Internal.Shaping.OpenTypeTags");
        }

        private GlyphTypeface _typeface;

        private GlyphDefinitionsInspector _definitions;
        private GlyphSubstitutionsInspector _substitutions;
        private GlyphPositioningInspector _positioning;

        public GlyphDefinitionsInspector Definitions => _definitions;
        public GlyphSubstitutionsInspector Substitutions => _substitutions;
        public GlyphPositioningInspector Positioning => _positioning;

        public GlyphTypefaceInspector(GlyphTypeface typeface)
        {
            if (typeface == null)
                throw new ArgumentNullException();

            _typeface = typeface;
            _definitions = new GlyphDefinitionsInspector(_typeface);
            _substitutions = new GlyphSubstitutionsInspector(_typeface);
            _positioning = new GlyphPositioningInspector(_typeface);
        }

        internal static uint ToTagValue(string tag)
        {
            return
                (uint)tag[0] << 24 |
                (uint)tag[1] << 16 |
                (uint)tag[2] << 8 |
                (uint)tag[3];
        }
        internal static string ToTagString(uint tag)
        {
            return new string(new[] 
            {
                (char)((tag & 0xFF000000) >> 24),
                (char)((tag & 0x00FF0000) >> 16),
                (char)((tag & 0x0000FF00) >> 8) ,
                (char)((tag & 0x000000FF))}
            );
        }

        private static object ToOpenTypeTag(string tag)
        {
            if (tag == null)
                return Enum.ToObject(OpenTypeTags, 0u);

            if (tag.Length < 4)
                tag = tag + new string(' ', 4 - tag.Length);

            uint tagValue = 
                (uint)tag[0] << 24 |
                (uint)tag[1] << 16 |
                (uint)tag[2] << 8 |
                (uint)tag[3];

            return Enum.ToObject(OpenTypeTags, tagValue);
        }
        private static object ToOpenTypeTableTag(string tag)
        {
            if (tag == null)
                return Enum.ToObject(OpenTypeTableTag, 0u);

            return Enum.Parse(OpenTypeTableTag, tag);
        }

        private const uint DFLT = 0x44464C54;
        private const uint dflt = 0x64666C74;
        public static string GetKnownLanguageName(uint tag)
        {
            string friendlyName = null;

            if (KnownLanguages.TryGetValue(tag, out friendlyName))
                return friendlyName;

            return ToTagString(tag);
        }
        public static string GetKnownLanguageName(string tag)
        {
            if (tag == null || tag.Length != 4)
                return tag;

            return GetKnownLanguageName(ToTagValue(tag));
        }

        public static string GetKnownScriptName(uint tag)
        {
            string friendlyName = null;

            if (KnownScripts.TryGetValue(tag, out friendlyName))
                return friendlyName;

            return ToTagString(tag);
        }
        public static string GetKnownScriptName(string tag)
        {
            if (tag == null || tag.Length != 4)
                return tag;

            return GetKnownScriptName(ToTagValue(tag));
        }

        private const uint ssFeatureMask = 0x73730000;
        private const uint cvFeatureMask = 0x63760000;
        public static string GetKnownFeatureName(uint tag)
        {
            string friendlyName = null;

            if (KnownFeatureNames.TryGetValue(tag, out friendlyName))
                return friendlyName;

            if (TryGetKnownNumberedFeatureName(tag, out friendlyName, ssFeatureMask, "Stylistic Set "))
                return friendlyName;

            if (TryGetKnownNumberedFeatureName(tag, out friendlyName, cvFeatureMask, "Character Variant "))
                return friendlyName;

            return ToTagString(tag);
        }
        public static string GetKnownFeatureName(string tag)
        {
            if (tag == null || tag.Length != 4)
                return tag;

            return GetKnownFeatureName(ToTagValue(tag));
        }
        private static bool TryGetKnownNumberedFeatureName(uint tag, out string friendlyName, uint featureMask, string namePrefix)
        {
            friendlyName = null;

            if ((featureMask & 0xFFFF0000) == featureMask)
            {
                uint number1 = (tag & 0x0000FF00) >> 8;
                uint number2 = (tag & 0x000000FF);

                if (number1 >= '0' && number1 <= '9' &&
                    number2 >= '0' && number2 <= '9')
                    friendlyName = namePrefix + ((number1 - '0') * 10 + (number2 - '0'));
            }

            return friendlyName != null;
        }
    }
}
