﻿using CyberCAT.Core.Classes.NodeRepresentations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static CP2077SaveEditor.SaveFileHelper;

namespace CP2077SaveEditor
{
    public class AppearanceHelper
    {
        private SaveFileHelper activeSave;

        public AppearanceHelper(SaveFileHelper _saveFile)
        {
            activeSave = _saveFile;
            MainSections = new[] { activeSave.GetAppearanceContainer().FirstSection, activeSave.GetAppearanceContainer().SecondSection, activeSave.GetAppearanceContainer().ThirdSection };
        }

        public CharacterCustomizationAppearances.Section[] MainSections { get; set; }

        public string HairStyle {
            get {
                return "";
            }
            set
            {

            }
        }

        public List<object> GetEntries(CharacterCustomizationAppearances.Section appearanceSection, AppearanceEntryType entryType, string searchString)
        {
            var foundEntries = new List<object>();
            foreach (CharacterCustomizationAppearances.AppearanceSection subSection in appearanceSection.AppearanceSections)
            {
                if (entryType == AppearanceEntryType.MainListEntry)
                {
                    foreach (CharacterCustomizationAppearances.HashValueEntry mainListEntry in subSection.MainList)
                    {
                        if (CompareMainListAppearanceEntries(mainListEntry.SecondString, searchString) == true)
                        {
                            foundEntries.Add(mainListEntry);
                        }
                    }
                }
                else
                {
                    foreach (CharacterCustomizationAppearances.ValueEntry additionalListEntry in subSection.AdditionalList)
                    {
                        if (additionalListEntry.FirstString == searchString)
                        {
                            foundEntries.Add(additionalListEntry);
                        }
                    }
                }
            }
            return foundEntries;
        }

        public List<object> GetEntries(string searchString)
        {
            var location = StringToLocation(searchString);
            if (location != null)
            {
                return GetEntries(location.Section, location.EntryType, location.SearchString);
            }
            else
            {
                return new List<object>();
            }
        }

        public List<object> GetAllEntries(AppearanceEntryType entryType, string searchString)
        {
            var foundEntries = new List<object>();
            foreach (CharacterCustomizationAppearances.Section section in MainSections)
            {
                foundEntries.AddRange(GetEntries(section, entryType, searchString));
            }
            return foundEntries;
        }

        public void RemoveEntry(object entry)
        {
            foreach (CharacterCustomizationAppearances.Section section in MainSections)
            {
                foreach (CharacterCustomizationAppearances.AppearanceSection subSection in section.AppearanceSections)
                {
                    if (entry is CharacterCustomizationAppearances.HashValueEntry)
                    {
                        if (subSection.MainList.Contains((CharacterCustomizationAppearances.HashValueEntry)entry))
                        {
                            subSection.MainList.Remove((CharacterCustomizationAppearances.HashValueEntry)entry);
                        }
                    } else {
                        if (subSection.AdditionalList.Contains((CharacterCustomizationAppearances.ValueEntry)entry))
                        {
                            subSection.AdditionalList.Remove((CharacterCustomizationAppearances.ValueEntry)entry);
                        }
                    }
                }
            }
        }

        public void CreateEntry(object entry, string[] sectionNames, CharacterCustomizationAppearances.Section section)
        {
            var subSections = section.AppearanceSections.Where(x => sectionNames.Contains(x.SectionName));
            if (subSections != null)
            {
                foreach (CharacterCustomizationAppearances.AppearanceSection singleSubSection in subSections)
                {
                    if (entry is CharacterCustomizationAppearances.HashValueEntry)
                    {
                        singleSubSection.MainList.Add((CharacterCustomizationAppearances.HashValueEntry)entry);
                    }
                    else
                    {
                        singleSubSection.AdditionalList.Add((CharacterCustomizationAppearances.ValueEntry)entry);
                    }
                    
                }
            }
        }

        public void SetValue(AppearanceField field, string searchString, object value)
        {
            var entries = GetEntries(searchString);
            foreach (object entry in entries)
            {
                if (entry is CharacterCustomizationAppearances.HashValueEntry)
                {
                    switch (field)
                    {
                        case AppearanceField.FirstString:
                            ((CharacterCustomizationAppearances.HashValueEntry)entry).FirstString = (string)value;
                            break;
                        case AppearanceField.Hash:
                            ((CharacterCustomizationAppearances.HashValueEntry)entry).Hash = (ulong)value;
                            break;
                        case AppearanceField.SecondString:
                            ((CharacterCustomizationAppearances.HashValueEntry)entry).SecondString = (string)value;
                            break;
                    }
                }
                else if (entry is CharacterCustomizationAppearances.ValueEntry)
                {
                    switch (field)
                    {
                        case AppearanceField.FirstString:
                            ((CharacterCustomizationAppearances.ValueEntry)entry).FirstString = (string)value;
                            break;
                        case AppearanceField.SecondString:
                            ((CharacterCustomizationAppearances.ValueEntry)entry).SecondString = (string)value;
                            break;
                    }
                }
            }
        }

        public string GetValue(CharacterCustomizationAppearances.Section appearanceSection, AppearanceEntryType entryType, AppearanceField fieldToGet, string searchString)
        {
            var entries = GetEntries(appearanceSection, entryType, searchString);

            if (entries.Count < 1)
            {
                return "default";
            }

            if (entries[0] is CharacterCustomizationAppearances.HashValueEntry)
            {
                var castedEntry = (CharacterCustomizationAppearances.HashValueEntry)entries[0];
                if (fieldToGet == AppearanceField.FirstString)
                {
                    return castedEntry.FirstString;
                }
                else if (fieldToGet == AppearanceField.Hash)
                {
                    return castedEntry.Hash.ToString();
                }
                else
                {
                    return castedEntry.SecondString;
                }
            } else {
                var castedEntry = (CharacterCustomizationAppearances.ValueEntry)entries[0];
                if (fieldToGet == AppearanceField.FirstString)
                {
                    return castedEntry.FirstString;
                }
                else
                {
                    return castedEntry.SecondString;
                }
            }
        }

        public string GetValue(string searchString)
        {
            var location = StringToLocation(searchString);
            if (location != null)
            {
                return GetValue(location.Section, location.EntryType, location.EntryField, location.SearchString);
            }
            else
            {
                return "default";
            }
        }

        public void SetConcatedValue(string searchString, string newValue, int position = -1)
        {
            string currentValue;
            if (position < 0)
            {
                currentValue = GetValue(searchString).Split("__", StringSplitOptions.None).Last();
            }
            else
            {
                currentValue = GetValue(searchString).Split("__", StringSplitOptions.None)[position];
            }

            var sections = new[] { activeSave.GetAppearanceContainer().FirstSection, activeSave.GetAppearanceContainer().SecondSection, activeSave.GetAppearanceContainer().ThirdSection };
            foreach (CharacterCustomizationAppearances.Section section in sections)
            {
                foreach (CharacterCustomizationAppearances.AppearanceSection subSection in section.AppearanceSections)
                {
                    foreach (CharacterCustomizationAppearances.HashValueEntry mainEntry in subSection.MainList)
                    {
                        try
                        {
                            var valueParts = mainEntry.FirstString.Split("__", StringSplitOptions.None);
                            var targetPart = valueParts.Last();

                            if (position > -1)
                            {
                                targetPart = valueParts[position];
                            }

                            if (targetPart == currentValue)
                            {
                                if (position < 0)
                                {
                                    valueParts[valueParts.Length - 1] = newValue;
                                }
                                else
                                {
                                    valueParts[position] = newValue;
                                }

                                mainEntry.FirstString = string.Join("__", valueParts);
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
            }
        }

        public void SetFacialValue(string fieldName, int fieldNum, int value)
        {
            var entries = GetEntries("first.additional." + fieldName);

            if (entries.Count < 1)
            {
                var newEntry = new CharacterCustomizationAppearances.ValueEntry();
                newEntry.FirstString = fieldName;
                newEntry.SecondString = "h000";

                CreateEntry(newEntry, new[] { "TPP", "character_customization" }, MainSections[0]);
                SetFacialValue(fieldName, fieldNum, value);
            }
            else
            {
                if (value == 1)
                {
                    foreach (CharacterCustomizationAppearances.ValueEntry entry in entries)
                    {
                        RemoveEntry(entry);
                    }
                }
                else
                {
                    foreach (CharacterCustomizationAppearances.ValueEntry entry in entries)
                    {
                        entry.SecondString = "h" + (value - 1).ToString("00") + fieldNum.ToString();
                    }
                }
            }
        }

        public void SetAllValues(CharacterCustomizationAppearances newValues)
        {
            var sections = new[] { activeSave.GetAppearanceContainer().FirstSection, activeSave.GetAppearanceContainer().SecondSection, activeSave.GetAppearanceContainer().ThirdSection };
            var newSections = new[] { newValues.FirstSection, newValues.SecondSection, newValues.ThirdSection };

            var i = 0;
            foreach (CharacterCustomizationAppearances.Section section in sections)
            {
                section.AppearanceSections.Clear();
                foreach (CharacterCustomizationAppearances.AppearanceSection subSection in newSections[i].AppearanceSections)
                {
                    section.AppearanceSections.Add(subSection);
                }
                i++;
            }

            if (newValues.Strings != null)
            {
                activeSave.GetAppearanceContainer().Strings.Clear();
                foreach (string singleString in newValues.Strings)
                {
                    activeSave.GetAppearanceContainer().Strings.Add(singleString);
                }
            }
        }

        public AppearanceEntryLocation StringToLocation(string searchString)
        {
            var searchValues = searchString.Split('.');
            if (searchValues.Length < 3 && searchValues.Length > 4)
            {
                return null;
            }

            var result = new AppearanceEntryLocation(activeSave.GetAppearanceContainer().FirstSection, AppearanceEntryType.MainListEntry, (searchValues.Length == 3) ? searchValues[2] : searchValues[3], AppearanceField.FirstString);
            if (searchValues[0] == "second")
            {
                result.Section = activeSave.GetAppearanceContainer().SecondSection;
            }
            else if (searchValues[0] == "third")
            {
                result.Section = activeSave.GetAppearanceContainer().ThirdSection;
            }

            if (searchValues[1] == "additional")
            {
                result.EntryType = AppearanceEntryType.AdditionalListEntry;
            }

            if (searchValues.Length == 4)
            {
                if (searchValues[2] == "hash")
                {
                    result.EntryField = AppearanceField.Hash;
                }
                else if (searchValues[2] == "second")
                {
                    result.EntryField = AppearanceField.SecondString;
                }
            }
            
            return result;
        }

        public void SetHairStyle(string friendlyName)
        {
            if (friendlyName != "Shaved")
            {
                SetValue(AppearanceField.Hash, "first.main.hair_color", AppearanceValueLists.HairStyles[friendlyName]);
            }
        }

        public void SetHairColor(string colorString)
        {
            if (colorString != "None")
            {
                SetValue(AppearanceField.FirstString, "first.main.hair_color", colorString);
                if (activeSave.GetAppearanceContainer().Strings.Count < 1)
                {
                    activeSave.GetAppearanceContainer().Strings.Add(colorString.Substring(3));
                    activeSave.GetAppearanceContainer().Strings.Add("Short");
                }
                else
                {
                    activeSave.GetAppearanceContainer().Strings[0] = colorString.Substring(3);
                }
            }
        }

        public void CreateHairEntry(string friendlyName)
        {
            var hairsList = activeSave.GetAppearanceContainer().FirstSection.AppearanceSections.Where(x => x.SectionName == "hairs").FirstOrDefault().MainList;

            var newEntry = new CharacterCustomizationAppearances.HashValueEntry();
            newEntry.FirstString = AppearanceValueLists.HairColors[0];
            newEntry.Hash = AppearanceValueLists.HairStyles[friendlyName];
            newEntry.SecondString = "hair_color1";

            hairsList.Add(newEntry);
        }

        public void DeleteHairEntry()
        {
            var hairsList = activeSave.GetAppearanceContainer().FirstSection.AppearanceSections.Where(x => x.SectionName == "hairs").FirstOrDefault().MainList;
            hairsList.Remove(hairsList[0]);

            var hairsCreationList = activeSave.GetAppearanceContainer().FirstSection.AppearanceSections.Where(x => x.SectionName == "character_customization").FirstOrDefault().MainList;
            var creationEntry = hairsCreationList.Where(x => CompareMainListAppearanceEntries("hair_color", x.SecondString)).FirstOrDefault();

            if (creationEntry != null)
            {
                hairsCreationList.Remove(creationEntry);
            }
        }

        public void SetSkinColor(string colorString)
        {
            SetConcatedValue("third.main.first.body_color", colorString);
        }

        public void SetEyeColor(string colorString)
        {
            SetConcatedValue("first.main.first.eyes_color", colorString);
        }

        public bool CompareMainListAppearanceEntries(string entry1, string entry2)
        {
            return Regex.Replace(entry1, @"[\d-]", string.Empty) == Regex.Replace(entry2, @"[\d-]", string.Empty);
        }
    }

    public class AppearanceEntryLocation
    {
        public CharacterCustomizationAppearances.Section Section { get; set; }
        public AppearanceEntryType EntryType { get; set; }
        public AppearanceField EntryField { get; set; }
        public string SearchString { get; set; }

        public AppearanceEntryLocation(CharacterCustomizationAppearances.Section _sec, AppearanceEntryType _type, string _searchString, AppearanceField _field = AppearanceField.FirstString)
        {
            Section = _sec;
            EntryType = _type;
            EntryField = _field;
            SearchString = _searchString;
        }
    }

    public static class AppearanceValueLists
    {
        public static Dictionary<string, ulong> HairStyles { get; } = JsonConvert.DeserializeObject<Dictionary<string, ulong>>(CP2077SaveEditor.Properties.Resources.HairStyles);
        public static List<string> HairColors { get; } = JsonConvert.DeserializeObject<List<string>>(CP2077SaveEditor.Properties.Resources.HairColors);
        public static List<string> SkinColors { get; } = JsonConvert.DeserializeObject<List<string>>(CP2077SaveEditor.Properties.Resources.SkinColors);
        public static List<string> EyeColors { get; } = JsonConvert.DeserializeObject<List<string>>(CP2077SaveEditor.Properties.Resources.EyeColors);
    }
}
