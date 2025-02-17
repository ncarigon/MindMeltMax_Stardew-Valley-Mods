﻿using StardewModdingAPI;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trading.Utilities
{
    public class Config
    {
        public string TradeMenuButton { get; set; } = "G, LeftStick";

        public int Radius { get; set; } = 1;

        [JsonIgnore]
        public IEnumerable<SButton> TradeMenuSButton => ParseButtons(TradeMenuButton);

        private IEnumerable<SButton> ParseButtons(string btn)
        {
            List<SButton> open = new List<SButton>();
            string[] buttons = btn.Split(',');
            for (int i = 0; i < buttons.Length; i++)
                if (Enum.TryParse(buttons[i].Trim(), out SButton sButton))
                    open.Add(sButton);

            return open;
        }
    }
}
