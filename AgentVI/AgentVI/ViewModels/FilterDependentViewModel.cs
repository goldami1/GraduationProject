﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms.Extended;
using System.Threading.Tasks;
using AgentVI.Interfaces;

namespace AgentVI.ViewModels
{
    public abstract class FilterDependentViewModel<T> : IBindableVM, INotifyPropertyChanged
    {
        protected IEnumerable<T> enumerableCollection;
        private const int pageSize = 10;
        private IEnumerator<T> collectionEnumerator;
        protected bool canLoadMore = false;
        protected bool IsFilterStateChanged { get; set; }
        public ObservableCollection<T> ObservableCollection { get; set; }
        private bool _isStillLoading = true;
        public bool IsStillLoading
        {
            get => _isStillLoading;
            set
            {
                _isStillLoading = value;
                if (!_isStillLoading && IsEmptyFolder)
                    IsEmptyView = true;
                else
                    IsEmptyView = false;
                OnPropertyChanged(nameof(IsStillLoading));
            }
        }
        private bool _isEmptyView = false;
        public bool IsEmptyView
        {
            get => _isEmptyView;
            set
            {
                _isEmptyView = value;
                OnPropertyChanged(nameof(IsEmptyView));
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                updateIsStillLoading();
                OnPropertyChanged(nameof(IsBusy));
            }
        }
        private bool _isEmptyFolder = true;
        public bool IsEmptyFolder
        {
            get => _isEmptyFolder;
            set
            {
                _isEmptyFolder = value;
                updateIsStillLoading();
                OnPropertyChanged(nameof(IsEmptyFolder));
            }
        }

        private void updateIsStillLoading()
        {
            IsStillLoading = IsBusy && IsEmptyFolder;
        }

        private void updateFolderState()
        {
            if (ObservableCollection.Count == 0)
            {
                IsEmptyFolder = true;
            }
            else
            {
                IsEmptyFolder = false;
            }
        }


        public FilterDependentViewModel()
        {
            ObservableCollection = new InfiniteScrollCollection<T>()
            {
                OnLoadMore = async () =>
                {
                    IsBusy = true;
                    await Task.Factory.StartNew(() => FetchCollection());
                    IsBusy = false;
                    return null;
                },
                OnCanLoadMore = () =>
                {
                    return canLoadMore;
                }
            };
        }

        protected virtual void FetchCollection()
        {
            Console.WriteLine("###Logger###   -   in FilterDependentVM.FetchCollection main thread @ begin FetchCollection");
            bool hasNext = true;
            int fetchedItems = 0;

            IsBusy = true;
            Console.WriteLine("###Logger###   -   in FilterDependentVM.FetchCollection main thread @ 1");
            if (collectionEnumerator == null || IsFilterStateChanged)
            {
                Console.WriteLine("###Logger###   -   in FilterDependentVM.FetchCollection main thread @ 2");
                IsFilterStateChanged = false;
                Console.WriteLine("###Logger###   -   in FilterDependentVM.FetchCollection main thread @ 3");
                collectionEnumerator = enumerableCollection.GetEnumerator();
                Console.WriteLine("###Logger###   -   in FilterDependentVM.FetchCollection main thread @ 4");
            }

            try
            {
                Console.WriteLine("###Logger###   -   in FilterDependentVM.FetchCollection main thread @ 5");
                while (hasNext = collectionEnumerator.MoveNext() && canLoadMore)
                {
                    Console.WriteLine("###Logger###   -   in FilterDependentVM.FetchCollection main thread @ 6. 1");
                    ObservableCollection.Add(collectionEnumerator.Current);
                    Console.WriteLine("###Logger###   -   in FilterDependentVM.FetchCollection main thread @ 7. 2");
                    if (IsEmptyFolder)
                        IsEmptyFolder = !IsEmptyFolder;
                    if (fetchedItems++ == pageSize)
                    {
                        break;
                    }
                }
            }catch(ArgumentOutOfRangeException)
            {
                hasNext = false;
                Console.WriteLine("###Logger###   -   in FilterDependentVM.FetchCollection main thread @ !8!");
            }

            if(hasNext == false)
            {
                canLoadMore = false;
            }

            updateFolderState();
            IsBusy = false;
            Console.WriteLine("###Logger###   -   in FilterDependentVM.FetchCollection main thread @ end FetchCollection");
        }

        public virtual void PopulateCollection()
        {
            ObservableCollection.Clear();
            IsFilterStateChanged = true;
            canLoadMore = true;
        }

        public virtual void OnFilterStateUpdated(object source, EventArgs e)
        {
            IsFilterStateChanged = true;
        }
    }
}
