using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Syroot.IO;

namespace gmdtool
{
    class GMD
    {
        public GMDHeader Header;
        public GMDEntry[] Entries;

        public GMD(string path)
        {
            var fs = File.OpenRead(path);
            BinaryDataReader reader = new BinaryDataReader(fs, Encoding.UTF8);
            Header = new GMDHeader(reader);
            Entries = new GMDEntry[Header.MessageCount];
            int feed = ((int)Header.MessageCount - (int)Header.LableCount);
            for (int i = 0; i < Header.MessageCount; i++)
            {
                Entries[i] = new GMDEntry();
            }
            for (int i = 0; i < Header.LableCount; i++)
            {
                Entries[i + feed].ID = reader.ReadUInt32();
                Entries[i + feed].Address = reader.ReadUInt32();
            }
            for (int i = 0; i < Header.LableCount; i++)
            {
                Entries[i + feed].Label = reader.ReadString(BinaryStringFormat.ZeroTerminated);
            }
            for (int i = 0; i < Header.MessageCount; i++)
            {
                Entries[i].Message = reader.ReadString(BinaryStringFormat.ZeroTerminated);
            }
        }
        public void Save(string path)
        {
            var fs = File.Create(path);
            BinaryDataWriter writer = new BinaryDataWriter(fs, Encoding.UTF8);
            Header.WriteTo(writer);
            for (int i = 0; i < Header.MessageCount; i++)
            {
                if (Entries[i].Label != null)
                {
                    writer.Write(Entries[i].ID);
                    writer.Write(Entries[i].Address);
                }
            }
            var lbl_start = writer.Position;
            for (int i = 0; i < Header.MessageCount; i++)
            {
                if (Entries[i].Label != null)
                {
                    writer.Write(Entries[i].Label, BinaryStringFormat.ZeroTerminated);
                }
            }
            var msg_start = writer.Position;
            for (int i = 0; i < Header.MessageCount; i++)
            {
                if (Entries[i].Message != null)
                {
                    writer.Write(Entries[i].Message, BinaryStringFormat.ZeroTerminated);
                }
            }
            Header.LabelBlockSize = (uint)(msg_start - lbl_start);
            Header.MessageBlockSize = (uint)(writer.Position - msg_start);
            writer.Seek(0, SeekOrigin.Begin);
            Header.WriteTo(writer);
            writer.Flush();
            fs.Close();
        }
        public void FromString(string s)
        {
            Regex regex = new Regex(@"No\.\d+\r\nName:.*\r\n－+?\r\n[\s|\S]+?\r\n－+?\r\n([\s|\S]+?)\r\n＝+?\r\n");
            var mc = regex.Matches(s);
            if (mc.Count != Header.MessageCount)
            {
                Console.WriteLine("Message count mismatch");
                return;
            }
            int i = 0;
            foreach(Match m in mc)
            {
                Entries[i].Message = m.Groups[1].Value;
                i++;
            }
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            int i = 0;
            foreach (var e in Entries)
            {
                i++;
                sb.AppendFormat(
@"No.{0:D03}
Name: {1}
－－－－－－－－－－－－－－－
{2}
－－－－－－－－－－－－－－－
{2}
＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝



", i, e.Label, e.Message, e.Message);
            }
            return sb.ToString();
        }

        public class GMDHeader
        {
            public string Signeture
            {
                get
                {
                    return "GMD";
                }
                set
                {
                    if (value != "GMD") { throw new ArgumentException("File format error."); }
                }
            }
            public uint Version
            {
                get
                {
                    return 0x010101;
                }
                set
                {
                    if (value != 0x010101) { throw new ArgumentException("Unsupport GMD version!"); }
                }
            }
            public uint Unknown;
            public uint LableCount;
            public uint MessageCount;
            public uint LabelBlockSize;
            public uint MessageBlockSize;
            public uint NameLength;
            public string Name;

            public GMDHeader(byte[] data)
            {
                using (MemoryStream ms = new MemoryStream(data))
                {
                    BinaryDataReader reader = new BinaryDataReader(ms, Encoding.UTF8);
                    Read(reader);
                }
            }
            public GMDHeader(BinaryDataReader reader)
            {
                Read(reader);
            }
            private void Read(BinaryDataReader reader)
            {
                Signeture = reader.ReadString(BinaryStringFormat.ZeroTerminated);
                Version = reader.ReadUInt32();
                Unknown = reader.ReadUInt32();
                LableCount = reader.ReadUInt32();
                MessageCount = reader.ReadUInt32();
                LabelBlockSize = reader.ReadUInt32();
                MessageBlockSize = reader.ReadUInt32();
                NameLength = reader.ReadUInt32();
                Name = reader.ReadString(BinaryStringFormat.ZeroTerminated);
            }
            public void WriteTo(BinaryDataWriter writer)
            {
                writer.Write(Signeture, BinaryStringFormat.ZeroTerminated);
                writer.Write(Version);
                writer.Write(Unknown);
                writer.Write(LableCount);
                writer.Write(MessageCount);
                writer.Write(LabelBlockSize);
                writer.Write(MessageBlockSize);
                writer.Write(NameLength);
                writer.Write(Name, BinaryStringFormat.ZeroTerminated);
            }
        }
        public class GMDEntry
        {
            public uint ID;
            public uint Address;
            public string Label;
            public string Message;
        }
    }
}
