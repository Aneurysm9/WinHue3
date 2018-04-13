﻿using WinHue3.Utils;

namespace WinHue3.Functions.User_Management
{
    public class ManageUsersModel : ValidatableBindableBase
    {
        private string _appname;
        private string _devtype;
        private string _lastused;
        private string _created;
        private string _key;

        public ManageUsersModel()
        {
            _appname = string.Empty;
            _devtype = string.Empty;
            _lastused = string.Empty;
            _created = string.Empty;
            _key = string.Empty;

        }

        public string ApplicationName
        {
            get => _appname;
            set => SetProperty(ref _appname,value);
        }

        public string Devtype
        {
            get => _devtype;
            set => SetProperty(ref _devtype,value);
        }

        public string Lastused
        {
            get => _lastused;
            set => SetProperty(ref _lastused,value);
        }

        public string Created
        {
            get => _created;
            set => SetProperty(ref _created,value);
        }

        public string Key
        {
            get => _key;
            set => SetProperty(ref _key,value);
        }

    }
}
