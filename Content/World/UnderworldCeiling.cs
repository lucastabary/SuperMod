using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SuperMod.Content.World
{
    public class UnderworldCeiling
    {
        public static void Generate()
        {
            int hellBottom = Main.maxTilesY - 200; 
            ushort tileType = (ushort)ModContent.TileType<Tiles.VulcanicRockTile>();

            // 1. GÉNÉRATION DE LA COQUE SOLIDE
            for (int x = 0; x < Main.maxTilesX; x++)
            {
                // Bruit de base pour la silhouette
                double slowNoise = Math.Sin(x * 0.01) * 8; 
                double mediumNoise = Math.Sin(x * 0.03) * 4;
                
                int currentTopY = (hellBottom - 50) + (int)(slowNoise + mediumNoise);
                int thickness = 35 + WorldGen.genRand.Next(-3, 4);
                int currentBottomY = currentTopY + thickness;

                for (int y = currentTopY; y <= currentBottomY; y++)
                {
                    if (!WorldGen.InWorld(x, y)) continue;

                    Tile tile = Main.tile[x, y];
                    
                    // On remplit tout pour boucher les trous d'air (effet barrière)
                    if (tile.TileType != TileID.ObsidianBrick && tile.TileType != TileID.HellstoneBrick)
                    {
                        WorldGen.PlaceTile(x, y, tileType, true, true);
                        tile.LiquidAmount = 0; // On vide l'eau/lave emprisonnée
                    }
                }
            }

            // 2. DOUBLE ÉROSION PAR "BLOBS" (DESSUS ET DESSOUS)
            // On utilise une densité de 40% pour un aspect vraiment organique
            int erosionPasses = (int)(Main.maxTilesX * 0.40); 

            for (int i = 0; i < erosionPasses; i++)
            {
                int randX = WorldGen.genRand.Next(10, Main.maxTilesX - 10);
                double noise = Math.Sin(randX * 0.01) * 8 + Math.Sin(randX * 0.03) * 4;
                int baseLineY = hellBottom - 50 + (int)noise;

                // A. Érosion au DESSUS (Transition avec les cavernes)
                if (WorldGen.genRand.NextBool(2))
                {
                    WorldGen.TileRunner(
                        randX, 
                        baseLineY + WorldGen.genRand.Next(-3, 6), 
                        WorldGen.genRand.Next(5, 12), // Taille variable pour le naturel
                        WorldGen.genRand.Next(3, 8), 
                        -1, // On creuse le vide
                        false, 0f, 0f, false, true
                    );
                }

                // B. Érosion au DESSOUS (Transition avec l'air des enfers)
                if (WorldGen.genRand.NextBool(2))
                {
                    int bottomLineY = baseLineY + 35;
                    WorldGen.TileRunner(
                        randX, 
                        bottomLineY + WorldGen.genRand.Next(-6, 3), 
                        WorldGen.genRand.Next(5, 12), 
                        WorldGen.genRand.Next(3, 8), 
                        -1, 
                        false, 0f, 0f, false, true
                    );
                }
            }
        }
    }
}