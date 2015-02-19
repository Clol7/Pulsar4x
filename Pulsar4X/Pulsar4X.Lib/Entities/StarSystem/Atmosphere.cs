﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pulsar4X.Entities
{
    
    /// <summary>
    /// The Atmosphere of a Planet or Moon.
    /// @todo Make this a generic component.
    /// @todo Add a ToString overload which will retunr a string like this: 
    /// "75% Nitrogen (N), 21% Oxygen (O), 3% Carbon dioxide (CO2), 1% Argon (Ar)"
    /// </summary>
    public class Atmosphere
    {
        /// <summary>
        /// Atmospheric Presure
        /// In Earth Atmospheres (atm).
        /// </summary>
        public float Pressure { get; set; }
        
        /// <summary>
        /// Weather or not the planet has abundent water.
        /// </summary>
        public bool Hydrosphere { get; set; }

        /// <summary>
        /// The percentage of the bodies sureface covered by water.
        /// </summary>
        public short HydrosphereExtent { get; set; }

        /// <summary>
        /// A measure of the greenhouse factor provided by this Atmosphere.
        /// </summary>
        public float GreenhouseFactor { get; set; }

        /// <summary>
        /// How much light the body reflects. Affects temp.
        /// a number from 0 to 1.
        /// </summary>
        public float Albedo { get; set; }

        /// <summary>
        /// Temperature of the planet AFTER greenhouse effects are taken into considuration. 
        /// This is a factor of the base temp and Green House effects.
        /// In Degrees C.
        /// </summary>
        public float SurfaceTemperature { get; set; }

        private Dictionary<AtmosphericGas, float> _composition = new Dictionary<AtmosphericGas, float>();
        /// <summary>
        /// The composition of the atmosphere, i.e. what gases make it up and in what ammounts.
        /// In Earth Atmospheres (atm).
        /// </summary>
        public Dictionary<AtmosphericGas, float> Composition { get { return _composition; } }

        /// <summary>
        /// Returns true if The atmosphere exists (i.e. there are any gases in it), else it return false.
        /// </summary>
        public bool Exists 
        {
            get
            {
                if (_composition.Count > 0)
                    return true;

                return false;
            } 
        }

        public bool CanModify
        {
            get
            {
                if (ParentBody.Type == Planet.PlanetType.Terrestrial
                    || ParentBody.Type == Planet.PlanetType.Moon)
                    return true;  // only these bodies have atmospheres that can be terraformed.

                return false;
            }
        }

        private Planet _parentBody;

        /// <summary>
        /// The body this atmosphere belong to.
        /// </summary>
        public Planet ParentBody { get { return _parentBody; } }

        public Atmosphere(Planet parentBody)
        {
            _parentBody = parentBody;
        }

        /// <summary>
        /// Updates the state of the bodies atmosphere. Run this after adding removing gasses or modifing albedo.
        /// </summary>
        public void UpdateState()
        {
            if (Exists)
            {
                if (ParentBody.Type == Planet.PlanetType.GasDwarf 
                    || ParentBody.Type == Planet.PlanetType.GasGiant
                    || ParentBody.Type == Planet.PlanetType.IceGiant)
                {
                    Pressure = 1;       // because thats the deffenition of the surface of these planets, when 
                    // atmosphereic pressure = the pressure of earths atmosphere at its surface (what we call 1 atm).
                }

                ///< @todo Update Atmospheric pressure
                ///< @todo Calc Hydrosphere changes & update albedo accordingly.
                ///< @todo Calc greehouse effect based on atmosphere and apply it + albedo to surface temp. 
            }
            else
            {
                // simply apply albedo, see here: http://en.wikipedia.org/wiki/Stefan%E2%80%93Boltzmann_law
                Pressure = 0;
                SurfaceTemperature = ParentBody.BaseTemperature * (1 - Albedo);
            }
        }

        /// <summary>
        /// Use this when adding gass for terrforming.
        /// @note System gen does not use this function, instead it adds gasses directly to the Composition.
        /// </summary>
        /// <param name="gas">The gass to add.</param>
        /// <param name="ammount">The ammount of gass to add in atm. Provide a negative number to remove gas.</param>
        public void AddGas(AtmosphericGas gas, float ammount)
        {
            if (CanModify == false)
                return; // we dont care!!

            if (_composition.ContainsKey(gas))
            {
                _composition[gas] += ammount;

                if (_composition[gas] <= 0)
                    _composition.Remove(gas);  // if there is none left, remove it.
            }
            else if (ammount > 0)               // only add new gas if it is actuall adding (i.e. ammount is positive).
            {
                _composition.Add(gas, ammount);
            }

            UpdateState();                  // update other state to reflect the new gas ammount.
        }
    }
}
