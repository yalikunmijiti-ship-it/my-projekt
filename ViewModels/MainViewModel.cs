using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Storage;
using SkiaSharp;

namespace MauiAppMessSkizze.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private List<SKPoint> _points = new();
        private double? _scaleCmPerPixel;
        private byte[]? _originalBytes;
        public event PropertyChangedEventHandler? PropertyChanged;

        public ImageSource? OriginalImage { get; private set; }
        public string MeasurementInfo { get; private set; } = "Tippe 2 Punkte für Referenz";

        public ICommand PickPhotoCommand { get; }
        public ICommand CapturePhotoCommand { get; }

        public MainViewModel()
        {
            PickPhotoCommand = new Command(async () => await PickPhoto());
            CapturePhotoCommand = new Command(async () => await CapturePhoto());
            //CapturePhotoCommand = new Command(async () => await CapturePhoto());
        }

        private async Task PickPhoto()
        {
            var result = await MediaPicker.Default.PickPhotoAsync();
            if (result == null) return;

            using var stream = await result.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            OriginalImage = ImageSource.FromStream(() => new MemoryStream(ms.ToArray()));
            OnPropertyChanged(nameof(OriginalImage));
        }

        public void RegisterPoint(SKPoint point)
        {
            _points.Add(point);

            if (_points.Count == 2 && _scaleCmPerPixel == null)
            {
                // Referenzlinie: Pixelabstand
                var distPx = Distance(_points[0], _points[1]);

                // ⚡ Hier: User-Eingabe für reale Länge (z. B. per Popup)
                double realCm = 10.0; // Beispiel: 10 cm

                _scaleCmPerPixel = realCm / distPx;
                MeasurementInfo = $"Maßstab gesetzt: 1 px = {_scaleCmPerPixel:F3} cm";
                OnPropertyChanged(nameof(MeasurementInfo));
            }
            else if (_points.Count == 2 && _scaleCmPerPixel != null)
            {
                // zweite Messung (mit Maßstab)
                var distPx = Distance(_points[0], _points[1]);
                var distCm = distPx * _scaleCmPerPixel.Value;
                MeasurementInfo = $"Distanz: {distCm:F2} cm";
                OnPropertyChanged(nameof(MeasurementInfo));
                _points.Clear(); // reset für nächste Messung
            }
        }

        public void Draw(SKCanvas canvas)
        {
            var paintLine = new SKPaint
            {
                Color = SKColors.Red,
                StrokeWidth = 3,
                IsAntialias = true
            };
            var paintText = new SKPaint
            {
                Color = SKColors.Blue,
                TextSize = 32,
                IsAntialias = true
            };

            if (_points.Count == 2)
            {
                canvas.DrawLine(_points[0], _points[1], paintLine);

                var distPx = Distance(_points[0], _points[1]);
                string text = _scaleCmPerPixel == null
                    ? $"{distPx:F1} px"
                    : $"{distPx * _scaleCmPerPixel.Value:F2} cm";

                var mid = new SKPoint((_points[0].X + _points[1].X) / 2, (_points[0].Y + _points[1].Y) / 2);
                canvas.DrawText(text, mid, paintText);
            }

            foreach (var p in _points)
            {
                canvas.DrawCircle(p, 8, paintLine);
            }
        }

        private double Distance(SKPoint p1, SKPoint p2)
        {
            var dx = p2.X - p1.X;
            var dy = p2.Y - p1.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
        private async Task CapturePhoto()
        {
            try
            {
                var result = await MediaPicker.Default.CapturePhotoAsync();
                if (result == null) return;

                using var stream = await result.OpenReadAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                _originalBytes = ms.ToArray();

                OriginalImage = ImageSource.FromStream(() => new MemoryStream(_originalBytes));
                OnPropertyChanged(nameof(OriginalImage));
            }
            catch (Exception ex)
            {
                // TODO: Fehlerbehandlung
            }
        }

        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
