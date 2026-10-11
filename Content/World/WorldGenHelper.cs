using Terraria;

namespace SuperMod.Content.World
{
    // Fonctions utilitaires partagées par les passes de génération
    public static class WorldGenHelper
    {
        // Renvoie le premier bloc solide du sol en ignorant les îles flottantes, ou -1
        public static int FindSurface(int x)
        {
            for (int y = 50; y < Main.worldSurface; y++)
            {
                if (!IsSolid(x, y))
                    continue;

                // Une île flottante a un grand vide en dessous d'elle
                if (HasAirGapBelow(x, y, 60, 20))
                {
                    y += 20;
                    continue;
                }
                return y;
            }
            return -1;
        }

        public static bool HasAirGapBelow(int x, int y, int range, int gapSize)
        {
            int consecutiveAir = 0;
            for (int ty = y + 1; ty <= y + range && ty < Main.maxTilesY; ty++)
            {
                consecutiveAir = IsSolid(x, ty) ? 0 : consecutiveAir + 1;
                if (consecutiveAir >= gapSize)
                    return true;
            }
            return false;
        }

        public static bool IsSolid(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && Main.tileSolid[tile.TileType];
        }
    }
}
