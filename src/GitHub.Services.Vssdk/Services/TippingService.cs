﻿using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using GitHub.Logging;
using Microsoft;
using Serilog;
using IServiceProvider = System.IServiceProvider;

namespace GitHub.Services.Vssdk.Services
{
    /// <summary>
    /// This service is a thin wrapper around <see cref="Microsoft.Internal.VisualStudio.Shell.Interop.IVsTippingService"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="IVsTippingService"/> interface is public, but contained within the 'Microsoft.VisualStudio.Shell.UI.Internal' assembly.
    /// To avoid a direct dependency on 'Microsoft.VisualStudio.Shell.UI.Internal', we use reflection to call this service.
    /// </remarks>
    public class TippingService : ITippingService
    {
        static readonly ILogger log = LogManager.ForContext<TippingService>();

        // This is the only supported ClientId
        public static readonly Guid ClientId = new Guid("D5D3B674-05BB-4942-B8EC-C3D13B5BD6EE");

        readonly IServiceProvider serviceProvider;

        public TippingService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public void RequestCalloutDisplay(Guid calloutId, string title, string message,
            bool isPermanentlyDismissible, FrameworkElement targetElement,
            Guid vsCommandGroupId, uint vsCommandId)
        {
            var screenPoint = !Splat.ModeDetector.InUnitTestRunner() ?
                    targetElement.PointToScreen(new Point(targetElement.ActualWidth / 2, 0)) : default;
            var point = new Microsoft.VisualStudio.OLE.Interop.POINT { x = (int)screenPoint.X, y = (int)screenPoint.Y };
            RequestCalloutDisplay(ClientId, calloutId, title, message, isPermanentlyDismissible,
                point, vsCommandGroupId, vsCommandId);
        }

        // The option to pass a command option is only available on Visual Studio 2017+.
        void RequestCalloutDisplay(Guid clientId, Guid calloutId, string title, string message,
            bool isPermanentlyDismissible, FrameworkElement targetElement,
            Guid vsCommandGroupId, uint vsCommandId, object commandOption = null)
        {
            var tippingService = serviceProvider.GetService(typeof(SVsTippingService));
            Assumes.Present(tippingService);
            var parameterTypes = new Type[] { typeof(Guid), typeof(Guid), typeof(string), typeof(string),
                typeof(bool), typeof(FrameworkElement), typeof(Guid), typeof(uint), typeof(object) };
            var method = tippingService.GetType().GetInterfaces()
                .Where(i => i.Name == "IVsTippingService")
                .FirstOrDefault()?.GetMethod("RequestCalloutDisplay", parameterTypes);
            var arguments = new object[] { clientId, calloutId, title, message, isPermanentlyDismissible, targetElement,
                    vsCommandGroupId, vsCommandId, commandOption };
            method.Invoke(tippingService, arguments);
        }

        // Available on Visual Studio 2015
        void RequestCalloutDisplay(Guid clientId, Guid calloutId, string title, string message, bool isPermanentlyDismissible,
            Microsoft.VisualStudio.OLE.Interop.POINT anchor, Guid vsCommandGroupId, uint vsCommandId)
        {
            var tippingService = serviceProvider.GetService(typeof(SVsTippingService));
            if (tippingService == null)
            {
                log.Error("Can't find {ServiceType}", typeof(SVsTippingService));
                return;
            }

            Assumes.Present(tippingService);
            var parameterTypes = new Type[] { typeof(Guid), typeof(Guid), typeof(string), typeof(string), typeof(bool),
                typeof(Microsoft.VisualStudio.OLE.Interop.POINT), typeof(Guid), typeof(uint) };
            var tippingServiceType = tippingService.GetType();
            var method = tippingService.GetType().GetInterfaces().Where(i => i.Name == "IVsTippingService")
                .FirstOrDefault()?.GetMethod("RequestCalloutDisplay", parameterTypes);
            if (method == null)
            {
                log.Error("Couldn't find method on {Type} with parameters {Parameters}", tippingServiceType, parameterTypes);
                return;
            }

            var arguments = new object[] { clientId, calloutId, title, message, isPermanentlyDismissible, anchor,
                    vsCommandGroupId, vsCommandId };
            method.Invoke(tippingService, arguments);
        }
    }

    [Guid("DCCC6A2B-F300-4DA1-92E1-8BF4A5BCA795")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [TypeIdentifier]
    [ComImport]
    public interface SVsTippingService
    {
        void RequestCalloutDisplay(Guid clientId, Guid calloutId, string title, string message,
            bool isPermanentlyDismissible, FrameworkElement targetElement,
            Guid vsCommandGroupId, uint vsCommandId, object commandOption = null);
    }
}
