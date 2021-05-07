﻿using System;
using Betterloid;
using Yamaha.VOCALOID.VOCALOID5;

namespace PhonemeConverter
{
    public class PhonemeConverter : IPlugin // Inheriting from IPlugin allows the plugin to be recognized and stored by Betterloid, this is required
    {
        PhonemeConverterDialog dialog;
        public void Startup() // The startup function is the entry point of your plugin, this is comparable to a Main function
        {
            if (dialog == null) // The lambda logic is there to make sure only one instance of the plugin is open at a time
            {
                dialog = new PhonemeConverterDialog();
                dialog.Owner = App.Current.MainWindow;
                EventHandler closedEvent = null;
                closedEvent = (sender, args) => { dialog.Closed -= closedEvent;  dialog = null;  };
                dialog.Closed += closedEvent;
                dialog.Show();
            }
        }
    }
}
