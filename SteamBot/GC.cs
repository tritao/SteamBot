using SteamKit2.GC.TF2;
using SteamKit2.Internal;
using System.IO;

namespace SteamBot
{
    public class CMsgCraft : IGCSerializableMessage
    {
        public const ushort UnknownBlueprint = 0xFFFF;

        public uint GetEMsg() { return EGCMsg.Craft; }

        public ushort Blueprint { get; set; }
        public ushort ItemCount { get; set; }
        public ulong[] Items { get; set; }

        public CMsgCraft()
        {
            Blueprint = 0;
            ItemCount = 0;
            Items = new ulong[ItemCount];
        }

        public void Serialize(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);

            bw.Write(Blueprint);
            bw.Write(ItemCount);
            int i = 0;
            while (i < ItemCount)
            {
                bw.Write(Items[i]);
                i++;
            }
        }

        public void Deserialize(Stream stream)
        {
        }
    }

    public class CMsgCraftResponse : IGCSerializableMessage
    {
        public uint GetEMsg() { return EGCMsg.CraftResponse; }

        public ushort Blueprint { get; set; }
        public ushort Unknown { get; set; }
        public uint ItemId { get; set; }

        public CMsgCraftResponse()
        {
            Blueprint = 0;
            Unknown = 0;
            ItemId = 0;
        }

        public void Serialize(Stream stream)
        {
        }

        public void Deserialize(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);

            Blueprint = br.ReadUInt16();
            Unknown = br.ReadUInt16();
            ItemId = br.ReadUInt32();
        }
    }

    public class CMsgPaint : IGCSerializableMessage
    {
        public uint GetEMsg() { return EGCMsg.PaintItem; }

        public ulong ItemId { get; set; }
        public ulong PaintId { get; set; }

        public CMsgPaint()
        {
        }

        public void Serialize(Stream stream)
        {
            BinaryWriter bw = new BinaryWriter(stream);
        }

        public void Deserialize(Stream stream)
        {
        }
    }


    public class CMsgPaintResponse : IGCSerializableMessage
    {
        public uint GetEMsg() { return EGCMsg.CraftResponse; }

        public CMsgPaintResponse()
        {
        }

        public void Serialize(Stream stream)
        {
        }

        public void Deserialize(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
        }
    }
}
