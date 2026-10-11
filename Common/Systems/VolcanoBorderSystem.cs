using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using SuperMod.Content.NPCs.Bosses.MagmaLord;
using SuperMod.Content.World;

namespace SuperMod.Common.Systems
{
    // Frontière au milieu du monde : tant que le Magma Lord n'est pas vaincu,
    // le joueur ne peut pas passer dans la moitié opposée à son point d'apparition.
    public class VolcanoBorderSystem : ModSystem
    {
        // Distance (en blocs) autour du sommet à partir de laquelle le boss apparaît, en plus du rayon du cratère
        private const int BossTriggerMargin = 50;
        // Hauteur (en blocs) au-dessus du sommet où le boss apparaît
        private const int BossSpawnHeight = 30;

        // Mondes sans volcan (générés avant son ajout) : le boss apparaît au-dessus du joueur
        private const float FallbackTriggerDistance = 100 * 16f;
        private const float FallbackSpawnHeight = 25 * 16f;

        private int spawnCheckTimer;

        // Sommet du volcan (en blocs), défini à la génération du monde. Point.Zero = pas de volcan
        public static Point VolcanoSummit;

        public static int BorderTileX => Main.maxTilesX / 2;
        public static float BorderWorldX => BorderTileX * 16f;

        // 1 : la moitié droite est bloquée (le joueur apparaît à gauche), -1 : la moitié gauche
        public static int LockedSide => Main.spawnTileX < BorderTileX ? 1 : -1;

        public static bool BorderActive => !DownedBossSystem.downedMagmaLord;

        public static bool IsBeyondBorder(Rectangle hitbox)
        {
            return LockedSide == 1 ? hitbox.Right > BorderWorldX : hitbox.Left < BorderWorldX;
        }

        public override void ClearWorld()
        {
            VolcanoSummit = Point.Zero;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            if (VolcanoSummit != Point.Zero)
            {
                tag["volcanoSummitX"] = VolcanoSummit.X;
                tag["volcanoSummitY"] = VolcanoSummit.Y;
            }
        }

        public override void LoadWorldData(TagCompound tag)
        {
            if (tag.ContainsKey("volcanoSummitX"))
                VolcanoSummit = new Point(tag.GetInt("volcanoSummitX"), tag.GetInt("volcanoSummitY"));
        }

        // Apparition automatique du boss (serveur / solo uniquement)
        public override void PostUpdateWorld()
        {
            if (!BorderActive)
                return;

            if (++spawnCheckTimer < 30)
                return;
            spawnCheckTimer = 0;

            int bossType = ModContent.NPCType<MagmaLord>();
            if (NPC.AnyNPCs(bossType))
                return;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (!player.active || player.dead)
                    continue;

                if (IsInBossArena(player))
                {
                    SpawnBoss(player, bossType);
                    return;
                }
            }
        }

        private static bool IsInBossArena(Player player)
        {
            if (VolcanoSummit == Point.Zero)
                return Math.Abs(player.Center.X - BorderWorldX) < FallbackTriggerDistance;

            float triggerDistance = (VolcanoCone.RimRadius + BossTriggerMargin) * 16f;
            return Vector2.Distance(player.Center, VolcanoSummit.ToWorldCoordinates()) < triggerDistance;
        }

        private static void SpawnBoss(Player player, int bossType)
        {
            int x, y;
            if (VolcanoSummit == Point.Zero)
            {
                x = (int)BorderWorldX;
                y = (int)(player.Center.Y - FallbackSpawnHeight);
            }
            else
            {
                // Le boss domine le cratère, au-dessus du sommet
                x = VolcanoSummit.X * 16;
                y = (VolcanoSummit.Y - BossSpawnHeight) * 16;
            }

            int index = NPC.NewNPC(new EntitySource_WorldEvent(), x, y, bossType, Target: player.whoAmI);
            if (index == Main.maxNPCs)
                return;

            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.SyncNPC, number: index);
                ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Announcement.HasAwoken", Main.npc[index].GetTypeNetName()), new Color(175, 75, 255));
            }
            else
            {
                Main.NewText(Language.GetTextValue("Announcement.HasAwoken", Main.npc[index].TypeName), 175, 75, 255);
            }
        }
    }
}
