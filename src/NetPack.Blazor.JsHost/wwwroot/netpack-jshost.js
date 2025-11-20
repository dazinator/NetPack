// NetPack Blazor JS Host Integration
// Provides the bridge between Blazor host and JavaScript applications

window.NetPackJsHost = window.NetPackJsHost || (function () {
    'use strict';

    const hosts = new Map();

    /**
     * Creates a host API wrapper that JavaScript applications can use.
     */
    function createHostApi(dotNetRef) {
        return {
            async checkPermission(permission) {
                return await dotNetRef.invokeMethodAsync('CheckPermission', permission);
            },

            async getUserInfo() {
                return await dotNetRef.invokeMethodAsync('GetUserInfo');
            },

            async getConfig(key) {
                return await dotNetRef.invokeMethodAsync('GetConfig', key);
            },

            async raiseHostEvent(eventName, eventData) {
                return await dotNetRef.invokeMethodAsync('RaiseHostEvent', eventName, eventData);
            },

            async fetch(request) {
                return await dotNetRef.invokeMethodAsync('Fetch', request);
            }
        };
    }

    /**
     * Creates a fallback transport using browser APIs for standalone mode.
     */
    function createFallbackTransport() {
        return {
            async fetch(request) {
                const response = await window.fetch(request.url, {
                    method: request.method || 'GET',
                    headers: request.headers,
                    body: request.body
                });

                const headers = {};
                response.headers.forEach((value, key) => {
                    headers[key] = value;
                });

                return {
                    status: response.status,
                    statusText: response.statusText,
                    ok: response.ok,
                    headers: headers,
                    body: await response.text()
                };
            }
        };
    }

    return {
        /**
         * Initialize the host for a specific element.
         */
        initialize: function (elementId, dotNetRef) {
            const hostApi = createHostApi(dotNetRef);
            const fallbackTransport = createFallbackTransport();

            hosts.set(elementId, {
                dotNetRef: dotNetRef,
                hostApi: hostApi,
                fallbackTransport: fallbackTransport,
                app: null
            });
        },

        /**
         * Load and start a JavaScript application.
         */
        loadAndStart: async function (elementId, appUrl, startFunction) {
            const host = hosts.get(elementId);
            if (!host) {
                console.error('Host not initialized for element:', elementId);
                return;
            }

            const element = document.getElementById(elementId);
            if (!element) {
                console.error('Element not found:', elementId);
                return;
            }

            try {
                // Dynamically import the JavaScript module
                const module = await import(appUrl);

                // Check if the start function exists
                if (typeof module[startFunction] !== 'function') {
                    console.error(`Start function '${startFunction}' not found in module`);
                    return;
                }

                // Create the environment object
                const env = {
                    hostCapabilities: host.hostApi,
                    fallbackTransport: host.fallbackTransport
                };

                // Start the application
                const app = await module[startFunction](element, env);
                host.app = app;

                console.log('NetPack JS app started:', elementId);
            } catch (error) {
                console.error('Error loading or starting JS app:', error);
            }
        },

        /**
         * Dispose of a host and clean up resources.
         */
        dispose: function (elementId) {
            const host = hosts.get(elementId);
            if (host) {
                // Call app's dispose method if it exists
                if (host.app && typeof host.app.dispose === 'function') {
                    host.app.dispose();
                }

                hosts.delete(elementId);
            }
        },

        /**
         * Get the host API for a specific element (for debugging).
         */
        getHost: function (elementId) {
            return hosts.get(elementId);
        }
    };
})();
