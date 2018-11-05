﻿using AgentVI.Models;
using AgentVI.Services;
using AgentVI.Utils;
using FFImageLoading.Svg.Forms;
using System;
using Xamarin.Forms;
using AgentVI.Interfaces;
using AgentVI.Views;
using Plugin.DeviceOrientation;
using Plugin.DeviceOrientation.Abstractions;
using EContentUpdateType = AgentVI.Utils.UpdatedContentEventArgs.EContentUpdateType;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace AgentVI.ViewModels
{
    public partial class MainPageViewModel : IBindableVM, INotifyContentViewChanged
    {
        internal MainPageViewModel(IProgress<ProgressReportModel> i_ProgressReporter, Dictionary<EAppTab, SvgCachedImage> i_AppTabs)
        {
            waitingPage = new WaitingPage();
            progressReporter = i_ProgressReporter;
            ProgressReportModel report = new ProgressReportModel(k_NumberOfInitializations);

            updateReporter("Initializing filter...", report);
            FilterIndicator = new FilterIndicatorViewModel();
            FilterPage = new FilterPage(FilterIndicator);
            updateReporter("Filter initialized correctly", report);

            updateReporter("Fetching account...", report);
            LoginContext = new LoginPageViewModel();
            updateReporter("Account initialized.", report);

            initPagesCollectionHelper(report, i_AppTabs);
        }

        internal async void ResetHierarchyToRootLevel()
        {
            OnContentViewUpdateEvent(this, new UpdatedContentEventArgs(EContentUpdateType.Buffering));

            await Task.Factory.StartNew(() =>
            {
                FilterPage = new FilterPage(FilterIndicator);
                FilterIndicator.UpdateCurrentPath();
            });

            OnContentViewUpdateEvent(this, new UpdatedContentEventArgs(EContentUpdateType.Pop));
        }

        private void initPagesCollectionHelper(ProgressReportModel i_Report, Dictionary<EAppTab, SvgCachedImage> i_AppTabs)
        {
            updateReporter("Initializing app pages...", i_Report);
            PagesCollection = new Dictionary<EAppTab, Tuple<IBindable, SvgCachedImage>>();

            updateReporter("Fetching Cameras...", i_Report);
            CamerasPage camerasPageInstance = new CamerasPage();
            camerasPageInstance.RaiseContentViewUpdateEvent += OnContentViewUpdateEvent;
            PagesCollection.Add(EAppTab.SensorsPage, new Tuple<IBindable, SvgCachedImage>(camerasPageInstance, i_AppTabs[EAppTab.SensorsPage]));

            updateReporter("Fetching Settings...", i_Report);
            PagesCollection.Add(EAppTab.SettingsPage, new Tuple<IBindable, SvgCachedImage>(new SettingsPage(), i_AppTabs[EAppTab.SettingsPage]));
            PagesCollection.Add(EAppTab.HealthPage, new Tuple<IBindable, SvgCachedImage>(new HealthStatPage(), i_AppTabs[EAppTab.HealthPage]));

            updateReporter("Fetching Events...", i_Report);
            EventsPage eventsPageBuf = new EventsPage();
            eventsPageBuf.RaiseContentViewUpdateEvent += OnContentViewUpdateEvent;
            PagesCollection.Add(EAppTab.EventsPage, new Tuple<IBindable, SvgCachedImage>(eventsPageBuf, i_AppTabs[EAppTab.EventsPage]));
        }

        private void addToContentViewStack(Tuple<ContentPage,IBindableVM> i_updatedContent)
        {
            if (contentViewStack == null)
            {
                contentViewStack = new Stack<Tuple<ContentPage, IBindableVM>>();
            }
            contentViewStack.Push(i_updatedContent);
        }

        private void resetContentViewStack()
        {
            contentViewStack = null;
            CrossDeviceOrientation.Current.LockOrientation(DeviceOrientations.Portrait);
        }

        private void popFromControlViewStack()
        {
            Console.WriteLine("###Logger###   -   in MainPageViewModel.popFromControlViewStack main thread @ begin retrieving current saved page");
            Tuple<ContentPage, IBindableVM> stackTop = contentViewStack.Pop();
            if (stackTop != null)
            {
                currentPageInContentView = stackTop.Item1;
                currentPageVMInContentView = stackTop.Item2;
                Console.WriteLine("###Logger###   -   in MainPageViewModel.popFromControlViewStack main thread @ before popping invoke");
                RaiseContentViewUpdateEvent?.Invoke(null, new UpdatedContentEventArgs(EContentUpdateType.Pop,
                    currentPageInContentView, currentPageVMInContentView));
                Console.WriteLine("###Logger###   -   in MainPageViewModel.popFromControlViewStack main thread @ after popping invoke");
            }
            else
            {
                throw new Exception("MainPage.PopFromControlViewStack called unexpectedely");
            }
            Console.WriteLine("###Logger###   -   in MainPageViewModel.popFromControlViewStack main thread @ end retrieving current saved page");
        }

        private void updateReporter(string i_StageCompleted, ProgressReportModel i_Report)
        {
            if (progressReporter != null && i_Report != null)
            {
                i_Report.Progress();
                i_Report.AddStage(i_StageCompleted);
                progressReporter.Report(i_Report);
            }
        }

        private void OnContentViewUpdateEvent(object sender, UpdatedContentEventArgs e)
        {
            Console.WriteLine("###Logger###   -   begin MainPageViewModel.OnContentViewUpdateEvent main thread");
            lock (contentViewUpdateLock)
            {
                Console.WriteLine("###Logger###   -   in MainPageViewModel.OnContentViewUpdateEvent main thread @ start lock");
                try
                {
                    if (e == null || e.ContentUpdateType == EContentUpdateType.Buffering)
                    {
                        Console.WriteLine("###Logger###   -   in MainPageViewModel.OnContentViewUpdateEvent main thread @ begin buffering");
                        Console.WriteLine("###Logger###   -   in MainPageViewModel.OnContentViewUpdateEvent main thread @ begin saving current page");
                        addToContentViewStack
                            (
                            new Tuple<ContentPage, IBindableVM>(currentPageInContentView, currentPageVMInContentView)
                            );
                        Console.WriteLine("###Logger###   -   in MainPageViewModel.OnContentViewUpdateEvent main thread @ end saving current page");
                        Console.WriteLine("###Logger###   -   in MainPageViewModel.OnContentViewUpdateEvent main thread @ before buffering invoke");
                        RaiseContentViewUpdateEvent?.Invoke(null, new UpdatedContentEventArgs(EContentUpdateType.Buffering, waitingPage));
                        Console.WriteLine("###Logger###   -   in MainPageViewModel.OnContentViewUpdateEvent main thread @ after buffering invoke");
                        Console.WriteLine("###Logger###   -   in MainPageViewModel.OnContentViewUpdateEvent main thread @ end buffering");
                    }
                    else if (e.ContentUpdateType == EContentUpdateType.Pop)
                    {
                        popFromControlViewStack();
                    }
                    else if (e.ContentUpdateType == EContentUpdateType.PushAsync)
                    {
                        addToContentViewStack
                            (
                            new Tuple<ContentPage, IBindableVM>(currentPageInContentView, currentPageVMInContentView)
                            );
                        try
                        {
                            (e.UpdatedContent as IFocusable).Refocus();
                        }
                        catch (NullReferenceException ex) { Console.WriteLine(ex.Message); }
                        RaiseContentViewUpdateEvent?.Invoke(null, e);
                    }
                    else if (e.ContentUpdateType == EContentUpdateType.PopAsync)
                    {
                        RaiseContentViewUpdateEvent?.Invoke(null, e);
                        Tuple<ContentPage, IBindableVM> stackTop = contentViewStack.Pop();
                        try
                        {
                            (stackTop.Item1 as IFocusable).Refocus();
                        }
                        catch (NullReferenceException ex) { Console.WriteLine(ex.Message); }
                    }
                    else                                        //UpdatedContentEventArgs.EContentUpdateType.Push
                    {
                        Console.WriteLine("###Logger###   -   in MainPageViewModel.OnContentViewUpdateEvent main thread @ begin pushing page on NavigationStack");
                        Console.WriteLine("###Logger###   -   in MainPageViewModel.OnContentViewUpdateEvent main thread @ begin setting ContentView");
                        currentPageInContentView = e.UpdatedContent;
                        Console.WriteLine("###Logger###   -   in MainPageViewModel.OnContentViewUpdateEvent main thread @ end setting ContentView View");
                        Console.WriteLine("###Logger###   -   in MainPageViewModel.OnContentViewUpdateEvent main thread @ begin setting ContentView VM");
                        currentPageVMInContentView = e.UpdatedVM;
                        Console.WriteLine("###Logger###   -   in MainPageViewModel.OnContentViewUpdateEvent main thread @ end setting ContentView VM");
                        Console.WriteLine("###Logger###   -   in MainPageViewModel.OnContentViewUpdateEvent main thread @ before invoke push on ContentView");
                        RaiseContentViewUpdateEvent?.Invoke(null, e);
                        Console.WriteLine("###Logger###   -   in MainPageViewModel.OnContentViewUpdateEvent main thread @ after invoke push on ContentView");
                    }
                }catch(AggregateException ae)
                {
                    Console.WriteLine("Aggregate Exception Catched!");
                }
                Console.WriteLine("###Logger###   -   in MainPageViewModel.OnContentViewUpdateEvent main thread @ end lock");
            }
            Console.WriteLine("###Logger###   -   end MainPageViewModel.OnContentViewUpdateEvent main thread");
        }

        internal void updateContentView(EAppTab i_AppTab ,ContentPage i_UpdatedContent, IBindableVM i_UpdatedVM)
        {
            currentPageInContentView = i_UpdatedContent;
            currentPageVMInContentView = i_UpdatedVM;
            RaiseContentViewUpdateEvent?.Invoke(null, new UpdatedContentEventArgs(EContentUpdateType.Push, i_UpdatedContent, i_UpdatedVM));

            if(i_AppTab != EAppTab.None)
            {
                resetContentViewStack();
                markSelectedTab(i_AppTab);
            }
        }

        private void markSelectedTab(EAppTab i_SelectedTab)
        {
            PagesCollection[i_SelectedTab].Item2.Source = MainPageViewModel.EnumToSVGPath(i_SelectedTab, ESelection.Active);
            foreach (KeyValuePair<EAppTab, Tuple<IBindable, SvgCachedImage>> kvPair in PagesCollection)
            {
                if (kvPair.Key != i_SelectedTab)
                {
                    Device.BeginInvokeOnMainThread(() => kvPair.Value.Item2.Source = MainPageViewModel.EnumToSVGPath(kvPair.Key, ESelection.Passive));
                }
            }
        }

        internal static string EnumToSVGPath(EAppTab i_TabPage, ESelection i_SelectionStatus)
        {
            string res;

            if (i_TabPage == EAppTab.EventsPage && i_SelectionStatus == ESelection.Active)
            {
                res = Settings.EventsTabSelectedSVGPath;
            }
            else if (i_TabPage == EAppTab.EventsPage && i_SelectionStatus == ESelection.Passive)
            {
                res = Settings.EventsTabSVGPath;
            }
            else if (i_TabPage == EAppTab.SensorsPage && i_SelectionStatus == ESelection.Active)
            {
                res = Settings.SensorsTabSelectedSVGPath;
            }
            else if (i_TabPage == EAppTab.SensorsPage && i_SelectionStatus == ESelection.Passive)
            {
                res = Settings.SensorsTabSVGPath;
            }
            else if (i_TabPage == EAppTab.HealthPage && i_SelectionStatus == ESelection.Active)
            {
                res = Settings.HealthTabSelectedSVGPath;
            }
            else if (i_TabPage == EAppTab.HealthPage && i_SelectionStatus == ESelection.Passive)
            {
                res = Settings.HealthTabSVGPath;
            }
            else if (i_TabPage == EAppTab.SettingsPage && i_SelectionStatus == ESelection.Active)
            {
                res = Settings.SettingsTabSelectedSVGPath;
            }
            else if (i_TabPage == EAppTab.SettingsPage && i_SelectionStatus == ESelection.Passive)
            {
                res = Settings.SettingsTabSVGPath;
            }
            else
            {
                res = null;
            }

            return res;
        }
    }
}
