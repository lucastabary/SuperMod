using System;
using Terraria;

namespace SuperMod.Content.World
{
    // Déplace le point d'apparition sur un des bords du monde (juste après l'océan),
    // loin du volcan et de la frontière au centre de la carte
    public class EdgeSpawn
    {
        // Distance minimale (en blocs) avec l'entrée du donjon
        private const int DungeonClearance = 80;

        public static void Generate()
        {
            bool leftSide = WorldGen.genRand.NextBool();
            int distanceFromEdge = Math.Max(350, (int)(Main.maxTilesX * 0.08));
            int startX = leftSide ? distanceFromEdge : Main.maxTilesX - distanceFromEdge;
            // Si l'emplacement ne convient pas, on avance vers le centre
            int step = leftSide ? 1 : -1;

            for (int i = 0; i < 400; i++)
            {
                int x = startX + i * step;
                if (Math.Abs(x - Main.dungeonX) < DungeonClearance)
                    continue;

                int y = WorldGenHelper.FindSurface(x);
                if (y == -1 || y > Main.worldSurface)
                    continue;

                // Pas d'apparition sous l'eau
                if (Main.tile[x, y - 1].LiquidAmount > 0)
                    continue;

                Main.spawnTileX = x;
                Main.spawnTileY = y;
                return;
            }
            // Aucun emplacement trouvé : on garde le point d'apparition vanilla
        }
    }
}
