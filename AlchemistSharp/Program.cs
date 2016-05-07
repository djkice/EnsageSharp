using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using System.Timers;
using System.IO;

namespace AlchemistSharp
{
    internal class Program
    {
        private static Ability Stun;
        private static Item manta;
        private static Hero me;
        private static readonly Menu Menu = new Menu("AlchemistSharp", "AlchemistSharp", true, "npc_dota_hero_Alchemist", true);



        static void Main(string[] args)
        {
            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;
            var menuAlchemistSharp = new Menu("Cancel Stun", "opsi");
            Menu.AddSubMenu(menuAlchemistSharp);
            menuAlchemistSharp.AddItem(new MenuItem("enable", "Enable").SetValue(true));
        }

        public static void Game_OnUpdate(EventArgs args)
        {
            me = ObjectManager.LocalHero;

            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
                return;

            if (me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Alchemist)
                return;

            if (manta == null)
                manta = me.FindItem("item_manta");

            if (Stun == null)
                Stun = me.Spellbook.Spell2;

            var modifiers = me.Modifiers.ToList();
            float stunDur = 5.4f;

            foreach (var modif in modifiers)
            {
                    if (modif.Name == "modifier_alchemist_unstable_concoction" && modif.ElapsedTime >= stunDur && Menu.Item("enable").GetValue<bool>())
                    {
                        if (manta != null && manta.CanBeCasted() && Utils.SleepCheck("manta"))
                        {
                            manta.UseAbility();
                            Utils.Sleep(150 + Game.Ping, "manta");
                        }
                    }
            }
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {

        }
    }
}


