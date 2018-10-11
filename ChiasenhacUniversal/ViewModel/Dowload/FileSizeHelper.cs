using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChiasenhacUniversal.ViewModel.Dowload
{
    public class FileSizeHelper
    {
        public static string GetFileSize(ulong size)
        {
            var count = 0;
            while (size > 1024)
            {
                ++count;
                size = size / 1024;
            }

            switch (count)
            {
                case 0:
                    if (size > 1)
                        return size + " bytes";
                    return size + " byte";

                case 1:
                    return size + " KB";

                case 2:
                    return size + " MB";

                case 3:
                    return size + " GB";

                case 4:
                    return size + " TB";

                default:
                    return "Dung lượng file quá lớn";
            }
        }
    }
}
