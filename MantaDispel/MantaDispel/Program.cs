using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using SharpDX;

namespace MantaDispel
{

    // version 1.0 inital release item dispel
    // version 1.1 addition of hero debuffs
    // version 1.2 addition of non-hero debuffs
    // version 1.3 dispell projectils (wait for Ensage fix for prejectile calculation)
    internal class Program
    {
        private static Item mantaItem;
        private static bool useitemCheck;
        private static Hero me;
        private static AbilityToggler useItem;
        private static readonly Menu Menu = new Menu("MantaDispel", "MantaDispel", true, "item_manta", true);

        static void Main(string[] args)
        {
            Game.OnUpdate += Game_OnUpdate;
            
            var menuManta = new Menu("Dispel using Manta", "opsi");
            Menu.AddItem(new MenuItem("dispelTog", "Use Manta to Dispel").SetValue(true));

            Menu.AddSubMenu(menuManta);
            Menu.AddToMainMenu();
        }

        public static void Game_OnUpdate(EventArgs args)
        {
            var dispelBuffs = new List<string>
            {
                "modifier_item_diffusal_blade_slow", "modifier_bloodthorn_debuff", "modifier_desolator_buff", "modifier_item_dustofappearance", "modifier_item_ethereal_blade_slow", "modifier_bloodthorn_debuff", "modifier_desolator_buff", "modifier_rod_of_atos_debuff", "modifier_orchid_malevolence_debuff", "modifier_item_shivas_guard_blast", "modifier_item_solar_crest_armor_reduction", "modifier_item_urn_of_shadows", "modifier_item_veil_of_discord_debuff"
            };

            me = ObjectManager.LocalHero;

            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
                return;

            if (me == null)
                return;

            if (mantaItem == null)
                mantaItem = me.FindItem("item_manta");

            foreach (var dispModif in dispelBuffs)
            {
                var hasModifier = Program.me.FindModifier(dispModif);
               
                if (hasModifier != null)
                {
                    if (mantaItem != null && mantaItem.CanBeCasted() && useItem.IsEnabled(mantaItem.Name) && Utils.SleepCheck("manta") && Menu.Item("dispelTog").GetValue<bool>())
                    {
                        mantaItem.UseAbility();
                        Utils.Sleep(150 + Game.Ping, "mantaItem");
                    }
                }

            }



        }

    }
}

