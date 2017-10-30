﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Ninject;
using RatTracker.Api;
using RatTracker.Firebird;
using RatTracker.Properties;
using RatTracker.ViewModels;

namespace RatTracker.Bootstrapping
{
  public class Bootstrapper : BootstrapperBase
  {
    private StandardKernel kernel;

    public Bootstrapper()
    {
      Initialize();
    }

    protected override void Configure()
    {
      kernel = new StandardKernel(new Module());
      kernel.Bind<IWindowManager>().To<WindowManager>().InSingletonScope();
      kernel.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();
    }

    protected override object GetInstance(Type service, string key)
    {
      return kernel.Get(service);
    }

    protected override IEnumerable<object> GetAllInstances(Type service)
    {
      return kernel.GetAll(service);
    }

    protected override void OnExit(object sender, EventArgs e)
    {
      var starSystemDatabase = kernel.Get<StarSystemDatabase>();
      starSystemDatabase.CloseConnection();
      base.OnExit(sender, e);
    }

    protected override async void OnStartup(object sender, StartupEventArgs e)
    {
      var commandLineArgs = Environment.GetCommandLineArgs();
      var oauthArg = commandLineArgs.FirstOrDefault(x => x.StartsWith("rattracker"));
      if (oauthArg != null)
      {
        var oAuthHandler = kernel.Get<OAuthHandler>();
        await oAuthHandler.ExchangeToken(oauthArg);
      }

      if (string.IsNullOrWhiteSpace(Settings.Default.OAuthToken))
      {
        DisplayRootViewFor<OAuthStartupDialogViewModel>();
      }
      else
      {
        DisplayRootViewFor<RatTrackerViewModel>();
        kernel.Get<EventBus>();
        kernel.Get<Cache>();
        var websocketHandler = kernel.Get<WebsocketHandler>();
        var updater = kernel.Get<Updater>();
        var websocketTask = Task.Run(() => websocketHandler.Initialize(true));
        var systemsDataBaseTask = Task.Run(async () => await updater.EnsureDatabase());

        await Task.WhenAll(websocketTask, systemsDataBaseTask);
      }
    }
  }
}