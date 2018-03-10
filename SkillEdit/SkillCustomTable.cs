using System;
using System.Collections.Generic;
using System.IO;
namespace SkillEdit
{
    class SkillCustomTable
    {
        public static readonly int NAME_MAX = 60;

        private int numberOfEntries;
        private int[] offsets;
        public SJISString[] sjisStrings { get; }

        public SkillCustomTable(string nameTableLocation)
        {
            var customTableData = File.ReadAllBytes(nameTableLocation);
            numberOfEntries = MergeBytesIntoInt(customTableData, 0);
            offsets = new int[numberOfEntries];

            offsets[0] = 0;

            for (int i = 1; i < numberOfEntries; i++)
            {
                offsets[i] = MergeBytesIntoInt(customTableData, i * 4);

                // malformed tables?
                if (offsets[i] >= customTableData.Length)
                {
                    offsets[i] = offsets[1];
                }
            }

            var sjisDataLoc = 4 * (numberOfEntries + 1); // accounts for starting count of entries
            sjisStrings = new SJISString[numberOfEntries];

            for (int i = 0; i < numberOfEntries; i++)
            {
                var lastByte = 0x1;
                var stringPos = 0;
                var constructedData = new ushort[NAME_MAX];

                while (lastByte != 0x0)
                {
                    var currentPos = sjisDataLoc + offsets[i] + (stringPos * 2);

                    constructedData[stringPos] = MergeCharBytesIntoShort(customTableData, currentPos);
                    stringPos += 1;

                    if (currentPos < customTableData.Length)
                    {
                        lastByte = customTableData[currentPos];
                    }

                    else
                    {
                        lastByte = 0x0;
                    }
                }

                sjisStrings[i] = new SJISString(constructedData);
            }
        }

        public void ReplaceString(int skillId, SJISString newString)
        {
            var lenDiff = newString.ByteLength() - sjisStrings[skillId].ByteLength();

            for (int i = skillId + 1; i < numberOfEntries; i++)
            {
                if (i != 0) // don't touch offset 0, that's hardcoded into the game...
                {
                    offsets[i] += lenDiff;
                }
            }

            sjisStrings[skillId] = newString;
        }

        public void Write(string path)
        {
            var newFile = new List<byte>();

            newFile.InsertRange(0, BitConverter.GetBytes(numberOfEntries));

            // insert new offsets
            // skip the first because that's hardcoded
            for (int i = 1; i < numberOfEntries; i++)
            {
                newFile.InsertRange(i * 4, BitConverter.GetBytes(offsets[i]));
            }

            // dummy EOF offset (why??)
            newFile.Add(0x0);
            newFile.Add(0x0);
            newFile.Add(0x0);
            newFile.Add(0x0);

            var sjisDataLoc = 4 * (numberOfEntries + 1); // accounts for starting count of entries

            // insert new string byte data
            for (int i = 0; i < numberOfEntries; i++)
            {
                if (sjisStrings[i] == null)
                {
                    newFile.Add(0x0);
                }

                else
                {
                    // can't just write out bytes b/c arrays are statically-defined
                    foreach (var dataByte in sjisStrings[i].characterBytes)
                    {
                        newFile.Add(dataByte);

                        // break after we've written a null terminator
                        if (dataByte == 0x0)
                        {
                            break;
                        }
                    }
                }
            }

            File.WriteAllBytes(path, newFile.ToArray());
        }

        private static int MergeBytesIntoInt(byte[] table, int leftByteIndex)
        {
            var byte1 = (int) table[leftByteIndex];
            var byte2 = table[leftByteIndex + 1] << 8;
            var byte3 = table[leftByteIndex + 2] << 16;
            var byte4 = table[leftByteIndex + 3] << 24;

            return ((table[leftByteIndex + 3] << 24) +
                (table[leftByteIndex + 2] << 16) +
                (table[leftByteIndex + 1] << 8) +
                table[leftByteIndex]);
        }

        private static ushort MergeCharBytesIntoShort(byte[] table, int leftByteIndex)
        {
            try
            {
                return (ushort)((table[leftByteIndex] << 8) +
                    table[leftByteIndex + 1]);
            }

            catch (IndexOutOfRangeException)
            {
                return 0x0;
            }
        }
    }
}
