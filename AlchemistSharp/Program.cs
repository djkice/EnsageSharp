using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxeSharp
{
    class Program
    {
        private static readonly Menu Menu = new Menu("AxeSharp", "axesharp", true, "npc_dota_hero_axe", true);

        private const int agroDistance = 300;
        private const int blinkRadius = 1180;
        private const double agroDelay = 0.4;

        private static AbilityToggler useAbility;

        private static Item blink, bladeMail;

        private static bool useitemCheck;
        private static bool useabilityCheck;

        private static Hero me, target, killedTarget;
        private static Ability call, hunger, culling;
        private static ParticleEffect targetParticle;

        static void Main(string[] args)
        {

            Game.OnUpdate += Game_OnUpdate;
            Console.WriteLine("AxeSharp loaded!");

            var useAbilities = new Dictionary<string, bool>
            {
              {"item_blink", true}, {"item_blade_mail", true }
            };

            var useItems = new Dictionary<string, bool>
            {
              {"axe_berserkers_call", true}, {"axe_battle_hunger", true }, { "axe_culling_blade", true }
            };

            Menu.AddItem(new MenuItem("Items", "Use Items:").SetValue(new AbilityToggler(useItems)));
            Menu.AddItem(new MenuItem("Abilities", "Use Abilities:").SetValue(new AbilityToggler(useAbilities)));

            Menu.AddToMainMenu();

        }

        private static void Game_OnUpdate(EventArgs args)
        {
            Hero me = ObjectManager.LocalHero;

            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
                return;

            
            if (me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Axe)
                return;

            if (me == null)
                return;

            if (call == null)
                call = me.Spellbook.SpellQ;

            if (hunger == null)
                hunger = me.Spellbook.SpellW;

            if (culling == null)
                hunger = me.Spellbook.SpellR;

            if (blink == null)
                blink = me.FindItem("item_blink");

            if (blink == null)
                blink = me.FindItem("tem_blade_mail");

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


            //add targetting, combo and orbwalk

            if (culling != null && culling.Level > 0)
            {
                target = GetLowHpHeroInDistance(me, blinkRadius);

                //check for blink
                if (target != null && me.Health > 400 && blink != null && blink.CanBeCasted() && Utils.SleepCheck("blink") && useItem.IsEnabled(blink.Name))
                {
                    if (!useAbilityAndGetResult(blink, "blink", target, true, me))
                    {
                        return;
                    }
                }

                target = GetLowHpHeroInDistance(me, 300);

                //check for ult
                if (target != null && culling != null && (culling.Level > 0) && culling.CanBeCasted() && Utils.SleepCheck("culling"))
                {
                    if (!useAbilityAndGetResult(culling, "culling", target, false, me))
                    {
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
            }

            killedTarget = target;

            target = GetHeroInAgro(me);

            if (target != null && !target.Equals(killedTarget))
            {
                //agro+blade mail combo

                if (!useAbilityAndGetResult(call, "call", null, false, me))
                {
                    return;
                }


                if (call != null)
                {
                    if (!call.CanBeCasted())
                    {
                        if ((call.CooldownLength - call.Cooldown) < 3.2)
                        {
                            if (!useAbilityAndGetResult(bladeMail, "bladeMail", null, false, me))
                            {
                                return;
                            }
                        }
                    }
                }
            }

        }

        private static bool useAbilityAndGetResult(Ability ability, string codeWord, Hero target, bool isPos, Hero me)
        {
            if (ability == null)
            {
                return true;
            }

            if (ability.CanBeCasted() && !ability.IsInAbilityPhase && Utils.SleepCheck(codeWord))
            {
                if (target != null)
                {
                    if (isPos)
                    {

                        ability.UseAbility(target.Position);
                    }
                    else
                    {
                        ability.UseAbility(target);
                    }
                }
                else
                {
                    ability.UseAbility();
                }

                Utils.Sleep(ability.GetCastDelay(me, target, true) * 1000, codeWord);

                if (ability.CanBeCasted())
                {
                    return false;
                }
            }
            return true;
        }

        private static Hero GetLowHpHeroInDistance(Hero me, float maxDistance)
        {
            var enemies = ObjectManager.GetEntities<Hero>()
                    .Where(x => x.IsAlive && !x.IsIllusion && x.Team != me.Team && (getULtDamage(me) > (x.Health - 5)
                    && NotDieFromBladeMail(x, me, getULtDamage(me)))).ToList();

            target = getHeroInDistance(me, enemies, maxDistance);

            return target;
        }

        private static bool NotDieFromBladeMail(Unit enemy, Unit me, double damageDone)
        {
            return !(enemy.Modifiers.FirstOrDefault(modifier => modifier.Name == "modifier_item_blade_mail_reflect") != null
                && me.Health < damageDone);
        }

        private static int getULtDamage(Hero me)
        {
            Item aghanim = me.FindItem("item_ultimate_scepter");

            int[] ultDamage;
            if (aghanim != null)
            {
                ultDamage = new int[3] { 300, 425, 550 };
            }
            else
            {
                ultDamage = new int[3] { 250, 325, 400 };
            }
            var ultLevel = me.Spellbook.SpellR.Level;
            var damage = ultDamage[ultLevel - 1];

            return damage;
        }

        private static Hero GetHeroInAgro(Hero me)
        {

            var enemies = ObjectManager.GetEntities<Hero>()
                    .Where(x => x.IsAlive && !x.IsIllusion && x.Team != me.Team).ToList();

            List<Hero> enemiesForAgro = new List<Hero>();

            foreach (var hero in enemies)
            {
                int targetSpeed = hero.MovementSpeed;

                float distanceBefore = calculateDistance(me, hero);

                double distanceAfter = distanceBefore + targetSpeed * (agroDelay - getTimeToTurn(me, hero));

                //Console.WriteLine("distanceBefore" + distanceBefore);
                //Console.WriteLine("getTimeToTurn(me, hero)" + getTimeToTurn(me, hero));
                //Console.WriteLine("distanceAfter" + distanceAfter);
                if (distanceAfter <= agroDistance && hero.IsAlive)
                {
                    enemiesForAgro.Add(hero);
                }
            }

            target = null;
            if (enemiesForAgro.Count > 0)
            {
                target = getHeroInDistance(me, enemiesForAgro, agroDistance);
            }

            return target;
        }

        private static float calculateDistance(Hero me, Hero target)
        {
            var pos = target.Position;
            var mePosition = me.Position;
            return mePosition.Distance2D(pos);
        }

        private static double getTimeToTurn(Hero me, Hero enemy)
        {
            Vector3 myPos = me.Position;
            Vector3 enemyPos = enemy.Position;

            var difX = myPos.X - enemyPos.X;
            var difY = myPos.Y - enemyPos.Y;
            var degree = Math.Atan2(difY, difX);

            var enemyDirection = Math.Atan2(enemy.Vector2FromPolarAngle().Y, enemy.Vector2FromPolarAngle().X);

            var difDegree = Math.Abs(enemyDirection - degree);
            var turnRate = Game.FindKeyValues(enemy.Name + "/MovementTurnRate", KeyValueSource.Hero).FloatValue;
            var timeToTurn = 0.03 * (Math.PI - difDegree) / turnRate;
            return timeToTurn;
        }

        private static Hero getHeroInDistance(Hero me, List<Hero> enemies, float maxDistance)
        {
            target = null;
            float minDistance = maxDistance;
            foreach (var hero in enemies)
            {
                var distance = me.Distance2D(hero);
                if (distance <= maxDistance && distance <= minDistance)
                {
                    minDistance = distance;
                    target = hero;
                }
            }

            return target;

        }

    }
}
