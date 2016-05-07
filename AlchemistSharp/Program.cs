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
        public static StreamWriter aLogger = new StreamWriter(@"C:\modifierlog.txt");
        private static Ability Stun;
        private static Item manta;
        private static Hero me;
        //private static readonly Menu Menu = new Menu("AlchemistSharp", "AlchemistSharp", true, "npc_dota_hero_Alchemist", true);
        private static System.Timers.Timer stunTimer;

        static void Main(string[] args)
        {
            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;
            //var menuAlchemistSharp = new Menu("Cancel Stun", "opsi");
            //Menu.AddSubMenu(menuAlchemistSharp);
            //menuAlchemistSharp.AddItem(new MenuItem("enable", "Enable").SetValue(true));
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

            var hasModifier = me.HasModifier("modifier_alchemist_unstable_concoction");
            //aLogger.WriteLine(hasModifier);


            stunTimer = new System.Timers.Timer();
            stunTimer.Interval = 5500;
            PrintModifiers(me);

            if (manta != null && manta.CanBeCasted() && Utils.SleepCheck("manta"))
            {
               // if (me.Modifiers.Any(x => x.Name == "modifier_alchemist_unstable_concoction"))
               if (hasModifier)
                {
                    aLogger.WriteLine(DateTime.Now + " - Hit inside modifiers.any point");
                    PrintModifiers(me);
                    stunTimer = new System.Timers.Timer();
                    stunTimer.Interval = 5500;
                    aLogger.WriteLine("Firing event");
                    stunTimer.Elapsed += OnTimedEvent;
                    stunTimer.AutoReset = false;
                    stunTimer.Enabled = true;
                }
            }

        }

        private static void PrintModifiers(Unit unit)
        {

            var buffs = unit.Modifiers.ToList();
            if (buffs.Any())
            {
                foreach (var buff in buffs)
                {
                    aLogger.WriteLine(DateTime.Now + " - "+ unit.Name + " has modifier: " + buff.Name);
                }
            }
            else
            {
                aLogger.WriteLine(unit.Name + " does not have any buff");
            }
        }


        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                aLogger.WriteLine(DateTime.Now + " - trying to use manta");
                manta.UseAbility();
                Utils.Sleep(150 + Game.Ping, "manta");
            }
            catch
            {
                aLogger.WriteLine(DateTime.Now + " - Exception Caught: {0}", e);

            }
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {

        }
    }
}

