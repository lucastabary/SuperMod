using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace SuperMod.Content.World
{
    public class VulcanicChasm
    {
        public static void Generate()
        {
            int centerX = Main.maxTilesX / 2;
            int startY = (int)Main.worldSurface - 20;
            int endY = Main.maxTilesY - 200;
            
            int biomeWidth = Main.maxTilesX / 50; 
            ushort tileType = (ushort)ModContent.TileType<Tiles.VulcanicRockTile>();

            // 1. REMPLISSAGE INITIAL ET ÉROSION EXTÉRIEURE
            for (int y = startY; y < endY; y++)
            {
                double noise = Math.Sin(y * 0.04) * 8 + Math.Sin(y * 0.01) * 15;
                int currentCenterX = centerX + (int)noise;
                int leftEdge = currentCenterX - biomeWidth;
                int rightEdge = currentCenterX + biomeWidth;

                for (int x = leftEdge; x <= rightEdge; x++)
                {
                    if (WorldGen.InWorld(x, y)) WorldGen.PlaceTile(x, y, tileType, true, true);
                }

                if (WorldGen.genRand.NextBool(8))
                {
                    int erodeX = WorldGen.genRand.NextBool() ? leftEdge : rightEdge;
                    WorldGen.TileRunner(erodeX, y, WorldGen.genRand.Next(3, 7), WorldGen.genRand.Next(2, 5), -1, false, 0f, 0f, false, true);
                }
            }

            // 2. GÉNÉRATION DES ÎLES (DENSITÉ AUGMENTÉE)
            // Intervalle vertical réduit (10 à 18 blocs) pour plus d'étages
            for (int y = startY + 40; y < endY - 60; y += WorldGen.genRand.Next(10, 18))
            {
                double noise = Math.Sin(y * 0.04) * 8 + Math.Sin(y * 0.01) * 15;
                int currentCenterX = centerX + (int)noise;

                // On augmente le nombre d'îles par palier (jusqu'à 4)
                int platformCount = WorldGen.genRand.Next(2, 5);
                for (int i = 0; i < platformCount; i++)
                {
                    // Répartition aléatoire sur toute la largeur
                    int islandX = currentCenterX + WorldGen.genRand.Next(-biomeWidth + 8, biomeWidth - 8);
                    int radiusX = WorldGen.genRand.Next(10, 20);
                    int radiusY = WorldGen.genRand.Next(4, 9); 

                    for (int tx = islandX - radiusX; tx <= islandX + radiusX; tx++)
                    {
                        for (int ty = y - radiusY; ty <= y + radiusY; ty++)
                        {
                            if (WorldGen.InWorld(tx, ty))
                            {
                                float dx = tx - islandX;
                                float dy = ty - y;
                                // Équation d'ellipse pour un rendu lisse
                                if ((dx * dx) / (radiusX * radiusX) + (dy * dy) / (radiusY * radiusY) <= 1f)
                                {
                                    WorldGen.PlaceTile(tx, ty, TileID.Stone, true, true);
                                }
                            }
                        }
                    }
                }
            }

            // 3. VIDAGE, ÉROSION ET NETTOYAGE SINUSOÏDAL
            for (int y = startY; y < endY; y++)
            {
                double noise = Math.Sin(y * 0.04) * 8 + Math.Sin(y * 0.01) * 15;
                int currentCenterX = centerX + (int)noise;
                
                int cleanupRange = biomeWidth + 5; 
                int caveWidth = (int)(biomeWidth * 0.75);

                for (int x = currentCenterX - cleanupRange; x <= currentCenterX + cleanupRange; x++)
                {
                    if (!WorldGen.InWorld(x, y)) continue;

                    Tile tile = Main.tile[x, y];

                    // Vidage central (respecte les îles en Pierre)
                    if (Math.Abs(x - currentCenterX) <= caveWidth)
                    {
                        if (tile.TileType != TileID.Stone)
                        {
                            WorldGen.KillTile(x, y, false, false, true);
                        }
                    }

                    // Nettoyage précis (liquides et murs)
                    if (!tile.HasTile)
                    {
                        tile.LiquidAmount = 0;
                        WorldGen.KillWall(x, y);
                    }

                    // Remplacement des îles
                    if (tile.HasTile && tile.TileType == TileID.Stone)
                    {
                        tile.TileType = tileType;
                    }
                }

                // Érosion intérieure (grignotage des murs)
                if (WorldGen.genRand.NextBool(5))
                {
                    int erodeX = WorldGen.genRand.NextBool() ? (currentCenterX - caveWidth) : (currentCenterX + caveWidth);
                    WorldGen.TileRunner(erodeX, y, WorldGen.genRand.Next(5, 10), WorldGen.genRand.Next(3, 5), -1, false, 0f, 0f, false, true);
                }
            }
        }
    }
}