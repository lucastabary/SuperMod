using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace SuperMod.Content.Items
{
    
    public class WeirdShroom : ModItem
    {
        private static readonly int[] PossibleEffects = new int[]
        {
            BuffID.Swiftness,
            BuffID.Ironskin,
            BuffID.Confused,
            BuffID.Gravitation,
            BuffID.Burning,
            BuffID.NightOwl
        };

        private static readonly int BuffDuration = 1800;

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.UseSound = SoundID.Item2;
            Item.consumable = true;
            Item.maxStack = 999;
            Item.rare = ItemRarityID.Quest;
            Item.value = Item.buyPrice(silver: 2);
        }

        public override bool? UseItem(Player player)
        {
            int selectedBuff = Main.rand.Next(PossibleEffects);
            player.AddBuff(selectedBuff, BuffDuration);
            if (Main.myPlayer == player.whoAmI) 
            {
                Main.NewText("Une étrange sensation parcourt ton corps...", 155, 0, 255);
            }

            return true;
        }
    }
}