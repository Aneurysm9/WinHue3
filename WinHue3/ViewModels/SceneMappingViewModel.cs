﻿using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Input;
using HueLib2;

namespace WinHue3.ViewModels
{
    public class SceneMappingViewModel : ValidatableBindableBase
    {
        private DataTable _dt;
        private string _filter;
        private object _selectedcell;
        private object _row;
        private Dictionary<string, Scene> _listscenes;
        private Dictionary<string, Light> _listlights;
        private Bridge _bridge;

        public SceneMappingViewModel()
        {

        }

        public void Initialize(Dictionary<string, Scene> scenes, Dictionary<string, Light> lights, Bridge bridge)
        {
            _bridge = bridge;
            _listscenes = scenes;
            _listlights = lights;
            BuildSceneMapping();
        }

        public DataView SceneMapping => _dt?.DefaultView;

        public string Filter
        {
            get { return _filter; }
            set
            {
                SetProperty(ref _filter,value);
                FilterData();
                
            }
        }

        public object Row
        {
            get
            {
                return _row;           
            }
            set
            {
                SetProperty(ref _row,value);

            }
        }

        public void FilterData()
        {
            if (_filter == string.Empty)
            {
                _dt.DefaultView.RowFilter = string.Empty;
            }
            else
            {
                StringBuilder sb = new StringBuilder();

                foreach (DataColumn column in _dt.Columns)
                {                 
                    sb.Append($"[{column.ColumnName}] Like '%{_filter}%' OR ");
                }

                sb.Remove(sb.Length - 3, 3);
                _dt.DefaultView.RowFilter = sb.ToString();
            }
        }

        public void BuildSceneMapping()
        {
            Dictionary<string, Scene> lscenes;

            if (!WinHueSettings.settings.ShowHiddenScenes)
                lscenes = _listscenes.Where(
                        x => x.Value.name.Contains("HIDDEN") == false)
                    .ToDictionary(p => p.Key, p => p.Value);
            else
                lscenes = _listscenes;

            Dictionary<string, Light> llights = _listlights;

            DataTable dt = new DataTable();


            dt.Columns.Add("ID");
            dt.Columns.Add("Name");

            // Add all light columns
            foreach (KeyValuePair<string, Light> kvp in llights)
            {
                dt.Columns.Add(kvp.Value.name);
            }

            dt.Columns.Add("Locked");
            dt.Columns.Add("Recycle");
            dt.Columns.Add("Version");

            // Map each scenes and lights
            foreach (KeyValuePair<string, Scene> svp in lscenes)
            {

                object[] data = new object[llights.Count + 5];

                data[0] = new string(svp.Key.ToCharArray());
                data[1] = new string(svp.Value.name.ToCharArray());
                int i = 2;
                foreach (KeyValuePair<string, Light> lvp in llights)
                {
                    string value = svp.Value.lights.Contains(lvp.Key) ? "Assigned" : "";

                    data[i] = new string(value.ToCharArray());
                    i++;
                }
                data[i] = new string(svp.Value.locked.ToString().ToCharArray());
                data[i + 1] = new string(svp.Value.recycle.ToString().ToCharArray());
                data[i + 2] = new string(svp.Value.version.ToString().ToCharArray());

                dt.Rows.Add(data);
            }


            _dt = dt;
            RaisePropertyChanged("SceneMapping");
            
            
        }

        public void RefreshSceneMapping()
        {
            BuildSceneMapping();
            FilterData();
        }

        public void ProcessDoubleClick()
        {
            if (_row == null) return;
            _bridge.ActivateScene(((DataRowView) _row).Row.ItemArray[0].ToString());
        }

        private bool ObjectSelected()
        {
            return Row != null;
        }

       
        public ICommand RefreshMappingCommand => new RelayCommand(param => RefreshSceneMapping(), (param) => ObjectSelected());
        public ICommand DoubleClickObjectCommand => new RelayCommand(param => ProcessDoubleClick(), (param) => ObjectSelected());
    }
}