using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SuperMod.Content.Tiles;

namespace SuperMod.Content.Items.Placeable // Doit être dans un dossier "Items"
{
    public class VulcanicRock : ModItem
    {
        public override void SetDefaults() {
            // Configuration visuelle et physique
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 9999;
            
            // Configuration de l'utilisation
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;

            // LIEN AVEC LE BLOC
            Item.createTile = ModContent.TileType<Tiles.VulcanicRockTile>(); 
        }
    }
}