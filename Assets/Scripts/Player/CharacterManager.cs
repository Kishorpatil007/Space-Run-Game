using System;
using UnityEngine;
using SpaceJet.Core;

namespace SpaceJet.Player
{
    [System.Serializable]
    public class CharacterDefinition
    {
        public int id;
        public string pilotName;
        public string callsign;
        public string trait;
        public string perkDescription;
        public Color pilotColor;
        public int unlockCost;
        public float speedBonus;       // e.g. +15%
        public float shieldBonus;      // e.g. +2.5s duration
        public float energyBonus;      // e.g. +20% fuel efficiency
        public float magnetBonus;      // e.g. +3.5m magnet radius
    }

    /// <summary>
    /// Manages pilot characters and their gameplay perks.
    /// Simple, punchy, and engaging without complex clutter.
    /// </summary>
    public static class CharacterManager
    {
        public static event Action<int> OnCharacterEquipped;

        public static readonly CharacterDefinition[] Characters = new CharacterDefinition[]
        {
            new CharacterDefinition
            {
                id = 0,
                pilotName = "Nova Vance",
                callsign = "APEX",
                trait = "Speed Specialist",
                perkDescription = "✦ +15% Flight Speed",
                pilotColor = new Color(0.1f, 0.9f, 1f),
                unlockCost = 0,
                speedBonus = 0.15f,
                shieldBonus = 0f,
                energyBonus = 0f,
                magnetBonus = 0f
            },
            new CharacterDefinition
            {
                id = 1,
                pilotName = "Kaelen Voss",
                callsign = "TITAN",
                trait = "Shield Guardian",
                perkDescription = "✦ +25% Shield Duration",
                pilotColor = new Color(1f, 0.65f, 0.15f),
                unlockCost = 250,
                speedBonus = 0f,
                shieldBonus = 2.5f,
                energyBonus = 0f,
                magnetBonus = 0f
            },
            new CharacterDefinition
            {
                id = 2,
                pilotName = "Lyra Chen",
                callsign = "SOLARIS",
                trait = "Energy Engineer",
                perkDescription = "✦ +20% Fuel Saver",
                pilotColor = new Color(0.15f, 1f, 0.55f),
                unlockCost = 500,
                speedBonus = 0f,
                shieldBonus = 0f,
                energyBonus = 0.20f,
                magnetBonus = 0f
            },
            new CharacterDefinition
            {
                id = 3,
                pilotName = "Zane Rayner",
                callsign = "MAGNETO",
                trait = "Coin Magnet",
                perkDescription = "✦ +50% Magnet Range",
                pilotColor = new Color(0.85f, 0.35f, 1f),
                unlockCost = 750,
                speedBonus = 0f,
                shieldBonus = 0f,
                energyBonus = 0f,
                magnetBonus = 3.5f
            }
        };

        public static CharacterDefinition GetActiveCharacter()
        {
            int index = Mathf.Clamp(SaveManager.SelectedCharacterIndex, 0, Characters.Length - 1);
            return Characters[index];
        }

        public static CharacterDefinition GetCharacter(int index)
        {
            index = Mathf.Clamp(index, 0, Characters.Length - 1);
            return Characters[index];
        }

        public static bool EquipCharacter(int index)
        {
            if (index < 0 || index >= Characters.Length) return false;
            if (!IsCharacterUnlocked(index)) return false;

            SaveManager.SelectedCharacterIndex = index;
            OnCharacterEquipped?.Invoke(index);
            return true;
        }

        public static bool IsCharacterUnlocked(int index)
        {
            if (index <= 0) return true;
            return SaveManager.IsCharacterUnlocked(index);
        }

        public static bool UnlockCharacter(int index)
        {
            if (index < 0 || index >= Characters.Length) return false;
            if (IsCharacterUnlocked(index)) return true;

            int cost = Characters[index].unlockCost;
            if (SaveManager.SpendCoins(cost))
            {
                SaveManager.UnlockCharacter(index);
                EquipCharacter(index);
                return true;
            }
            return false;
        }

        public static float GetActiveSpeedMultiplier()
        {
            return 1f + GetActiveCharacter().speedBonus;
        }

        public static float GetActiveShieldBonus()
        {
            return GetActiveCharacter().shieldBonus;
        }

        public static float GetActiveEnergyDrainMultiplier()
        {
            return 1f - GetActiveCharacter().energyBonus;
        }

        public static float GetActiveMagnetBonus()
        {
            return GetActiveCharacter().magnetBonus;
        }
    }
}
