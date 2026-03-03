using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace SuperMod.Content.NPCs.Bosses.MagmaLord
{
    [AutoloadBossHead]
    public class MagmaLord : ModNPC
    {
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
            // lifeMax = somme des pv des deux mains (2000 * 2)
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
            
            // Priorise les mains pour le curseur : on ne montre pas le nom du corps au survol
            NPC.ShowNameOnHover = false;

            Music = MusicID.Boss2; 
        }

        public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
        {
            // Le corps est invulnérable : on annule tous les dégàts
            modifiers.FinalDamage.Base -= modifiers.FinalDamage.Base;
            modifiers.SetMaxDamage(0);
        }

        public override void AI()
        {
            // Serverside
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                {
                    NPC.TargetClosest(true);
                    NPC.netUpdate = true; 
                }
            }

            Player player = Main.player[NPC.target];
            if (player.dead || !player.active) {
                Despawn();
                return;
            }

            // Hand spawning logic (server only)
            if (Main.netMode != NetmodeID.MultiplayerClient && NPC.localAI[0] == 0f)
            {
                NPC.localAI[0] = 1f;
                // Spawn Left Hand (écarté plus à l'extérieur)
                NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X - 250, (int)NPC.Center.Y, ModContent.NPCType<MagmaLordHand>(), ai0: NPC.whoAmI, ai1: -1f);
                // Spawn Right Hand (écarté plus à l'extérieur)
                NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X + 250, (int)NPC.Center.Y, ModContent.NPCType<MagmaLordHand>(), ai0: NPC.whoAmI, ai1: 1f);
                NPC.netUpdate = true;
            }

            // Phase 2 detection — server side only, result is synced via netUpdate
            // NPC.ai[1] == 0f => phase 1, NPC.ai[1] == 1f => phase 2
            if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[1] == 0f)
            {
                int aliveHands = 0;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC n = Main.npc[i];
                    if (n.active && n.type == ModContent.NPCType<MagmaLordHand>() && (int)n.ai[0] == NPC.whoAmI)
                        aliveHands++;
                }
                // Une main est morte : on passe en phase 2
                if (aliveHands < 2)
                {
                    NPC.ai[1] = 1f;
                    NPC.netUpdate = true;
                }
            }

            // Synchronisation des PV du body avec la somme des PV des mains (server-side)
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int totalLife = 0;
                int totalLifeMax = 0;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC n = Main.npc[i];
                    if (n.active && n.type == ModContent.NPCType<MagmaLordHand>() && (int)n.ai[0] == NPC.whoAmI)
                    {
                        totalLife += n.life;
                        totalLifeMax += n.lifeMax;
                    }
                }

                // Met à jour le lifeMax dynamiquement selon le nombre de mains actives
                NPC.lifeMax = totalLifeMax > 0 ? totalLifeMax : 1;
                NPC.life = Math.Clamp(totalLife, 0, NPC.lifeMax);
                NPC.netUpdate = true;
            }

            // The body remains stationary
            NPC.velocity *= 0.9f;
        }

        private void Despawn() {
            NPC.velocity *= 0.95f; // Rallentit le mouvement avant de despawn
            NPC.alpha += 5; // Augmente la transparence (le fade out)
            
            if (NPC.alpha >= 255) {
                NPC.active = false;
                
                // On s'assure que seul le serveur gère la mise à jour réseau
                if (Main.netMode != NetmodeID.MultiplayerClient) {
                    NPC.netUpdate = true;
                }
            }
        }
    }
}