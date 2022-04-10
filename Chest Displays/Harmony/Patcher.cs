﻿using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chest_Displays.Harmony
{
    public class Patcher
    {
        private static HarmonyInstance harmony;

        public static void Init(IModHelper helper)
        {
            harmony = HarmonyInstance.Create(helper.ModRegistry.ModID);

            harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.draw), new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
                postfix: new HarmonyMethod(typeof(ChestPatches), nameof(ChestPatches.draw_postfix))
            );
        }
    }
}
