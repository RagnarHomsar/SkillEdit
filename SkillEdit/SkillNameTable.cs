using System;
using System.Collections.Generic;
using System.IO;
namespace SkillEdit
{
    class SkillNameTable
    {
        public static readonly int NAME_MAX = 60;

        private short numberOfEntries;
        private ushort[] offsets;
        public SJISString[] sjisStrings;

        public SkillNameTable(string nameTableLocation)
        {
            var nameTableData = File.ReadAllBytes(nameTableLocation);
            numberOfEntries = (short) MergeBytesIntoShort(nameTableData, 0);
            offsets = new ushort[numberOfEntries];

            offsets[0] = 0;

            for (int i = 1; i < numberOfEntries; i++)
            {
                offsets[i] = MergeBytesIntoShort(nameTableData, i * 2);
            }

            var sjisDataLoc = 2 * (numberOfEntries + 1); // accounts for starting count of entries
            sjisStrings = new SJISString[numberOfEntries];

            for (int i = 0; i < numberOfEntries; i++)
            {
                var lastByte = 0x1;
                var stringPos = 0;
                var constructedData = new ushort[NAME_MAX];

                while (lastByte != 0x0)
                {
                    constructedData[stringPos] = MergeCharBytesIntoShort(nameTableData, sjisDataLoc + offsets[i] + (stringPos * 2));
                    stringPos += 1;
                    var currentPos = sjisDataLoc + offsets[i] + (stringPos * 2);

                    if (currentPos < nameTableData.Length)
                    {
                        lastByte = nameTableData[currentPos];
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
                    offsets[i] += (ushort)lenDiff;
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
                newFile.InsertRange(i * 2, BitConverter.GetBytes(offsets[i]));
            }

            // dummy EOF offset (why??)
            newFile.Add(0x0);
            newFile.Add(0x0);

            var sjisDataLoc = 2 * (numberOfEntries + 1); // accounts for starting count of entries

            // insert new string byte data
            for (int i = 0; i < numberOfEntries; i++)
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

            File.WriteAllBytes(path, newFile.ToArray());
        }

        private static ushort MergeBytesIntoShort(byte[] table, int leftByteIndex)
        {
            return (ushort)((table[leftByteIndex + 1] << 8) +
                table[leftByteIndex]);
        }

        private static ushort MergeCharBytesIntoShort(byte[] table, int leftByteIndex)
        {
            return (ushort)((table[leftByteIndex] << 8) +
                table[leftByteIndex + 1]);
        }
    }
}
