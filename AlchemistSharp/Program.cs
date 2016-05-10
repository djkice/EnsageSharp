using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;

namespace AlchemistSharp
{
    internal class Program
    {
       
        private static Ability rage;
        private static Item manta;
        private static Hero me;
        private static readonly Menu Menu = new Menu("AlchemistSharp", "AlchemistSharp", true, "npc_dota_hero_Alchemist", true);

        static void Main(string[] args)
        {
            Game.OnUpdate += Game_OnUpdate;
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("---------------------------------");
            Console.WriteLine("| AlchemistSharp 1.1.0.0 Loaded |");
            Console.WriteLine("---------------------------------");
            Console.ResetColor();
            Game.OnWndProc += Game_OnWndProc;
            var menuRage = new Menu("Auto Chemical Range", "opsi");
            Menu.AddItem(new MenuItem("dodgetog", "Use Manta - Dodge Self Stun").SetValue(true));
            menuRage.AddItem(new MenuItem("ragetog", "Use Chemical Range").SetValue(true));
            menuRage.AddItem(new MenuItem("ragehealth", "If Health % is:").SetValue(new Slider(15)));
            Menu.AddSubMenu(menuRage);
            Menu.AddToMainMenu();
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

            if (rage == null)
                rage = me.Spellbook.SpellR;

            var modifier = me.FindModifier("modifier_alchemist_unstable_concoction");
            float stunDur = 5.4f - (Game.Ping / 1000);

            
            if (modifier != null && modifier.ElapsedTime >= stunDur && Menu.Item("dodgetog").GetValue<bool>())
            {
                if (manta != null && manta.CanBeCasted() && Utils.SleepCheck("manta"))
                {
                    manta.UseAbility();
                    Utils.Sleep(150 + Game.Ping, "manta");
                }
            }

            if (rage != null && rage.IsValid && rage.CanBeCasted() &&  Menu.Item("ragetog").GetValue<bool>() && me.Health <= me.MaximumHealth / 100 * Menu.Item("ragehealth").GetValue<Slider>().Value && Utils.SleepCheck("rage"))
            {
                 if (me.CanAttack() && me.CanCast() && !me.IsChanneling())
                {
                    rage.UseAbility();
                    Utils.Sleep(250 + Game.Ping, "rage");
                }
               
            }


        }

        private static void Game_OnWndProc(WndEventArgs args)
        {

        }
    }
}


