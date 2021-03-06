﻿using System;
using System.Numerics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Animation;
using Stanton.App.Helpers;

namespace Stanton.App.Views.Detail
{
    public sealed partial class ShipDetailPage : Page
    {
        private static readonly WebView ModelWebView = WebViewSingleton.GetInstance();

        public ShipDetailPage()
        {
            this.InitializeComponent();
        }

        private void Page_LayoutUpdated(object sender, object e)
        {
            HeaderUpdated();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadWebView();
        }

        private void GridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var col = Math.Min(Math.Ceiling(ActualWidth / 400), GridView.Items.Count);
            ((ItemsWrapGrid)GridView.ItemsPanelRoot).ItemWidth = e.NewSize.Width / col;
        }

        private void MyWebView_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            WebViewProgressRing.IsActive = false;
            ModelWebView.Opacity = 1;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (WebViewGrid.Children.Contains(ModelWebView))
            {
                WebViewGrid.Children.Remove(ModelWebView);
            }
            if (e.NavigationMode == NavigationMode.Back)
            {
                if(FlipView.SelectedIndex == 0)
                {
                    //ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("backAnimation", DestinationImage);
                    //animation.Configuration = new DirectConnectedAnimationConfiguration();
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("forwardAnimation");
            if (animation != null)
            {
                //animation.TryStart(DestinationImage, new UIElement[] { MainContents });
            }
        }

        private void LoadWebView()
        {
            var uri = new Uri("https://sketchfab.com/models/a6af6d1ae2744a55820d00599aca71f2/embed?autostart=1&internal=1&ui_infos=0&ui_snapshots=1&ui_stop=0&ui_watermark=0");
            if (ModelWebView.Source != uri)
            {
                DisableWebView();
                ModelWebView.Navigate(uri);
            }
            else
            {
                EnableWebView();
            }
            if (!WebViewGrid.Children.Contains(ModelWebView))
            {
                WebViewGrid.Children.Add(ModelWebView);
            }
            ModelWebView.DOMContentLoaded += MyWebView_DOMContentLoaded;
        }

        private void EnableWebView()
        {
            WebViewProgressRing.IsActive = false;
            ModelWebView.Opacity = 1;
        }

        private void DisableWebView()
        {
            WebViewProgressRing.IsActive = true;
            ModelWebView.Opacity = 0.1;
        }

        private void HeaderUpdated()
        {
            FlipView.Height = Image.ActualHeight;
            var height = FlipView.ActualHeight - HeaderText.ActualHeight + 1;

            CompositionPropertySet scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(MainScrollViewer);
            Compositor compositor = scrollerPropertySet.Compositor;

            // Get the visual that represents our HeaderTextBlock 
            // And define the progress animation string
            var headerVisual = ElementCompositionPreview.GetElementVisual(ScrollHeader);
            String progress = $"Clamp(Abs(scroller.Translation.Y) / {height}, 0.0, 1.0)";

            var textExpression = compositor.CreateExpressionAnimation("Lerp(1.5, 1, " + progress + ")");
            textExpression.SetReferenceParameter("scroller", scrollerPropertySet);

            var offsetExpression = compositor.CreateExpressionAnimation($"-scroller.Translation.Y - {progress} * {height}");
            offsetExpression.SetReferenceParameter("scroller", scrollerPropertySet);
            headerVisual.StartAnimation("Offset.Y", offsetExpression);

            Visual textVisual = ElementCompositionPreview.GetElementVisual(HeaderText);
            Vector3 finalOffset = new Vector3(0, 0, 0);
            var headerOffsetAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector3(0,0,0), finalOffset, {progress})");
            headerOffsetAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            headerOffsetAnimation.SetVector3Parameter("finalOffset", finalOffset);
            textVisual.StartAnimation(nameof(Visual.Offset), headerOffsetAnimation);

            var opacityExpression = compositor.CreateExpressionAnimation($"{progress}");
            opacityExpression.SetReferenceParameter("scroller", scrollerPropertySet);
            textVisual.StartAnimation("opacity", opacityExpression);
        }

        private void MainContents_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ModelWebView.Width = e.NewSize.Width - 12;
            ModelWebView.Height = ModelWebView.Width * 9 / 16;
        }
    }
}
