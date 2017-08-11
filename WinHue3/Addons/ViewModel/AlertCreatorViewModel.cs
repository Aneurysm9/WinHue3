﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using WinHue3.Addons.Model;
using WinHue3.Addons.RssFeedMonitor;
using WinHue3.ExtensionMethods;
using WinHue3.Philips_Hue.HueObjects.Common;
using WinHue3.Utils;
using WinHue3.ViewModels;
using Bridge = WinHue3.Philips_Hue.BridgeObject.Bridge;

namespace WinHue3.Addons.ViewModel
{
    public class AlertCreatorViewModel : ValidatableBindableBase
    {
        private AlertCreatorModel _alertCreatorModel;
        private ObservableCollection<Criteria> _criterias;
        private IBaseProperties _action;
        private string _value;
        private string _rsselement;
        private string _operator;
        private Bridge _bridge;


        public AlertCreatorModel AlertCreatorModel
        {
            get => _alertCreatorModel;
            set => SetProperty(ref _alertCreatorModel,value);
        }

        public void Initialize(Bridge bridge)
        {
            _bridge = bridge;
        }


        public string Value
        {
            get => _value;
            set => SetProperty(ref _value,value);
        }

        public ObservableCollection<Criteria> Criterias
        {
            get => _criterias;
            set => SetProperty(ref _criterias, value);
        }

        public IBaseProperties Action
        {
            get => _action;
            set => SetProperty(ref _action, value);
        }

        public string RssElement
        {
            get => _rsselement;
            set => SetProperty(ref _rsselement,value);
        }

        public string Operator
        {
            get => _operator;
            set => SetProperty(ref _operator,value);
        }

        public Alert Alert
        {
            get
            {
                Alert alert = new Alert
                {
                    Name = AlertCreatorModel.Name,
                    Description = AlertCreatorModel.Description,
                    Criterias = Criterias,
                    Enabled = AlertCreatorModel.Enable
                };
                return alert;
            }
            set
            {
                AlertCreatorModel.Name = value.Name;
                AlertCreatorModel.Description = value.Description;
                AlertCreatorModel.Enable = value.Enabled;
                Criterias = value.Criterias;
            }
        }

        public AlertCreatorViewModel()
        {
            AlertCreatorModel = new AlertCreatorModel();
            Criterias = new ObservableCollection<Criteria>();
            RssElement = "Title";
            Operator = "Contains";
        }

        private void CheckRssFeedUrl()
        {
            bool result = Uri.TryCreate(AlertCreatorModel.Url, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            try
            {
                XmlReader reader = XmlReader.Create(uriResult.ToString());
                SyndicationFeed obj = SyndicationFeed.Load(reader);
            
            }
            catch (Exception)
            {
                MessageBox.Show("The entered url does not seem to be valid.\r\n Please enter a valid RSS feed url.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddCriteria()
        {
            if (Criterias.Any(x => x.RSSElement == RssElement && x.Condition == Operator))
            {
                if (
                    MessageBox.Show(GlobalStrings.RssFeedMonitor_CriteriaAlreadyExists, GlobalStrings.Error,
                        MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                {
                    int index = Criterias.FindIndex(x => x.RSSElement == RssElement && x.Condition == Operator);
                    if (index != -1)
                    {
                        Criterias.RemoveAt(index);
                        AddCriteriaFinal();
                        ClearCondition();
                    }
                    
                }
            }
            else
            {
                AddCriteriaFinal();
            }

        }

        private void AddCriteriaFinal()
        {
            Criterias.Add(new Criteria
            {
                RSSElement = RssElement,
                Condition = Operator,
                UserCondition = Value
            });

            ClearCondition();
        }
    

        private void ClearCondition()
        {
            Operator = "Contains";
            Value = string.Empty;
            RssElement = "Title";
        }

        private void SelectAction()
        {
            RssFeedMonitor.Form_ActionPicker fap = new RssFeedMonitor.Form_ActionPicker(_bridge);
            if((bool)fap.ShowDialog())
            {
                //TODO : Set Action
            }
                
        }

        private bool CanAddCriteria()
        {
            if (string.IsNullOrWhiteSpace(Value) || string.IsNullOrEmpty(Value)) return false;
            if (RssElement == string.Empty) return false;
            if (Operator == string.Empty) return false;
            return true;
        }

        private bool CanCheckUrl()
        {
            return AlertCreatorModel.Url != string.Empty;
        }

        public ICommand CheckRssFeedUrlCommand => new RelayCommand(param => CheckRssFeedUrl(), (param) => CanCheckUrl());
        public ICommand AddCriteriaCommand => new RelayCommand(param => AddCriteria(), (param) => CanAddCriteria());
        public ICommand SelectActionCommand => new RelayCommand(param => SelectAction());


    }
}
