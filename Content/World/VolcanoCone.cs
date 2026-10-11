using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SuperMod.Common.Systems;

namespace SuperMod.Content.World
{
    // Cône du volcan au centre du monde. Son cratère s'ouvre sur le gouffre volcanique
    // (VolcanicChasm), généré juste après. Le Magma Lord apparaît au-dessus du sommet.
    public class VolcanoCone
    {
        // Rayon (en blocs) du bord du cratère : assez large pour englober l'entrée sinueuse du gouffre
        public static int RimRadius => VolcanicChasm.CaveWidth + 25;
        // Rayon du fond du cratère, entièrement ouvert sur le gouffre
        private static int OpeningRadius => Math.Max(VolcanicChasm.CaveWidth - 25, 10);

        public static void Generate()
        {
            int centerX = Main.maxTilesX / 2;
            int height = (int)(Main.maxTilesY * 0.09);
            int baseRadius = RimRadius + (int)(height * 1.5);

            int baseY = FindGroundLevel(centerX, baseRadius);
            if (baseY == -1)
                return;

            // Le sommet ne doit pas monter jusque dans l'espace
            int peakY = Math.Max(baseY - height, 100);
            height = baseY - peakY;

            // Le fond du cratère rejoint le haut du gouffre
            int craterBottomY = VolcanicChasm.StartY;
            ushort rockType = (ushort)ModContent.TileType<Tiles.VolcanicRockTile>();

            for (int x = centerX - baseRadius; x <= centerX + baseRadius; x++)
            {
                if (!WorldGen.InWorld(x, Main.maxTilesY / 2, 10))
                    continue;

                int distance = Math.Abs(x - centerX);

                // 1. FOND DU CRATÈRE : on vide tout jusqu'au gouffre
                if (distance <= OpeningRadius)
                {
                    ClearColumn(x, peakY - 40, craterBottomY);
                    continue;
                }

                int topY;
                if (distance < RimRadius)
                {
                    // 2. PAROI INTÉRIEURE DU CRATÈRE : raide près du bord, plate près du fond
                    double t = (double)(distance - OpeningRadius) / (RimRadius - OpeningRadius);
                    topY = craterBottomY - (int)((craterBottomY - peakY) * t * t);
                    // L'intérieur du cratère est vidé, terrain naturel compris
                    ClearColumn(x, peakY - 40, topY - 1);
                }
                else
                {
                    // 3. FLANC EXTÉRIEUR : pente concave, comme un stratovolcan
                    double t = (double)(distance - RimRadius) / (baseRadius - RimRadius);
                    double noise = Math.Sin(x * 0.05) * 2 + Math.Sin(x * 0.13);
                    topY = baseY - (int)(height * Math.Pow(1 - t, 1.6) + noise);
                    // On coupe les arbres et plantes qui dépasseraient du flanc
                    ClearNonSolidAbove(x, topY, 40);
                }

                FillColumn(x, topY, baseY, rockType, distance >= RimRadius);
            }

            VolcanoBorderSystem.VolcanoSummit = new Point(centerX, peakY);
        }

        // Hauteur médiane du sol autour du centre, pour poser le volcan
        private static int FindGroundLevel(int centerX, int radius)
        {
            List<int> heights = new List<int>();
            for (int x = centerX - radius; x <= centerX + radius; x += 5)
            {
                if (!WorldGen.InWorld(x, Main.maxTilesY / 2, 10))
                    continue;

                int y = WorldGenHelper.FindSurface(x);
                if (y != -1)
                    heights.Add(y);
            }

            if (heights.Count == 0)
                return -1;

            heights.Sort();
            return heights[heights.Count / 2];
        }

        // Couche de cendre en surface, roche volcanique en dessous.
        // On descend jusqu'au sol naturel pour ne laisser aucun trou sous le volcan.
        private static void FillColumn(int x, int topY, int baseY, ushort rockType, bool grassOnTop)
        {
            int ashDepth = 5 + WorldGen.genRand.Next(3);

            for (int y = Math.Max(topY, 10); y < baseY + 60 && y < Main.maxTilesY - 10; y++)
            {
                Tile tile = Main.tile[x, y];

                // Sous la base, on s'arrête au premier bloc solide (ou meuble) rencontré
                if (y > baseY && tile.HasTile && (Main.tileSolid[tile.TileType] || Main.tileFrameImportant[tile.TileType]))
                    break;

                ushort type = rockType;
                if (y == topY && grassOnTop)
                    type = TileID.AshGrass;
                else if (y - topY < ashDepth)
                    type = TileID.Ash;

                tile.ResetToType(type);
                tile.LiquidAmount = 0;
            }
        }

        private static void ClearColumn(int x, int fromY, int toY)
        {
            for (int y = Math.Max(fromY, 10); y <= toY; y++)
            {
                Main.tile[x, y].ClearEverything();
            }
        }

        private static void ClearNonSolidAbove(int x, int topY, int range)
        {
            for (int y = topY - 1; y >= topY - range && y > 10; y--)
            {
                Tile tile = Main.tile[x, y];
                if (tile.HasTile && !Main.tileSolid[tile.TileType])
                    tile.ClearTile();
            }
        }
    }
}
