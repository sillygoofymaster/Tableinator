using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;

namespace TableinatorMAUIApp
{
    public class FileManager
    {
        private IFileSaver Saver;

        private IFilePicker Loader;

        private FileResult OpenedFile;

        

        public FileManager()
        {
            Saver = FileSaver.Default;
            Loader = FilePicker.Default;
        }

        public async Task SaveAs(TableinatorMAUIApp.Models.TableAsFile representation)
        {
            var text = JsonSerializer.Serialize<TableinatorMAUIApp.Models.TableAsFile>(representation);
            using var stream = new MemoryStream(Encoding.Default.GetBytes(text));
            OpenedFile = new FileResult((await Saver.SaveAsync("NewTable.json", stream, new CancellationTokenSource().Token)).FilePath);
        }

        public async Task<TableinatorMAUIApp.Models.TableAsFile> Load()
        {
            OpenedFile = await Loader.PickAsync();
            if (OpenedFile == null) return null;
            using var fileStream = await OpenedFile.OpenReadAsync();
            return await JsonSerializer.DeserializeAsync<TableinatorMAUIApp.Models.TableAsFile>(fileStream);
        }
    }
}
