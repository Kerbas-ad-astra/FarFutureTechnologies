﻿/// ModuleCoreHeat
/// ---------------------------------------------------
/// A version of ModuleCoreHeat that does not do any kind of catchup for
/// its thermal parameters

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI;
using KSP.Localization;

namespace FarFutureTechnologies
{
    public class ModuleFusionCore : ModuleCoreHeat
    {

        public void ZeroThermal()
        {
            base.lastFlux = 0d;
            base.lastUpdateTime = Planetarium.GetUniversalTime();
        }
        public override void OnSave(ConfigNode node)
        {
            lastFlux = 0d;
            base.OnSave(node);
        }
        void Update()
        {
            //base.lastUpdateTime = Planetarium.GetUniversalTime() - 0.5d;
            //Debug.Log(String.Format("lastflux: {0}, {1} thermalE {2}", lastFlux, lastUpdateTime, base.CoreThermalEnergy));
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            //Debug.Log(String.Format("lastflux: {0}, {1} thermalE {2}", lastFlux, GetLastFlux(), base.CoreThermalEnergy));
        }
    }


}
