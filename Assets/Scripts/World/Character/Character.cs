﻿using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

public class Character
{
    public readonly Race race;
    public Characteristics characteristics;
    public Name name;
    public int age = 20;
    public int birthDate = 1;

    public Image pawn;
    public Region position;

    float health = 100f;

    public Character(Name _name, Race _race, int _birthDate = 0, int _age = 0) : this(_race)
    {
        birthDate = _birthDate > 0 ? _birthDate : birthDate;
        age = _age > 0 ? _age : age;
        name = _name;
    }
    
    public Character(Race race) {
        this.race = race;
        LoadCharacteristicsDefinitions();
    }

    public static Character CreateCharacter()
    {
        var race = Library.races[Library.races.Keys.ToList().PickRandom(Game.state.random)];
        var chara = new Character(race);
        
        chara.birthDate = Game.state.random.Range(1, Rules.set[RULE.DAYS_IN_YEAR].GetInt());
        chara.age = Game.state.random.Range(
            race.rulerCreationRules.majority,
            KMaths.RoundToInt(Rules.set[RULE.LIFESPAN_MULTIPLIER].GetFloat() * race.rulerCreationRules.maximumLifespan)
        );

        chara.name = race.GetRandomCharacterName();

        return chara;
    }
    
    void LoadCharacteristicsDefinitions()
    {
        characteristics = new Characteristics();

        foreach(var def in race.rulerCreationRules.characteristicDefinitions.Keys)
        {
            characteristics.Add(def, new Characteristic(race.rulerCreationRules.characteristicDefinitions[def]));
        }

    }

    public class Name {
        public string firstName;
        public string lastName;
        public string preTitle = "";
        string postTitle = "";
        string nameFormat = "{0} {1}";

        public Name(string firstName, string lastName, Race race)
        {
            this.firstName = firstName;
            this.lastName = lastName;
            this.nameFormat = race.characterNameFormat;
        }

        public void SetPostTitle(string postTitle)
        {
            this.postTitle = postTitle;
        }

        public string GetFullName()
        {
            return preTitle+" "+string.Format(nameFormat, firstName, lastName)+" "+postTitle;
        }

        public string GetShortName()
        {
            return string.Format(nameFormat, firstName, lastName);
        }

        public string GetPreTitle()
        {
            return string.Format(preTitle, "");
        }

        public override string ToString()
        {
            return "[NAME:"+GetShortName()+"]";
        }
    }

    [System.Serializable]
    public class CreationRules
    {
         public CharacteristicDefinitions characteristicDefinitions = new CharacteristicDefinitions();
        public int maximumLifespan = 60;
        public int majority = 16;
    }

    [System.Serializable]
    // Varying characteristic
    public class Characteristic
    {
        int value;
         public readonly CharacteristicDefinition definition;

        public Characteristic(CharacteristicDefinition _definition)
        {
            definition = _definition;
            value = definition.min;
        }

        public void Increase(Characteristics otherChars, int amount=1)
        {
            var oldVal = value;
            value = KMaths.Clamp(value + amount, definition.min, definition.max);
            definition.rules.onChange(otherChars, value - oldVal);
        }

        public void Decrease(Characteristics otherChars, int amount = 1)
        {
            var oldVal = value;
            value = KMaths.Clamp(value - amount, definition.min, definition.max);
            definition.rules.onChange(otherChars, value - oldVal);
        }

        public void SetRaw(int val)
        {
            value = val;
        }

        public int GetValue()
        {
            return value;
        }

        public int GetClampedValue()
        {
            return KMaths.Clamp(value, definition.min, definition.max);
        }
    }

    // "set-in-stone" definition, rules, chara behavior
    [System.Serializable]
    public class CharacteristicDefinition
    {
        public int min = 1;
        public int max = 10;
        public int cost = 1;
        public Rules rules = new Rules();

        [System.Serializable]
        public class Rules
        {
            // <current characteristics, concerned chara, amount of change>
            public System.Action<Characteristics, int> onChange;
            public bool isFrozen = false;   // The player cannot change this variable value
            public bool isBad = false; // The characteristic is considered to be a negative trait
        }
    }

    // static set of varying characteristics
    [System.Serializable]
    public class Characteristics : Dictionary<string, Characteristic> { }

    // static definitions set
    [System.Serializable]
    public class CharacteristicDefinitions : Dictionary<string, CharacteristicDefinition> { }

}
