using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChiasenhacUniversal.ViewModel.Dowload
{
    public class DownloadProgress : INotifyPropertyChanged
    {
        #region Variables
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            var handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string _videoUrl;
        public string VideoUrl
        {
            get { return _videoUrl; }
            set { _videoUrl = value; NotifyPropertyChanged("VideoUrl"); }
        }

        private string _fileName;
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; NotifyPropertyChanged("FileName"); }
        }

        private int _current;
        public int Current
        {
            get { return _current; }
            set { _current = value; NotifyPropertyChanged("Current"); }
        }

        public string Downloaded
        {
            get { return _current + "%"; }
        }

        private string _fileSize;
        public string FileSize
        {
            get { return _fileSize; }
            set { _fileSize = value; NotifyPropertyChanged("FileSize"); }
        }

        private DownloadStatus _status;
        public DownloadStatus Status
        {
            get { return _status; }
            set { _status = value; NotifyPropertyChanged("Status"); }
        }
        public string DownloadStatus
        {
            get
            {
                switch (Status)
                {
                    case Dowload.DownloadStatus.Waiting:
                        return "Chạm để tải";
                    case Dowload.DownloadStatus.Downloading:
                        return "Đang tải";
                    case Dowload.DownloadStatus.Paused:
                        return "Tạm dừng tải";
                    case Dowload.DownloadStatus.Cancel:
                        return "Hủy tải xuống";
                    case Dowload.DownloadStatus.Done:
                        return "Đã tải xong";
                    default:
                        return "Huỷ tải xuống";
                }
            }
        }

        private string _quality;
        public string Quality
        {
            get { return _quality; }
            set { _quality = value; NotifyPropertyChanged("Quality"); }
        }
        #endregion

        #region Constructors
        public DownloadProgress(string videoUrl, string fileName, string quality)
        {
            VideoUrl = videoUrl;
            FileName = fileName;
            Quality = quality;
        }
        #endregion
    }
}
