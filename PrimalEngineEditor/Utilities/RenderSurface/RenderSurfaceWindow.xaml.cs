using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PrimalEngineEditor.Utilities
{
    /// <summary>
    /// Interaction logic for RenderSurfaceWindow.xaml
    /// </summary>
    public partial class RenderSurfaceWindow : UserControl, IDisposable
    {
        private RenderSurfaceHost _host = null;

        public RenderSurfaceWindow()
        {
            InitializeComponent();
            Loaded += OnRenderSurfaceWindowLoaded;
        }

        private void OnRenderSurfaceWindowLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnRenderSurfaceWindowLoaded;
            _host = new RenderSurfaceHost(ActualWidth, ActualHeight);
            Content = _host;
        }
        #region IDisposable support
        private bool _disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _host.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
