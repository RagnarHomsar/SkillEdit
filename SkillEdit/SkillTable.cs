using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillEdit
{
    class SkillTable
    {
        int numberOfSkills;
        public EO3Skill[] skillData;

        public SkillTable(string tableFileLoc)
        {
            var tableByteData = File.ReadAllBytes(tableFileLoc);
            var tableByteLength = new FileInfo(tableFileLoc).Length;
            numberOfSkills = (int) tableByteLength / EO3Skill.SKILL_LENGTH;
            skillData = new EO3Skill[numberOfSkills];

            for (int i = 0; i < numberOfSkills; i++)
            {
                skillData[i] = new EO3Skill(tableByteData.Skip(i * EO3Skill.SKILL_LENGTH).Take(EO3Skill.SKILL_LENGTH).ToArray());
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
                for (int i = 0; i < numberOfSkills; i++)
                {
                    writer.Write(skillData[i].SkillLevel);
                    writer.Write(skillData[i].SkillType);
                    writer.Write(skillData[i].BodyParts);
                    writer.Write(skillData[i].StatusRequired);
                    writer.Write(skillData[i].TargetType);
                    writer.Write(skillData[i].TargetTeam);
                    writer.Write(skillData[i].UsableState);
                    writer.Write(skillData[i].ModifierStatus);
                    writer.Write(skillData[i].ModifierType);
                    writer.Write(skillData[i].Unknown);
                    writer.Write(skillData[i].ModifierElement);
                    writer.Write(skillData[i].DamageType);
                    writer.Write(skillData[i].InflictionFlag);
                    writer.Write(skillData[i].DisablesInflicted);
                    writer.Write(skillData[i].SkillFlags);
                    writer.Write(skillData[i].Repurposed);

                    for (int j = 0; j < EO3Skill.NUMBER_OF_SUBHEADERS; j++)
                    {
                        writer.Write(skillData[i].SubheaderData[j].subheader);

                        for (int k = 0; k < EO3Skill.NUMBER_OF_LEVELS; k++)
                        {
                            writer.Write(skillData[i].SubheaderData[j].levelValues[k]);
                        }
                    }
                }

                return stream.ToArray();
            }
        }
    }
}
