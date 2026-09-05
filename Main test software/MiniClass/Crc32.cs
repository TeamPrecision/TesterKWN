using System.Text;

namespace ATS.Models
{
    public class Crc32
    {
        static readonly uint[] Table;

        static Crc32()
        {
            uint polynomial = 0xedb88320;
            Table = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                uint crc = i;
                for (uint j = 8; j > 0; j--)
                {
                    if ((crc & 1) == 1)
                        crc = (crc >> 1) ^ polynomial;
                    else
                        crc >>= 1;
                }
                Table[i] = crc;
            }
        }

        public static string Compute(JsonFormat message)
        {
            string input = $"{message.Header}{message.Date}{message.Time}{message.Tester}{message.Command}";
            input += $"{message.Payload.Data1}{message.Payload.Data2}{message.Payload.Data3}{message.Payload.Data4}{message.Payload.Data5}";

            uint crc = 0xffffffff;
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            foreach (byte b in bytes)
            {
                byte tableIndex = (byte)((crc & 0xff) ^ b);
                crc = (crc >> 8) ^ Table[tableIndex];
            }

            crc = ~crc;
            return crc.ToString("X8").ToLower();
        }
    }
}
