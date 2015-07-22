﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Pulsar4X.ECSLib
{
    [StaticDataAttribute(false)]
    public struct AtmosphericGasSD
    {
        public string Name;
        public string ChemicalSymbol;
        public bool IsToxic;
        public double BoilingPoint;
        public double MeltingPoint;

        /// <summary>
        /// A value representing the Greenhouse effect this gas has (if any).
        ///  0 = Inert/No Effect
        ///  A negative number would be an Anti-Greenhouse gas.
        ///  A positive Number would be a Greenhouse gas.
        ///  The Magnitude of the number could be used to have different gases have a greater or lesser greenhouse effect.
        /// </summary>
        public double GreenhouseEffect;

    }
}