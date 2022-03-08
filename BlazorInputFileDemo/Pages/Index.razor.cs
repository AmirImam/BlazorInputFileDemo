using EdenPlugins.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Drawing;

namespace BlazorInputFileDemo.Pages
{
    public partial class Index
    {
        [Inject]
        private IJSRuntime Js { get; set; }
        private List<IBrowserFile> source = new();
        private List<FileModel> pickedFiles = new();

        private async void PickFile(InputFileChangeEventArgs e)
        {
            source.AddRange(e.GetMultipleFiles());
            
        }

        private async void Submit()
        {
            foreach (IBrowserFile file in source)
            {
                Stream stream = file.OpenReadStream(int.MaxValue);
                MemoryStream ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                var bytes = ms.ToByteArrayAsync();// await stream.ToByteArrayAsync();
                FileModel model = await ConvertIBrowserFileToFileModelAsync(file, file.Name, "png", new Point() { X = 100, Y = 100 }, "text");
                pickedFiles.Add(model);
                StateHasChanged();
            }
        }

        string ConvertBytesToBase64String(byte[] bytes) => $"data:image/png;base64, {Convert.ToBase64String(bytes)}";

        async Task<FileModel> ConvertIBrowserFileToFileModelAsync(IBrowserFile file, string fileName, string? format = "png", Point? dimentions = null, string? watermark = null)
        {
            var image = dimentions == null ? file :
                await file.RequestImageFileAsync(format, dimentions.Value.X, dimentions.Value.Y);

            var bytes = await image.OpenReadStream(int.MaxValue).ToByteArrayAsync();
            if (watermark != null)
            {
                var imageWithwatermark = await Js.InvokeAsync<string>("setWatermark", ConvertBytesToBase64String(bytes), watermark);
                imageWithwatermark = imageWithwatermark.Replace("data:image/png;base64, ", "").Replace("data:image/png;base64,", "");
                bytes = Convert.FromBase64String(imageWithwatermark);
            }

            var fileModel = new FileModel()
            {
                Name = fileName,
                Data = bytes,
                BytesUrl = ConvertBytesToBase64String(bytes)
            };
            return fileModel;
        }
    }

    public class FileModel
    {
        public string Name { get; set; }
        public byte[] Data { get; set; }
        public long Size { get; set; }
        public string BytesUrl { get; set; }
    }
}