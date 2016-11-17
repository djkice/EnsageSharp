using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using SharpDX;

namespace BatmanSharp
{
    internal class Program
    {
        private static Ability napalm, flamebreak, firefly, lasso;
        private static Item blink, force;
        private static Hero me, target, lassoTarget;
        private static readonly Menu Menu = new Menu("BatmanSharp", "BatmanSharp", true, "npc_dota_hero_batrider", true);
        private static bool doCombo;
        private static bool doUlt;
        private static AbilityToggler useItem;
        private static AbilityToggler useAbility;
        private static bool useitemCheck;
        private static bool useabilityCheck;
        private static ParticleEffect targetParticle;

        private static readonly int[] wDamage = new int[5] { 0, 100, 150, 210, 280 };


        static void Main(string[] args)
        {

            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;
            Game.OnUpdate += Killsteal;
            Game.OnUpdate += NapalmHarras;

            Menu.AddItem(new MenuItem("comboKey", "Combo Key").SetValue(new KeyBind('G', KeyBindType.Press)));
            Menu.AddItem(new MenuItem("ultKey", "Ultimate Modes").SetValue(new KeyBind('L', KeyBindType.Toggle)).SetTooltip("Will lock target, move towards and use items to lasso"));
            Menu.AddItem(new MenuItem("sticky", "Napalm Harras").SetValue(new KeyBind('O', KeyBindType.Toggle)).SetTooltip("Harras closest enemy with Sticky Napalm"));
            Menu.AddItem(new MenuItem("flameks", "Flamebreak Kill Steal").SetValue(true)).SetTooltip("Kill steal with flamebreak if possible");

            var comboItems = new Dictionary<string, bool>
             {
               {"item_blink", true}, { "item_force_staff", true }
             };

            var comboAbilities = new Dictionary<string, bool>
             {
               {"batrider_sticky_napalm", true}, { "batrider_flamebreak", true }, {"batrider_firefly", true }, {"batrider_flaming_lasso", true }
             };

            Menu.AddItem(new MenuItem("Items", "Use Items:").SetValue(new AbilityToggler(comboItems)));
            Menu.AddItem(new MenuItem("Abilities", "Use Abilities:").SetValue(new AbilityToggler(comboAbilities)));

            Menu.AddToMainMenu();

        }

        public static void Game_OnUpdate(EventArgs args)
        {
            me = ObjectManager.LocalHero;

            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
                return;

            if (me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Batrider)
                return;

            if (me == null)
                return;

            if (napalm == null)
                napalm = me.Spellbook.SpellQ;
            var nrange = me.Spellbook.SpellQ.CastRange;

            if (flamebreak == null)
                flamebreak = me.Spellbook.SpellW;
            var frange = me.Spellbook.SpellW.CastRange;

            if (firefly == null)
                flamebreak = me.Spellbook.SpellE;
            var ffrange = me.Spellbook.SpellE.CastRange;

            if (lasso == null)
                lasso = me.Spellbook.SpellR;
            var lrange = me.Spellbook.SpellR.CastRange;

            if (force == null)
                force = me.FindItem("item_force_staff");

            if (blink == null)
                blink = me.FindItem("item_blink");

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
                target = TargetSelector.ClosestToMouse(me);

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
                        target = TargetSelector.ClosestToMouse(me);
                    }
                    else if (target == null || !Orbwalking.AttackOnCooldown(target) && target.HasModifiers(new[] { "modifier_dazzle_shallow_grave", "modifier_item_blade_mail_reflect" }, false))
                    {
                        var bestAa = me.BestAATarget();
                        if (bestAa != null)
                        {
                            target = TargetSelector.BestAutoAttackTarget(me); ;
                        }
                    }
                }



                if (target != null && target.IsAlive && !target.IsInvul() && !target.IsIllusion)
                {

                    var targetDistance = me.Distance2D(target);

                    if (me.CanAttack() && me.CanCast())
                    {

                        if (!Utils.SleepCheck("attacking"))
                        {
                            Orbwalking.Orbwalk(target, Game.Ping);
                            Utils.Sleep(200, "attacking");
                        }

                        if (blink != null && blink.CanBeCasted() && useItem.IsEnabled(blink.Name) && targetDistance > 500 && targetDistance <= 1170 && Utils.SleepCheck("blink"))
                        {
                            blink.UseAbility(target.Position);
                            Utils.Sleep(250 + Game.Ping, "blink");
                        }

                        if (force != null && force.CanBeCasted() && useItem.IsEnabled(force.Name) && Utils.SleepCheck("force") && targetDistance > 200 && targetDistance <= (570 + me.AttackRange))
                        {
                            force.UseAbility(me);
                            Utils.Sleep(250 + Game.Ping, "force");
                        }

                        if (napalm != null & napalm.CanBeCasted() && useAbility.IsEnabled(napalm.Name) && Utils.SleepCheck("napalm") && targetDistance <= nrange)
                        {
                            napalm.UseAbility(target.Position);
                            Utils.Sleep(100 + Game.Ping, "napalm");
                        }

                        if (firefly != null & firefly.CanBeCasted() && useAbility.IsEnabled(firefly.Name) && Utils.SleepCheck("firefly") && targetDistance <= ffrange)
                        {
                            firefly.UseAbility();
                            Utils.Sleep(400 + Game.Ping, "firefly");
                        }

                        if (flamebreak != null & flamebreak.CanBeCasted() && useAbility.IsEnabled(flamebreak.Name) && Utils.SleepCheck("flamebreak") && targetDistance <= frange)
                        {
                            flamebreak.UseAbility(target.Position);
                            Utils.Sleep(170 + Game.Ping, "flamebreak");
                        }
                    }
                }
               else
                {
                    me.Move(Game.MousePosition);
                }
            }
            else if (doUlt)
            {
                if (me.IsAlive)
                {
                    target = lassoTarget;
                    
                    var targetDistance = me.Distance2D(target);

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

                    if (lasso != null & lasso.CanBeCasted() && useAbility.IsEnabled(lasso.Name) && Utils.SleepCheck("lasso"))
                    {

                        if (force != null && force.CanBeCasted() && useItem.IsEnabled(force.Name) && Utils.SleepCheck("force") && blink != null && blink.CanBeCasted() && useItem.IsEnabled(blink.Name) && Utils.SleepCheck("blink") && targetDistance > 1170 && targetDistance <= (1760 + lrange))
                        {
                            blink.UseAbility(target.Position);
                            force.UseAbility(me);
                            Utils.Sleep(250 + Game.Ping, "blink");
                            Utils.Sleep(250 + Game.Ping, "force");
                        }

                        if (blink != null && blink.CanBeCasted() && useItem.IsEnabled(blink.Name) && targetDistance > 500 && targetDistance <= (1170 + lrange) && Utils.SleepCheck("blink"))
                        {
                            blink.UseAbility(target.Position);
                            Utils.Sleep(250 + Game.Ping, "blink");
                        }

                        if (force != null && force.CanBeCasted() && useItem.IsEnabled(force.Name) && Utils.SleepCheck("force") && targetDistance > 200 && targetDistance <= (570 + lrange))
                        {
                            force.UseAbility(me);
                            Utils.Sleep(250 + Game.Ping, "force");
                        }

                        if (targetDistance <= lrange)
                        {
                            lasso.UseAbility(target);
                            Utils.Sleep(600 + Game.Ping, "lasso");
                        }

                    }
                    else if (!me.HasModifier("modifier_batrider_flaming_lasso") && me.IsAlive)
                    {
                            me.Move(target.Predict(lrange));
                    }

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
                else if (Menu.Item("ultKey").GetValue<KeyBind>().Active)
                {
                    if (me.IsAlive)
                    {
                        lassoTarget = TargetSelector.ClosestToMouse(me);
                        doUlt = true;

                    }
                }
                else
                {
                    doCombo = false;
                    doUlt = false;
                }
            }
        }

        public static void Killsteal(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
            {
                return;
            }
            me = ObjectManager.LocalHero;

            if (me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Batrider) return;

            if (flamebreak == null)
                flamebreak = me.Spellbook.SpellW;

            var range = me.Spellbook.SpellW.CastRange;

            var flamebreaklvl = me.Spellbook.SpellW.Level;

            if (Utils.SleepCheck("killstealW") && Menu.Item("flameks").GetValue<bool>())
            {
                if (flamebreak.CanBeCasted() && me.Mana > flamebreak.ManaCost)
                {
                    var enemy = ObjectManager.GetEntities<Hero>().Where(e => e.Team != me.Team && e.IsAlive && e.IsVisible && !e.IsIllusion && !e.UnitState.HasFlag(UnitState.MagicImmune) && me.Distance2D(e) < range).ToList();
                    foreach (var v in enemy)
                    {
                        //var damage = Math.Floor((wDamage[flamebreaklvl] * (1 - v.MagicDamageResist)) - (v.HealthRegeneration * 5));

                        var damage = wDamage[flamebreaklvl];
                        if (v.Health < damage && me.Distance2D(v) < range)
                        {
                            flamebreak.UseAbility(v.Position);
                            Utils.Sleep(200 + Game.Ping, "killstealW");
                        }
                    }
                }

            }
        }

        public static void NapalmHarras(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
            {
                return;
            }

            me = ObjectManager.LocalHero;


            if (me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Batrider) return;

            if (napalm == null)
                napalm = me.Spellbook.SpellQ;

            var range = me.Spellbook.SpellQ.CastRange;

            if (Utils.SleepCheck("napalm") && Menu.Item("sticky").GetValue<KeyBind>().Active)
            {
                if (napalm.CanBeCasted() && me.Mana > napalm.ManaCost)
                {
                    var enemy = ObjectManager.GetEntities<Hero>().Where(e => e.Team != me.Team && e.IsAlive && e.IsVisible && !e.IsIllusion && !e.UnitState.HasFlag(UnitState.MagicImmune) && me.Distance2D(e) < range).ToList();
                    foreach (var v in enemy)
                    {
                        if (me.Distance2D(v) < range)
                        {
                            napalm.UseAbility(v.Position);
                            Utils.Sleep(100 + Game.Ping, "napalm");
                        }
                    }
                }

            }
        }

    }
}

