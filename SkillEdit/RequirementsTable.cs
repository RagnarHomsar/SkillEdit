using System;
using System.IO;
using System.Linq;

namespace SkillEdit
{
    class RequirementsTable
    {
        int numberOfEntries;
        public SkillRequirements[] Skills { get; }

        public RequirementsTable(string fileName)
        {
            var requirementsBytes = File.ReadAllBytes(fileName);
            numberOfEntries = requirementsBytes.Length / 8; // 8 bytes per skill requirement
            Skills = new SkillRequirements[numberOfEntries];

            for (int i = 0; i < numberOfEntries; i++)
            {
                Skills[i] = new SkillRequirements(requirementsBytes.Skip(i * 8).Take(8).ToArray());
            }
        }

        public void WriteToFile(string fileName)
        {
            var bytesToWrite = ToByteArray();
            File.WriteAllBytes(fileName, bytesToWrite);
        }

        private byte[] ToByteArray()
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                for (int i = 0; i < numberOfEntries; i++)
                {
                    for (int j = 0; j < SkillRequirements.NUMBER_OF_REQUIREMENTS; j++)
                    {
                        writer.Write(Skills[i].requiredSkills[j].Item1);
                        writer.Write(Skills[i].requiredSkills[j].Item2);
                    }

                    writer.Write(Skills[i].orRequirements);
                    writer.Write(Skills[i].unused);
                }

                return stream.ToArray();
            }
        }
    }

    class SkillRequirements
    {
        public static readonly int NUMBER_OF_REQUIREMENTS = 3;

        // byte 1 is skill ID, byte 2 is level
        public Tuple<byte, byte>[] requiredSkills { get; set; }
        public byte orRequirements { get; set; }
        public byte unused { get; }

        public SkillRequirements(byte[] data)
        {
            requiredSkills = new Tuple<byte, byte>[NUMBER_OF_REQUIREMENTS];

            for (int i = 0; i < NUMBER_OF_REQUIREMENTS; i++)
            {
                requiredSkills[i] = new Tuple<byte, byte>(data[i * 2], data[(i * 2) + 1]);
            }

            orRequirements = data[6];
            unused = data[7];
        }
    }
}
