using SoulsFormats;

namespace ER_Save_Tool
{
    class SaveFile
    {
        public byte[] Checksum { get; set; }

        public byte[] Data { get; set; }

        public long SteamID { get; set; }

        public SaveFile(BinaryReaderEx br, bool menu)
        {
            Checksum = br.GetBytes(0, 16);
            Data = br.GetBytes(16, (int)br.Stream.Length - 16);
            if (menu)
                SteamID = br.GetInt64(20);
            else
                SteamID = 0;
        }

    }
}
