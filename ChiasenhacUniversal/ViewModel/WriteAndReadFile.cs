using System.Threading.Tasks;
using Windows.Storage;
using System;
using System.Collections.Generic;
using BackgroundAudioShare.Model;
using Newtonsoft.Json;
using ChiasenhacUniversal.Model;

namespace ChiasenhacUniversal.ViewModel
{
    public static class WriteAndReadFile
    {
        public static async Task WriteFile(string filename, string filecontent)
        {
            try
            {
                byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(filecontent);

                StorageFolder folder = ApplicationData.Current.LocalFolder;

                StorageFile sampleFile = await folder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

                await FileIO.WriteTextAsync(sampleFile, filecontent);
            }
            catch { }
        }

        public static async Task<string> ReadFile(string filename)
        {
            string text = "";
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            var listFile = await storageFolder.GetFilesAsync();
            if (listFile != null)
            {
                for (int i = 0; i < listFile.Count; i++)
                {
                    if (listFile[i].Name == filename)
                    {
                        StorageFile sampleFile = await storageFolder.GetFileAsync(filename);
                        text = await FileIO.ReadTextAsync(sampleFile);

                    }
                }
            }
            return text;
        }

        public static async Task<bool> CheckFileExist(string fileName)
        {
            bool isFileExist = false;
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            IReadOnlyList<StorageFile> listFiles = await storageFolder.GetFilesAsync();

            if (listFiles.Count != 0)
            {
                for (int i = 0; i < listFiles.Count; i++)
                {
                    if (listFiles[i].Name == fileName)
                    {
                        isFileExist = true;
                        break;
                    }
                    else
                    {
                        isFileExist = false;
                    }
                }
            }

            return isFileExist;
        }

        public static async void SaveListNowPlaying(List<SongDetail.MusicInfo> listNowPlaying)
        {
            string dataToSave = JsonConvert.SerializeObject(listNowPlaying);
            await WriteFile("ListNowPlaying", "");
            await WriteFile("ListNowPlaying", dataToSave);
        }

        public async static void SaveListSearchKey(List<SearchKey> listSearchKey)
        {
            string dataToSave = JsonConvert.SerializeObject(listSearchKey);

            await WriteFile("ListSearchKey", dataToSave);
        }
    }
}
