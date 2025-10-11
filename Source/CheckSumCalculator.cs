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

            bool Result = WritePEHeaderFileCheckSum(CurrentStream, BitConverter.GetBytes((uint)CheckSum));
            Helpers.Instance.CloseFileStream(CurrentStream);

            return Result;
        }

        private bool WritePEHeaderFileCheckSum(FileStream Stream, byte[] CheckSum)
        {
            BinaryReader Reader = new(Stream);

            Reader.BaseStream.Position = 0x3C;
            Reader.BaseStream.Position = Reader.ReadInt32();

            if (Reader.ReadInt32() != 0x4550)
            {
                Console.WriteLine("[ERROR] CheckSum offset not found! Skipping ...");
                return false;
            }

            Reader.BaseStream.Position += 0x54;
            Helpers.Instance.WriteToFile(Stream, Reader.BaseStream.Position, CheckSum);

            return true;
        }
    }
}
