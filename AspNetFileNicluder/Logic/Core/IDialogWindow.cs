using Microsoft.VisualStudio.PlatformUI;
using System;

namespace AspNetFileNicluder.Logic.Core
{
    interface IDialogWindow
    {
        DialogWindow Create(Func<bool, bool> callback = null);
    }
}
