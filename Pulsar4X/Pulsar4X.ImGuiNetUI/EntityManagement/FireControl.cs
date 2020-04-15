using System;
using System.Linq;
using ImGuiNET;
using Pulsar4X.ECSLib;
using Pulsar4X.ECSLib.ComponentFeatureSets.Missiles;
using Pulsar4X.ECSLib.ComponentFeatureSets.RailGun;
using Pulsar4X.SDL2UI;

namespace Pulsar4X.ImGuiNetUI
{
    public class FireControl : PulsarGuiWindow
    {
        private EntityState _orderEntityState;
        private Entity _orderEntity { get { return _orderEntityState.Entity; } }
        
        //string[] _allWeaponNames = new string[0];
        //ComponentDesign[] _allWeaponDesigns = new ComponentDesign[0];
        //ComponentInstance[] _allWeaponInstances = new ComponentInstance[0];
        ComponentInstance[] _missileLaunchers = new ComponentInstance[0];
        ComponentInstance[] _railGuns = new ComponentInstance[0];
        ComponentInstance[] _beamWpns = new ComponentInstance[0];
        
        SensorContact[] _allSensorContacts = new SensorContact[0];
        string[] _ownEntityNames = new string[0];
        EntityState[] _ownEntites = new EntityState[0];
        
        
        ComponentInstance[] _allFireControl = new ComponentInstance[0];
        FireControlAbilityState[] _fcState = new FireControlAbilityState[0];
        int[][] _assignedWeapons = new int[0][];

        string[] _fcTarget = new string[0];
        
        private FireControl()
        {
            
        }



        public static FireControl GetInstance(EntityState orderEntity)
        {
            FireControl thisitem;
            if (!_uiState.LoadedWindows.ContainsKey(typeof(FireControl)))
            {
                thisitem = new FireControl();
                thisitem.HardRefresh(orderEntity);
            }
            else
            {
                thisitem = (FireControl)_uiState.LoadedWindows[typeof(FireControl)];
                if(thisitem._orderEntityState != orderEntity)
                    thisitem.HardRefresh(orderEntity);
            }
            
            return thisitem;
        }
        

        public override void OnSystemTickChange(DateTime newdate)
        {
            
            for (int i = 0; i < _allFireControl.Length; i++)
            {
                _fcTarget[i] = "None";
                if (_allFireControl[i].TryGetAbilityState(out FireControlAbilityState fcstate))
                {
                    _fcState[i] = fcstate;
                    if (fcstate.Target != null)
                    {
                        _fcTarget[i] = fcstate.Target.GetDataBlob<NameDB>().GetName(_orderEntity.FactionOwner);
                    }
                }
            }
        }
        
        void HardRefresh(EntityState orderEntity)
        {
            _orderEntityState = orderEntity;
            if(orderEntity.DataBlobs.ContainsKey(typeof(FireControlAbilityDB)))
            {
                var instancesDB = orderEntity.Entity.GetDataBlob<ComponentInstancesDB>();
            
                if( instancesDB.TryGetComponentsByAttribute<BeamFireControlAtbDB>(out var fcinstances))
                {
                    _allFireControl = new ComponentInstance[fcinstances.Count];
                    _fcTarget = new string[fcinstances.Count]; 
                    _fcState = new FireControlAbilityState[fcinstances.Count];
                    for (int i = 0; i < fcinstances.Count; i++)
                    {
                        _allFireControl[i] = fcinstances[i];
                        _fcTarget[i] = "None";
                        if (fcinstances[i].TryGetAbilityState(out FireControlAbilityState fcstate))
                        {
                            _fcState[i] = fcstate;
                            if (fcstate.Target != null)
                            {
                                _fcTarget[i] = fcstate.Target.GetDataBlob<NameDB>().GetName(orderEntity.Entity.FactionOwner);
                            }
                        }
                    }
                }

                
                if (instancesDB.TryGetComponentsByAttribute<MissileLauncherAtb>(out var mlinstances))
                {
                    _missileLaunchers = mlinstances.ToArray();
                }
                if (instancesDB.TryGetComponentsByAttribute<RailGunAtb>(out var railGuns))
                {
                    _railGuns = railGuns.ToArray();
                }

                if (instancesDB.TryGetComponentsByAttribute<SimpleBeamWeaponAtbDB>(out var beams))
                {
                    _beamWpns = beams.ToArray();
                }
            }
            
            var sysstate = _uiState.StarSystemStates[_uiState.SelectedStarSysGuid];

            var contacts = sysstate.SystemContacts;
            _allSensorContacts = contacts.GetAllContacts().ToArray();
            _ownEntites = sysstate.EntityStatesWithPosition.Values.ToArray();
            
            
            

        }
        

        internal override void Display()
        {
            if (!IsActive)
                return;
            if (ImGui.Begin("Fire Control"))
            {
                ImGui.Columns(2);
                DisplayFC();
                ImGui.NextColumn();
                Display2ndColomn();
            }
        }

        void DisplayFC()
        {
            for (int i = 0; i < _allFireControl.Length; i++)
            {
                BorderGroup.BeginBorder(_allFireControl[i].Name);
                
                ImGui.Text("Target: " + _fcTarget[i]);
                ImGui.SameLine();
                if (ImGui.SmallButton("Set New"))
                {
                    _fcIndex = i;
                    _c2type = C2Type.SetTarget;
                }

                for (int j = 0; j < _fcState[i].AssignedWeapons.Count; j++)
                {
                    var wpn = _fcState[i].AssignedWeapons[j];
                    ImGui.Text(wpn.Name);
                }

                if (ImGui.Button("Assign Weapons"))
                {
                    _fcIndex = i;
                    _c2type = C2Type.SetWeapons;
                }

                BorderGroup.EndBoarder();
                
            }
            
        }

        enum C2Type
        {
            Nill,
            SetTarget,
            SetWeapons,
        }
        private int _fcIndex;
        private C2Type _c2type;
        private bool _showOwnAsTarget;
        void Display2ndColomn()
        {
            if (_c2type == C2Type.Nill)
                return;
            if (_c2type == C2Type.SetTarget)
            {
                BorderGroup.BeginBorder("Set Target:");
                ImGui.Checkbox("Show Own", ref _showOwnAsTarget);

                for (int i = 0; i < _allSensorContacts.Length; i++)
                {
                    var contact = _allSensorContacts[i];
                    ImGui.SmallButton("Set");
                    ImGui.SameLine();
                    ImGui.Text(contact.Name);
                }

                if (_showOwnAsTarget)
                {
                    for (int i = 0; i < _ownEntites.Length; i++)
                    {
                        var contact = _ownEntites[i];
                        ImGui.SmallButton("Set");
                        ImGui.SameLine();
                        ImGui.Text(contact.Name);
                    }
                }
                BorderGroup.EndBoarder();
            }

            if (_c2type == C2Type.SetWeapons)
            {
                BorderGroup.BeginBorder("Missile Launchers:");
                for (int i = 0; i < _missileLaunchers.Length; i++)
                {
                    ImGui.Text(_missileLaunchers[i].Name); 
                    
                }
                BorderGroup.EndBoarder();
                ImGui.NewLine();
                BorderGroup.BeginBorder("Rail Guns:");
                for (int i = 0; i < _railGuns.Length; i++)
                {
                    ImGui.Text(_railGuns[i].Name); 
                    
                }
                BorderGroup.EndBoarder();
                ImGui.NewLine();
                BorderGroup.BeginBorder("Beam Weapons:");
                for (int i = 0; i < _beamWpns.Length; i++)
                {
                    ImGui.Text(_beamWpns[i].Name); 
                    
                }
                BorderGroup.EndBoarder();
            }
        }
    }
}