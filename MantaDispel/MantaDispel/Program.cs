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


    // version 1.2 addition of non-hero debuffs
    // version 1.3 dispell projectils (wait for Ensage fix for prejectile calculation)
    internal class Program
    {
        private static Item mantaItem;

        private static Hero me;

        private static readonly Menu Menu = new Menu("MantaDispel", "MantaDispel", true, "item_manta", true);

        static void Main(string[] args)
        {
            Game.OnUpdate += Game_OnUpdate;

            Menu.AddItem(new MenuItem("dispelITog", "Use Manta to Dispel(Items)").SetValue(true));
            Menu.AddItem(new MenuItem("dispelSTog", "Use Manta to Dispel(Spells)").SetValue(true));
            Menu.AddToMainMenu();
        }

        public static void Game_OnUpdate(EventArgs args)
        {
            var dispelBuffs = new List<string>
            {
                "modifier_item_diffusal_blade_slow", "modifier_bloodthorn_debuff", "modifier_desolator_buff", "modifier_item_dustofappearance", "modifier_item_ethereal_blade_slow", "modifier_bloodthorn_debuff", "modifier_desolator_buff", "modifier_rod_of_atos_debuff", "modifier_orchid_malevolence_debuff", "modifier_item_shivas_guard_blast", "modifier_item_solar_crest_armor_reduction", "modifier_item_urn_of_shadows", "modifier_item_veil_of_discord_debuff"
            };
            var dispelSpells = new List<string>
            {
              "modifier_skeleton_king_hellfire_blast", "modifier_skeleton_king_reincarnate_slow", "modifier_warlock_upheaval", "modifier_warlock_shadow_word", "modifier_visage_grave_chill_debuff", "modifier_viper_corrosive_skin_slow", "modifier_viper_poison_attack_slow", "modifier_vengefulspirit_wave_of_terror", "modifier_ursa_earthshock", "modifier_tusk_walrus_punch_slow", "modifier_tusk_walrus_kick_slow", "modifier_troll_warlord_whirling_axes_slow", "modifier_treant_leech_seed", "modifier_tinker_laser_blind", "modifier_shredder_chakram_debuff", "modifier_shredder_whirling_death_debuff", "modifier_tidehunter_anchor_smash", "modifier_tidehunter_gush", "modifier_terrorblade_reflection_slow", "modifier_templar_assassin_trap_slow", "modifier_templar_assassin_meld_armor", "modifier_storm_spirit_overload_debuff", "modifier_sniper_headshot", "modifier_slardar_amplify_damage", "modifier_slithereen_crush", "modifier_skywrath_mage_ancient_seal", "modifier_skywrath_mage_concussive_shot_slow", "modifier_silencer_global_silence", "modifier_silencer_last_word", "modifier_silencer_last_word_disarm", "modifier_silencer_curse_of_the_silent", "modifier_nevermore_requiem", "modifier_shadow_demon_purge_slow", "modifier_shadow_demon_shadow_poison", "modifier_sand_king_epicenter_slow", "modifier_sand_king_caustic_finale", "modifier_sand_king_caustic_finale_orb", "modifier_sandking_caustic_finale_orb", "modifier_rubick_fade_bolt_debuff", "modifier_razor_unstablecurrent_slow", "modifier_queenofpain_shadow_strike", "modifier_pugna_decrepify", "modifier_phoenix_fire_spirit_burn", "modifier_phantom_lancer_spirit_lance", "modifier_phantom_assassin_stiflingdagger", "modifier_oracle_fortunes_end_purge", "modifier_oracle_fates_edict", "modifier_ogre_magi_ignite", "modifier_night_stalker_crippling_fear", "modifier_night_stalker_void", "modifier_furion_wrath_of_nature_thinker", "modifier_naga_siren_rip_tide", "modifier_naga_siren_riptide", "modifier_naga_siren_ensnare", "modifier_meepo_geostrike_debuff", "modifier_meepo_earthbind", "modifier_magnataur_skewer_slow", "modifier_lion_voodoo", "modifier_life_stealer_open_wounds", "modifier_lich_frostarmor_slow", "modifier_lich_chain_frost_thinker", "modifier_lich_slow", "modifier_leshrac_lightning_storm_slow", "modifier_kunkka_ghost_ship_movespeed", "modifier_kunkka_torrent_slow", "modifier_keeper_of_the_light_blinding_light", "modifier_keeper_of_the_light_mana_leak", "modifier_jakiro_liquid_fire_burn", "modifier_jakiro_dual_breath_slow", "modifier_wisp_tether_slow", "modifier_invoker_tornado", "modifier_invoker_chaos_meteor_burn", "modifier_invoker_cold_snap", "modifier_huskar_life_break_slow", "modifier_gyrocopter_call_down_slow", "modifier_faceless_void_timelock_freeze", "modifier_enchantress_enchant_slow", "modifier_enigma_malefice", "modifier_cold_feet", "modifier_axe_battle_hunger", "modifier_batrider_sticky_napalm", "modifier_beastmaster_primal_roar_slow", "modifier_bloodseeker_bloodrage", "modifier_bounty_hunter_jinada_slow", "modifier_brewmaster_thunder_clap", "modifier_brewmaster_drunken_haze", "modifier_bristleback_viscous_nasal_goo", "modifier_broodmother_spawn_spiderite_debuff", "modifier_broodmother_poison_sting_debuff", "modifier_chaos_knight_chaos_strike", "modifier_chen_penitence", "modifier_crystal_maiden_frostbite_ministun", "modifier_crystal_maiden_crystal_nova", "modifier_dazzle_poison_touch", "modifier_disruptor_thunder_strike", "modifier_dragon_knight_frost_breath_slow", "modifier_dragon_knight_corrosive_breath", "modifier_dragon_knight_splash_attack", "modifier_drow_ranger_frost_arrows_slow", "modifier_earth_spirit_rolling_boulder_slow", "modifier_earth_spirit_geomagnetic_grip", "modifier_earth_spirit_magnetize", "modifier_elder_titan_earth_splitter", "modifier_ember_spirit_searing_chains", "modifier_enchantress_untouchable_slow"
            };

            me = ObjectManager.LocalHero;

            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
                return;

            if (me == null)
                return;

            if (mantaItem == null)
                mantaItem = me.FindItem("item_manta");

            foreach (var dispIModif in dispelBuffs)
            {
                var hasModifier = Program.me.FindModifier(dispIModif);

                if (hasModifier != null)
                {

                    if (mantaItem != null && mantaItem.CanBeCasted() && Utils.SleepCheck("manta") && Menu.Item("dispelITog").GetValue<bool>())
                    {
                        mantaItem.UseAbility();
                        Utils.Sleep(150 + Game.Ping, "mantaItem");
                    }
                }

            }

            foreach (var dispSModif in dispelSpells)
            {
                var hasModifier = Program.me.FindModifier(dispSModif);

                if (hasModifier != null)
                {

                    if (mantaItem != null && mantaItem.CanBeCasted() && Utils.SleepCheck("manta") &&
                        Menu.Item("dispelSTog").GetValue<bool>())
                    {
                        mantaItem.UseAbility();
                        Utils.Sleep(150 + Game.Ping, "mantaItem");
                    }
                }
            }
        }

    }
}

