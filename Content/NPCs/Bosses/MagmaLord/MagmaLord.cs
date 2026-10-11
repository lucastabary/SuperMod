using System;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using SuperMod.Common.Systems;

namespace SuperMod.Content.NPCs.Bosses.MagmaLord
{
    [AutoloadBossHead]
    public class MagmaLord : ModNPC
    {
        // ai[1] = phase (0 = phase 1, 1 = phase 2)
        // ai[2] = somme des PV max des mains (0 tant que les mains ne sont pas apparues)

        // Distance (pixels) au-delà de laquelle le boss abandonne sa cible et disparaît
        private const float DespawnDistance = 400 * 16f;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) {
            return false;
        }

        public override void SetDefaults() {
            NPC.boss = true;
            NPC.width = 620;
            NPC.height = 120;
            NPC.damage = 80;
            NPC.defense = 30;
            // Remplacé par la somme des PV des mains dès leur apparition
            NPC.lifeMax = 4000;
            NPC.HitSound = SoundID.NPCHit3;
            NPC.DeathSound = SoundID.NPCDeath3;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 15);
            NPC.SpawnWithHigherTime(30);
            NPC.lavaImmune = true;
            NPC.aiStyle = -1;
            NPC.BossBar = ModContent.GetInstance<MagmaLordBossBar>();

            // Le corps est invulnérable : les projectiles le traversent et visent les mains
            NPC.dontTakeDamage = true;

            // Priorise les mains pour le curseur : on ne montre pas le nom du corps au survol
            NPC.ShowNameOnHover = false;

            Music = MusicID.Boss2;
        }

        public override void AI()
        {
            // Serverside
            if (Main.netMode != NetmodeID.MultiplayerClient && !HasValidTarget())
            {
                int oldTarget = NPC.target;
                NPC.TargetClosest(true);
                if (NPC.target != oldTarget)
                    NPC.netUpdate = true;
            }

            if (!HasValidTarget()) {
                Despawn();
                return;
            }
            NPC.alpha = Math.Max(NPC.alpha - 15, 0);

            // Hand spawning logic (server only)
            if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[2] == 0f)
            {
                SpawnHands();
            }

            // Les PV du corps = somme des PV des mains (calculé partout, les PV des mains sont déjà synchronisés)
            int aliveHands = 0;
            int totalLife = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (n.active && n.type == ModContent.NPCType<MagmaLordHand>() && (int)n.ai[0] == NPC.whoAmI)
                {
                    aliveHands++;
                    totalLife += n.life;

                    // Les joueurs qui ont frappé une main comptent pour le loot du boss
                    for (int p = 0; p < Main.maxPlayers; p++)
                    {
                        if (n.playerInteraction[p])
                            NPC.playerInteraction[p] = true;
                    }
                }
            }

            if (NPC.ai[2] > 0f)
                NPC.lifeMax = (int)NPC.ai[2];
            NPC.life = Math.Clamp(totalLife, 1, NPC.lifeMax);

            if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[2] > 0f)
            {
                // Les deux mains sont mortes : le boss meurt
                if (aliveHands == 0)
                {
                    NPC.dontTakeDamage = false;
                    NPC.StrikeInstantKill();
                    return;
                }

                // Une main est morte : on passe en phase 2
                if (aliveHands < 2 && NPC.ai[1] == 0f)
                {
                    NPC.ai[1] = 1f;
                    NPC.netUpdate = true;
                }
            }

            // The body remains stationary
            NPC.velocity *= 0.9f;
        }

        private bool HasValidTarget()
        {
            if (NPC.target < 0 || NPC.target >= Main.maxPlayers)
                return false;

            Player player = Main.player[NPC.target];
            return player.active && !player.dead && NPC.Distance(player.Center) < DespawnDistance;
        }

        private void SpawnHands()
        {
            int handType = ModContent.NPCType<MagmaLordHand>();
            int totalLifeMax = 0;

            // Main gauche (ai1 = -1) puis main droite (ai1 = 1), écartées vers l'extérieur
            for (int direction = -1; direction <= 1; direction += 2)
            {
                int index = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X + direction * 250, (int)NPC.Center.Y, handType, ai0: NPC.whoAmI, ai1: direction);
                if (index == Main.maxNPCs)
                    continue;

                totalLifeMax += Main.npc[index].lifeMax;
                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendData(MessageID.SyncNPC, number: index);
            }

            NPC.ai[2] = Math.Max(totalLifeMax, 1);
            NPC.netUpdate = true;
        }

        public override void OnKill()
        {
            // Ouvre la frontière (sauvegardé dans le monde et synchronisé aux clients)
            NPC.SetEventFlagCleared(ref DownedBossSystem.downedMagmaLord, -1);

            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.WorldData);
                ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Mods.SuperMod.Barrier.Lifted"), new Color(255, 120, 40));
            }
            else
            {
                Main.NewText(Language.GetTextValue("Mods.SuperMod.Barrier.Lifted"), 255, 120, 40);
            }
        }

        private void Despawn() {
            NPC.velocity *= 0.95f; // Rallentit le mouvement avant de despawn
            NPC.alpha = Math.Min(NPC.alpha + 5, 255); // Augmente la transparence (le fade out)

            // Seul le serveur fait disparaître le boss, les clients suivent
            if (NPC.alpha >= 255 && Main.netMode != NetmodeID.MultiplayerClient) {
                NPC.active = false;
                NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI);
            }
        }
    }
}
