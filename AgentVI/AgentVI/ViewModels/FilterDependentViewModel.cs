﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms.Extended;
using System.Threading.Tasks;

namespace AgentVI.ViewModels
{
    public abstract class FilterDependentViewModel<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected IEnumerable<T> enumerableCollection;
        private const int pageSize = 10;
        private IEnumerator<T> collectionEnumerator;
        protected bool canLoadMore = false;
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<T> ObservableCollection { get; set; }


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
            bool hasNext = true;
            int fetchedItems = 0;
            bool isEndOfPage = false;

            if(collectionEnumerator == null)
            {
                collectionEnumerator = enumerableCollection.GetEnumerator();
            }

            while(hasNext = collectionEnumerator.MoveNext() && !isEndOfPage)
            {
                ObservableCollection.Add(collectionEnumerator.Current);
                if(fetchedItems++ == pageSize)
                {
                    isEndOfPage = true;
                }
            }

            if(hasNext == false)
            {
                canLoadMore = false;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual void PopulateCollection()
        {
            ObservableCollection.Clear();
            canLoadMore = true;
        }

        public abstract void OnFilterStateUpdated(object source, EventArgs e);
    }
}
