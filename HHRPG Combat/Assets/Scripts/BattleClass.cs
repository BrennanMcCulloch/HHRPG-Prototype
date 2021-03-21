﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleClass : MonoBehaviour
{ 
    public PartyMemberClass leader;
    private PartyMemberClass[] party;
    public string difficulty;

    private static int maxPartySize = 3;
    public int maxRowSize = 4;
    private static int maxNumberOfRows = 3;

    public EnemyClass[] front;
    public EnemyClass[] mid;
    public EnemyClass[] back;
    private EnemyClass[][] enemies = new EnemyClass[maxNumberOfRows][];

    public PartyMemberClass[] restOfParty = new PartyMemberClass[maxPartySize];

    BattleClass(PartyMemberClass _lead, int _rowSize, PartyMemberClass[] _party, EnemyClass[][] _enemy, string _bpm)
    {
        leader = _lead;
        maxRowSize = _rowSize;
        //enemies = new EnemyClass[maxNumberOfRows, maxRowSize];

        //initializing rest of party array
        for (int x = 0; x < _party.Length; x++)
        {
            if (x >= maxPartySize) { throw new System.Exception("Tried to put too many people in the party!"); }
            restOfParty[x] = _party[x];
        }


        //initializing party array with random assortment besides leader
        party = new PartyMemberClass[_party.Length + 1];
        int currentSlot = 0;
        for (float x = restOfParty.Length; x > 0; x--)
        {
            if (x == restOfParty.Length) { party[currentSlot] = leader; currentSlot++; } //put leader in first
            else if (x == 0) { party[currentSlot] = restOfParty[0]; currentSlot++; } //don't random search for the last entry
            else
            {
                int select = Mathf.RoundToInt(Random.Range(0, x)); //selects a random party member in array
                party[currentSlot] = restOfParty[select]; //slots it in position
                while (select < restOfParty.Length - 1) //moves the rest of the future members up array for future random selection
                {
                    restOfParty[select] = restOfParty[select + 1];
                    select++;
                }
                currentSlot++;
            }
        }

        //initializing enemy 2D array
        for(int x = 0; x < _enemy.Length; x++)
        {
            if (x >= maxNumberOfRows) { throw new System.Exception("Tried to make too many rows!"); }

            for(int y = 0; y < _enemy[x].Length; y++)
            {
                if(y >= maxRowSize) { throw new System.Exception("Tried to put too many enemies in a row!"); }

                enemies[x][y] = _enemy[x][y];
            }
        }

        difficulty = _bpm;
    }

    //Party Phase of battle overview and turn movement
    private void PartyPhase()
    {
        int turn = 0;
        while (turn < party.Length)
        {
            PartyMemberTurn(party[turn], turn);
            turn++;
        }
    }

    //Enemy Phase of battle overview and turn movement
    private void EnemyPhase()
    {
        for(int row = 0; row < maxNumberOfRows; row++)
        {
            for(int col = 0; col < maxRowSize; col++)
            {
                if (enemies[row][col] != null) { EnemyTurn(enemies[row][col], row, col); }
            }
        }

    }

    //Interactivity code for party member turn
    private void PartyMemberTurn(PartyMemberClass person, int leader)
    {
        //PUT INTERACTIVE STUFF HERE
    }

    //Things to do on enemy turn
    private void EnemyTurn(EnemyClass enemy, int row, int col)
    {
        //PUT ENEMY STUFF HERE
        switch(row)
        {
            case 0: //front row
                if(enemy.frontMoves.Length > 0)
                {
                    int whichFront = Random.Range(0, enemy.frontMoves.Length);
                    GameObject doItFront = enemy.frontMoves[whichFront];
                    MoveClass theThingFront = doItFront.GetComponent<MoveClass>();
                    EnemyMove(enemy, theThingFront);
                }
                break;
            case 1: //mid row
                if(enemy.midMoves.Length > 0)
                {
                    int whichMid = Random.Range(0, enemy.midMoves.Length);
                    GameObject doItMid = enemy.midMoves[whichMid];
                    MoveClass theThingMid = doItMid.GetComponent<MoveClass>();
                    EnemyMove(enemy, theThingMid);
                }
                break;
            case 2: //back row
                if(enemy.backMoves.Length > 0)
                {
                    int whichBack = Random.Range(0, enemy.backMoves.Length);
                    GameObject doItBack = enemy.backMoves[whichBack];
                    MoveClass theThingBack = doItBack.GetComponent<MoveClass>();
                    EnemyMove(enemy, theThingBack);
                }
                break;
            default:
                throw new System.Exception("You fell into default in the enemy turn function of battle class");
        }

    }


    //actual enemy attack
    private void EnemyMove(EnemyClass doer, MoveClass move)
    {
        switch(move.type)
        {
            case "Physical": //attack
            case "Drum":
            case "Bass":
            case "Guitar":
            case "Piano":
                //Determine who we're attacking
                PartyMemberClass victim;
                if(difficulty == "Easy")
                {
                    //Pick the worst enemy to attack
                    victim = PickWorstPartyToHit(move.type);
                }
                else if(difficulty == "Medium")
                {
                    //Randomly pick an enemy
                    victim = party[Random.Range(0, maxPartySize + 1)].GetComponent<PartyMemberClass>();
                }
                else if(difficulty == "Hard")
                {
                    //Pick the best enemy to attack
                    victim = PickBestPartyToHit(move.type);
                }
                else
                {
                    throw new System.Exception("No difficulty selected in battle class");
                }

                //Determine attack value
                int d20 = Random.Range(1, 21);
                double percent = d20 * 0.02;
                int statInQuestion;
                string affinityInQuestion;
                int agility;
                victim.affinities.TryGetValue(move.type, out affinityInQuestion);
                victim.stats.TryGetValue("Agility", out agility);
                if(move.type == "Physical")
                {
                    victim.stats.TryGetValue("Physical Defence", out statInQuestion);
                }
                else
                {
                    victim.stats.TryGetValue("Rhythm Defence", out statInQuestion);
                }
                int crit = 1;
                if(d20 >= 19 || affinityInQuestion == "Weak") { crit = 2; }

                //Do they dodge?
                int d100 = Random.Range(1, 101);
                if (d100 <= agility)
                {
                    //DODGE STUFF HERE
                    Debug.Log("Dodge!");
                    break;
                }

                //If not, calculate damage
                double damagePercent = (crit * percent) + 0.6;
                int potentialDamage;
                doer.stats.TryGetValue(move.type, out potentialDamage);
                double damageDealt = damagePercent * potentialDamage;

                int d20def = Random.Range(1, 21);
                double percentDefended = ((d20def * 0.01) + 0.8) * statInQuestion / 100;

                double damageNotRounded = damageDealt * percentDefended;
                if(affinityInQuestion == "Strong") { damageNotRounded = damageNotRounded * 0.5; }
                int damage = (int) System.Math.Round(damageNotRounded);

                //Impact numbers
                if (affinityInQuestion == "Absorb") { victim.currentHealth += damage; }
                else if (affinityInQuestion == "Reflect") {doer.currentHealth -= damage; }//REFLECT (FIX LATER)
                else { victim.currentHealth -= damage; }

                break;
            case "Strength Buff": //Buffs
            case "Rhythm Buff":
            case "Physical Defence Buff":
            case "Rhythm Defence Buff":
            case "Agility Buff":
            case "Potential Buff":

                break;
            case "Strength Debuff": //Debuffs
            case "Rhythm Debuff":
            case "Physical Defence Debuff":
            case "Rhythm Defence Debuff":
            case "Agility Debuff":
            case "Potential Debuff":

                break;
            case "Heal": //Healing.... obv

                break;
            default:
                throw new System.Exception("Fell into default in enemymove method in battle class");
        }
    }

    public struct data
    {
        public string affinity { get; set; }
        public float healthPercentage { get; set; }
        public bool currentlyGuarding { get; set; }
        public int points { get; set; }
    }

    //AI STUFF
    private PartyMemberClass PickWorstPartyToHit(string type)
    {
        data[] information = new data[party.Length];
        PartyMemberClass victim;

        //Establish AI data
        for(int x = 0; x < party.Length; x++)
        {
            string temp;
            int maxHealth;
            party[x].affinities.TryGetValue(type, out temp);
            party[x].stats.TryGetValue("HP", out maxHealth);
            information[x].affinity = temp;
            information[x].healthPercentage = party[x].currentHealth / maxHealth;
            information[x].currentlyGuarding = party[x].currentlyGuarding;
        }

        //Impact AI point values 
        for(int x = 0; x < information.Length; x++)
        {
            //affecting points to pick the worst one
            if (information[x].affinity == "Weak") { information[x].points += 3; }
            else if (information[x].affinity == "Strong") { information[x].points -= 1; }
            else if (information[x].affinity == "Absorb" || information[x].affinity == "Reflect") { information[x].points -= 2; }

            if (information[x].healthPercentage < 0.5) { information[x].points += 2; }

            if (information[x].currentlyGuarding == true) { information[x].points -= 2; }
            else { information[x].points += 3; }
        }

        //pick WORST one
        int lowestPoint = 100;
        int lowestPerson = -1;
        for(int x = 0; x < party.Length; x++)
        {
            if(information[x].points <= lowestPoint)
            {
                lowestPoint = information[x].points;
                lowestPerson = x;
            }
        }

        victim = party[lowestPerson];
        return victim;
    }

    private PartyMemberClass PickBestPartyToHit(string type)
    {
        data[] information = new data[party.Length];
        PartyMemberClass victim;

        //Establish AI data
        for (int x = 0; x < party.Length; x++)
        {
            string temp;
            int maxHealth;
            party[x].affinities.TryGetValue(type, out temp);
            party[x].stats.TryGetValue("HP", out maxHealth);
            information[x].affinity = temp;
            information[x].healthPercentage = party[x].currentHealth / maxHealth;
            information[x].currentlyGuarding = party[x].currentlyGuarding;
        }

        //Impact AI point values 
        for (int x = 0; x < information.Length; x++)
        {
            //affecting points to pick the worst one
            if (information[x].affinity == "Weak") { information[x].points += 3; }
            else if (information[x].affinity == "Strong") { information[x].points -= 1; }
            else if (information[x].affinity == "Absorb" || information[x].affinity == "Reflect") { information[x].points -= 2; }

            if (information[x].healthPercentage < 0.5) { information[x].points += 2; }

            if (information[x].currentlyGuarding == true) { information[x].points -= 2; }
            else { information[x].points += 3; }
        }

        //pick BEST one
        int highestPoint = -100;
        int highestPerson = -1;
        for (int x = 0; x < party.Length; x++)
        {
            if (information[x].points > highestPoint)
            {
                highestPoint = information[x].points;
                highestPerson = x;
            }
        }

        victim = party[highestPerson];
        return victim;
    }

    // Start is called before the first frame update
    void Start()
    {
        enemies[0] = front;
        enemies[1] = mid;
        enemies[2] = back;
    }

    // Update is called once per frame
    // LINK BETWEEN PARTY AND ENEMY PHASE SHOULD BE HERE
    void Update()
    {
        
    }


}
