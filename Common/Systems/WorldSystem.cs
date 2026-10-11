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
                tasks.Insert(underworldIndex + 1, new PassLegacy("Volcanic Underworld Ceiling", (progress, configuration) => 
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
                // Le cône du volcan d'abord : son cratère s'ouvre sur le haut du gouffre
                tasks.Insert(cleanupIndex, new PassLegacy("Volcano Cone", (progress, configuration) =>
                {
                    progress.Message = "Raising the volcano...";
                    VolcanoCone.Generate();
                }));

                tasks.Insert(cleanupIndex + 1, new PassLegacy("Volcanic Chasm Generation", (progress, configuration) =>
                {
                    progress.Message = "Génération du gouffre volcanique...";
                    VolcanicChasm.Generate();
                }));
            }

            // --- ÉTAPE 3 : LE POINT D'APPARITION SUR UN BORD ---
            // Juste après le "Spawn Point" vanilla, pour que le Guide et les passes suivantes utilisent le nouveau spawn
            int spawnPointIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Spawn Point"));

            if (spawnPointIndex != -1)
            {
                tasks.Insert(spawnPointIndex + 1, new PassLegacy("Edge Spawn Point", (progress, configuration) =>
                {
                    progress.Message = "Choosing a spawn point on the edge of the world...";
                    EdgeSpawn.Generate();
                }));
            }
        }
    }
}