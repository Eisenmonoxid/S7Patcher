using S7Patcher.Properties;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace S7Patcher.Source
{
    internal class Patcher(FileStream Stream)
    {
        private readonly FileStream GlobalStream = Stream;

        public bool IsExecutableValid()
        {
            byte[] Identifier = [0x8B, 0x01];
            byte[] Result = new byte[Identifier.Length];

            GlobalStream.Position = 0x00D24C;
            GlobalStream.ReadExactly(Result);

            return Result.SequenceEqual(Identifier);
        }

        public void PatchFile()
        {
            Helpers.Instance.WriteToFile(GlobalStream, 0x00D40D, [0xE8, 0xBC, 0x99, 0x68, 0x00, 0x90]);
            Helpers.Instance.WriteToFile(GlobalStream, 0x696DCE, [0x55, 0x89, 0xE5, 0xC6, 0x05, 0x79, 0x5B, 0x0E, 0x01, 0x01, 0x89, 0xEC, 0x5D, 0xC3]);
            Helpers.Instance.WriteToFile(GlobalStream, 0x1A978E, [0xEB]);
            Helpers.Instance.WriteToFile(GlobalStream, 0x1A977C, [0x90, 0x90]);
            Helpers.Instance.WriteToFile(GlobalStream, 0x64477C, [0xB0, 0x00]);
            Helpers.Instance.WriteToFile(GlobalStream, 0x21929C, [0xB0, 0x00]);
            Helpers.Instance.WriteToFile(GlobalStream, 0x219224, [0xB0, 0x00]);
            Helpers.Instance.WriteToFile(GlobalStream, 0x195C34, [0xEB, 0x15]);
            Helpers.Instance.WriteToFile(GlobalStream, 0x69000F, [0x94]);
            Helpers.Instance.WriteToFile(GlobalStream, 0x58BC2E, [0x01]);
            Helpers.Instance.WriteToFile(GlobalStream, 0x696D83, [0x90, 0x90, 0x90, 0x90, 0x90]);
            Helpers.Instance.WriteToFile(GlobalStream, 0x696DC8, [0xE9, 0x0B, 0x03, 0x00, 0x00, 0x90]);
        }

        public void ReplaceDataInProfileFile()
        {
            string ProfilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Settlers7", "Profiles.xml");
            if (File.Exists(ProfilePath) == false)
            {
                Console.WriteLine(ProfilePath + " does not exist! Skipping .xml file patch ...");
                return;
            }

            UpdateProfileXML(ProfilePath);
        }

        private void UpdateProfileXML(string Filepath)
        {
            string[] Lines;
            try
            {
                Lines = File.ReadAllLines(Filepath, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }

            for (ushort Index = 0; Index < Lines.Length; Index++)
            {
                if (Lines[Index].Contains("<Titles>") && Lines[Index + 1].Contains("</TitleSystem>"))
                {
                    Lines[Index - 1] = Resources.Branch;
                    Lines[Index] = Resources.Title;
                }
                else if (Lines[Index].Contains("<LastSynchTime>"))
                {
                    Lines[Index + 2] = Resources.Year;
                }
            }

            try
            {
                File.WriteAllLines(Filepath, Lines);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
