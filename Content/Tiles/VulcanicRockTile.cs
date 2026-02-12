using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SuperMod.Content.Tiles // Adapte "MonMod" au nom de ton projet
{
    public class VulcanicRockTile : ModTile
    {
        public override void SetStaticDefaults() {
            Main.tileSolid[Type] = true; // Le bloc est solide
            Main.tileMergeDirt[Type] = true; // Fusionne avec la terre
            Main.tileBlockLight[Type] = true; // Bloque la lumière
            Main.tileLighted[Type] = false;

            MineResist = 1f; // Même résistance que la pierre
            MinPick = 100; // Pioche de base suffisante
            
            AddMapEntry(new Color(50, 50, 50)); // Couleur grise foncée sur la map

            DustType = DustID.Stone; // Particules de pierre quand on le casse
            HitSound = SoundID.Tink; // Son métallique de la pierre
        }
    }
}