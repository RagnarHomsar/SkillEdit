using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillEdit
{
    public class SJISString
    {
        public byte[] characterBytes;

        private static readonly Dictionary<char, char> SJIS_ASCII_LOOKUP = new Dictionary<char, char>()
        {
            { (char) 0x0, (char) 0x0 },
            { '　', ' ' },
            { '’', '\'' },
            { '−', '-' },
            { '０', '0' },
            { '１', '1' },
            { '２', '2' },
            { '３', '3' },
            { '４', '4' },
            { '５', '5' },
            { '６', '6' },
            { '７', '7' },
            { '８', '8' },
            { '９', '9' },
            { 'Ａ', 'A' },
            { 'Ｂ', 'B' },
            { 'Ｃ', 'C' },
            { 'Ｄ', 'D' },
            { 'Ｅ', 'E' },
            { 'Ｆ', 'F' },
            { 'Ｇ', 'G' },
            { 'Ｈ', 'H' },
            { 'Ｉ', 'I' },
            { 'Ｊ', 'J' },
            { 'Ｋ', 'K' },
            { 'Ｌ', 'L' },
            { 'Ｍ', 'M' },
            { 'Ｎ', 'N' },
            { 'Ｏ', 'O' },
            { 'Ｐ', 'P' },
            { 'Ｑ', 'Q' },
            { 'Ｒ', 'R' },
            { 'Ｓ', 'S' },
            { 'Ｔ', 'T' },
            { 'Ｕ', 'U' },
            { 'Ｖ', 'V' },
            { 'Ｗ', 'W' },
            { 'Ｘ', 'X' },
            { 'Ｙ', 'Y' },
            { 'Ｚ', 'Z' },
            { 'ａ', 'a' },
            { 'ｂ', 'b' },
            { 'ｃ', 'c' },
            { 'ｄ', 'd' },
            { 'ｅ', 'e' },
            { 'ｆ', 'f' },
            { 'ｇ', 'g' },
            { 'ｈ', 'h' },
            { 'ｉ', 'i' },
            { 'ｊ', 'j' },
            { 'ｋ', 'k' },
            { 'ｌ', 'l' },
            { 'ｍ', 'm' },
            { 'ｎ', 'n' },
            { 'ｏ', 'o' },
            { 'ｐ', 'p' },
            { 'ｑ', 'q' },
            { 'ｒ', 'r' },
            { 'ｓ', 's' },
            { 'ｔ', 't' },
            { 'ｕ', 'u' },
            { 'ｖ', 'v' },
            { 'ｗ', 'w' },
            { 'ｘ', 'x' },
            { 'ｙ', 'y' },
            { 'ｚ', 'z' },
            { '↑', '^' },
            { '↓', '?' }, // stupid kludge but i don't really care right now
            { '；', ';' },
            { '：', ':' },
            { '＋', '+' },
            { '％', '%' },
            { '→', '>' },
            { '／', '/' }
        };

        private static readonly Dictionary<ushort, char> SJIS_BYTE_LOOKUP = new Dictionary<ushort, char>()
        {
            { 0x0000, (char) 0x0 },
            { 0x3B20, (char) 0x0 }, // i...don't really know what's up with this, but this is a sequence that's in the vanilla table exactly once, and it's causing problems
            { 0x8140, '　' },
            { 0x8146, '：' },
            { 0x8147, '；' },
            { 0x815E, '／' },
            { 0x8166, '’' },
            { 0x817B, '＋' },
            { 0x817C, '−' },
            { 0x8193, '％' },
            { 0x81AA, '↑' },
            { 0x81A8, '→' },
            { 0x81AB, '↓' },
            { 0x824F, '０' },
            { 0x8250, '１' },
            { 0x8251, '２' },
            { 0x8252, '３' },
            { 0x8253, '４' },
            { 0x8254, '５' },
            { 0x8255, '６' },
            { 0x8256, '７' },
            { 0x8257, '８' },
            { 0x8258, '９' },
            { 0x8260, 'Ａ' },
            { 0x8261, 'Ｂ' },
            { 0x8262, 'Ｃ' },
            { 0x8263, 'Ｄ' },
            { 0x8264, 'Ｅ' },
            { 0x8265, 'Ｆ' },
            { 0x8266, 'Ｇ' },
            { 0x8267, 'Ｈ' },
            { 0x8268, 'Ｉ' },
            { 0x8269, 'Ｊ' },
            { 0x826A, 'Ｋ' },
            { 0x826B, 'Ｌ' },
            { 0x826C, 'Ｍ' },
            { 0x826D, 'Ｎ' },
            { 0x826E, 'Ｏ' },
            { 0x826F, 'Ｐ' },
            { 0x8270, 'Ｑ' },
            { 0x8271, 'Ｒ' },
            { 0x8272, 'Ｓ' },
            { 0x8273, 'Ｔ' },
            { 0x8274, 'Ｕ' },
            { 0x8275, 'Ｖ' },
            { 0x8276, 'Ｗ' },
            { 0x8277, 'Ｘ' },
            { 0x8278, 'Ｙ' },
            { 0x8279, 'Ｚ' },
            { 0x8281, 'ａ' },
            { 0x8282, 'ｂ' },
            { 0x8283, 'ｃ' },
            { 0x8284, 'ｄ' },
            { 0x8285, 'ｅ' },
            { 0x8286, 'ｆ' },
            { 0x8287, 'ｇ' },
            { 0x8288, 'ｈ' },
            { 0x8289, 'ｉ' },
            { 0x828A, 'ｊ' },
            { 0x828B, 'ｋ' },
            { 0x828C, 'ｌ' },
            { 0x828D, 'ｍ' },
            { 0x828E, 'ｎ' },
            { 0x828F, 'ｏ' },
            { 0x8290, 'ｐ' },
            { 0x8291, 'ｑ' },
            { 0x8292, 'ｒ' },
            { 0x8293, 'ｓ' },
            { 0x8294, 'ｔ' },
            { 0x8295, 'ｕ' },
            { 0x8296, 'ｖ' },
            { 0x8297, 'ｗ' },
            { 0x8298, 'ｘ' },
            { 0x8299, 'ｙ' },
            { 0x829A, 'ｚ' }
        };

        public SJISString(ushort[] input)
        {
            characterBytes = new byte[SkillNameTable.NAME_MAX * 2];

            for (int i = 0; i < SkillNameTable.NAME_MAX; i++)
            {
                var upperByte = input[i] >> 8;
                var lowerByte = input[i] & 0xFF;

                characterBytes[i * 2] = (byte)upperByte;

                // when we hit a null byte, stop
                if (upperByte == 0x00)
                {
                    break;
                }

                characterBytes[(i * 2) + 1] = (byte)lowerByte;
            }
        }

        public SJISString(string input)
        {
            characterBytes = new byte[SkillNameTable.NAME_MAX * 2];
            var counter = 0;

            foreach (var character in input)
            {
                var sjisChar = SJIS_ASCII_LOOKUP.FirstOrDefault(x => x.Value == character).Key;
                var sjisByte = SJIS_BYTE_LOOKUP.FirstOrDefault(x => x.Value == sjisChar).Key;

                var upperByte = sjisByte >> 8;
                var lowerByte = sjisByte & 0xFF;

                characterBytes[counter * 2] = (byte)upperByte;

                // when we hit a null byte, stop
                if (upperByte == 0x00)
                {
                    break;
                }

                characterBytes[(counter * 2) + 1] = (byte)lowerByte;
                counter += 1;
            }
        }

        private static ushort MergeBytesIntoShort(byte lower, byte upper)
        {
            return (ushort) ((upper << 8) + lower);
        }

        public string GetAscii()
        {
            var asciiLength = ((characterBytes.Length - 1) / 2) + 1;
            var asciiChars = "";

            for (int i = 0; i < asciiLength; i++)
            {
                var sjisByte = MergeBytesIntoShort(characterBytes[(i * 2) + 1], characterBytes[(i * 2)]);
                var sjisChar = SJIS_BYTE_LOOKUP[sjisByte];

                if (sjisChar == 0x0)
                {
                    break;
                }

                asciiChars += SJIS_ASCII_LOOKUP[sjisChar];
            }

            return asciiChars;
        }

        public int ByteLength()
        {
            int length = 0;

            foreach (var charByte in characterBytes)
            {
                length += 1;

                if (charByte == 0x0)
                {
                    break;
                }
            }

            return length;
        }
    }
}
