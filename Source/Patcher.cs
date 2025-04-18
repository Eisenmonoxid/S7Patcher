using S7Patcher.Properties;
using System;
using System.Collections.Generic;
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

            try
            {
                GlobalStream.Position = 0x00D24C;
                GlobalStream.ReadExactly(Result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

            return Result.SequenceEqual(Identifier);
        }

        public void PatchFile(bool EasyDebug)
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
            Helpers.Instance.WriteToFile(GlobalStream, 0x62F0A9, [0xE9, 0xF2, 0x00, 0x00, 0x00, 0x90]);

            if (EasyDebug)
            {
                Helpers.Instance.WriteToFile(GlobalStream, 0x00D2D9, [0x90, 0x90]); // Always show message before startup happens
            }
        }

        public void ReplaceDataInProfileFile()
        {
            string ProfilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Settlers7", "Profiles.xml");
            if (File.Exists(ProfilePath) == false)
            {
                do
                {
                    Console.WriteLine("\n" + ProfilePath + " not found!\nPlease input the path to the Profiles.xml file:\n(Input skip to skip .xml file patching)\n");
                    ProfilePath = Console.ReadLine();
                    if (ProfilePath == "skip")
                    {
                        Console.WriteLine("Skipping .xml file patching ...");
                        return;
                    }
                    else if (File.Exists(ProfilePath))
                    {
                        break;
                    }
                } while (true);
            }

            Console.WriteLine("Going to patch file: " + ProfilePath);
            UpdateProfileXML(ProfilePath);
        }

        private void UpdateProfileXML(string Filepath)
        {
            List<string> Lines;
            try
            {
                Lines = [.. File.ReadAllLines(Filepath, Encoding.UTF8)];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }

            int StartIndex = Lines.FindIndex(0, Element => Element.Contains("<TitleSystem>"));
            int EndIndex = Lines.FindIndex(0, Element => Element.Contains("</TitleSystem>"));
            int SynchIndex = Lines.FindIndex(0, Element => Element.Contains("<LastSynchTime>"));

            if (StartIndex == -1 || EndIndex == -1 || SynchIndex == -1)
            {
                return;
            }

            if ((EndIndex - StartIndex) != 4)
            {
                Lines.RemoveRange(StartIndex + 3, (EndIndex - StartIndex) - 4);
            }

            Lines[StartIndex + 2] = Resources.Branch;
            Lines[StartIndex + 3] = PopulateTitleSystem(Resources.Title);
            Lines[SynchIndex + 2] = Resources.Year;

            try
            {
                File.WriteAllLines(Filepath, Lines);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private string PopulateTitleSystem(string Input)
        {
            List<string> TitleCollection = ["    <Titles>"];
            string Title = Input.Replace("%x2", "10");

            for (ushort TitleID = 0; TitleID < 4; TitleID++)
            {
                TitleCollection.Add(Title.Replace("%x1", TitleID.ToString()));
            }

            TitleCollection.Add(Input.Replace("%x1", "28").Replace("%x2", "0"));
            TitleCollection.Add("    </Titles>");

            return string.Join("\r\n", TitleCollection);
        }
    }
}
