﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FarFutureTechnologies
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class AntimatterFactory : MonoBehaviour
    {
        public static AntimatterFactory Instance { get; private set; }

        public int FactoryLevel { get { return factoryLevel; } }
        public bool Researched { get { return researched; } }
        public double Antimatter { get { return curAntimatter; } }
        public double AntimatterRate { get { return curAntimatterRate; } }
        public double AntimatterMax { get { return maxAntimatter; } }
        public double DeferredAntimatterAmount { get { return deferredAntimatterAmount; } }

        private bool productionOn = false;
        private bool researched = false;

        private int factoryLevel = 0;

        private double curAntimatter = 0d;
        private double maxAntimatter = 0d;
        private double curAntimatterRate = 0d;
        private double deferredAntimatterAmount = 0d;

        private AntimatterFactoryLevelData curLevelDat;

        private double lastUpdateTime = 0d;

        public bool IsMaxLevel()
        {
            if (factoryLevel >= FarFutureTechnologySettings.factoryLevels.Count-1)
                return true;
            return false;
        }
        public float GetNextLevelCost()
        {
            if (factoryLevel >= FarFutureTechnologySettings.factoryLevels.Count - 1)
                return 0f;
            else
                return (float)FarFutureTechnologySettings.GetAMFactoryLevelData(factoryLevel + 1).purchaseCost;
        }
        public string GetStatusString()
        {
            if (productionOn)
                return "Fully Operational";
            else
                return "Not Functional";
        }
        public void SetProductionStatus(bool status)
        {
            productionOn = status;
        }
        public void ToggleProduction()
        {
            productionOn = !productionOn;
        }
        public void Upgrade()
        {
            curLevelDat = FarFutureTechnologySettings.GetAMFactoryLevelData(factoryLevel+1);
            factoryLevel = factoryLevel + 1;
            maxAntimatter = curLevelDat.maxCapacity;
            curAntimatterRate = curLevelDat.baseRate;
        }
        void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            double worldTime = Planetarium.GetUniversalTime();

            if (worldTime - lastUpdateTime > 0d)
            {
                Utils.Log(String.Format("Delta time of {0} detected, catching up", worldTime-lastUpdateTime));
                // update storage to reflect delta
                //CatchupProduction(worldTime - lastUpdateTime);
                lastUpdateTime = worldTime;
            }
            
        }

        public void Initialize(int loadedLevel, double loadedStorage, double deferredConsumption)
        {
            factoryLevel = loadedLevel;
            curAntimatter = loadedStorage;
            deferredAntimatterAmount = deferredConsumption;

            // If game mode is sandbox, set the level to max immediately and begin production
            if (HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX)
            {
                Utils.Log("Detected Sandbox, setting AM factory to max and activating");
                researched = true;
                factoryLevel = FarFutureTechnologySettings.factoryLevels.Count - 1;
                SetProductionStatus(true);
            }
            // If science sandbox, check for the needed technology first
            else if (HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX)
            {
                Utils.Log("Detected Science Sandbox, setting AM factory to max, detecting tech");
                bool isResearched = Utils.CheckTechPresence(FarFutureTechnologySettings.antimatterFactoryUnlockTech);
                if (isResearched)
                {
                    researched = true;
                    factoryLevel = FarFutureTechnologySettings.factoryLevels.Count - 1;
                    SetProductionStatus(true);
                }
                else
                {
                    researched = false;
                    SetProductionStatus(false);
                }

            }
            else
            {
                Utils.Log("Detected Career, setting AM factory to stored level, detecting tech");
                bool isResearched = Utils.CheckTechPresence(FarFutureTechnologySettings.antimatterFactoryUnlockTech);
                if (isResearched)
                {
                    researched = true;
                    factoryLevel = loadedLevel;
                    SetProductionStatus(true);
                }
                else
                {
                    researched = false;
                    SetProductionStatus(false);
                }
            }

            Utils.Log("Completed data load, setting up factory for level "+ factoryLevel.ToString());
            curLevelDat =  FarFutureTechnologySettings.GetAMFactoryLevelData(factoryLevel);

            maxAntimatter = curLevelDat.maxCapacity;
            curAntimatterRate = curLevelDat.baseRate;

            if (curAntimatter > maxAntimatter)
            {
                curAntimatter = maxAntimatter;
            }

            if (HighLogic.LoadedSceneIsFlight && DeferredAntimatterAmount > 0d)
            {
                ConsumeAntimatter(deferredAntimatterAmount);
                deferredAntimatterAmount = 0d;
            }
        }

        public void ScheduleConsumeAntimatter(double amt)
        {
            deferredAntimatterAmount = amt;
        }

        public void ConsumeAntimatter(double amt)
        {
            curAntimatter = curAntimatter - amt;
            if (curAntimatter < 0d)
            {
                curAntimatter = 0d;
            }
             
        }

        void CatchupProduction(double elapsed)
        {
            curAntimatter = curAntimatter + curAntimatterRate * elapsed;
            if (curAntimatter > maxAntimatter)
            {
                curAntimatter = maxAntimatter;
            }
        }

        void FixedUpdate()
        {
            bool isTechReady = Utils.CheckTechPresence(FarFutureTechnologySettings.antimatterFactoryUnlockTech);

            if (isTechReady && !researched)
            {
                researched = true;
                SetProductionStatus(true);
            }

            if (productionOn)
            {
                curAntimatter = curAntimatter + ConvertRate( curAntimatterRate) * TimeWarp.fixedDeltaTime;
                if (curAntimatter > maxAntimatter)
                {
                    curAntimatter = maxAntimatter;
                }
            }
        }

        double ConvertRate(double rateDays)
        {
            double rateSeconds = 0d;
            if (GameSettings.KERBIN_TIME)
            {
                rateSeconds = rateDays / 21600d;
            }
            else
            {
                rateSeconds = rateDays / 86400d;
            }
            return rateSeconds;
        }

    }
}
