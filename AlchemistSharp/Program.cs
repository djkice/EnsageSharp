using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;

namespace AlchemistSharp
{
    internal class Program
    {

        private static Ability acid, concoction, rage;
        private static Item blink, abyssal, manta, bkb;
        private static Hero me, target;
        private static readonly Menu Menu = new Menu("AlchemistSharp", "AlchemistSharp", true, "npc_dota_hero_Alchemist", true);
        private static bool doCombo;
        private static AbilityToggler useItem;
        private static AbilityToggler useAbility;
        private static bool useitemCheck;
        private static bool useabilityCheck;

        static void Main(string[] args)
        {
            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;

            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("---------------------------------");
            Console.WriteLine("| AlchemistSharp 2.0.0.0 Loaded |");
            Console.WriteLine("---------------------------------");
            Console.ResetColor();


            var menuRage = new Menu("Auto Chemical Range", "opsi");
            Menu.AddItem(new MenuItem("comboKey", "Combo Key").SetValue(new KeyBind(32, KeyBindType.Press)));
            Menu.AddItem(new MenuItem("dodgeTog", "Use Manta - Dodge Self Stun").SetValue(true));

            // Trying priority changer thanks to Moones

            Menu.AddItem(
                new MenuItem("myComboPriority", "ComboPriority: ").SetValue(
                    new PriorityChanger(
                        new List<string>(
                            new[]
                                {
                                    "item_blink", "alchemist_unstable_concoction", "item_abyssal_blade", "alchemist_chemical_rage",
                                    "alchemist_acid_spray", "item_manta", "item_black_king_bar"
                                }),
                        "MyComboPriority")));

            var comboItems = new Dictionary<string, bool>
            {
              {"item_blink", true}, { "item_abyssal_blade", true }, {"item_manta", true }, {"item_black_king_bar", true }
            };

            var comboAbilities = new Dictionary<string, bool>
            {
              {"alchemist_acid_spray", true}, { "alchemist_unstable_concoction", true }, {"alchemist_chemical_rage", true }
            };

            Menu.AddItem(new MenuItem("Items", "Use Items:").SetValue(new AbilityToggler(comboItems)));
            Menu.AddItem(new MenuItem("Abilities", "Use Abilities:").SetValue(new AbilityToggler(comboAbilities)));

            menuRage.AddItem(new MenuItem("rageTog", "Use Chemical Range").SetValue(true));
            menuRage.AddItem(new MenuItem("ragehealth", "If Health % is:").SetValue(new Slider(15)));
            Menu.AddSubMenu(menuRage);
            Menu.AddToMainMenu();

        }

        public static void Game_OnUpdate(EventArgs args)
        {
            me = ObjectManager.LocalHero;
            var modifier = me.FindModifier("modifier_alchemist_unstable_concoction");
            float stunBrew = 5.4f - (Game.Ping / 1000);
            float maxStun = 5.0f - (Game.Ping / 1000);
            var stunrange = 175;
            var acidrange = 625;
            var invisModif = me.Modifiers.Any(x => x.Name == "modifier_item_silver_edge_windwalk" || x.Name == "modifier_item_invisibility_edge_windwalk");

            var priority = Menu.Item("myComboPriority").GetValue<PriorityChanger>();
            var spells = ObjectManager.LocalHero.Spellbook.Spells.OrderByDescending(spell => priority.GetPriority(spell.Name));


            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
                return;

            if (me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Alchemist)
                return;

            if (me == null)
                return;

            if (acid == null)
                acid = me.Spellbook.Spell1;

            if (concoction == null)
                concoction = me.Spellbook.Spell2;

            if (rage == null)
                rage = me.Spellbook.SpellR;

            if (manta == null)
                manta = me.FindItem("item_manta");

            if (blink == null)
                blink = me.FindItem("item_blink");

            if (abyssal == null)
                abyssal = me.FindItem("item_abyssal_blade");

            if (bkb == null)
                bkb = me.FindItem("item_black_king_bar");

            if (!useitemCheck)
            {
                useItem = Menu.Item("Items").GetValue<AbilityToggler>();
                useitemCheck = true;
            }
            if (!useabilityCheck)
            {
                useAbility = Menu.Item("Abilities").GetValue<AbilityToggler>();
                useabilityCheck = true;
            }

            if (doCombo)
            {

                target = me.ClosestToMouseTarget(1000);

                if (target != null && (!target.IsValid || !target.IsVisible || !target.IsAlive || target.Health <= 0))
                {
                    target = null;
                }

                var canCancel = Orbwalking.CanCancelAnimation();
                if (canCancel)
                {
                    if (target != null && !target.IsVisible && !Orbwalking.AttackOnCooldown(target))
                    {
                        target = me.ClosestToMouseTarget();
                    }
                    else if (target == null || !Orbwalking.AttackOnCooldown(target) && target.HasModifiers(new[]
                                    {
                                        "modifier_dazzle_shallow_grave", "modifier_item_blade_mail_reflect",
                                    }, false))
                    {
                        var bestAa = me.BestAATarget();
                        if (bestAa != null)
                        {
                            target = me.BestAATarget();
                        }
                    }
                }

                if (target != null && target.IsAlive && !target.IsInvul() && !target.IsIllusion)
                {

                    if (me.CanAttack() && me.CanCast())
                    {
                        if (!invisModif)
                        {
                            if (!Utils.SleepCheck("attacking"))
                            {
                                {
                                    Orbwalking.Orbwalk(target, Game.Ping);
                                }
                                Utils.Sleep(200, "attacking");
                            }
                        }

                        // add armlet toggle
                        // add mana check for ult


                        foreach (var spell in spells)
                        {
                            if (spell.Name == "alchemist_acid_spray")
                            {
                                if (acid != null & acid.CanBeCasted() && useAbility.IsEnabled(acid.Name) && Utils.SleepCheck("acid") && me.Distance2D(target) <= acidrange)
                                {
                                    acid.UseAbility(target.Position);
                                    Utils.Sleep(150 + Game.Ping, "acid");
                                }
                            }

                            if (spell.Name == "alchemist_chemical_rage")
                            {
                                if (rage != null && rage.CanBeCasted() && useAbility.IsEnabled(rage.Name) && Utils.SleepCheck("rage"))
                                {
                                    rage.UseAbility();
                                    Utils.Sleep(150 + Game.Ping, "rage");
                                }
                            }

                            if (spell.Name == "item_blink")
                            {
                                if (blink != null && blink.CanBeCasted() && useItem.IsEnabled(blink.Name) && me.Distance2D(target) > 500 && me.Distance2D(target) <= 1170 && Utils.SleepCheck("blink"))
                                {
                                    blink.UseAbility(target.Position);
                                    Utils.Sleep(250 + Game.Ping, "blink");
                                }
                            }
                            if (spell.Name == "alchemist_unstable_concoction")
                            {
                                if (modifier != null && useAbility.IsEnabled(concoction.Name) && modifier.ElapsedTime < stunBrew && modifier.ElapsedTime > maxStun && me.Distance2D(target) <= stunrange)
                                {
                                    concoction.UseAbility(target.Position);
                                    Utils.Sleep(250 + Game.Ping, "concoction");
                                }
                                if (concoction != null && concoction.CanBeCasted() && useAbility.IsEnabled(concoction.Name) && Utils.SleepCheck("concoction") && me.Distance2D(target) <= stunrange && !target.UnitState.HasFlag(UnitState.MagicImmune))
                                {
                                    concoction.UseAbility();
                                    Utils.Sleep(150 + Game.Ping, "Concoction");
                                }
                            }

                            // if (target.CanAttack) {
                            if (spell.Name == "item_abyssal_blade")
                            {
                                if (abyssal != null && abyssal.CanBeCasted() && useItem.IsEnabled(abyssal.Name) && Utils.SleepCheck("abyssal"))
                                {
                                    abyssal.CastStun(target);
                                    Utils.Sleep(250 + Game.Ping, "abyssal");
                                }
                            }
                            // }
                            if (spell.Name == "item_manta")
                            {
                                if (manta != null && manta.CanBeCasted() && useItem.IsEnabled(manta.Name) && Utils.SleepCheck("manta"))
                                {
                                    manta.UseAbility();
                                    Utils.Sleep(150 + Game.Ping, "manta");
                                }
                            }


                            if (spell.Name == "item_black_king_bar")
                            {
                                if (bkb != null && bkb.CanBeCasted() && useItem.IsEnabled(bkb.Name) && Utils.SleepCheck("bkb") && me.Distance2D(target) <= 620)
                                {
                                    bkb.UseAbility();
                                    Utils.Sleep(150 + Game.Ping, "bkb");
                                }
                            }
                        }
                        var illusions = ObjectManager.GetEntities<Hero>().Where(f => f.IsAlive && f.IsControllable && f.Team == me.Team && f.IsIllusion && f.Modifiers.Any(y => y.Name != "modifier_kill")).ToList();

                        foreach (var illusion in illusions.TakeWhile(illusion => Utils.SleepCheck("illu_attacking" + illusion.Handle)))
                        {
                            illusion.Attack(target);
                            Utils.Sleep(350, "illu_attacking" + illusion.Handle);
                        }
                    }
                }
            }

            if (modifier != null && modifier.ElapsedTime >= stunBrew && Menu.Item("dodgeTog").GetValue<bool>())
            {
                if (manta != null && manta.CanBeCasted() && Utils.SleepCheck("manta"))
                {
                    manta.UseAbility();
                    Utils.Sleep(150 + Game.Ping, "manta");
                }
            }

            if (rage != null && rage.IsValid && rage.CanBeCasted() && Menu.Item("rageTog").GetValue<bool>() && me.Health <= me.MaximumHealth / 100 * Menu.Item("ragehealth").GetValue<Slider>().Value && Utils.SleepCheck("rage"))
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
            if (!Game.IsChatOpen)
            {

                if (Menu.Item("comboKey").GetValue<KeyBind>().Active)
                {
                    doCombo = true;
                }
                else
                {
                    doCombo = false;
                }
            }

        }
    }
}