﻿using System;
using System.Collections.Generic;
using Game.Display;
using Game.Engine.Monsters;
using Game.Engine.Skills;
using Game.Engine.Interactions;

namespace Game.Engine
{
    // class representing a battle event
    class Battle : ImageInteraction
    {
        protected BattleScene battleScene;
        protected int hpCopy, strCopy, armCopy, prCopy, mgCopy, staCopy; // after the battle, all statistics of the player are restored
        public Monster Monster { get; set; }
        public Battle(GameSession ses, BattleScene scene, Monster monster) : base("battle0001", ses)
        {
            Monster = monster;
            battleScene = scene;
            battleScene.ImgSetup = GetImage();
        }
        protected override void RunContent()
        {
            parentSession.SendText("\nBattle!");
            battleScene.SetupDisplay();
            CopyPlayerState();
            // battle
            if (Monster.BattleGreetings != null)
            {
                battleScene.SendColorText(Monster.BattleGreetings, "red");
                battleScene.SendBattleText("");
            }
            battleScene.SetSkills(parentSession.currentPlayer.ListAvailableSkills());
            while (Monster.Health > 0) // reminder: there will be a separate mechanism for what happens when Player.Health == 0 
            {
                if(parentSession.currentPlayer.ListAvailableSkills().Count == 0) // player has run out of stamina
                {
                    RestorePlayerState();
                    battleScene.SendColorText("Defeat!", "red");
                    parentSession.Wait(300);
                    parentSession.SendText("You lost!");
                    battleScene.EndDisplay();
                    return;
                }
                // monster always attacks first
                List<StatPackage> monsterAttack = parentSession.ModifyDefensive(Monster.BattleMove());
                foreach (StatPackage i in monsterAttack) battleScene.SendColorText(i.CustomText, "red");
                parentSession.currentPlayer.React(monsterAttack);
                battleScene.RefreshStats();
                //ReportStats();
                battleScene.SendBattleText("");
                // now the player
                Skill playerResponse = parentSession.GetListBoxResponse();
                List<StatPackage> playerAttack = parentSession.ModifyOffensive(playerResponse.BattleMove(parentSession.currentPlayer));
                foreach (StatPackage i in playerAttack) battleScene.SendColorText(i.CustomText, "green");
                Monster.React(playerAttack);
                battleScene.RefreshStats();
                parentSession.UpdateStat(6, -1*playerResponse.StaminaCost);
                battleScene.SetSkills(parentSession.currentPlayer.ListAvailableSkills());
                battleScene.ResetChoice();
                //ReportStats();  
            }
            // restore player state
            RestorePlayerState();
            battleScene.SendColorText("Victory!", "green");
            parentSession.Wait(300);
            battleScene.EndDisplay();
            parentSession.SendText("You won! XP gained: " + Monster.XPValue);
            //parentSession.UpdateStat(7, Monster.XPValue); // for smoother display, this one was moved to GameSession.cs
        }
        protected void CopyPlayerState()
        {
            // not very pretty, but I can't think of another solution that wouldn't make things more complicated
            hpCopy = parentSession.currentPlayer.Health - parentSession.currentPlayer.HealthBuff;
            strCopy = parentSession.currentPlayer.Strength - parentSession.currentPlayer.StrengthBuff;
            armCopy = parentSession.currentPlayer.Armor - parentSession.currentPlayer.ArmorBuff;
            prCopy = parentSession.currentPlayer.Precision - parentSession.currentPlayer.PrecisionBuff;
            mgCopy = parentSession.currentPlayer.Precision - parentSession.currentPlayer.PrecisionBuff;
            staCopy = parentSession.currentPlayer.Stamina - parentSession.currentPlayer.StaminaBuff;
        }
        protected void RestorePlayerState()
        {
            parentSession.currentPlayer.Health = hpCopy;
            parentSession.currentPlayer.Strength = strCopy;
            parentSession.currentPlayer.Armor = armCopy;
            parentSession.currentPlayer.Precision = prCopy;
            parentSession.currentPlayer.MagicPower = mgCopy;
            parentSession.currentPlayer.Stamina = staCopy;
            parentSession.RefreshStats();
        }

        protected void ReportStats()
        {
            battleScene.SendColorText("Your HP: " + parentSession.currentPlayer.Health + " Your Stamina: " + parentSession.currentPlayer.Stamina, "blue");
            battleScene.SendColorText("Monster HP: " + Monster.Health, "blue");
        }

    }
}