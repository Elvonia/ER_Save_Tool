using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ER_Save_Tool
{
    class Regulation
    {
        public byte[] Key = { 0x99, 0xBF, 0xFC, 0x36, 0x6A, 0x6B, 0xC8, 0xC6, 0xF5, 0x82,
            0x7D, 0x09, 0x36, 0x02, 0xD6, 0x76, 0xC4, 0x28, 0x92, 0xA0, 0x1C, 0x20, 0x7F,
            0xB0, 0x24, 0xD3, 0xAF, 0x4E, 0x49, 0x3F, 0xEF, 0x99 };

        public byte[] Data { get; set; }

        public BND4 Binder4 { get; set; }

        public string Path { get; set; }

        public Regulation(string path)
        {
            Path = path;
            Data = File.ReadAllBytes(path);
            BinaryReaderEx br = new BinaryReaderEx(false, Data);
            byte[] iv = br.GetBytes(0, 16);
            byte[] content = br.GetBytes(16, Data.Length - 16);
            byte[] decrypted = Decrypt(Key, iv, content);
            Binder4 = Decompress(decrypted);
        }

        public byte[] Decrypt(byte[] key, byte[] iv, byte[] data)
        {
            using (Aes aes = Aes.Create())
            {
                byte[] decrypted;
                aes.Mode = CipherMode.CBC;
                aes.KeySize = 256;
                aes.Padding = PaddingMode.Zeros;
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                    }
                    decrypted = ms.ToArray();
                    return decrypted;
                }
            }
        }

        public BND4 Decompress(byte[] data)
        {
            return BND4.Read(data);
        }

    }
}
