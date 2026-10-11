using System.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace SuperMod.Common.Systems
{
    // Garde en mémoire (dans la sauvegarde du monde) les boss déjà vaincus
    public class DownedBossSystem : ModSystem
    {
        public static bool downedMagmaLord = false;

        public override void ClearWorld()
        {
            downedMagmaLord = false;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            if (downedMagmaLord)
                tag["downedMagmaLord"] = true;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            downedMagmaLord = tag.ContainsKey("downedMagmaLord");
        }

        // Synchronisation serveur -> clients (envoyé avec MessageID.WorldData)
        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(downedMagmaLord);
        }

        public override void NetReceive(BinaryReader reader)
        {
            downedMagmaLord = reader.ReadBoolean();
        }
    }
}
