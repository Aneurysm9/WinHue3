﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using HueLib2;

namespace WinHue3.ViewModels
{
    public class BulbsViewViewModel : ValidatableBindableBase
    {
        private DataTable _dt;
        private string _filter;
        private bool _reverse;
        private List<Light> _listlights;

        public BulbsViewViewModel()
        {

        }

        public void Initialize(List<Light> lights)
        {
            Listlights = lights;
            BuildBulbsViewReverse();
        }

        public DataView BulbsDetails => _dt?.DefaultView;

        public bool Reverse
        {
            get { return _reverse; }
            set
            {
                SetProperty(ref _reverse,value);
                if (value == true)
                {
                    BuildBulbsView();
                }
                else
                {
                    BuildBulbsViewReverse();
                }
            }
        }

        private void BuildBulbsView()
        {


            List<Light> llights = Listlights;
            DataTable dt = new DataTable();

            dt.Columns.Add("Properties");
            foreach (Light lvp in llights)
            {
                dt.Columns.Add(lvp.Name);
            }

            PropertyInfo[] listproperties = typeof(Light).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            PropertyInfo[] liststate = typeof(State).GetProperties();
            PropertyInfo[] listPropertyInfos = new PropertyInfo[listproperties.Length + liststate.Length];

            listproperties.CopyTo(listPropertyInfos, 0);
            liststate.CopyTo(listPropertyInfos, listproperties.Length);

            object[] data = new object[llights.Count + 1];

            foreach (PropertyInfo pi in listPropertyInfos)
            {
                if (pi.Name == "state" || pi.Name == "name" || pi.Name.Contains("_inc") || pi.Name == "Image") continue;

                data[0] = pi.Name;

                int i = 1;
                foreach (Light l in llights)
                {
                    if (Array.Find(liststate, x => x.Name == pi.Name) != null)
                    {
                        data[i] = pi.GetValue(l.state);
                    }
                    else
                    {
                        data[i] = pi.GetValue(l);
                    }

                    i++;
                }


                dt.Rows.Add(data);
            }
            _dt = dt;
            RaisePropertyChanged("BulbsDetails");

        }

        private void BuildBulbsViewReverse()
        {

            List<Light> llights = Listlights;
            if (llights == null) return;
            DataTable dt = new DataTable();
            dt.Columns.Add("Lights");

            PropertyInfo[] listproperties =
                typeof(Light).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            PropertyInfo[] liststate = typeof(State).GetProperties();
            PropertyInfo[] listPropertyInfos = new PropertyInfo[listproperties.Length + liststate.Length];

            listproperties.CopyTo(listPropertyInfos, 0);
            liststate.CopyTo(listPropertyInfos, listproperties.Length);

            foreach (PropertyInfo pi in listPropertyInfos)
            {
                if (pi.Name == "state" || pi.Name == "name" || pi.Name.Contains("_inc") || pi.Name == "Image") continue;
                dt.Columns.Add(pi.Name);
            }

            int nbrcol = 1 + listPropertyInfos.Length - 2 - liststate.Count(x => x.Name.Contains("_inc"));

            object[] data = new object[nbrcol];

            foreach (Light l in llights)
            {
                int i = 1;
                data[0] = l.Name;

                foreach (PropertyInfo pi in listPropertyInfos)
                {
                    if (pi.Name == "state" || pi.Name == "name" || pi.Name.Contains("_inc") || pi.Name == "Image") continue;

                    if (Array.Find(liststate, x => x.Name == pi.Name) != null)
                    {
                        data[i] = pi.GetValue(l.state);
                    }
                    else
                    {
                        data[i] = pi.GetValue(l);
                    }
                    i++;

                }
                dt.Rows.Add(data);
            }

            _dt = dt;
            RaisePropertyChanged("BulbsDetails");

        }

        public string Filter
        {
            get { return _filter; }
            set
            {
                SetProperty(ref _filter,value);
                
                FilterData();

            }
        }

        public List<Light> Listlights
        {
            get
            {
                return _listlights;
            }

            set
            {
                SetProperty(ref _listlights,value);
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

    }
}
