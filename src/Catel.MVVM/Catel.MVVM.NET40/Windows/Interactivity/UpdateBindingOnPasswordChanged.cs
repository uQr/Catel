﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdateBindingOnPasswordChanged.cs" company="Catel development team">
//   Copyright (c) 2008 - 2013 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.Windows.Interactivity
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// This behavior automatically updates the binding of a <see cref="PasswordBox"/> when the
    /// <c>PasswordChanged</c> event occurs.
    /// </summary>
    public class UpdateBindingOnPasswordChanged : BehaviorBase<PasswordBox>
    {
        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password
        {
            get { return (string) GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        /// <summary>
        /// The Password Property
        /// </summary>
        public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register("Password", typeof (string), typeof(UpdateBindingOnPasswordChanged),
            new PropertyMetadata(null, (sender, e) => ((UpdateBindingOnPasswordChanged)sender).OnPasswordChanged(e)));

        /// <summary>
        /// Called when the password has been changed.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private void OnPasswordChanged(DependencyPropertyChangedEventArgs e)
        {
            if (AssociatedObject != null)
            {
                if (AssociatedObject.Password != Password)
                {
                    AssociatedObject.Password = Password;
                }
            }
        }

        /// <summary>
        /// Called when the associated object is loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void OnAssociatedObjectLoaded(object sender, System.EventArgs e)
        {
            AssociatedObject.PasswordChanged += OnPasswordChanged;
        }

        /// <summary>
        /// Called when the associated object is unloaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void OnAssociatedObjectUnloaded(object sender, System.EventArgs e)
        {
            AssociatedObject.PasswordChanged -= OnPasswordChanged;
        }

        /// <summary>
        /// Passwords the box password changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
#if NET
            Password = AssociatedObject.Password;
#else
            var binding = AssociatedObject.GetBindingExpression(PasswordBox.PasswordProperty);
            if (binding != null)
            {
                binding.UpdateSource();
            }
#endif
        }
    }
}