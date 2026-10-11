using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using SuperMod.Common.Systems;

namespace SuperMod.Common.Players
{
    // Empêche le joueur de franchir la frontière tant que le Magma Lord n'est pas vaincu
    public class VolcanoBorderPlayer : ModPlayer
    {
        // Distance (en pixels) à partir de laquelle on affiche les particules de la frontière
        private const float VisualDistance = 30 * 16f;

        private Vector2 lastValidPosition;
        private int messageCooldown;

        public override void PostUpdate()
        {
            // Chaque client gère la position de son propre joueur
            if (Player.whoAmI != Main.myPlayer)
                return;

            if (messageCooldown > 0)
                messageCooldown--;

            if (!VolcanoBorderSystem.BorderActive)
                return;

            SpawnBorderDust();

            if (!VolcanoBorderSystem.IsBeyondBorder(Player.Hitbox))
            {
                lastValidPosition = Player.position;
                return;
            }

            int side = VolcanoBorderSystem.LockedSide;

            // Retour à la dernière position valide, sauf en cas de téléportation (trop loin)
            if (lastValidPosition != Vector2.Zero && Vector2.Distance(lastValidPosition, Player.position) < 20 * 16f)
                Player.position = lastValidPosition;
            else
                Player.position.X = side == 1 ? VolcanoBorderSystem.BorderWorldX - Player.width - 1 : VolcanoBorderSystem.BorderWorldX + 1;

            // Petit rebond vers l'arrière
            Player.velocity.X = -side * 4f;

            if (messageCooldown == 0)
            {
                Main.NewText(Language.GetTextValue("Mods.SuperMod.Barrier.Blocked"), 255, 120, 40);
                messageCooldown = 180;
            }
        }

        private void SpawnBorderDust()
        {
            if (Math.Abs(Player.Center.X - VolcanoBorderSystem.BorderWorldX) > VisualDistance || !Main.rand.NextBool(2))
                return;

            Vector2 position = new Vector2(VolcanoBorderSystem.BorderWorldX, Player.Center.Y + Main.rand.NextFloat(-500f, 500f));
            Dust dust = Dust.NewDustPerfect(position, DustID.Torch, new Vector2(0f, -1.5f), Scale: 1.6f);
            dust.noGravity = true;
        }
    }
}
