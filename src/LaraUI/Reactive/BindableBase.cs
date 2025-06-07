﻿/*
Copyright (c) 2019-2021 Integrative Software LLC
Created: 8/2019
Author: Pablo Carbonell
*/

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Integrative.Lara
{
    /// <summary>
    ///     Implementation of <see cref="INotifyPropertyChanged" /> to simplify models.
    /// </summary>
    public abstract class BindableBase : INotifyPropertyChanged
    {
        /// <summary>
        ///     Multicast event for property change notifications.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        ///     Checks if a property already matches a desired value.  Sets the property and
        ///     notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">
        ///     Name of the property used to notify listeners.  This
        ///     value is optional and can be provided automatically when invoked from compilers that
        ///     support CallerMemberName.
        /// </param>
        /// <returns>
        ///     True if the value was changed, false if the existing value matched the
        ///     desired value.
        /// </returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        ///     Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">
        ///     Name of the property used to notify listeners.  This
        ///     value is optional and can be provided automatically when invoked from compilers
        ///     that support <see cref="CallerMemberNameAttribute" />.
        /// </param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (_holdCounter > 0)
            {
                _pendingEvents = true;
            }
            else
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private int _holdCounter;
        private bool _pendingEvents;

        /// <summary>
        /// Holds all property changed notifications.
        /// </summary>
        public void BeginUpdate()
        {
            _holdCounter++;
        }

        /// <summary>
        /// Stops holding property changed notifications.
        /// If any property changed pending since BeginUpdate, a single property changed event is triggered.
        /// </summary>
        public void EndUpdate()
        {
            _holdCounter--;
            if (_holdCounter != 0 || !_pendingEvents) return;
            _pendingEvents = false;
            OnPropertyChanged(string.Empty);
        }
    }
}
