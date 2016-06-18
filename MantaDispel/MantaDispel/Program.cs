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
        static void Main(string[] args)
        {
            Game.OnUpdate += Game_OnUpdate;

            var menuManta = new Menu("Dispel using Manta", "opsi");
            Menu.AddItem(new MenuItem("dispelTog", "Use Manta to Dispel").SetValue(true));
            var dispelBuffs = new Dictionary<string, bool>
            {

              {"modifier_item_diffusal_blade_slow", true}, { "modifier_bloodthorn_debuff", true }, {"modifier_desolator_buff", true }, {"modifier_item_dustofappearance", true },
              {"modifier_item_ethereal_blade_slow", true }, { "modifier_item_medallion_of_courage_armor_reduction", true }, { "modifier_rod_of_atos_debuff", true },
              { "modifier_orchid_malevolence_debuff", true }, { "modifier_item_shivas_guard_blast", true }, { "modifier_item_solar_crest_armor_reduction", true }, { "modifier_item_urn_of_shadows", true }, { "modifier_item_veil_of_discord_debuff", true }
            };

            Menu.AddItem(new MenuItem("Buffs", "Auto Dispel:").SetValue(new AbilityToggler(dispelBuffs)));
            Menu.AddSubMenu(menuManta);
            Menu.AddToMainMenu();
        }

        public static void Game_OnUpdate(EventArgs args)
        {
            me = ObjectManager.LocalHero;

            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
                return;

            if (me == null)
                return;

            if (mantaItem == null)
                mantaItem = me.FindItem("item_manta");

            if (!useitemCheck)
            {
                useItem = Menu.Item("dispelTog").GetValue<bool>();
                useitemCheck = true;
            }



            if (mantaItem != null && mantaItem.CanBeCasted() && useItem.IsEnabled(mantaItem.Name) &&
                Utils.SleepCheck("manta") && Menu.Item("dispelTog").GetValue<bool>())
            {
                mantaItem.UseAbility();
                Utils.Sleep(150 + Game.Ping, "mantaItem");
            }


        }

    }
}

