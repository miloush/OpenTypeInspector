# OpenType Inspector

[![Microsoft Reference Source License](https://img.shields.io/badge/license-MS--RSL-%23373737)](https://referencesource.microsoft.com/license.html)

A tool for exploring OpenType files. The tool was originally developed as an aid for the [TERKA project](https://terka.microframework.cz). Its goal was to give an overview of the most commonly used OpenType feature types so that their support can be prioritized.

![image](https://github.com/miloush/OpenTypeInspector/assets/10546952/40c7115b-d6db-4b51-99ae-2d3caaece850)

Features:
 * Sortable counts of glyphs and various GSUB and GPOS features.
 * Searching fonts by script tags, language tags, feature tags and/or supported characters.
 * Filtering glyphs and characters by glyph number, Unicode names and codepoints.
 * Overview of script and feature tags for both GSUB and GPOS with list of features and lookups they use.
 * Visualization of single, multiple, alternate, ligature and context substitutions, with filtering by glyph number involved. (Note: visualization of chaining substitutions is not implemented)
 * Loading fonts from system, user and custom folders. Font files can also be dropped on the font list. Fonts with zero substitutions are not listed, but can be dropped. Loading can be stopped by pressing the <kbd>Esc</kbd> key.
 * Experimental typing area with various typography controls as available in WPF.

The application uses WPF features for displaying glyphs. It does not directly parse font files or test text shaping. Only font file types supported by WPF will be shown.

![image](https://github.com/miloush/OpenTypeInspector/assets/10546952/01f9c8f0-525c-4a3b-9741-c111aedc75c2)

![image](https://github.com/miloush/OpenTypeInspector/assets/10546952/2a470a23-2ec8-4c1d-8924-69f4ca6ab9f4)
