﻿using System;

namespace IgOutlook.Infrastructure.Dialogs
{
    public interface IDialog
    {
        double Left { get; set; }
        double Top { get; set; }
        void Close();
        void Show();
    }
}
