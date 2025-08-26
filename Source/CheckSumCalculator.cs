using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace S7Patcher.Source
{
    internal class CheckSumCalculator
    {
        [DllImport("imagehlp.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int CheckSumMappedFile(SafeMemoryMappedViewHandle BaseAddress, uint FileLength, 
            ref uint HeaderSum, ref uint CheckSum);
        private uint UpdatePEHeaderFileCheckSum(string Path, long Size)
        {
            // This will only work on Windows
            uint CheckSum = 0x0;
            uint HeaderSum = 0x0;

            using MemoryMappedFile Mapping = MemoryMappedFile.CreateFromFile(Path);
            using MemoryMappedViewAccessor View = Mapping.CreateViewAccessor();

            CheckSumMappedFile(View.SafeMemoryMappedViewHandle, (uint)Size, ref HeaderSum, ref CheckSum); 

            Console.WriteLine("Calculated new CheckSum: 0x" + $"{CheckSum.ToString():X}");
            return CheckSum;
        }

        public void WritePEHeaderFileCheckSum(FileStream Stream)
        {
            string Path = Stream.Name;
            long Size = Stream.Length;

            CloseStream(Stream);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == false)
            {
                return;
            }

            uint CheckSum = UpdatePEHeaderFileCheckSum(Path, Size);
            FileStream CurrentStream = Helpers.Instance.OpenFileStream(Path);
            if (CurrentStream == null)
            {
                return;
            }

            Helpers.Instance.WriteToFile(CurrentStream, 0x168, BitConverter.GetBytes(CheckSum));
            CloseStream(CurrentStream);
        }

        private void CloseStream(FileStream Stream)
        {
            if (Stream != null)
            {
                Stream.Close();
                Stream.Dispose();
            }
        }
    }
}
