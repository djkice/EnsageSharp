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
        public static StreamWriter aLogger = new StreamWriter(@"C:\PlaySharp\Logs\allitems.txt");
        public static StreamWriter bLogger = new StreamWriter(@"C:\PlaySharp\Logs\modifier.txt");
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
            var test = me.Modifiers.ToList();
            float stunDur = 5.5f;
            foreach (var modif in test)
            {
                

                if (me.HasModifier("modifier_stunned"))
                {
                    bLogger.WriteLine(DateTime.Now + "- Now Stunned: {0} - {1}", modif.Name, modif.ElapsedTime);
                }

                if (hasModifier)
                {
                    var elapsed = modif.ElapsedTime;
                    bLogger.WriteLine(DateTime.Now + "- Modifier elapsed time: {0} - {1}", modif.Name, modif.ElapsedTime);
                    if (modif.ElapsedTime <= stunDur)
                    {
                        if (manta != null && manta.CanBeCasted() && Utils.SleepCheck("manta"))
                        {

                            manta.UseAbility();
                            aLogger.WriteLine(DateTime.Now + " - Used manta");
                            Utils.Sleep(150 + Game.Ping, "manta");

                        }

                    }

                }


            }
            //aLogger.WriteLine(hasModifier);
            //me.FindModifier("unstable_concoction)");


            //stunTimer = new System.Timers.Timer();
            //stunTimer.Interval = 5500;
            //PrintModifiers(me);


            // if (me.Modifiers.Any(x => x.Name == "modifier_alchemist_unstable_concoction"))
            //if (hasModifier)
            //{

            // {
            //aLogger.WriteLine(DateTime.Now + " - I have modifier_alchemist_unstable_concoction");
            //if (manta != null && manta.CanBeCasted() && Utils.SleepCheck("manta"))
            //    {

            //        manta.UseAbility();
            //        aLogger.WriteLine(DateTime.Now + " - Used manta");
            //        Utils.Sleep(150 + Game.Ping, "manta");

            //    }
            //}

            //aLogger.WriteLine(DateTime.Now + " - Hit inside modifiers.any point");
            //PrintModifiers(me);


            //stunTimer = new System.Timers.Timer();
            //stunTimer.Interval = 5300;
            //aLogger.WriteLine("Firing event");
            //stunTimer.Elapsed += OnTimedEvent;
            //stunTimer.AutoReset = false;
            //stunTimer.Enabled = true;
            //}
        }




        private static void PrintModifiers(Unit unit)
        {

            var buffs = unit.Modifiers.ToList();
            if (buffs.Any())
            {
                foreach (var buff in buffs)
                {
                    aLogger.WriteLine(DateTime.Now + " - " + unit.Name + " has modifier: " + buff.Name);
                }
            }
            else
            {
                aLogger.WriteLine(unit.Name + " does not have any buff");
            }
        }


        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (manta != null && manta.CanBeCasted() && Utils.SleepCheck("manta"))
            {
                aLogger.WriteLine(DateTime.Now + " - trying to use manta");
                manta.UseAbility();
                Utils.Sleep(150 + Game.Ping, "manta");
            }
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            //if (!Game.IsInGame)
            //{
            //    return;
            //}

            ////Check if the message is a key down message
            //if (args.Msg == (uint)Utils.WindowsMessages.WM_KEYDOWN)
            //{
            //    //now check which key was pressed using wparam
            //    if (args.WParam == 'W')
            //    {
            //        PrintModifiers(me);
            //        if (me.Modifiers.Any(x => x.Name == "modifier_alchemist_unstable_concoction")) { 

            //        stunTimer = new System.Timers.Timer();
            //        stunTimer.Interval = 4000;
            //        aLogger.WriteLine("Firing event");
            //        stunTimer.Elapsed += OnTimedEvent;
            //        stunTimer.AutoReset = false;
            //        stunTimer.Enabled = true;
            //    }

            //    }

            //}
        }
    }
}


