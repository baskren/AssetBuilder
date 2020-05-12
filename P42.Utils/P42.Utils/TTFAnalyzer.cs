using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace P42.Utils
{
    public class TTFAnalyzer
    {

        // Font file; must be seekable
        //RandomAccessFile m_file;
        Stream _stream;

        // Helper I/O functions
        int ReadByte()
        {
            return _stream.ReadByte() & 0xFF;
            //return m_file.Read() & 0xFF;
        }

        int ReadWord()
        {
            var b1 = ReadByte();
            var b2 = ReadByte();
            return b1 << 8 | b2;
        }

        int ReadDword()
        {
            var b1 = ReadByte();
            var b2 = ReadByte();
            var b3 = ReadByte();
            var b4 = ReadByte();
            return b1 << 24 | b2 << 16 | b3 << 8 | b4;
        }

        void Read(byte[] array)
        {
            if (_stream.Read(array, 0, array.Length) != array.Length)
                throw new IOException();
        }

        // Helper
        static int GetWord(byte[] array, int offset)
        {
            var b1 = array[offset] & 0xFF;
            var b2 = array[offset + 1] & 0xFF;
            return b1 << 8 | b2;
        }
        
        void Seek(int offset)
        {
            _stream.Seek(offset, SeekOrigin.Begin);
        }


        public static string FontFamily(string fontFilePath)
        {
            String result = null;
            using (var stream = System.IO.File.Open(fontFilePath, System.IO.FileMode.Open))
            {
                result = FontFamily(stream);
            }
            return result;
        }

        public static string FontFamily(Stream fontFileStream)
        {
            try
            {
                var analyzer = new TTFAnalyzer
                {

                    // Parses the TTF file format.
                    // See http://developer.apple.com/fonts/ttrefman/rm06/Chap6.html
                    //_stream = new RandomAccessFile(fontFilename, "r");
                    _stream = fontFileStream
                };

                // Read the version first
                var version = analyzer.ReadDword();

                // The version must be either 'true' (0x74727565) or 0x00010000 or 'OTTO' (0x4f54544f) for CFF style fonts.
                if (version != 0x74727565 && version != 0x00010000 && version != 0x4f54544f)
                    return null;

                // The TTF file consist of several sections called "tables", and we need to know how many of them are there.
                var numTables = analyzer.ReadWord();

                // Skip the rest in the header
                analyzer.ReadWord(); // skip searchRange
                analyzer.ReadWord(); // skip entrySelector
                analyzer.ReadWord(); // skip rangeShift

                // Now we can read the tables
                for (int i = 0; i < numTables; i++)
                {
                    // Read the table entry
                    var tag = analyzer.ReadDword();
                    analyzer.ReadDword(); // skip checksum
                    var offset = analyzer.ReadDword();
                    var length = analyzer.ReadDword();

                    // Now here' the trick. 'name' field actually contains the textual string name.
                    // So the 'name' string in characters equals to 0x6E616D65
                    if (tag == 0x6E616D65)
                    {
                        // Here's the name section. Read it completely into the allocated buffer
                        var table = new byte[length];

                        analyzer.Seek(offset);
                        analyzer.Read(table);

                        // This is also a table. See http://developer.apple.com/fonts/ttrefman/rm06/Chap6name.html
                        // According to Table 36, the total number of table records is stored in the second word, at the offset 2.
                        // Getting the count and string offset - remembering it's big endian.
                        var count = TTFAnalyzer.GetWord(table, 2);
                        var string_offset = TTFAnalyzer.GetWord(table, 4);

                        //List<string> names = new List<string>();

                        // Record starts from offset 6
                        for (int record = 0; record < count; record++)
                        {
                            // Table 37 tells us that each record is 6 words -> 12 bytes, and that the nameID is 4th word so its offset is 6.
                            // We also need to account for the first 6 bytes of the header above (Table 36), so...
                            var nameid_offset = record * 12 + 6;
                            var platformID = TTFAnalyzer.GetWord(table, nameid_offset);
                            var nameid_value = TTFAnalyzer.GetWord(table, nameid_offset + 6);


                            // Table 42 lists the valid name Identifiers. We're interested in 1 (Font Family Name) but not in Unicode encoding (for simplicity).
                            // The encoding is stored as PlatformID and we're interested in Mac encoding
                            if (nameid_value == 1) // && platformID == 1)
                            {
                                // We need the string offset and length, which are the word 6 and 5 respectively
                                var name_length = TTFAnalyzer.GetWord(table, nameid_offset + 8);
                                var name_offset = TTFAnalyzer.GetWord(table, nameid_offset + 10);

                                // The real name string offset is calculated by adding the string_offset
                                name_offset += string_offset;

                                // Make sure it is inside the array
                                if (name_offset >= 0 && name_offset + name_length < table.Length)
                                {
                                    var chars = new byte[name_length];
                                    System.Buffer.BlockCopy(table, name_offset, chars, 0, name_length);
                                    switch (platformID)
                                    {
                                        case 1:
                                            return System.Text.Encoding.UTF8.GetString(chars, 0, name_length);
                                        case 2:
                                            break;
                                        case 3:
                                            return System.Text.Encoding.BigEndianUnicode.GetString(chars, 0, name_length);
                                        default:
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }

                return null;
            }
#pragma warning disable 0168
            catch (FileNotFoundException e)
#pragma warning restore 0168
            {
                // Permissions?
                return null;
            }
#pragma warning disable 0168
            catch (IOException)
#pragma warning restore 0168
            {
                // Most likely a corrupted font file
                return null;
            }
        }


        public static string FontAttributes(string fontFilePath)
        {
            using (var stream = System.IO.File.Open(fontFilePath, FileMode.Open))
                return FontAttributes(stream);
        }

        public static string FontAttributes(Stream stream)
        {
            try
            {
                var analyzer = new TTFAnalyzer
                {

                    // Parses the TTF file format.
                    // See http://developer.apple.com/fonts/ttrefman/rm06/Chap6.html
                    //_stream = new RandomAccessFile(fontFilename, "r");
                    _stream = stream
                };

                // Read the version first
                var version = analyzer.ReadDword();

                // The version must be either 'true' (0x74727565) or 0x00010000 or 'OTTO' (0x4f54544f) for CFF style fonts.
                if (version != 0x74727565 && version != 0x00010000 && version != 0x4f54544f)
                    return null;

                // The TTF file consist of several sections called "tables", and we need to know how many of them are there.
                var numTables = analyzer.ReadWord();

                // Skip the rest in the header
                analyzer.ReadWord(); // skip searchRange
                analyzer.ReadWord(); // skip entrySelector
                analyzer.ReadWord(); // skip rangeShift

                // Now we can read the tables
                for (int i = 0; i < numTables; i++)
                {
                    // Read the table entry
                    var tag = analyzer.ReadDword();
                    analyzer.ReadDword(); // skip checksum
                    var offset = analyzer.ReadDword();
                    var length = analyzer.ReadDword();

                    // Now here' the trick. 'name' field actually contains the textual string name.
                    // So the 'name' string in characters equals to 0x6E616D65
                    if (tag == 0x6E616D65)
                    {
                        // Here's the name section. Read it completely into the allocated buffer
                        var table = new byte[length];

                        //_stream.Seek(offset);
                        analyzer.Seek(offset);
                        analyzer.Read(table);

                        // This is also a table. See http://developer.apple.com/fonts/ttrefman/rm06/Chap6name.html
                        // According to Table 36, the total number of table records is stored in the second word, at the offset 2.
                        // Getting the count and string offset - remembering it's big endian.
                        var count = TTFAnalyzer.GetWord(table, 2);
                        var string_offset = TTFAnalyzer.GetWord(table, 4);

                        // Record starts from offset 6
                        for (int record = 0; record < count; record++)
                        {
                            // Table 37 tells us that each record is 6 words -> 12 bytes, and that the nameID is 4th word so its offset is 6.
                            // We also need to account for the first 6 bytes of the header above (Table 36), so...
                            var nameid_offset = record * 12 + 6;
                            var platformID = TTFAnalyzer.GetWord(table, nameid_offset);
                            var nameid_value = TTFAnalyzer.GetWord(table, nameid_offset + 6);

                            // Table 42 lists the valid name Identifiers. We're interested in 1 (Font Subfamily Name) but not in Unicode encoding (for simplicity).
                            // The encoding is stored as PlatformID and we're interested in Mac encoding
                            if (nameid_value == 2 && platformID == 1)
                            {
                                // We need the string offset and length, which are the word 6 and 5 respectively
                                var name_length = TTFAnalyzer.GetWord(table, nameid_offset + 8);
                                var name_offset = TTFAnalyzer.GetWord(table, nameid_offset + 10);

                                // The real name string offset is calculated by adding the string_offset
                                name_offset += string_offset;

                                // Make sure it is inside the array
                                if (name_offset >= 0 && name_offset + name_length < table.Length)
                                {
                                    //return new String( table, name_offset, name_length );
                                    //char[] chars = new char[name_length];
                                    /*
									System.Buffer.BlockCopy(table, name_offset, chars, 0, name_length);
									*/
                                    /*
									for(int nameI=0;nameI<name_length;nameI++) {
										chars [nameI] = (char)table [name_offset + nameI];
									}
									*/
                                    var chars = new byte[name_length];
                                    System.Buffer.BlockCopy(table, name_offset, chars, 0, name_length);
                                    //var str = new string(chars);
                                    //var str = System.Text.Encoding.Default.GetString(chars);
                                    var str = System.Text.Encoding.UTF8.GetString(chars, 0, name_length);
                                    return str;
                                }
                            }
                        }
                    }
                }

                return null;
            }
#pragma warning disable 0168
            catch (FileNotFoundException e)
#pragma warning restore 0168
            {
                // Permissions?
                return null;
            }
#pragma warning disable 0168
            catch (IOException)
#pragma warning restore 0168
            {
                // Most likely a corrupted font file
                return null;
            }
        }
    }
}
