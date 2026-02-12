using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using SuperMod.Content.World;

namespace SuperMod.Common.Systems
{
    public class WorldGenSystem : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            // --- ÉTAPE 1 : LE PLAFOND DES ENFERS ---
            // On l'insère juste après la création de l'Underworld pour qu'il puisse se "poser" sur la cendre
            int underworldIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Underworld"));
            
            if (underworldIndex != -1)
            {
                tasks.Insert(underworldIndex + 1, new PassLegacy("Vulcanic Underworld Ceiling", (progress, configuration) => 
                {
                    progress.Message = "Hardening the Underworld ceiling...";
                    UnderworldCeiling.Generate();
                }));
            }

            // --- ÉTAPE 2 : LE GOUFFRE VOLCANIQUE ---
            // On le garde vers la fin (Final Cleanup) pour qu'il soit bien propre et ne soit pas modifié par d'autres biomes
            int cleanupIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Final Cleanup"));
            
            if (cleanupIndex != -1)
            {
                tasks.Insert(cleanupIndex, new PassLegacy("Vulcanic Chasm Generation", (progress, configuration) => 
                {
                    progress.Message = "Génération du gouffre volcanique...";
                    VulcanicChasm.Generate();
                }));
            }
        }
    }
}