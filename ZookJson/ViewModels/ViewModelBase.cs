using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using ZookJson.Views;

namespace ZookJson.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {

        protected readonly Window? _view;
        public ViewModelBase() : this(null) { }
        public ViewModelBase(Window? window)
        {
            this._view= window;
        }
    }
}
