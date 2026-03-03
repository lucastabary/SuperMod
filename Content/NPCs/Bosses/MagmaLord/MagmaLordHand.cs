using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace SuperMod.Content.NPCs.Bosses.MagmaLord
{
    public class MagmaLordHand : ModNPC
    {
        // ── États (localAI[0]) ──────────────────────────────────────────────
        // 0 = Hover       → frame index 0 (état 1 de la spritesheet)
        // 1 = Dash attack → frame index 0
        // 2 = Fetch       → frame index 1 (état 2 de la spritesheet) — fonce vers joueur
        // 3 = Return      → frame index 2 (état 3 de la spritesheet) — ramène le joueur

        // Distance à partir de laquelle la main va chercher le joueur (pixels)
        private const float FetchDistance = 1400f;
        // Distance de contact avec le joueur pour déclencher le retour
        private const float GrabDistance = 130f;
        // Distance au boss à partir de laquelle on considère que le retour est terminé
        private const float ReturnDoneDistance = 320f;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 5;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
        }

        public override void SetDefaults() {
            NPC.width = 89;
            NPC.height = 89;
            NPC.damage = 60;
            NPC.defense = 20;
            NPC.lifeMax = 2000;
            NPC.HitSound = SoundID.NPCHit3;
            NPC.DeathSound = SoundID.NPCDeath3;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.lavaImmune = true;
            NPC.aiStyle = -1;

            // Permet de choisir à quel moment l'entité doit être dessinée
            NPC.hide = true;
        }

        public override void DrawBehind(int index)
        {
            // Dessine les mains au premier plan, devant tous les autres PNJ (y compris le boss)
            Main.instance.DrawCacheNPCProjectiles.Add(index);
        }

        public override void AI()
        {
            // ai[0] = Index du boss (MagmaLord)
            // ai[1] = Direction de la main (-1 gauche, 1 droite)

            NPC boss = Main.npc[(int)NPC.ai[0]];

            // Despawn si le boss est mort ou manquant
            if (!boss.active || boss.type != ModContent.NPCType<MagmaLord>())
            {
                Despawn();
                return;
            }

            // Hérite de la cible du boss
            NPC.target = boss.target;
            Player player = Main.player[NPC.target];
            if (player.dead || !player.active) {
                return;
            }

            // Direction du sprite (miroir inversé)
            NPC.spriteDirection = -(int)NPC.ai[1];

            // Lecture de la phase (0 = normale, 1 = phase 2)
            bool isPhase2 = boss.ai[1] == 1f;

            // Paramètres selon la phase
            float hoverSpeed   = isPhase2 ? 16f  : 10f;
            float hoverInertia = isPhase2 ? 12f  : 20f;
            float attackCooldown = isPhase2 ? 90f  : 180f;
            float dashSpeed    = isPhase2 ? 36f  : 24f;

            // Position de vol stationnaire à côté du boss
            Vector2 hoverPosition = boss.Center + new Vector2(NPC.ai[1] * 350f, -100f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f) * 30f);

            float distancePlayerToBoss = Vector2.Distance(player.Center, boss.Center);

            // ── Machine d'états ────────────────────────────────────────────────
            switch ((int)NPC.localAI[0])
            {
                // ── État 0 : Hover ────────────────────────────────────────────
                case 0:
                    MoveTowards(hoverPosition, hoverSpeed, hoverInertia);
                    NPC.localAI[1]++;

                    // Si le joueur s'éloigne trop → Fetch
                    if (distancePlayerToBoss > FetchDistance)
                    {
                        SwitchState(2);
                        break;
                    }

                    // Timer d'attaque écoulé → Dash
                    if (NPC.localAI[1] > attackCooldown)
                    {
                        NPC.localAI[1] = 0f;
                        Vector2 chargeDir = player.Center - NPC.Center;
                        if (chargeDir != Vector2.Zero) chargeDir.Normalize();
                        NPC.velocity = chargeDir * dashSpeed;
                        SwitchState(1);
                    }
                    break;

                // ── État 1 : Dash / Attaque ───────────────────────────────────
                case 1:
                    NPC.velocity *= 0.96f;
                    NPC.localAI[1]++;
                    if (NPC.localAI[1] > 60f) SwitchState(0);
                    break;

                // ── État 2 : Fetch (aller chercher le joueur) ─────────────────
                case 2:
                    MoveTowards(player.Center, 22f, 6f);

                    // Si le joueur est revenu assez près tout seul → retour en hover
                    if (distancePlayerToBoss < FetchDistance * 0.6f)
                    {
                        SwitchState(0);
                        break;
                    }

                    // Contact avec le joueur → déclencher le retour
                    if (Vector2.Distance(NPC.Center, player.Center) < GrabDistance)
                    {
                        SwitchState(3);
                    }
                    break;

                // ── État 3 : Return (ramener le joueur vers le boss) ──────────
                case 3:
                    MoveTowards(boss.Center, 18f, 10f);

                    // Pousser le joueur vers le boss (seulement en solo ou sur le client)
                    // Le push est visuel/gameplay ; la logique réseau reste serveur
                    if (Main.netMode != NetmodeID.Server)
                    {
                        Vector2 pushDir = boss.Center - player.Center;
                        float dist = pushDir.Length();
                        if (dist > ReturnDoneDistance && dist > 1f)
                        {
                            pushDir.Normalize();
                            // Force de poussée proportionnelle à la distance
                            float pushStrength = Math.Clamp(dist / 600f, 0.5f, 4f);
                            player.velocity += pushDir * pushStrength;
                            // Annule le frein naturel du joueur pour maintenir l'effet
                            player.velocity.X = Math.Clamp(player.velocity.X, -25f, 25f);
                            player.velocity.Y = Math.Clamp(player.velocity.Y, -25f, 25f);
                        }
                    }

                    // Retour terminé : joueur proche du boss OU main proche du boss
                    if (distancePlayerToBoss < ReturnDoneDistance ||
                        Vector2.Distance(NPC.Center, boss.Center) < ReturnDoneDistance)
                    {
                        SwitchState(0);
                    }
                    break;
            }
        }

        private void SwitchState(int newState)
        {
            NPC.localAI[0] = newState;
            NPC.localAI[1] = 0f;
            if (Main.netMode != NetmodeID.MultiplayerClient)
                NPC.netUpdate = true;
        }

        private void MoveTowards(Vector2 destination, float speed, float inertia)
        {
            Vector2 direction = destination - NPC.Center;
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
                direction *= speed;
                NPC.velocity = (NPC.velocity * (inertia - 1) + direction) / inertia;
            }
        }

        private void Despawn() {
            NPC.velocity *= 0.95f;
            NPC.alpha += 5;

            if (NPC.alpha >= 255) {
                NPC.active = false;
                if (Main.netMode != NetmodeID.MultiplayerClient) {
                    NPC.netUpdate = true;
                }
            }
        }

        public override void FindFrame(int frameHeight)
        {
            // Correspondance état → frame de la spritesheet (chaque frame = 89px)
            int frameIndex = (int)NPC.localAI[0] switch {
                2 => 1,   // Fetch  → frame 2 (index 1)
                3 => 2,   // Return → frame 3 (index 2)
                _ => 0,   // Hover / Dash → frame 1 (index 0)
            };
            NPC.frame.Y = frameIndex * frameHeight;
        }
    }
}