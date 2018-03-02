using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SkillEdit
{
    public partial class SkillEditForm : Form
    {
        SkillTable st;
        SkillNameTable snt;
        RequirementsTable rt;
        EO3Skill selectedSkill;

        public SkillEditForm()
        {
            InitializeComponent();

            skillList.SelectedIndexChanged += SkillChanged;
            skillTypeTextBox.KeyPress += HexTextBoxLimiter;
            statusRequiredTextBox.KeyPress += HexTextBoxLimiter;
            skillFlagsTextBox.KeyPress += HexTextBoxLimiter;
            repurposedTextBox.KeyPress += HexTextBoxLimiter;
            usableStateTextBox.KeyPress += HexTextBoxLimiter;
            subheaderIdTextBox.KeyPress += HexTextBoxLimiter;
            subheaderSelector.ValueChanged += UpdateSubheaderInfo;

            if (File.Exists("last_table.txt") == false)
            {
                openLastTableToolStripMenuItem.Enabled = false;
            }
        }

        private void openTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var openTableDialog = new OpenFileDialog();

            openTableDialog.Filter = "Player skill table|playerskilltable.tbl|Enemy skill table|enemyskilltable.tbl";
            openTableDialog.RestoreDirectory = true;

            if ((openTableDialog.ShowDialog() == DialogResult.OK))
            {
                st = new SkillTable(openTableDialog.FileName);
                snt = new SkillNameTable(openTableDialog.FileName.Replace("skilltable", "skillnametable"));

                // bad kludge. very bad kludge. who cares tho
                if (openTableDialog.FileName.Contains("player"))
                {
                    rt = new RequirementsTable(openTableDialog.FileName.Replace("skilltable", "skilllearntable"));
                }

                InitSkillEditor(openTableDialog.FileName, snt);
            }
        }

        private void InitSkillEditor(string tableName, SkillNameTable snt)
        {
            skillList.Items.Clear();

            for (int i = 0; i < snt.sjisStrings.Length; i++)
            {
                skillList.Items.Add(
                    "0x" + i.ToString("X3") + ": " +
                    snt.sjisStrings[i].GetAscii()
                );
            }

            if (rt != null)
            {
                foreach (ComboBox skillList in requirementsEditorBox.Controls.OfType<ComboBox>())
                {
                    skillList.Items.Clear();

                    for (int i = 0; i < snt.sjisStrings.Length; i++)
                    {
                        skillList.Items.Add(
                            "0x" + i.ToString("X3") + ": " +
                            snt.sjisStrings[i].GetAscii()
                        );
                    }
                }
            }

            skillList.SelectedIndex = 0;

            if (File.Exists("last_table.txt") == false)
            {
                File.Create("last_table.txt").Close();
            }

            var lastTableContents = new List<string>();
            lastTableContents.Add(tableName);
            File.WriteAllLines("last_table.txt", lastTableContents);

            foreach (Control skillEditControl in Controls)
            {
                skillEditControl.Enabled = true;
            }

            foreach (RadioButton button in modifierStatusBox.Controls)
            {
                button.CheckedChanged += ModifierChanged;
            }

            disableVulnRadio.CheckedChanged += DisableChanged;
            inflictNo.CheckedChanged += DisableChanged;
            inflictYes.CheckedChanged += DisableChanged;
            inflictCures.CheckedChanged += DisableChanged;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void openLastTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var lastTableContents = File.ReadAllLines("last_table.txt");
            st = new SkillTable(lastTableContents[0]);
            snt = new SkillNameTable(lastTableContents[0].Replace("skilltable", "skillnametable"));

            // bad kludge. very bad kludge. who cares tho
            if (lastTableContents[0].Contains("player"))
            {
                rt = new RequirementsTable(lastTableContents[0].Replace("skilltable", "skilllearntable"));
            }

            InitSkillEditor(lastTableContents[0], snt);
        }

        private void SkillChanged(object sender, EventArgs e)
        {
            var senderList = (ComboBox) sender;
            var skillId = senderList.SelectedIndex;
            selectedSkill = st.skillData[skillId];

            skillNameTextBox.Text = skillList.Text.Split(new string[] { ": " }, StringSplitOptions.None)[1];

            // set max level
            if (selectedSkill.SkillLevel == 1) { maxLevel1.Checked = true; }
            else if (selectedSkill.SkillLevel == 5) { maxLevel5.Checked = true; }
            else if (selectedSkill.SkillLevel == 10) { maxLevel10.Checked = true; }

            // set skill type
            skillTypeTextBox.Text = selectedSkill.SkillType.ToString("x");

            // set body part and weapon
            foreach (CheckBox entry in bodyPartsWeaponsBox.Controls)
            {
                entry.Checked = false;
                var entryFlag = int.Parse(entry.Tag.ToString().Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);

                if ((entryFlag & selectedSkill.BodyParts) == entryFlag)
                {
                    entry.Checked = true;
                }
            }

            // set status required
            statusRequiredTextBox.Text = selectedSkill.StatusRequired.ToString("x");

            // set usable state
            usableStateTextBox.Text = selectedSkill.UsableState.ToString("x");

            // set target team
            foreach (RadioButton entry in targetTeamBox.Controls)
            {
                entry.Checked = false;
                var entryFlag = int.Parse(entry.Tag.ToString().Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);
                
                if (entryFlag == selectedSkill.TargetTeam)
                {
                    entry.Checked = true;
                    break;
                }
            }

            // set target type
            foreach (RadioButton entry in targetTypeBox.Controls)
            {
                entry.Checked = false;
                var entryFlag = int.Parse(entry.Tag.ToString().Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);

                if (entryFlag == selectedSkill.TargetType)
                {
                    entry.Checked = true;
                    break;
                }
            }

            // set modifier status
            foreach (RadioButton entry in modifierStatusBox.Controls)
            {
                entry.Checked = false;
                var entryFlag = int.Parse(entry.Tag.ToString().Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);

                if (entryFlag == selectedSkill.ModifierStatus)
                {
                    entry.Checked = true;

                    // set modifier type (only need to do this if status isn't empty!)
                    foreach (RadioButton nestedEntry in modifierTypeBox.Controls)
                    {
                        var nestedEntryFlag = int.Parse(nestedEntry.Tag.ToString().Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);

                        if (nestedEntryFlag == selectedSkill.ModifierType)
                        {
                            nestedEntry.Checked = true;
                            break;
                        }
                    }
                }
            }

            // set modifier element
            foreach (CheckBox entry in modifierElementBox.Controls)
            {
                entry.Checked = false;
                var entryFlag = int.Parse(entry.Tag.ToString().Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);

                if ((entryFlag & selectedSkill.ModifierElement) == entryFlag)
                {
                    entry.Checked = true;
                }
            }

            // set damage type
            foreach (CheckBox entry in damageTypeBox.Controls)
            {
                entry.Checked = false;
                var entryFlag = int.Parse(entry.Tag.ToString().Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);

                if ((entryFlag & selectedSkill.DamageType) == entryFlag)
                {
                    entry.Checked = true;
                }
            }

            // set infliction flag
            foreach (RadioButton entry in inflictPanel.Controls)
            {
                entry.Checked = false;
                var entryFlag = int.Parse(entry.Tag.ToString().Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);

                if (entryFlag == selectedSkill.InflictionFlag)
                {
                    entry.Checked = true;
                    break;
                }
            }

            // set disables flags, if the box is enabled
            if (disableBox.Enabled == true)
            {
                foreach (CheckBox entry in disableBox.Controls)
                {
                    entry.Checked = false;
                    var entryFlag = int.Parse(entry.Tag.ToString().Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);

                    if ((entryFlag & selectedSkill.DisablesInflicted) == entryFlag)
                    {
                        entry.Checked = true;
                    }
                }
            }

            // set skill flags
            skillFlagsTextBox.Text = selectedSkill.SkillFlags.ToString("x");

            // set repurposed
            repurposedTextBox.Text = selectedSkill.Repurposed.ToString("x");
            
            UpdateSubheaderInfo(null, null);

            // set requirements
            for (int i = 0; i < 3; i++)
            {
                ComboBox listToSet = requirementsEditorBox.Controls.OfType<ComboBox>().FirstOrDefault(x => x.Name == "skill" + (i + 1) + "List");
                NumericUpDown levelToSet = requirementsEditorBox.Controls.OfType<NumericUpDown>().FirstOrDefault(x => x.Name == "skill" + (i + 1) + "Level");
                listToSet.SelectedIndex = rt.Skills[skillId].requiredSkills[i].Item1;
                levelToSet.Value = rt.Skills[skillId].requiredSkills[i].Item2;
            }
        }

        private void UpdateSubheaderInfo(object sender, EventArgs e)
        {
            var subheaderId = subheaderSelector.Value;
            subheaderIdTextBox.Text = selectedSkill.SubheaderData[(int) subheaderId].subheader.ToString("x");

            for (int i = 0; i < EO3Skill.NUMBER_OF_LEVELS; i++)
            {
                var currentLevel = (i + 1).ToString();
                var associatedTextBox = subheaderGroupBox.Controls.OfType<TextBox>().FirstOrDefault(x => x.Name == "level" + currentLevel);
                associatedTextBox.Text = selectedSkill.SubheaderData[(int)subheaderId].levelValues[i].ToString();
            }
        }

        private void applyChangesButton_Click(object sender, EventArgs e)
        {
            var skillId = skillList.SelectedIndex;

            // update name in memory, and reset the skill list
            var oldIndex = skillList.SelectedIndex;
            var newSjisName = new SJISString(skillNameTextBox.Text);
            snt.ReplaceString(skillList.SelectedIndex, newSjisName);

            skillList.Items.Clear();

            for (int i = 0; i < snt.sjisStrings.Length; i++)
            {
                skillList.Items.Add(
                    "0x" + i.ToString("X3") + ": " +
                    snt.sjisStrings[i].GetAscii()
                );
            }

            // time to write the important stuff
            selectedSkill.SkillLevel = byte.Parse(maxLevelBox.Controls.OfType<RadioButton>().FirstOrDefault(x => x.Checked == true).Tag.ToString().Replace("0x", ""),
                System.Globalization.NumberStyles.HexNumber);
            selectedSkill.SkillType = byte.Parse(skillTypeTextBox.Text, System.Globalization.NumberStyles.HexNumber);

            ushort bodyPartsWeaponsShort = 0;
            
            foreach (CheckBox entry in bodyPartsWeaponsBox.Controls)
            {
                if (entry.Checked == true)
                {
                    bodyPartsWeaponsShort += ushort.Parse(entry.Tag.ToString().Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);
                }
            }

            selectedSkill.BodyParts = bodyPartsWeaponsShort;
            selectedSkill.StatusRequired = ushort.Parse(statusRequiredTextBox.Text, System.Globalization.NumberStyles.HexNumber);
            selectedSkill.TargetTeam =  byte.Parse(targetTeamBox.Controls.OfType<RadioButton>().FirstOrDefault(x => x.Checked == true).Tag.ToString().Replace("0x", ""),
                System.Globalization.NumberStyles.HexNumber);
            selectedSkill.TargetType = byte.Parse(targetTypeBox.Controls.OfType<RadioButton>().FirstOrDefault(x => x.Checked == true).Tag.ToString().Replace("0x", ""),
                System.Globalization.NumberStyles.HexNumber);
            selectedSkill.ModifierStatus = byte.Parse(modifierStatusBox.Controls.OfType<RadioButton>().FirstOrDefault(x => x.Checked == true).Tag.ToString().Replace("0x", ""),
                System.Globalization.NumberStyles.HexNumber);

            try
            {
                selectedSkill.ModifierType = byte.Parse(modifierTypeBox.Controls.OfType<RadioButton>().FirstOrDefault(x => x.Checked == true).Tag.ToString().Replace("0x", ""),
                    System.Globalization.NumberStyles.HexNumber);
            }

            // null reference exception means nothing was set
            // yeah yeah using exceptions as program flow but
            // who really cares
            catch (NullReferenceException)
            {
                selectedSkill.ModifierType = 0x0;
            }

            ushort modifierElemShort = 0;

            foreach (CheckBox entry in modifierElementBox.Controls)
            {
                if (entry.Checked == true)
                {
                    modifierElemShort += ushort.Parse(entry.Tag.ToString().Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);
                }
            }

            selectedSkill.ModifierElement = modifierElemShort;

            ushort damageTypeShort = 0;

            foreach (CheckBox entry in damageTypeBox.Controls)
            {
                if (entry.Checked == true)
                {
                    damageTypeShort += ushort.Parse(entry.Tag.ToString().Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);
                }
            }

            selectedSkill.InflictionFlag = ushort.Parse(inflictPanel.Controls.OfType<RadioButton>().FirstOrDefault(x => x.Checked == true).Tag.ToString().Replace("0x", ""),
                System.Globalization.NumberStyles.HexNumber);

            ushort disableShort = 0;

            foreach (CheckBox entry in disableBox.Controls)
            {
                if (entry.Checked == true)
                {
                    disableShort += ushort.Parse(entry.Tag.ToString().Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);
                }
            }

            selectedSkill.DisablesInflicted = disableShort;
            selectedSkill.SkillFlags = ushort.Parse(skillFlagsTextBox.Text, System.Globalization.NumberStyles.HexNumber);
            selectedSkill.Repurposed = ushort.Parse(repurposedTextBox.Text, System.Globalization.NumberStyles.HexNumber);

            var subheaderId = int.Parse(subheaderSelector.Text);
            selectedSkill.SubheaderData[subheaderId].subheader = int.Parse(subheaderIdTextBox.Text, System.Globalization.NumberStyles.HexNumber);

            for (int i = 0; i < EO3Skill.NUMBER_OF_LEVELS; i++)
            {
                var textBoxContents = subheaderGroupBox.Controls.OfType<TextBox>().FirstOrDefault(x => x.Name == "level" + (i + 1).ToString()).Text;
                selectedSkill.SubheaderData[subheaderId].levelValues[i] = int.Parse(textBoxContents);
            }
            
            for (int i = 0; i < SkillRequirements.NUMBER_OF_REQUIREMENTS; i++)
            {
                var relevantList = requirementsEditorBox.Controls.OfType<ComboBox>().FirstOrDefault(x => x.Name == "skill" + (i + 1).ToString() + "List");
                var relevantSelector = requirementsEditorBox.Controls.OfType<NumericUpDown>().FirstOrDefault(x => x.Name == "skill" + (i + 1).ToString() + "Level");
                var reqSkillId = (byte) relevantList.SelectedIndex;
                var reqSkillLevel = byte.Parse(relevantSelector.Text);

                rt.Skills[skillId].requiredSkills[i] = new Tuple<byte, byte>(reqSkillId, reqSkillLevel);

                if (orCheckBox.Checked == true)
                {
                    rt.Skills[skillId].orRequirements = 1;
                }

                else
                {
                    rt.Skills[skillId].orRequirements = 0;
                }
            }

            // do this at the end, or else things get updated prematurely!
            skillList.SelectedIndex = oldIndex;
        }

        private void saveTableAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var saveNameTableDialog = new SaveFileDialog();
            saveNameTableDialog.RestoreDirectory = true;

            if (saveNameTableDialog.ShowDialog() == DialogResult.OK)
            {
                snt.Write(saveNameTableDialog.FileName);
            }
        }

        private void HexTextBoxLimiter(object sender, KeyPressEventArgs e)
        {
            if (char.IsDigit(e.KeyChar) ||
                char.ToUpper(e.KeyChar) == 'A' ||
                char.ToUpper(e.KeyChar) == 'B' ||
                char.ToUpper(e.KeyChar) == 'C' ||
                char.ToUpper(e.KeyChar) == 'D' ||
                char.ToUpper(e.KeyChar) == 'E' ||
                char.ToUpper(e.KeyChar) == 'F' ||
                char.IsControl(e.KeyChar))
            {
                e.Handled = false;
            }

            else
            {
                e.Handled = true;
            }
        }

        private void ModifierChanged(object sender, EventArgs e)
        {
            var radioSender = (RadioButton) sender;

            if (radioSender.Text != "Nope")
            {
                if (radioSender.Checked == true)
                {
                    EnableModifierBoxes();
                }

                else
                {
                    DisableModifierBoxes();
                }
            }

            else
            {
                if (radioSender.Checked == true)
                {
                    DisableModifierBoxes();
                }

                else
                {
                    EnableModifierBoxes();
                }
            }
        }

        private void DisableChanged(object sender, EventArgs e)
        {
            var radioSender = (RadioButton) sender;

            if (radioSender.Text == "No")
            {
                if (radioSender.Checked == true)
                {
                    DisableDisableBox();
                }

                else
                {
                    EnableDisableBox();
                }
            }

            else
            {
                if (radioSender.Checked == true)
                {
                    EnableDisableBox();
                }

                else
                {
                    DisableDisableBox();
                }
            }
        }

        private void EnableModifierBoxes()
        {
            modifierTypeBox.Enabled = true;
            modifierElementBox.Enabled = true;
        }

        private void DisableModifierBoxes()
        {
            modifierTypeBox.Enabled = false;
            modifierElementBox.Enabled = false;

            // make sure we don't write unnecessary data
            // i don't know how EO3 would react to that and i don't want to find out
            foreach (RadioButton entry in modifierTypeBox.Controls)
            {
                entry.Checked = false;
            }

            foreach (CheckBox entry in modifierElementBox.Controls)
            {
                entry.Checked = false;
            }
        }

        private void EnableDisableBox()
        {
            disableBox.Enabled = true;
        }

        private void DisableDisableBox()
        {
            disableBox.Enabled = false;

            // make sure we don't write unnecessary data
            // i don't know how EO3 would react to that and i don't want to find out
            foreach (CheckBox entry in disableBox.Controls)
            {
                entry.Checked = false;
            }
        }

        private void saveTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var saveDialog = new SaveFileDialog();
            saveDialog.RestoreDirectory = true;

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                st.WriteToFile(saveDialog.FileName);
                snt.Write(saveDialog.FileName.Replace("table", "nametable"));
                rt.WriteToFile(saveDialog.FileName.Replace("table", "learntable"));
            }
        }
    }
}