using System;
using System.Security.Cryptography;
using SoulsFormats;
using System.Linq;
using System.IO;

namespace ER_Save_Tool
{
    class SL2
    {
        public BND4 BND { get; set; }

        public SaveFile[] Files { get; set; }

        public bool[] Active { get; set; }

        public SL2(string path)
        {
            byte[] data = File.ReadAllBytes(path);
            BND = BND4.Read(data);
            Files = new SaveFile[12];
            Active = new bool[10];
            for (int i = 0; i < 12; i++)
            {
                BinaryReaderEx br = new BinaryReaderEx(false, BND.Files[i].Bytes);
                if (i == 10)
                {
                    Files[i] = new SaveFile(br, true);
                    Active = br.GetBooleans(6500, 10);
                }
                else
                    Files[i] = new SaveFile(br, false);
            }
        }

        public static bool ValidateChecksum(SaveFile s)
        {
            byte[] cs = CalculateMD5(s.Data);

            if (s.Checksum.SequenceEqual(cs))
                return true;
            else
            {
                s.Checksum = cs;
                return false;
            }
        }

        private static byte[] CalculateMD5(byte[] bytes)
        {
            byte[] checksum;
            using (var md5 = MD5.Create())
            {
                checksum = md5.ComputeHash(bytes);
            }
            return checksum;
        }

        public bool Save(SL2 sl2, string path)
        {
            for (int i = 0; i < 12; i++)
            {
                if (i == 10)
                {
                    byte[] steam = BitConverter.GetBytes(Files[i].SteamID);
                    Buffer.BlockCopy(steam, 0, Files[i].Data, 4, 8);
                    ValidateChecksum(Files[i]);
                    Buffer.BlockCopy(Files[i].Checksum, 0, BND.Files[i].Bytes, 0, 16);
                    Buffer.BlockCopy(Files[i].Data, 0, BND.Files[i].Bytes, 16, BND.Files[i].Bytes.Length - 16);
                }
                else
                {
                    ValidateChecksum(Files[i]);
                    Buffer.BlockCopy(Files[i].Checksum, 0, BND.Files[i].Bytes, 0, 16);
                    Buffer.BlockCopy(Files[i].Data, 0, BND.Files[i].Bytes, 16, BND.Files[i].Bytes.Length - 16);
                }
            }
            BND.Write(path);
            return true;
        }

        public bool PatchRegulation(SL2 sl2, Regulation regulation, string ver, bool overwrite)
        {
            if (!overwrite)
            {
                try
                {
                    byte[] newReg = new byte[0x240010];

                    var data = sl2.Files[11].Data;
                    var size = BitConverter.GetBytes(regulation.Data.Length);
                    var version = BitConverter.GetBytes(Int32.Parse(ver));

                    Buffer.BlockCopy(data, 0, newReg, 0, 8);
                    Buffer.BlockCopy(version, 0, newReg, 8, 4);
                    Buffer.BlockCopy(size, 0, newReg, 12, 4);
                    Buffer.BlockCopy(regulation.Data, 0, newReg, 16, regulation.Data.Length);

                    sl2.Files[11].Data = newReg;

                    return true;
                }
                catch { return false; }
            }
            else
            {
                try
                {
                    var data = sl2.Files[11].Data;
                    var size = BitConverter.ToInt32(data, 12);
                    var version = BitConverter.GetBytes(Int32.Parse(ver));

                    BinaryReaderEx br = new BinaryReaderEx(false, data);
                    byte[] iv = br.GetBytes(16, 16);
                    byte[] sRegData = br.GetBytes(32, size - 16);
                    byte[] sRegDec = regulation.Decrypt(regulation.Key, iv, sRegData);
                    BND4 sRegBND = regulation.Decompress(sRegDec);

                    regulation.Binder4.Files[0].Bytes = new byte[sRegBND.Files[0].Bytes.Length];
                    regulation.Binder4.Files[0].Bytes = sRegBND.Files[0].Bytes;
                    regulation.Binder4.Version = ver;
                    Buffer.BlockCopy(version, 0, sl2.Files[11].Data, 8, 4);

                    return true;
                }
                catch { return false; }
            }
            
        }

    }
}
