using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChiasenhacUniversal.ViewModel.Dowload
{
    public enum DownloadStatus
    {
        Waiting = 0,
        Downloading = 1,
        Paused = 2,
        Cancel = 3,
        Done = 4
    }
}
