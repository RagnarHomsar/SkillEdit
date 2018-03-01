using System;
using System.Linq;

namespace SkillEdit
{
    class EO3Skill
    {
        public static readonly int SKILL_LENGTH = 0x178;
        public static readonly int NUMBER_OF_SUBHEADERS = 8;
        public static readonly int NUMBER_OF_LEVELS = 10;
        public byte SkillLevel { get; set; }
        public byte SkillType { get; set; }
        public ushort BodyParts { get; set; }
        public ushort StatusRequired { get; set; }
        public byte TargetType { get; set; }
        public byte TargetTeam { get; set; }
        public byte UsableState { get; set; } // usable in battle, Labyrinth, etc.
        public byte ModifierStatus { get; set; }
        public byte ModifierType { get; set; }
        public byte Unknown { get; set; }
        public ushort ModifierElement { get; set; }
        public ushort DamageType { get; set; }
        public ushort InflictionFlag { get; set; }
        public ushort DisablesInflicted { get; set; }
        public ushort SkillFlags { get; set; }
        public ushort Repurposed { get; set; }
        public SkillSubheader[] SubheaderData { get; set; }

        public EO3Skill(byte[] skillData)
        {
            SkillLevel = skillData[0];
            SkillType = skillData[1];
            BodyParts = MergeBytesIntoShort(skillData, 2);
            StatusRequired = MergeBytesIntoShort(skillData, 4);
            TargetType = skillData[6];
            TargetTeam = skillData[7];
            UsableState = skillData[8];
            ModifierStatus = skillData[9];
            ModifierType = skillData[10];
            Unknown = skillData[11];
            ModifierElement = MergeBytesIntoShort(skillData, 12);
            DamageType = MergeBytesIntoShort(skillData, 14);
            InflictionFlag = MergeBytesIntoShort(skillData, 16);
            DisablesInflicted = MergeBytesIntoShort(skillData, 18);
            SkillFlags = MergeBytesIntoShort(skillData, 20);
            Repurposed = MergeBytesIntoShort(skillData, 22);

            SubheaderData = new SkillSubheader[8];

            for (int i = 0; i < NUMBER_OF_SUBHEADERS; i++)
            {
                SubheaderData[i] = new SkillSubheader(
                    MergeBytesIntoInt(skillData, (24 + (i * (4 * (NUMBER_OF_LEVELS + 1))))),
                    skillData.Skip(28 + (i * (4 * (NUMBER_OF_LEVELS + 1)))).Take(4 * NUMBER_OF_LEVELS).ToArray()
                );
            }
        }

        private static ushort MergeBytesIntoShort(byte[] skillData, int leftByteIndex)
        {
            return (ushort) ((skillData[leftByteIndex + 1] << 8) + 
                skillData[leftByteIndex]);
        }

        private static int MergeBytesIntoInt(byte[] skillData, int leftByteIndex)
        {
            return (
                (skillData[leftByteIndex + 3] << 24) +
                (skillData[leftByteIndex + 2] << 16) +
                (skillData[leftByteIndex + 1] << 8) +
                (skillData[leftByteIndex])
            );
        }

        public class SkillSubheader
        {
            public int subheader { get; set; }
            public int[] levelValues { get; set; }

            public SkillSubheader(int subheader, byte[] levelValues)
            {
                this.subheader = subheader;
                this.levelValues = new int[NUMBER_OF_LEVELS];

                if (levelValues.Length / 4 != NUMBER_OF_LEVELS)
                {
                    throw new SkillInvalidLength("Subheader had an invalid amount of levels!");
                }

                for (int i = 0; i < NUMBER_OF_LEVELS; i++)
                {
                    this.levelValues[i] = MergeBytesIntoInt(levelValues, i * 4);
                }
            }
        }

        public class SkillInvalidLength : Exception
        {
            public SkillInvalidLength(string message) : base(message) { }
        }
    }
}
