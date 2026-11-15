using System;
using System.Collections.Generic;
using KingdomFoundation.Api;
using UnityEngine;

namespace KingdomPortLayouts
{
    /// <summary>Optional integration API for querying rotated port geometry and declaring custom rotation constraints.</summary>
    public static class PortLayoutsApi
    {
        private sealed class DelegateDisposable : IDisposable
        {
            private Action dispose;
            internal DelegateDisposable(Action dispose) => this.dispose = dispose;
            public void Dispose()
            {
                var action = dispose;
                dispose = null;
                action?.Invoke();
            }
        }

        private static readonly List<Func<WorkspaceObject, bool>> RotationConstraints = new List<Func<WorkspaceObject, bool>>();

        /// <summary>Registers a predicate controlling whether a custom or native machine may rotate its ports.</summary>
        public static IDisposable RegisterRotationConstraint(Func<WorkspaceObject, bool> canRotate)
        {
            if (canRotate == null) throw new ArgumentNullException(nameof(canRotate));
            RotationConstraints.Add(canRotate);
            return new DelegateDisposable(() => RotationConstraints.Remove(canRotate));
        }

        /// <summary>Marks a specific machine type identifier as ineligible for port rotation.</summary>
        public static IDisposable RegisterUnrotatableMachineType(string machineTypeId)
        {
            if (string.IsNullOrWhiteSpace(machineTypeId))
                throw new ArgumentException("Machine type id is required.", nameof(machineTypeId));
            return RegisterRotationConstraint(element => !string.Equals(element?.TypeId, machineTypeId, StringComparison.Ordinal));
        }

        internal static bool AllowsRotation(WorkspaceObject element)
        {
            for (int i = 0; i < RotationConstraints.Count; i++)
            {
                if (!RotationConstraints[i](element))
                    return false;
            }
            return true;
        }

        internal static void ClearAll() => RotationConstraints.Clear();

        /// <summary>Returns whether a workspace building is eligible for port rotation.</summary>
        public static bool CanRotate(WorkspaceObject element) => element != null && PortGeometry.CanRotate(element);

        /// <summary>Gets the active six-way port layout index (0..5) for a building, or 0 for native default.</summary>
        public static int GetLayout(WorkspaceObject element) => element != null ? PortGeometry.Layout(element) : 0;

        /// <summary>Computes the effective building dimensions accounting for rotated top/bottom ports.</summary>
        public static Vector2 GetBuildingSize(WorkspaceObject element) => element != null ? PortGeometry.Size(element) : Vector2.zero;

        /// <summary>Computes the world-space pipe connection endpoint for a port, accounting for Port Layouts rotation.</summary>
        public static bool TryGetPortEndpoint(WorkspacePort port, out Vector3 endpoint)
        {
            if (port?.World == null || port.Parent?.World == null)
            {
                endpoint = default;
                return false;
            }
            endpoint = PortGeometry.Endpoint(port);
            return true;
        }

        /// <summary>Returns the world-space pipe connection endpoint for a port.</summary>
        public static Vector3 GetPortEndpoint(WorkspacePort port)
        {
            if (!TryGetPortEndpoint(port, out var endpoint))
                throw new InvalidOperationException("Port and parent building must have active world views.");
            return endpoint;
        }
    }
}

