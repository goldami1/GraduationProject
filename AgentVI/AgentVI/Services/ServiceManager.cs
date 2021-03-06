﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AgentVI.Services
{
    public sealed partial class ServiceManager
    {
        private static readonly object padlock = new object();
        private static ServiceManager _Instance = null;
        public static ServiceManager Instance
        {
            get
            {
                if(_Instance == null)
                {
                    lock (padlock)
                    {
                        if (_Instance == null)
                        {
                            _Instance = new ServiceManager();
                        }
                    }
                }
                return _Instance;
            }
        }

        public ILoginService LoginService { get; private set; }
        public IFilterService FilterService { get; private set; }


        private ServiceManager()
        {
            LoginService = new LoginServiceS();
            FilterService = new FilterServiceS();
        }
    }
}
