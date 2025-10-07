using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace S7Patcher.Source
{
    internal class BinaryParser
    {
        private readonly byte[] Magic = Encoding.UTF8.GetBytes("EMX");
        private readonly BinaryReader GlobalReader;
        private readonly UInt32 BlockOffset = 0x0;

        public BinaryParser(Stream BinaryStream)
        {
            if (BinaryStream == null || BinaryStream.CanRead == false)
            {
                throw new Exception("ERROR: Invalid binary stream.");
            }

            BlockOffset = (uint)(Magic.Length + sizeof(byte));
            GlobalReader = new BinaryReader(BinaryStream);

            if (!IsValidBinaryFile())
            {
                throw new Exception("ERROR: Invalid binary file.");
            }
        }

        public void Dispose()
        {
            GlobalReader?.Close();
            GlobalReader?.Dispose();
        }

        private bool IsValidBinaryFile()
        {
            byte[] Result = new byte[Magic.Length];
            GlobalReader.BaseStream.Seek(0, SeekOrigin.Begin);
            GlobalReader.Read(Result, 0, Result.Length);
            return Result.SequenceEqual(Magic);
        }

        public byte GetFileVersion()
        {
            GlobalReader.BaseStream.Position = Magic.Length;
            return GlobalReader.ReadByte();
        }

        public bool ParseBinaryFileContent(byte Identifier, out Dictionary<UInt32, byte[]> Result, string ID = "")
        {
            Result = [];
            byte[] IDBytes = (ID == "") ? [0x0, 0x0, 0x0] : Encoding.UTF8.GetBytes(ID);

            GlobalReader.BaseStream.Position = BlockOffset;
            byte BlockID = GlobalReader.ReadByte();
            while (BlockID != Identifier)
            {
                UInt32 Size = GlobalReader.ReadUInt32();
                if ((GlobalReader.BaseStream.Position + Size) > GlobalReader.BaseStream.Length)
                {
                    return false;
                }

                GlobalReader.BaseStream.Position += Size;
                BlockID = GlobalReader.ReadByte();
            }

            // Read blocks
            UInt32 BlockSize = GlobalReader.ReadUInt32();
            long Position = GlobalReader.BaseStream.Position;

            while ((Position + BlockSize) > GlobalReader.BaseStream.Position)
            {
                // Next three bytes are ID or zero
                byte[] EntryID = GlobalReader.ReadBytes(IDBytes.Length);
                if (EntryID.SequenceEqual(IDBytes) == false)
                {
                    GlobalReader.BaseStream.Position += sizeof(UInt32);
                    GlobalReader.ReadBytes(GlobalReader.ReadUInt16());
                    continue;
                }

                Result.Add(GlobalReader.ReadUInt32(), GlobalReader.ReadBytes(GlobalReader.ReadUInt16()));
            }

            return true;
        }
    }
}
