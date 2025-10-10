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

        private uint? UpdatePEHeaderFileCheckSum(string Path, long Size)
        {
            // This will only work on Windows
            uint CheckSum = 0x0;
            uint HeaderSum = 0x0;

            using MemoryMappedFile Mapping = MemoryMappedFile.CreateFromFile(Path);
            using MemoryMappedViewAccessor View = Mapping.CreateViewAccessor();

            int Result = CheckSumMappedFile(View.SafeMemoryMappedViewHandle, (uint)Size, ref HeaderSum, ref CheckSum); 
            if (Result == 0x0)
            {
                Console.WriteLine("[ERROR] CheckSumMappedFile failed with error code: " + Result);
                return null;
            }

            Console.WriteLine("[INFO] Calculated new CheckSum: 0x" + $"{CheckSum.ToString():X}");
            return CheckSum;
        }

        public bool WritePEHeaderFileCheckSum(string Path, long Size)
        {
            uint? CheckSum = UpdatePEHeaderFileCheckSum(Path, Size);
            if (CheckSum == null)
            {
                return false;
            }

            FileStream CurrentStream = Helpers.Instance.OpenFileStream(Path);
            if (CurrentStream == null)
            {
                return false;
            }

            Helpers.Instance.WriteToFile(CurrentStream, 0x168, BitConverter.GetBytes((uint)CheckSum));
            Helpers.Instance.CloseFileStream(CurrentStream);

            return true;
        }
    }
}
