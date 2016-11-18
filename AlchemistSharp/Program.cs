using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using SharpDX;

namespace AlchemistSharp
{
    internal class Program
    {
        // Add armlet logic to combo
        // Add shadow blade after casting stun

        private static Ability acid, concoction, rage, throwconc;
        private static Item blink, abyssal, manta, bkb;
        private static Hero me, target;
        private static readonly Menu Menu = new Menu("AlchemistSharp", "AlchemistSharp", true, "npc_dota_hero_Alchemist", true);
        private static bool doCombo;
        private static AbilityToggler useItem;
        private static AbilityToggler useAbility;
        private static bool useitemCheck;
        private static bool useabilityCheck;
        private static ParticleEffect targetParticle;

        static void Main(string[] args)
        {
            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;

            var menuRage = new Menu("Auto Chemical Rage", "opsi");
            Menu.AddItem(new MenuItem("comboKey", "Combo Key").SetValue(new KeyBind(32, KeyBindType.Press)));
            Menu.AddItem(new MenuItem("dodgeTog", "Use Manta - Dodge Self Stun").SetValue(true));

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

            menuRage.AddItem(new MenuItem("rageTog", "Use Chemical Rage").SetValue(true));
            menuRage.AddItem(new MenuItem("ragehealth", "If Health % is:").SetValue(new Slider(15)));
            Menu.AddSubMenu(menuRage);
            Menu.AddToMainMenu();

        }

        public static void Game_OnUpdate(EventArgs args)
        {
            me = ObjectManager.LocalHero;
            float stunBrew = 5.4f - (Game.Ping / 1000);
            float maxStun = 4.8f - (Game.Ping / 1000);
            var stunrange = 775;
            var acidrange = 625;
            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
                return;

            if (me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Alchemist)
                return;

            if (me == null)
                return;

            if (acid == null)
                acid = me.Spellbook.SpellQ;

            if (concoction == null)
                concoction = me.Spellbook.SpellW;

            if (throwconc == null)
                throwconc = me.FindSpell("alchemist_unstable_concoction_throw");

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

            var concModif = me.FindModifier("modifier_alchemist_unstable_concoction");
            var invisModif = me.Modifiers.Any(x => x.Name == "modifier_item_silver_edge_windwalk" || x.Name == "modifier_item_invisibility_edge_windwalk");
            if (doCombo)
            {
                //var target = TargetSelector.ClosestToMouse(me);
                target = me.ClosestToMouseTarget(1000);

                if (target != null && (!target.IsValid || !target.IsVisible || !target.IsAlive || target.Health <= 0))
                {
                    target = null;
                }

                if (targetParticle == null && target != null)
                {
                    targetParticle = new ParticleEffect(@"particles\ui_mouseactions\range_finder_tower_aoe.vpcf", target);
                }
                if ((target == null || !target.IsVisible || !target.IsAlive) && targetParticle != null)
                {
                    targetParticle.Dispose();
                    targetParticle = null;
                }
                
                if (target != null && targetParticle != null)
                {
                    targetParticle.SetControlPoint(2, me.Position);
                    targetParticle.SetControlPoint(6, new Vector3(1, 0, 0));
                    targetParticle.SetControlPoint(7, target.Position);
                }

                var canCancel = Orbwalking.CanCancelAnimation();

                if (canCancel)
                {
                    if (target != null && !target.IsVisible && !Orbwalking.AttackOnCooldown(target))
                    {
                        target = me.ClosestToMouseTarget();
                        //target = TargetSelector.ClosestToMouse(me);
                    }
                    else if (target == null || !Orbwalking.AttackOnCooldown(target) && target.HasModifiers(new[] { "modifier_dazzle_shallow_grave", "modifier_item_blade_mail_reflect" }, false))
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
                                Orbwalking.Orbwalk(target, Game.Ping);
                                Utils.Sleep(200, "attacking");
                            }

                            if (acid != null & acid.CanBeCasted() && useAbility.IsEnabled(acid.Name) && Utils.SleepCheck("acid") && me.Distance2D(target) <= acidrange)
                            {
                                acid.UseAbility(target.Position);
                                Utils.Sleep(150 + Game.Ping, "acid");
                            }

                            if (rage != null && rage.CanBeCasted() && useAbility.IsEnabled(rage.Name) && Utils.SleepCheck("rage"))
                            {
                                rage.UseAbility();
                                Utils.Sleep(250 + Game.Ping, "rage");
                            }

                            if (blink != null && blink.CanBeCasted() && useItem.IsEnabled(blink.Name) && me.Distance2D(target) > 500 && me.Distance2D(target) <= 1170 && Utils.SleepCheck("blink"))
                            {
                                blink.UseAbility(target.Position);
                                Utils.Sleep(250 + Game.Ping, "blink");
                            }

                            //if (concModif != null && useAbility.IsEnabled(concoction.Name) && concModif.ElapsedTime < stunBrew && concModif.ElapsedTime > maxStun && me.Distance2D(target) <= stunrange && !target.UnitState.HasFlag(UnitState.MagicImmune))
                            //{
                            //    throwconc.UseAbility(target);
                            //    Utils.Sleep(250 + Game.Ping, "throwconc");
                            //}
                            //if (concoction != null && concoction.CanBeCasted() && useAbility.IsEnabled(concoction.Name) && Utils.SleepCheck("concoction"))
                            //{
                            //    concoction.UseAbility();
                            //    Utils.Sleep(250 + Game.Ping, "concoction");
                            //}

                            if (me.Distance2D(target) < 1000)
                            {
                                if (concoction != null && concoction.CanBeCasted() && useAbility.IsEnabled(concoction.Name) && Utils.SleepCheck("concoction"))
                                {
                                    concoction.UseAbility();
                                    Utils.Sleep(250 + Game.Ping, "concoction");
                                }
                                if (concModif != null && useAbility.IsEnabled(concoction.Name) && /* !target.UnitState.HasFlag(UnitState.MagicImmune)  && */ me.Distance2D(target) < 1000)
                                {
                                    if (!Utils.SleepCheck("attacking"))
                                    {
                                        Orbwalking.Orbwalk(target, Game.Ping);
                                        Utils.Sleep(200, "attacking");
                                    }
                                    if (me.Distance2D(target) > stunrange)
                                    {
                                        if (!me.CanAttack())
                                        {
                                            me.Move(target.Predict(stunrange));
                                        }
                                        else
                                        {
                                            me.Attack(target);
                                        }
                                    }
                                    if (concModif.ElapsedTime < stunBrew && concModif.ElapsedTime > maxStun && me.Distance2D(target) <= stunrange)
                                    {
                                        throwconc.UseAbility(target);
                                        Utils.Sleep(250 + Game.Ping, "throwconc");
                                        return;
                                    }
                                }

                            }

                            if (abyssal != null && abyssal.CanBeCasted() && useItem.IsEnabled(abyssal.Name) && Utils.SleepCheck("abyssal"))
                            {
                                abyssal.CastStun(target);
                                Utils.Sleep(250 + Game.Ping, "abyssal");
                            }

                            if (manta != null && manta.CanBeCasted() && useItem.IsEnabled(manta.Name) && Utils.SleepCheck("manta"))
                            {
                                manta.UseAbility();
                                Utils.Sleep(150 + Game.Ping, "manta");
                            }

                            if (bkb != null && bkb.CanBeCasted() && useItem.IsEnabled(bkb.Name) && Utils.SleepCheck("bkb") && me.Distance2D(target) <= 620)
                            {
                                bkb.UseAbility();
                                Utils.Sleep(150 + Game.Ping, "bkb");
                            }
                            // orb walk fix attempt. 
                        // if (!me.IsAttacking() && Utils.SleepCheck("follow") && concModif == null)
                       // {
                       //     me.Move(Game.MousePosition);
                         //   Utils.Sleep(150 + Game.Ping, "follow");
                      //  }

                            // else 
                            // {
                            //    me.Move(target);
                            //  }

                        }

   var illusions = ObjectManager.GetEntities<Hero>().Where(f => f.IsAlive && f.IsControllable && f.Team == me.Team && f.IsIllusion && f.Modifiers.Any(y => y.Name != "modifier_kill")).ToList();

          foreach (var illusion in illusions.TakeWhile(illusion => Utils.SleepCheck("illu_attacking" + illusion.Handle)))
          {
              illusion.Attack(target);
                Utils.Sleep(350, "illu_attacking" + illusion.Handle);
                                       }
                   }
 
                }
        else
          me.Move(Game.MousePosition);
         }
            }

            if (concModif != null && concModif.ElapsedTime >= stunBrew && Menu.Item("dodgeTog").GetValue<bool>())
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
