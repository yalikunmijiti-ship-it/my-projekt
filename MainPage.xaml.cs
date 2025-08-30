using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using MauiAppMessSkizze.ViewModels;

namespace MauiAppMessSkizze
{

    public partial class MainPage : ContentPage
    {
        private readonly MainViewModel _vm;
        public MainPage(MainViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
            _vm = vm;
        }

        private void OnCanvasTouch(object sender, SKTouchEventArgs e)
        {
            if (e.ActionType == SKTouchAction.Released)
            {
                _vm.RegisterPoint(e.Location);
                OverlayCanvas.InvalidateSurface();
            }
            e.Handled = true;
        }

        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            _vm.Draw(canvas);
        }
    }

}
