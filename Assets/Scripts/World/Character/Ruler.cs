﻿using System.Collections;
using System.Linq;
using System.Collections.Generic;
using static GameLogger;

public class Ruler : Character
{
    public class NoKingdomException : System.Exception { public NoKingdomException(string message) : base(message) { logger.Error(message); } }

    public Brain brain;

    public Ruler(Name name, Race race, int birthDate = 0, int age = 0) : base (name, race, birthDate, age)
    {
        name.preTitle = race.rulerTitle;
        Game.players.localPlayer.Own(this);
    }

    Ruler(Race race) : base(race) {}

    public static Ruler CreateRuler()
    {
        var chara = CreateCharacter();
        var r = new Ruler(chara.race);
        r.name = chara.name;
        r.name.preTitle = chara.race.rulerTitle;
        r.age = chara.age;
        r.birthDate = chara.birthDate;

        r.pawn = chara.race.pawn;

        new AI().Own(r);

        return r;
    }

    [System.Serializable]
    public new class CreationRules : Character.CreationRules
    {
        public int stock = 15;
        public float lifespanToStockRatio = 0.8f;
        public int maxStartingAge;
    }

    public Kingdom GetOwnedKingdom(Map map)
    {
        foreach(var kingdom in map.world.kingdoms) {
            if (kingdom.ruler == this) {
                return kingdom;
            }
        }

        throw new NoKingdomException("The ruler "+name+":"+GetHashCode()+" owns no kingdom! This should not happen");
    }
}
