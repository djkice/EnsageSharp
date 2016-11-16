using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using SharpDX;

namespace ODSharp
{

    internal class Program
    {

        private static Ability astral, orb, eclipse;
        private static Item bkb, hex, orchid, shiva, atos, blink, force, veil, pike, thorn;
        private static readonly Menu Menu = new Menu("ODSharp", "odsharp", true, "npc_dota_hero_obsidian_destroyer", true);
        private static Hero me, target;
        private static Creep LastAttacked;
        private static float scaleX, scaleY;
        private static bool Combo;
        private static bool HPBar;
        private static StringList Targeting;
        private static bool KillSteal;
        public static bool AutoAttackDisable;
        private static AbilityToggler menuValue;
        private static bool menuvalueSet;
        private static readonly int[] orbb = new int[5] { 0, 1, 2, 3, 4 };
        private static readonly int[] ult = { 0, 8, 9, 10 };
        private static readonly int[] wDamage = new int[5] { 0, 75, 150, 225, 300 };
        private static readonly int[] ultRange = { 0, 375, 475, 575 };


        private static void Main()
        {
            Game.OnUpdate += Game_OnUpdate;
            Game.OnUpdate += Killsteal;
            Game.OnUpdate += AutoUlti;
            Game.OnUpdate += AstralHarras;
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnDraw += DrawUltiDamage;
            Game.OnWndProc += Game_OnWndProc;
            Console.WriteLine("ODSharp LOADED!");

            scaleX = ((float)Drawing.Width / 1920);
            scaleY = ((float)Drawing.Height / 1080);

            var orbwalk = new Menu("OrbWalk", "orb");
            var options = new Menu("Options", "opt");
            var ulti = new Menu("AutoUlti", "ult");
            ulti.AddItem(new MenuItem("autoUlt", "EnableAutoUlti").SetValue(true));
            ulti.AddItem(new MenuItem("Heel", "Min targets to ulti").SetValue(new Slider(2, 1, 5)));
            options.AddItem(new MenuItem("wks", "Astral Kill Steal").SetValue(true)).SetTooltip("If in range, it will steal kill with Astral imprisonment");
            options.AddItem(new MenuItem("whs", "Auto Steal").SetValue(new KeyBind(45, KeyBindType.Toggle)).SetTooltip("If enemy hero in range will harras using Astral (closest enemy)"));
            options.AddItem(new MenuItem("autoUlt", "EnableAutoUlt").SetValue(true));
            options.AddItem(new MenuItem("hpdraw", "Draw HP Bar").SetValue(true).SetTooltip("Will show ulti damage on HP Bar"));

            Menu.AddSubMenu(options);
            options.AddItem(new MenuItem("comboKey", "Combo Key").SetValue(new KeyBind(32, KeyBindType.Press)));
            options.AddItem(new MenuItem("ts", "Target Selector").SetValue(new StringList(new[] { "ClosestToMouse", "HighestHP", "HighestINT", "BestAATarget" })));
            Menu.AddToMainMenu();
            Menu.AddSubMenu(orbwalk);
            Menu.AddSubMenu(ulti);
            orbwalk.AddItem(new MenuItem("orbwalkk", "OrbWalk").SetValue(true));
            var dict = new Dictionary<string, bool>
            {
                { "item_black_king_bar", true },
                { "item_sheepstick", true },
                { "item_orchid", true },
                { "item_shivas_guard", true },
                { "item_rod_of_atos", true },
                { "item_blink", true },
                { "item_force_staff", true },
                { "item_hurricane_pike", true },
                { "veil_of_discord", true },
                { "item_bloodthorn", true }

            };
            Menu.AddItem(new MenuItem("Items", "Items:").SetValue(new AbilityToggler(dict)));
            Targeting = options.Item("ts").GetValue<StringList>();

        }

        public static void Game_OnUpdate(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
                return;

            me = ObjectMgr.LocalHero;
            if (me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Obsidian_Destroyer) return;

            if (astral == null)
                astral = me.Spellbook.SpellW;

            if (eclipse == null)
                eclipse = me.Spellbook.SpellR;


            if (bkb == null)
                bkb = me.FindItem("item_black_king_bar");

            if (orb == null)
                orb = me.Spellbook.Spell1;

            if (hex == null)
                hex = me.FindItem("item_sheepstick");

            if (orchid == null)
                orchid = me.FindItem("item_orchid");

            if (shiva == null)
                shiva = me.FindItem("item_shivas_guard");

            if (atos == null)
                atos = me.FindItem("item_rod_of_atos");

            if (blink == null)
                blink = me.FindItem("item_blink");

            if (force == null)
                force = me.FindItem("item_force_staff");

            if (pike == null)
                pike = me.FindItem("item_hurricane_pike");

            if (veil == null)
                veil = me.FindItem("veil_of_discord");

            if (thorn == null)
                thorn = me.FindItem("item_bloodthorn");

            if (!menuvalueSet)
            {
                menuValue = Menu.Item("Items").GetValue<AbilityToggler>();
                menuvalueSet = true;
            }


            if (Combo)
            {
                var ctm = Targeting.SelectedIndex != 2 || Targeting.SelectedIndex != 1 || Targeting.SelectedIndex != 3;
                var hhp = Targeting.SelectedIndex != 0 || Targeting.SelectedIndex != 2 || Targeting.SelectedIndex != 3;
                var hi = Targeting.SelectedIndex != 1 || Targeting.SelectedIndex != 0 || Targeting.SelectedIndex != 3;
                var baat = Targeting.SelectedIndex != 0 || Targeting.SelectedIndex != 1 || Targeting.SelectedIndex != 2;

                if (ctm)
                {
                    target = me.ClosestToMouseTarget(1000);
                }

                else if (hhp)
                {
                    target = TargetSelector.HighestHealthPointsTarget(me, 600);
                }

                else if (hi)
                {
                    target = HighestInt(me);
                }

                else if (baat)
                {
                    target = me.BestAATarget();
                }

                //orbwalk
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

                }
                if (target != null && target.IsAlive && !target.IsInvul() && !target.IsIllusion)
                {

                    var targetDistance = me.Distance2D(target);
                    if (me.CanAttack() && me.CanCast())

                        if (orb != null && orb.IsValid && orb.CanBeCasted() && me.CanCast() && Utils.SleepCheck("orb") && !target.UnitState.HasFlag(UnitState.MagicImmune))
                        {
                            orb.UseAbility(target);
                            Utils.Sleep(50, "orb");
                        }

                    if (atos != null && atos.IsValid && atos.CanBeCasted() && Utils.SleepCheck("atos") && menuValue.IsEnabled(atos.Name))
                    {
                        atos.UseAbility(target);
                        Utils.Sleep(50 + Game.Ping, "atos");
                    }

                    if (shiva != null && shiva.IsValid && shiva.CanBeCasted() && Utils.SleepCheck("shiva") && menuValue.IsEnabled(shiva.Name))
                    {
                        atos.UseAbility(target);
                        Utils.Sleep(50 + Game.Ping, "shiva");
                    }

                    if (!(targetDistance <= me.AttackRange)) return;
                    if (bkb != null && bkb.IsValid && bkb.CanBeCasted() && Utils.SleepCheck("bkb") && menuValue.IsEnabled(bkb.Name))
                    {
                        bkb.UseAbility();
                        Utils.Sleep(150 + Game.Ping, "bkb");
                    }

                    if (pike != null && pike.CanBeCasted() && menuValue.IsEnabled(pike.Name) && Utils.SleepCheck("pike"))
                    {
                        hurricane.UseAbility(target);
                        Utils.Sleep(150 + Game.Ping, "pike");

                    }

                    if (me.Modifiers.ToList().Exists(xk => x.Name == "modifier_item_hurricane_pike_range") && Utils.SleepCheck("orb"))

                    {
                        if (me.CanAttack() && me.CanCast())
                        {

                            if (orb != null && orb.IsValid && orb.CanBeCasted() && me.CanCast() && Utils.SleepCheck("orb") && !target.UnitState.HasFlag(UnitState.MagicImmune))
                            {
                                orb.UseAbility(target);
                                Utils.Sleep(50, "orb");
                            }
                        }
                        else {
                            me.Attack(target);
                            Utils.Sleep(150 + Game.Ping, "orb");
                        }
                    }

                    if (blink != null && blink.CanBeCasted() && menuValue.IsEnabled(blink.Name) && me.Distance2D(target) > 500 && me.Distance2D(target) <= 1170 && Utils.SleepCheck("blink"))
                    {
                        blink.UseAbility(target.Position);
                        Utils.Sleep(250 + Game.Ping, "blink");
                    }

                    if (force != null && force.CanBeCasted() && menuValue.IsEnabled(force.Name) && me.Distance2D(target) > 200 && me.Distance2D(target) <= 570 && Utils.SleepCheck("force"))
                    {
                        force.UseAbility(me);
                        Utils.Sleep(250 + Game.Ping, "force");
                    }

                    if (veil != null && veil.CanBeCasted() && !target.IsMagicImmune() && !target.IsIllusion && Utils.SleepCheck("veil") && menuValue.IsEnabled(veil.Name))
                    {
                        veil.UseAbility(target.Position);
                        Utils.Sleep(50 + Game.Ping, "veil");
                    }

                    if (hex != null && hex.IsValid && hex.CanBeCasted() && Utils.SleepCheck("hex") && menuValue.IsEnabled(hex.Name))
                    {
                        hex.CastStun(target);
                        Utils.Sleep(250 + Game.Ping, "hex");
                        return;
                    }

                    if (orchid != null && orchid.IsValid && orchid.CanBeCasted() && Utils.SleepCheck("orchid") && menuValue.IsEnabled(orchid.Name))
                    {
                        orchid.UseAbility(target);
                        Utils.Sleep(250 + Game.Ping, "orchid");
                    }


                    if (thorn != null && thorn.IsValid && thorn.CanBeCasted() && Utils.SleepCheck("thorn") && menuValue.IsEnabled(thorn.Name))
                    {
                        orchid.UseAbility(target);
                        Utils.Sleep(250 + Game.Ping, "thorn");
                    }

                    if (orb == null || orb.CanBeCasted() || !Utils.SleepCheck("orb")
                        || !Menu.Item("orbwalkk").GetValue<bool>() || !(targetDistance <= me.AttackRange))
                    {
                    }
                    else
                    {
                        Orbwalking.Orbwalk(target);
                        Utils.Sleep(Game.Ping + 150, "orb");
                    }

                    if (me.IsAttacking() || !(targetDistance >= me.AttackRange)
                        || !Utils.SleepCheck("follow"))
                    {
                        return;
                    }
                    me.Move(Game.MousePosition);
                    Utils.Sleep(150 + Game.Ping, "follow");
                }
                else if (!orb.CanBeCasted() && Utils.SleepCheck("orb") && target != null && target.UnitState.HasFlag(UnitState.MagicImmune))
                {
                    me.Attack(target);
                    Utils.Sleep(150, "noorb");
                }
                else
                {
                    me.Move(Game.MousePosition);
                }
            }

        }

        public static void Killsteal(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
            {
                return;
            }
            me = ObjectMgr.LocalHero;
            if (me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Obsidian_Destroyer) return;

            var astrallvl = me.Spellbook.SpellW.Level;
            var range = me.Spellbook.SpellW.CastRange;

            if (Utils.SleepCheck("killstealW") && Menu.Item("wks").GetValue<bool>())
            {
                if (astral.CanBeCasted() && me.Mana > astral.ManaCost)
                {
                    var enemy = ObjectMgr.GetEntities<Hero>().Where(e => e.Team != me.Team && e.IsAlive && e.IsVisible && !e.IsIllusion && !e.UnitState.HasFlag(UnitState.MagicImmune) && me.Distance2D(e) < range).ToList();
                    foreach (var v in enemy)
                    {
                        var damage = Math.Floor((wDamage[astrallvl] * (1 - v.MagicDamageResist)) - (v.HealthRegeneration * 5));
                        if (v.Health < damage && me.Distance2D(v) < range)
                        {
                            astral.UseAbility(v);
                            Utils.Sleep(300, "killstealW");
                        }
                    }
                }

            }
        }

        public static void AstralHarras(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
            {
                return;
            }
            me = ObjectMgr.LocalHero;
            if (me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Obsidian_Destroyer) return;

            var range = me.Spellbook.SpellW.CastRange;

            if (Utils.SleepCheck("asHarras") && Menu.Item("whs").GetValue<KeyBind>().Active)
            {
                if (astral.CanBeCasted() && me.Mana > astral.ManaCost)
                {
                    var enemy = ObjectMgr.GetEntities<Hero>().Where(e => e.Team != me.Team && e.IsAlive && e.IsVisible && !e.IsIllusion && !e.UnitState.HasFlag(UnitState.MagicImmune) && me.Distance2D(e) < range).ToList();
                    foreach (var v in enemy)
                    {
                        if (me.Distance2D(v) < range)
                        {
                            astral.UseAbility(v);
                            Utils.Sleep(300, "asHarras");
                        }
                    }
                }

            }
        }


        public static void AutoUlti(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
            {
                return;
            }
            me = ObjectMgr.LocalHero;
            if (me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Obsidian_Destroyer) return;


            var ultLvl = me.Spellbook.SpellR.Level;
            var enemy =
                ObjectMgr.GetEntities<Hero>()
                    .Where(y => y.Team != me.Team && y.IsAlive && y.IsVisible && !y.IsIllusion)
                    .ToList();
            foreach (var z in enemy)
            {
                if (!z.IsVisible || !z.IsAlive) continue;

                var meInt = Math.Floor(me.TotalIntelligence);
                var enemyInt = Math.Floor(z.TotalIntelligence);
                var damage = Math.Floor((ult[ultLvl] * (meInt - enemyInt)) * (1 - z.MagicDamageResist));

                var lens = me.Modifiers.Any(x => x.Name == "modifier_item_aether_lens");

                if (z.NetworkName == "CDOTA_Unit_Hero_Spectre" && z.Spellbook.Spell3.Level > 0)
                {
                    damage =
                        Math.Floor((ult[ultLvl] * (meInt - enemyInt)) *
                                   (1 - (0.10 + z.Spellbook.Spell3.Level * 0.04)) * (1 - z.MagicDamageResist));
                }
                if (z.NetworkName == "CDOTA_Unit_Hero_SkeletonKing" &&
                    z.Spellbook.SpellR.CanBeCasted())
                    damage = 0;
                if (lens) damage = damage * 1.08;
                var rum = z.Modifiers.Any(x => x.Name == "modifier_kunkka_ghost_ship_damage_absorb");
                if (rum) damage = damage * 0.5;
                var mom = z.Modifiers.Any(x => x.Name == "modifier_item_mask_of_madness_berserk");
                if (mom) damage = damage * 1.3;

                if (Menu.Item("autoUlt").GetValue<bool>() && me.IsAlive)
                {
                    if (z == null)
                        return;
                    //Console.WriteLine("D: " + damage);
                    if (eclipse != null && z != null && eclipse.CanBeCasted()
                        && !z.Modifiers.Any(y => y.Name == "modifier_tusk_snowball_movement")
                        && !z.Modifiers.Any(y => y.Name == "modifier_snowball_movement_friendly")
                        && !z.Modifiers.Any(y => y.Name == "modifier_templar_assassin_refraction_absorb")
                        && !z.Modifiers.Any(y => y.Name == "modifier_ember_spirit_flame_guard")
                        &&
                        !z.Modifiers.Any(y => y.Name == "modifier_ember_spirit_sleight_of_fist_caster_invulnerability")
                        &&
                        !z.Modifiers.Any(y => y.Name == "modifier_ember_spirit_sleight_of_fist_caster_invulnerability")
                        && !z.Modifiers.Any(y => y.Name == "modifier_obsidian_destroyer_astral_imprisonment_prison")
                        && !z.Modifiers.Any(y => y.Name == "modifier_puck_phase_shift")
                        && !z.Modifiers.Any(y => y.Name == "modifier_eul_cyclone")
                        && !z.Modifiers.Any(y => y.Name == "modifier_dazzle_shallow_grave")
                        && !z.Modifiers.Any(y => y.Name == "modifier_shadow_demon_disruption")
                        && !z.Modifiers.Any(y => y.Name == "modifier_necrolyte_reapers_scythe")
                        && !z.Modifiers.Any(y => y.Name == "modifier_necrolyte_reapers_scythe")
                        && !z.Modifiers.Any(y => y.Name == "modifier_storm_spirit_ball_lightning")
                        && !z.Modifiers.Any(y => y.Name == "modifier_ember_spirit_fire_remnant")
                        && !z.Modifiers.Any(y => y.Name == "modifier_nyx_assassin_spiked_carapace")
                        && !z.Modifiers.Any(y => y.Name == "modifier_phantom_lancer_doppelwalk_phase")
                        && !z.FindSpell("abaddon_borrowed_time").CanBeCasted() &&
                        !z.Modifiers.Any(y => y.Name == "modifier_abaddon_borrowed_time_damage_redirect")
                        && me.Distance2D(z) <= eclipse.GetCastRange() + 50
                        && !z.IsMagicImmune()
                        && enemy.Count(x => (x.Health - damage) <= 0 && x.Distance2D(z) <= ultRange[eclipse.Level - 1])
                        >= (Menu.Item("Heel").GetValue<Slider>().Value)
                        && enemy.Count(x => x.Distance2D(z) <= ultRange[eclipse.Level - 1])
                        >= (Menu.Item("Heel").GetValue<Slider>().Value)
                        && Utils.SleepCheck(z.Handle.ToString()))
                    {
                        eclipse.UseAbility(z.Position);
                        Utils.Sleep(150, z.Handle.ToString());
                        return;
                    }
                }
            }
        }

        private static void drawhpbar()
        {
            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
            {
                return;
            }

            me = ObjectMgr.LocalHero;
            if (me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Obsidian_Destroyer) return;

            var ultLvl = me.Spellbook.SpellR.Level;
            var enemy =
                ObjectMgr.GetEntities<Hero>()
                    .Where(y => y.Team != me.Team && y.IsAlive && y.IsVisible && !y.IsIllusion)
                    .ToList();

            foreach (var x in enemy)
            {
                //Console.WriteLine(1);
                var health = x.Health;
                var maxHealth = x.MaximumHealth;

                var meInt = Math.Floor(me.TotalIntelligence);
                var enemyInt = Math.Floor(x.TotalIntelligence);
                var damge = Math.Floor((ult[ultLvl] * (meInt - enemyInt)) * (1 - x.MagicDamageResist));
                var hpleft = health;
                var hpperc = hpleft / maxHealth;

                var dmgperc = Math.Min(damge, health) / maxHealth;
                Vector2 hbarpos;
                hbarpos = HUDInfo.GetHPbarPosition(x);

                Vector2 screenPos;
                var enemyPos = x.Position + new Vector3(0, 0, x.HealthBarOffset);
                if (!Drawing.WorldToScreen(enemyPos, out screenPos)) continue;

                var start = screenPos;


                hbarpos.X = start.X + (HUDInfo.GetHPBarSizeX(x) / 2);
                hbarpos.Y = start.Y;
                var hpvarx = hbarpos.X;
                var hpbary = hbarpos.Y;
                float a = (float)Math.Round((damge * HUDInfo.GetHPBarSizeX(x)) / (x.MaximumHealth));
                var position = hbarpos - new Vector2(a, 32 * scaleY);

                //Console.WriteLine("damage" + damge.ToString());

                try
                {
                    float left = (float)Math.Round(damge / 7);
                    Drawing.DrawRect(
                        position,
                        new Vector2(a, (float)(HUDInfo.GetHpBarSizeY(x))),
                        (x.Health > 0) ? new Color(150, 225, 150, 80) : new Color(70, 225, 150, 225));
                    Drawing.DrawRect(position, new Vector2(a, (float)(HUDInfo.GetHpBarSizeY(x))), Color.Black, true);
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.Message);
                }
            }
        }

        private static void DrawUltiDamage(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
            {
                return;
            }

            me = ObjectMgr.LocalHero;
            if (me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Obsidian_Destroyer) return;

            var ultLvl = me.Spellbook.SpellR.Level;
            var enemy =
                ObjectMgr.GetEntities<Hero>()
                    .Where(y => y.Team != me.Team && y.IsAlive && y.IsVisible && !y.IsIllusion)
                    .ToList();

            foreach (var v in enemy)
            {
                if (!v.IsVisible || !v.IsAlive) continue;

                var meInt = Math.Floor(me.TotalIntelligence);
                var enemyInt = Math.Floor(v.TotalIntelligence);
                var damage = Math.Floor((ult[ultLvl] * (meInt - enemyInt)) * (1 - v.MagicDamageResist));
                var dmg = v.Health - damage;
                var canKill = dmg <= 0;

                var screenPos = HUDInfo.GetHPbarPosition(v);
                if (!OnScreen(v.Position)) continue;

                var text = canKill ? "Yes" : "No, damage:" + Math.Floor(damage);
                var size = new Vector2(15, 15);
                var textSize = Drawing.MeasureText(text, "Arial", size, FontFlags.AntiAlias);
                var position = new Vector2(screenPos.X - textSize.X - 2, screenPos.Y - 3);
                Drawing.DrawText(
                    text,
                    position,
                    size,
                    (canKill ? Color.LawnGreen : Color.Red),
                    FontFlags.AntiAlias);
            }
        }

        public static Hero HighestInt(Hero source, float range = 1000)
        {
            var mousePosition = Game.MousePosition;
            return
                Heroes.GetByTeam(source.GetEnemyTeam())
                    .Where(hero => hero.IsValid && hero.IsAlive && hero.IsVisible && hero.Distance2D(mousePosition) <= range)
                    .MaxOrDefault(hero => hero.Intelligence);
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (!Game.IsChatOpen)
            {

                if (Menu.Item("comboKey").GetValue<KeyBind>().Active)
                {
                    Combo = true;
                }
                else
                {
                    Combo = false;
                }
            }
        }

        private static bool OnScreen(Vector3 v)
        {
            return !(Drawing.WorldToScreen(v).X < 0 || Drawing.WorldToScreen(v).X > Drawing.Width || Drawing.WorldToScreen(v).Y < 0 || Drawing.WorldToScreen(v).Y > Drawing.Height);
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Game.IsInGame)
                return;
            if (Menu.Item("hpdraw").GetValue<bool>())
            {
                drawhpbar();
            }

        }

    }
}
