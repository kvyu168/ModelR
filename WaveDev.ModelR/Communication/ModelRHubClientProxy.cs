﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using System.Collections.Generic;
using WaveDev.ModelR.Shared.Models;
using System.Net;
using System.Globalization;
using Xceed.Wpf.Toolkit;
using WaveDev.ModelR.Shared;

namespace WaveDev.ModelR.Communication
{
    internal class ModelRHubClientProxy
    {
        #region Private Fields

        private static ModelRHubClientProxy s_instance;

        private HubConnection _connection;
        private IHubProxy _proxy;
        private IEnumerable<SceneInfoModel> _cachedScenes;

        #endregion

        #region Static Members

        public static ModelRHubClientProxy GetInstance(string url)
        {
            if (s_instance == null || s_instance.ServerUrl.CompareTo(url) != 0)
                s_instance = new ModelRHubClientProxy(url);

            return s_instance;
        }

        #endregion

        #region Construction

        private ModelRHubClientProxy(string url)
        {
            ServerUrl = url;

            ConnectToServer();
        }

        #endregion

        #region Public Members

        public string ServerUrl
        {
            get;
            set;
        }
        
        public void Login(string user, string password)
        {
            var authenticationToken = string.Format("User={0} Password={1}", user, password);

            _connection.Headers.Add("myauthtoken", authenticationToken);
        }

        public IEnumerable<SceneInfoModel> Scenes
        {
            get
            {
                if (_cachedScenes == null)
                    _cachedScenes = _proxy.Invoke<IEnumerable<SceneInfoModel>>("GetAvailableScenes").Result;

                return _cachedScenes;
            }
        }

        public void JoinSceneEditorGroup(Guid sceneId)
        {
            _proxy.Invoke("JoinSceneEditorGroup", sceneId).Wait();
        }

        #endregion

        #region Private Methods

        private void ConnectToServer()
        {
            try
            {
                _connection = new HubConnection(ServerUrl)
                {
                    TraceLevel = TraceLevels.All,
                    TraceWriter = Console.Out
                };

                _proxy = _connection.CreateHubProxy(Constants.ModelRHubName);

                _connection.Start().Wait();
            }
            catch (AggregateException exception)
            {
                var error = exception.Message;

                foreach (var innerException in exception.InnerExceptions)
                    error = string.Format(CultureInfo.CurrentUICulture, "{0}{1}{2}", error, Environment.NewLine, innerException.Message);

                // TODO: [RS] Use MVVM Light here!
                MessageBox.Show(error, "ModelR - Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion
    }
}